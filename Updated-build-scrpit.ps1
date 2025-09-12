<#
.SYNOPSIS
    Unified TeamCity Build Pipeline Script for .NET 8 Projects

.DESCRIPTION
    - Restores solution
    - Starts SonarQube analysis
    - Builds solution
    - Runs tests with coverage (centralized results + unique trx filenames)
    - Imports all TRX results into TeamCity
    - Publishes artifacts
    - Reports Code Coverage statistics to TeamCity
    - Ends SonarQube analysis
    - Optionally publishes projects
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
    /d:sonar.cs.opencover.reportsPaths="$ArtifactsDir/**/*.xml" `
    /d:sonar.cs.vstest.reportsPaths="$ArtifactsDir/**/*.trx" `
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
TeamCity-ProgressStart "Running Unit Tests with Coverage"
Log-Info "Running tests with coverage..."

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

dotnet test $SolutionPath `
    --no-build -c Release `
    --filter "$Filter" `
    --results-directory "$ArtifactsDir" `
    --logger:"trx;LogFileName=test_results_$timestamp.trx" `
    --collect:"XPlat Code Coverage" `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
Fail-OnError "Tests failed"
TeamCity-ProgressFinish "Running Unit Tests with Coverage"

# Locate and log coverage reports
$coverageReports = Get-ChildItem -Path $ArtifactsDir -Recurse -Filter "coverage.opencover.xml"
if ($coverageReports) {
    Log-Info "Coverage reports generated:"
    $totalLines = 0
    $coveredLines = 0

    foreach ($report in $coverageReports) {
        Log-Info "  $($report.FullName)"
        [xml]$xml = Get-Content $report.FullName
        foreach ($module in $xml.CoverageSession.Modules.Module) {
            foreach ($file in $module.Files.File) {
                $lines = $module.Classes.Class.Methods.Method.SequencePoints.SequencePoint
                if ($lines) {
                    $totalLines += $lines.Count
                    $coveredLines += ($lines | Where-Object { $_.vc -gt 0 }).Count
                }
            }
        }
    }

    if ($totalLines -gt 0) {
        $coveragePercent = [math]::Round(($coveredLines / $totalLines) * 100, 2)
        Log-Info "Code Coverage: $coveragePercent% ($coveredLines/$totalLines)"

        # Report coverage stats to TeamCity
        Write-Host "##teamcity[buildStatisticValue key='CodeCoverageAbsLTotal' value='$totalLines']"
        Write-Host "##teamcity[buildStatisticValue key='CodeCoverageAbsLCovered' value='$coveredLines']"
        Write-Host "##teamcity[buildStatisticValue key='CodeCoverageL' value='$coveragePercent']"
    } else {
        Write-Warning "⚠ No sequence points found in coverage reports!"
    }
} else {
    Write-Warning "⚠ No coverage.opencover.xml found!"
}

# Locate and log TRX results (import all for full count)
$trxReports = Get-ChildItem -Path $ArtifactsDir -Recurse -Filter "*.trx"
if ($trxReports) {
    Log-Info "TRX test results generated:"
    foreach ($trx in $trxReports) {
        Log-Info "  $($trx.FullName)"
        Write-Host "##teamcity[importData type='vstest' path='$($trx.FullName)']"
    }
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
