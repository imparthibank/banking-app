<#
.SYNOPSIS
    TeamCity Build Pipeline Script for .NET 8 Projects (Final Fix)

.DESCRIPTION
    - Restores solution
    - Starts SonarQube
    - Builds solution
    - Runs tests per project (unique trx for each)
    - Waits for results and imports all trx into TeamCity
    - Publishes coverage + statistics
    - Publishes selected projects as artifacts
    - Ends SonarQube
#>

param(
    [string]$SolutionPath = "Projects\WholesaleMarginComputation\WholesaleMarginComputation.sln",
    [string]$Filter = "FullyQualifiedName!~Wmc.Integration.Tests&FullyQualifiedName!~Wmc.Core.Core.Tests.TransactionCacheNodeDownTests",
    [string]$PublishProjects = "",
    [string]$ArtifactsDir = "$(Join-Path (Split-Path -Parent $SolutionPath) 'TestArtifacts')"
)

function Log-Info($msg) { Write-Host ">>> $msg" }
function Fail-OnError($msg) { if ($LASTEXITCODE -ne 0) { Write-Error "❌ $msg"; exit 1 } }

# Ensure artifact folder exists
if (-not (Test-Path $ArtifactsDir)) { New-Item -ItemType Directory -Path $ArtifactsDir | Out-Null }

# 1. Restore
Write-Host "##teamcity[progressStart 'Restoring Solution']"
dotnet restore $SolutionPath
Fail-OnError "Restore failed"
Write-Host "##teamcity[progressFinish 'Restoring Solution']"

# 2. SonarQube Begin
Write-Host "##teamcity[progressStart 'Starting SonarQube Analysis']"
dotnet sonarscanner begin `
    /k:"%sonar.project%" `
    /d:sonar.host.url="%sonar.host.url%" `
    /d:sonar.login="%sonar.login%" `
    /v:"%sonar.project.version%" `
    /d:sonar.branch.name="%teamcity.build.branch%" `
    /d:sonar.cs.opencover.reportsPaths="$ArtifactsDir/**/*.xml" `
    /d:sonar.cs.vstest.reportsPaths="$ArtifactsDir/**/*.trx"
Fail-OnError "SonarQube begin failed"
Write-Host "##teamcity[progressFinish 'Starting SonarQube Analysis']"

# 3. Build Solution
Write-Host "##teamcity[progressStart 'Building Solution']"
dotnet build $SolutionPath --no-incremental -c Release
Fail-OnError "Build failed"
Write-Host "##teamcity[progressFinish 'Building Solution']"

# 4. Run Tests Per Project
Write-Host "##teamcity[progressStart 'Running Unit Tests']"
$testProjects = Get-ChildItem (Split-Path $SolutionPath -Parent) -Recurse -Filter *.csproj | 
    Where-Object { $_.DirectoryName -match "Test" -and $_.Name -notmatch "Integration" }

if (-not $testProjects) {
    Write-Warning "No test projects found!"
} else {
    foreach ($proj in $testProjects) {
        $projName = $proj.BaseName
        $trxFile = Join-Path $ArtifactsDir "$($projName)_$(Get-Date -Format 'yyyyMMddHHmmss').trx"
        Log-Info "Running tests for $projName"

        dotnet test $proj.FullName `
            --no-build -c Release `
            --filter "$Filter" `
            --results-directory "$ArtifactsDir" `
            --logger:"trx;LogFileName=$([IO.Path]::GetFileName($trxFile))" `
            --collect:"XPlat Code Coverage" `
            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

        Fail-OnError "Tests failed for $projName"
    }
}

# Import all trx results after all tests finish
Start-Sleep -Seconds 3
$trxReports = Get-ChildItem $ArtifactsDir -Filter *.trx
foreach ($trx in $trxReports) {
    Log-Info "Importing test result: $($trx.FullName)"
    Write-Host "##teamcity[importData type='vstest' path='$($trx.FullName)']"
}
Write-Host "##teamcity[progressFinish 'Running Unit Tests']"

# 5. Coverage Reporting
$coverageReports = Get-ChildItem $ArtifactsDir -Filter coverage.opencover.xml -Recurse
if ($coverageReports) {
    $total=0; $covered=0
    foreach ($report in $coverageReports) {
        [xml]$xml = Get-Content $report.FullName
        foreach ($seq in $xml.SelectNodes("//SequencePoint")) {
            $total++
            if ([int]$seq.vc -gt 0) { $covered++ }
        }
    }
    if ($total -gt 0) {
        $pct = [math]::Round(($covered/$total)*100,2)
        Write-Host "##teamcity[buildStatisticValue key='CodeCoverageAbsLTotal' value='$total']"
        Write-Host "##teamcity[buildStatisticValue key='CodeCoverageAbsLCovered' value='$covered']"
        Write-Host "##teamcity[buildStatisticValue key='CodeCoverageL' value='$pct']"
        Log-Info "Code Coverage: $pct% ($covered/$total)"
    }
}

# 6. Publish Project Artifacts
if ($PublishProjects) {
    Write-Host "##teamcity[progressStart 'Publishing Projects']"
    $projects = $PublishProjects.Split(',')
    foreach ($proj in $projects) {
        $csprojFile = Get-ChildItem (Split-Path $SolutionPath -Parent) -Recurse -Filter "$proj.csproj" | Select-Object -First 1
        if ($csprojFile) {
            $publishDir = "publish/$proj"
            dotnet publish $csprojFile.FullName -c Release -o $publishDir
            Fail-OnError "Publish failed for $proj"
            Write-Host "##teamcity[publishArtifacts '$publishDir => $proj.zip']"
            Log-Info "Published artifact for $proj to TeamCity"
        } else {
            Write-Warning "Could not find $proj.csproj"
        }
    }
    Write-Host "##teamcity[progressFinish 'Publishing Projects']"
}

# 7. SonarQube End
Write-Host "##teamcity[progressStart 'Ending SonarQube Analysis']"
dotnet sonarscanner end /d:sonar.login="%sonar.login%"
Fail-OnError "SonarQube end failed"
Write-Host "##teamcity[progressFinish 'Ending SonarQube Analysis']"

Log-Info "✅ Build pipeline completed successfully!"
