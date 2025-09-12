<#
.SYNOPSIS
    Unified TeamCity Build Pipeline Script for .NET 8 Projects
.DESCRIPTION
    - Restores solution
    - Starts SonarQube analysis
    - Builds solution
    - Runs tests with coverage
    - Logs report paths and publishes artifacts
    - Ends SonarQube analysis
    - Optionally publishes projects
#>

param(
    [string]$SolutionPath = "Projects\WholesaleMarginComputation\WholesaleMarginComputation.sln",
    [string]$Filter = "FullyQualifiedName!~Wmc.Integration.Tests&FullyQualifiedName!~Wmc.Core.Core.Tests.TransactionCacheNodeDownTests",
    [string]$PublishProjects = "",
    [string]$TestResultsFile = "test_results.trx",
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
        exit 1
    }
}

function TeamCity-ProgressStart($message) {
    Write-Host "##teamcity[progressStart '$message']"
}

function TeamCity-ProgressFinish($message) {
    Write-Host "##teamcity[progressFinish '$message']"
}

# Ensure artifacts directory exists
if (-not (Test-Path $ArtifactsDir)) {
    New-Item -ItemType Directory -Path $ArtifactsDir | Out-Null
}

# -----------------------
# Step 1: Restore Solution
# -----------------------
TeamCity-ProgressStart "Restoring Solution"
Log-Info "Restoring solution..."
dotnet restore $SolutionPath
Fail-OnError "Restore failed"
TeamCity-ProgressFinish "Restoring Solution"

# -----------------------
# Step 2: SonarQube Begin
# -----------------------
TeamCity-ProgressStart "Starting SonarQube Analysis - BEGIN"
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
TeamCity-ProgressFinish "Starting SonarQube Analysis - BEGIN"

# -----------------------
# Step 3: Build Solution
# -----------------------
TeamCity-ProgressStart "Building Solution"
Log-Info "Building solution..."
dotnet build $SolutionPath --no-incremental -c Release
Fail-OnError "Build failed"
TeamCity-ProgressFinish "Building Solution"

# -----------------------
# Step 4: Run Tests + Coverage
# -----------------------
Log-Info "Running tests with coverage..."
dotnet test $SolutionPath `
    --no-build -c Release `
    --filter "$Filter" `
    --collect:"XPlat Code Coverage" `
    --logger:"trx;LogFileName=$TestResultsFile" `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
Fail-OnError "Tests failed"

# Locate and log coverage reports
$coverageReports = Get-ChildItem -Path (Split-Path $SolutionPath -Parent) -Recurse -Filter "coverage.opencover.xml"
if ($coverageReports) {
    Log-Info "Coverage reports generated:"
    foreach ($report in $coverageReports) {
        Log-Info "  $($report.FullName)"
        Copy-Item $report.FullName $ArtifactsDir -Force
    }
} else {
    Write-Warning "⚠ No coverage.opencover.xml found!"
}

# Locate and log TRX results
$trxReports = Get-ChildItem -Path (Split-Path $SolutionPath -Parent) -Recurse -Filter $TestResultsFile
if ($trxReports) {
    Log-Info "TRX test results generated:"
    foreach ($trx in $trxReports) {
        Log-Info "  $($trx.FullName)"
        Copy-Item $trx.FullName $ArtifactsDir -Force
    }
    Write-Host "##teamcity[importData type='vstest' path='$($trxReports[0].FullName)']"
} else {
    Write-Warning "⚠ No .trx test results found!"
}

# Publish artifacts (zip)
Write-Host "##teamcity[publishArtifacts '$ArtifactsDir => TestArtifacts.zip']"
Log-Info "Published artifacts to TeamCity: TestArtifacts.zip"

# -----------------------
# Step 5: Optional Publish Projects
# -----------------------
if ($PublishProjects) {
    TeamCity-ProgressStart "Publishing Projects"
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
    TeamCity-ProgressFinish "Publishing Projects"
}

# -----------------------
# Step 6: SonarQube End
# -----------------------
TeamCity-ProgressStart "Ending SonarQube Analysis - END"
Log-Info "Ending SonarQube analysis..."
dotnet sonarscanner end /d:sonar.login="$($params.Login)"
Fail-OnError "SonarQube end failed"
TeamCity-ProgressFinish "Ending SonarQube Analysis - END"

Log-Info "✅ Build pipeline completed successfully!"
