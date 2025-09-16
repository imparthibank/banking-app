<#
.SYNOPSIS
    CI/CD pipeline script for TeamCity to build, test (with coverage), run SonarQube analysis, and publish selected projects.

.DESCRIPTION
    This script is designed to be used in TeamCity's PowerShell runner.
    - Uses TeamCity service messages for visibility in build logs
    - Accepts comma-separated values for TestProjects & PublishProjects
    - Supports optional NUnit filter expressions to exclude tests (e.g., skip integration tests)
#>

param(
    [string]$SolutionPath,        # Path to the .sln file
    [string]$TestProjects,        # Comma-separated list of test project .csproj files
    [string]$PublishProjects,     # Comma-separated list of publish project names (folders)
    [string]$ExcludeFilters,      # NUnit filter expression for exclusions (e.g. "FullyQualifiedName!~Wmc.Integration.Tests")
    [string]$SonarQubeProjectKey, # SonarQube project key
    [string]$SonarQubeHostUrl,    # SonarQube server URL
    [string]$SonarQubeToken,      # SonarQube authentication token
    [string]$BuildConfiguration = "Release", # Default build configuration
    [string]$PublishDir = "publish"          # Directory where published outputs will be stored
)

# Convert comma-separated strings into arrays
$TestProjectsArray = $TestProjects -split ","
$PublishProjectsArray = $PublishProjects -split ","

# Step 1: Notify TeamCity - Build starting
Write-Host "##teamcity[progressMessage 'Starting build for solution: $SolutionPath']"

# Step 2: Restore NuGet packages
Write-Host "##teamcity[progressMessage 'Restoring NuGet packages']"
dotnet restore $SolutionPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "##teamcity[buildProblem description='NuGet restore failed']"
    exit 1
}

# Step 3: Build solution
Write-Host "##teamcity[progressMessage 'Building solution in $BuildConfiguration mode']"
dotnet build $SolutionPath --no-restore -c $BuildConfiguration
if ($LASTEXITCODE -ne 0) {
    Write-Host "##teamcity[buildProblem description='Solution build failed']"
    exit 1
}

# Step 4: Begin SonarQube analysis
Write-Host "##teamcity[progressMessage 'Starting SonarQube analysis']"
dotnet sonarscanner begin `
    /k:"$SonarQubeProjectKey" `
    /d:sonar.host.url="$SonarQubeHostUrl" `
    /d:sonar.login="$SonarQubeToken" `
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
if ($LASTEXITCODE -ne 0) {
    Write-Host "##teamcity[buildProblem description='SonarQube analysis start failed']"
    exit 1
}

# Step 5: Run tests with coverage
Write-Host "##teamcity[progressMessage 'Running unit tests with coverage']"
foreach ($testProject in $TestProjectsArray) {
    Write-Host "##teamcity[progressMessage 'Testing project: $testProject']"
    $testCommand = @(
        "dotnet test `"$testProject`"",
        "--no-build",
        "-c $BuildConfiguration",
        "--collect:`"XPlat Code Coverage`"",
        "--logger:`"trx`"",
        "--results-directory:`"TestResults`""
    )

    # Append filter only if ExcludeFilters parameter is passed
    if ($ExcludeFilters -and $ExcludeFilters.Trim() -ne "") {
        $testCommand += "--filter `"$ExcludeFilters`""
    }

    Invoke-Expression ($testCommand -join " ")
    if ($LASTEXITCODE -ne 0) {
        Write-Host "##teamcity[buildProblem description='Unit tests failed for $testProject']"
        exit 1
    }
}

# Step 6: End SonarQube analysis
Write-Host "##teamcity[progressMessage 'Finishing SonarQube analysis']"
dotnet sonarscanner end /d:sonar.login="$SonarQubeToken"

# Step 7: Publish selected projects
foreach ($project in $PublishProjectsArray) {
    Write-Host "##teamcity[progressMessage 'Publishing project: $project']"
    $publishPath = Join-Path $PublishDir $project
    dotnet publish "Projects/WholesaleMarginComputation/$project/$project.csproj" `
        -c $BuildConfiguration `
        -o $publishPath
    if ($LASTEXITCODE -ne 0) {
        Write-Host "##teamcity[buildProblem description='Publish failed for $project']"
        exit 1
    }
}

# Step 8: Mark build as successful
Write-Host "##teamcity[progressMessage 'Build completed successfully!']"
