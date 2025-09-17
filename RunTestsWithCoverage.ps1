# ================================
# STEP 4: RUN UNIT TESTS WITH COVERAGE
# ================================

Write-Host "##teamcity[progressStart 'Running Unit Tests with Coverage']"
Write-Host ">>> Running tests with coverage..."

# Change directory to solution folder
cd "Projects\WholesaleMarginComputation"

# PARAMETERS
$solution = "WholesaleMarginComputation.sln"
$testFilter = "FullyQualifiedName!~Wmc.Integration.Tests"    # Exclude integration tests
$resultsDirectory = "$(Get-Location)\TestResults"            # Central results folder

# Ensure results directory exists
if (-not (Test-Path $resultsDirectory)) {
    New-Item -ItemType Directory -Force -Path $resultsDirectory | Out-Null
}

# Run tests with coverage, TeamCity + TRX logger enabled
dotnet test $solution `
    --no-build `
    -c Release `
    --filter "$testFilter" `
    --results-directory "$resultsDirectory" `
    --collect:"XPlat Code Coverage" `
    --logger:"trx;LogFileName=test_results.trx" `
    --logger:"console;verbosity=normal" `
    --logger:"teamcity" `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# Check exit code to fail build if tests fail
if ($LASTEXITCODE -ne 0) {
    Write-Error "Unit tests failed. See TeamCity test report for details."
    Write-Host "##teamcity[buildStatus status='FAILURE' text='Unit tests failed']"
    exit 1
}

Write-Host ">>> Tests completed successfully. Results stored in $resultsDirectory"
Write-Host "##teamcity[progressFinish 'Running Unit Tests with Coverage']"

# In your STEP 2 (SonarQube Begin), update the paths to match this central directory:
"/d:sonar.cs.vstest.reportsPaths=$(Get-Location)/TestResults/*.trx"
"/d:sonar.cs.opencover.reportsPaths=$(Get-Location)/TestResults/**/coverage.opencover.xml"
