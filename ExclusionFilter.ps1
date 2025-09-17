Write-Host "##teamcity[progressStart 'Running Unit Tests with Coverage']"
Write-Host ">>> Discovering and running tests from solution WholesaleMarginComputation.sln"

$solutionPath = "Projects\WholesaleMarginComputation\WholesaleMarginComputation.sln"
$solutionDir  = Split-Path $solutionPath -Parent
$resultsDir   = Join-Path $solutionDir "TestResults"

# Clean & recreate results folder
if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

# --- Parse .sln file to get project references ---
$slnContent = Get-Content $solutionPath
$projectPaths = @()

foreach ($line in $slnContent) {
    if ($line -match 'Project\(".*"\) = ".*", "([^"]+\.csproj)"') {
        $projPath = $Matches[1] -replace '\\','/'  # Normalize path separators
        $fullPath = Join-Path $solutionDir $projPath
        if (Test-Path $fullPath) {
            $projectPaths += $fullPath
        }
    }
}

# Filter only test projects (by naming convention)
$testProjects = $projectPaths | Where-Object { $_ -match '\.Tests\.csproj$' -or $_ -match 'Test\.csproj$' }

# ✅ Exclude Integration Tests Project(s)
$excludedProjects = @(
    "Wmc.Integration.Tests"  # add more project names here if needed
)
$testProjects = $testProjects | Where-Object {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($_)
    $excludedProjects -notcontains $projName
}

if (-not $testProjects) {
    Write-Warning "No test projects found in solution after filtering!"
    exit 0
}

foreach ($proj in $testProjects) {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($proj)
    $projResultsDir = Join-Path $resultsDir $projName
    New-Item -ItemType Directory -Force -Path $projResultsDir | Out-Null

    Write-Host ">>> Running tests for project: $projName"

    # ✅ Exclude specific test classes using --filter
    $filter = 'FullyQualifiedName!~TransactionCacheNodeDownTests'

    dotnet test $proj `
        --no-build `
        -c Release `
        --filter $filter `
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
