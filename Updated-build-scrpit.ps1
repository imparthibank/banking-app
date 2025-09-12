<#
.SYNOPSIS
    Unified TeamCity Build Pipeline Script for .NET 8 Projects
.DESCRIPTION
    - Restores solution
    - Starts SonarQube analysis
    - Builds solution
    - Runs tests per project with coverage (unique TRX for each)
    - Imports all results into TeamCity and pushes statistics
    - Publishes coverage + test artifacts
    - Optionally publishes selected projects
    - Ends SonarQube analysis
#>

param(
    [string]$SolutionPath = "Projects\WholesaleMarginComputation\WholesaleMarginComputation.sln",
    [string]$Filter = "FullyQualifiedName!~Wmc.Integration.Tests&FullyQualifiedName!~Wmc.Core.Core.Tests.TransactionCacheNodeDownTests",
    [string]$PublishProjects = "",
    [string]$ArtifactsDir = "$(Join-Path (Split-Path -Parent $SolutionPath) 'TestArtifacts')"
)

# -----------------------
# Helper Functions
# -----------------------
function Log-Info($message) {
    Write-Host ">>> $message"
}

function Fail-OnError($message) {
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ $message"
        Write-Host "##teamcity[buildProblem description='$message']"
        exit 1
    }
}

function TC-ProgressStart($message) {
    Write-Host "##teamcity[progressStart '$message']"
}

function TC-ProgressFinish($message) {
    Write-Host "##teamcity[progressFinish '$message']"
}

# Ensure artifacts directory exists
if (-not (Test-Path $ArtifactsDir)) {
    New-Item -ItemType Directory -Path $ArtifactsDir | Out-Null
}

# -----------------------
# Step 1: Restore Solution
# -----------------------
TC-ProgressStart "Restoring Solution"
Log-Info "Restoring solution..."
dotnet restore $SolutionPath
Fail-OnError "Restore failed"
TC-ProgressFinish "Restoring Solution"

# -----------------------
# Step 2: SonarQube Begin
# -----------------------
TC-ProgressStart "Starting SonarQube Analysis - BEGIN"
Log-Info "Starting SonarQube Analysis..."

$params = @{
    ProjectKey = "%sonar.project%"
    Login      = "%sonar.login%"
    HostUrl    = "%sonar.host.url%"
    Version    = "%sonar.project.version%"
    Branch     = "%teamcity.build.branch%"
    Exclusions = "%sonar.excluded.path%"
    JavaHome   = $env:JAVA_HOME
}

Log-Info "PARAMETERS RECEIVED FROM TEAMCITY"
foreach ($key in $params.Keys) {
    if (-not $params[$key]) { throw "❌ Missing parameter: $key" }
    Log-Info "$key = $($params[$key])"
}

dotnet sonarscanner begin `
    /k:"$($params.ProjectKey)" `
    /d:sonar.host.url="$($params.HostUrl)" `
    /d:sonar.login="$($params.Login)" `
    /v:"$($params.Version)" `
    /d:sonar.branch.name="$($params.Branch)" `
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" `
    /d:sonar.cs.vstest.reportsPaths="**/*.trx" `
    /d:sonar.exclusions="**/bin/**,**/obj/**" `
    /d:sonar.java.home="$($params.JavaHome)"
Fail-OnError "SonarQube begin failed"
TC-ProgressFinish "Starting SonarQube Analysis - BEGIN"

# -----------------------
# Step 3: Build Solution
# -----------------------
TC-ProgressStart "Building Solution"
Log-Info "Building solution..."
dotnet build $SolutionPath --no-incremental -c Release
Fail-OnError "Build failed"
TC-ProgressFinish "Building Solution"

# -----------------------
# Step 4: Run Tests Per Project (Unique TRX + Coverage)
# -----------------------
TC-ProgressStart "Running Unit Tests"
Log-Info "Running tests with coverage per project..."

$testProjects = Get-ChildItem (Split-Path $SolutionPath -Parent) -Recurse -Filter *.csproj |
    Where-Object { $_.DirectoryName -match "Test" -and $_.Name -notmatch "Integration" }

$trxFiles = @()
$totalTests = 0; $passedTests = 0; $failedTests = 0

foreach ($proj in $testProjects) {
    $projName = $proj.BaseName
    $trxFile = Join-Path $ArtifactsDir "$($projName)_$(Get-Date -Format 'yyyyMMddHHmmssfff').trx"
    $trxFiles += $trxFile

    Log-Info "Running tests for $projName"
    Write-Host "##teamcity[progressMessage 'Running tests for $projName']"

    dotnet test $proj.FullName `
        --no-build -c Release `
        --filter "$Filter" `
        --results-directory "$ArtifactsDir" `
        --logger:"trx;LogFileName=$([IO.Path]::GetFileName($trxFile))" `
        --collect:"XPlat Code Coverage" `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
    Fail-OnError "Tests failed for $projName"
}

# Wait and import all TRX files
foreach ($file in $trxFiles) {
    $retries = 5
    while (-not (Test-Path $file) -and $retries -gt 0) {
        Start-Sleep -Seconds 1
        $retries--
    }
}

foreach ($trx in $trxFiles) {
    if (Test-Path $trx) {
        Write-Host "##teamcity[importData type='vstest' path='$trx']"

        # Parse TRX and aggregate statistics
        [xml]$trxXml = Get-Content $trx
        $results = $trxXml.TestRun.Results.UnitTestResult
        $totalTests += $results.Count
        $passedTests += ($results | Where-Object { $_.outcome -eq "Passed" }).Count
        $failedTests += ($results | Where-Object { $_.outcome -eq "Failed" }).Count
    }
}

Write-Host "##teamcity[buildStatisticValue key='TotalTests' value='$totalTests']"
Write-Host "##teamcity[buildStatisticValue key='PassedTests' value='$passedTests']"
Write-Host "##teamcity[buildStatisticValue key='FailedTests' value='$failedTests']"
Log-Info "Test Summary: Passed=$passedTests / Failed=$failedTests / Total=$totalTests"

if ($failedTests -gt 0) {
    Write-Host "##teamcity[buildProblem description='Some tests failed: $failedTests failed out of $totalTests']"
}

# Copy coverage files
$coverageReports = Get-ChildItem -Path (Split-Path $SolutionPath -Parent) -Recurse -Filter "coverage.opencover.xml"
foreach ($report in $coverageReports) {
    Log-Info "Coverage report: $($report.FullName)"
    Copy-Item $report.FullName $ArtifactsDir -Force
}

Write-Host "##teamcity[publishArtifacts '$ArtifactsDir => TestArtifacts.zip']"
Log-Info "Published test artifacts to TeamCity"

TC-ProgressFinish "Running Unit Tests"

# -----------------------
# Step 5: Optional Publish Projects
# -----------------------
if ($PublishProjects) {
    TC-ProgressStart "Publishing Projects"
    $rootPath = "Projects"
    $projects = $PublishProjects.Split(',')

    foreach ($proj in $projects) {
        Log-Info "Publishing project: $proj"
        $csprojFile = Get-ChildItem -Path $rootPath -Filter "$proj.csproj" -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $csprojFile) {
            Write-Error "ERROR: Could not find $proj.csproj under $rootPath"
            exit 1
        }
        dotnet publish $csprojFile.FullName -c Release -o "publish/$($proj)"
        Fail-OnError "Publish failed for $proj"
        Write-Host "##teamcity[publishArtifacts 'publish/$($proj)']"
    }
    TC-ProgressFinish "Publishing Projects"
}

# -----------------------
# Step 6: SonarQube End
# -----------------------
TC-ProgressStart "Ending SonarQube Analysis - END"
Log-Info "Ending SonarQube analysis..."
dotnet sonarscanner end /d:sonar.login="$($params.Login)"
Fail-OnError "SonarQube end failed"
TC-ProgressFinish "Ending SonarQube Analysis - END"

Log-Info "✅ Build pipeline completed successfully!"
