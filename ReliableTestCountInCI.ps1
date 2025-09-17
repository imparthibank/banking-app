Write-Host "##teamcity[progressStart 'Running Unit Tests with Coverage']"
Write-Host ">>> Running tests with coverage (per project)..."

$solutionDir = "Projects\WholesaleMarginComputation"
$resultsDir = Join-Path $solutionDir "TestResults"
if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

# Discover all test projects except integration tests
$testProjects = Get-ChildItem -Path $solutionDir -Recurse -Filter "*.csproj" |
    Where-Object { $_.FullName -notmatch "Wmc.Integration.Tests" }

foreach ($proj in $testProjects) {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($proj.Name)
    $projResultsDir = Join-Path $resultsDir $projName
    New-Item -ItemType Directory -Force -Path $projResultsDir | Out-Null

    Write-Host ">>> Running tests for project: $projName"

    dotnet test $proj.FullName `
        --no-build `
        -c Release `
        --results-directory "$projResultsDir" `
        --collect:"XPlat Code Coverage" `
        --logger:"trx;LogFileName=$projName.trx" `
        --logger:"teamcity" `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed in $projName"
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Unit tests failed in $projName']"
        exit 1
    }
}

Write-Host ">>> All projects tested successfully. Results saved in $resultsDir"
Write-Host "##teamcity[progressFinish 'Running Unit Tests with Coverage']"



# Update SonarQube Parameters (STEP 2)

# Update your SonarQube begin step to aggregate results:
"/d:sonar.cs.vstest.reportsPaths=$(Get-Location)/TestResults/**/*.trx"
"/d:sonar.cs.opencover.reportsPaths=$(Get-Location)/TestResults/**/coverage.opencover.xml"
