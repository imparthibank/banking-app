<#
.SYNOPSIS
TeamCity CI Build Script for .NET 8 Solution with SonarQube & Coverage

.DESCRIPTION
This script performs a full CI pipeline:
1. Cleans previous artifacts
2. Restores NuGet packages
3. Builds the solution
4. Runs unit tests with coverage collection (excluding integration tests)
5. Merges coverage reports into a single OpenCover report
6. Runs SonarQube analysis with coverage data
7. Publishes selected projects (e.g., Web APIs, Services)
8. Publishes build artifacts (coverage & published output) to TeamCity

.PARAMETERS (Set in TeamCity UI)
- teamcity.project.solutionPath         : Path to .sln file
- teamcity.project.testProjects         : Semicolon-separated list of test project paths
- teamcity.project.publishProjects      : Semicolon-separated list of projects to publish
- teamcity.project.sonarProjectKey      : SonarQube Project Key
- teamcity.project.sonarHostUrl         : SonarQube Server URL
- teamcity.project.sonarLoginToken      : SonarQube Authentication Token (secure)
#>

param(
    [string]$SolutionPath = "%teamcity.project.solutionPath%",
    [string]$TestProjects = "%teamcity.project.testProjects%",
    [string]$PublishProjects = "%teamcity.project.publishProjects%",
    [string]$SonarProjectKey = "%teamcity.project.sonarProjectKey%",
    [string]$SonarHostUrl = "%teamcity.project.sonarHostUrl%",
    [string]$SonarLoginToken = "%teamcity.project.sonarLoginToken%"
)

# Helper function: stops the build and fails it with a message if something goes wrong
function Fail-Build($msg) {
    Write-Host "##teamcity[buildStatus status='FAILURE' text='$msg']"
    exit 1
}

Write-Host "##teamcity[progressMessage 'Starting CI Build Pipeline...']"

# 1️⃣ CLEAN PREVIOUS ARTIFACTS
# Purpose: Ensure we start with a clean state and no stale files affect the build.
Write-Host "##teamcity[progressMessage 'Cleaning artifacts folder...']"
Remove-Item -Recurse -Force .\artifacts -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path .\artifacts | Out-Null

# 2️⃣ RESTORE DEPENDENCIES
# Purpose: Download all NuGet packages required by the solution.
Write-Host "##teamcity[progressMessage 'Restoring NuGet packages...']"
dotnet restore $SolutionPath
if ($LASTEXITCODE -ne 0) { Fail-Build "NuGet restore failed" }

# 3️⃣ BUILD THE SOLUTION
# Purpose: Compile all projects in Release mode to ensure production-ready builds.
Write-Host "##teamcity[progressMessage 'Building solution in Release mode...']"
dotnet build $SolutionPath -c Release --no-restore
if ($LASTEXITCODE -ne 0) { Fail-Build "Build failed" }

# 4️⃣ RUN UNIT TESTS WITH COVERAGE
# Purpose: Execute only unit tests (excluding integration tests) and collect code coverage using Coverlet.
Write-Host "##teamcity[progressMessage 'Running Unit Tests with Coverage...']"

$projects = $TestProjects -split ";"
foreach ($proj in $projects) {
    Write-Host "##teamcity[progressMessage 'Testing project: $proj']"
    dotnet test $proj `
      -c Release `
      --no-build `
      --filter "FullyQualifiedName!~Wmc.Integration.Tests" ` # Filters out integration tests
      --collect:"XPlat Code Coverage" `
      /p:CollectCoverage=true `                              # Enables Coverlet coverage collection
      /p:CoverletOutput=../../artifacts/coverage/$proj/ `    # Stores coverage results per project
      /p:CoverletOutputFormat=opencover `                    # Generates OpenCover format (for SonarQube)
      /p:Exclude="[xunit.*]*,[*.Tests]*,[*]Migrations.*"     # Excludes test & migration code from coverage
    if ($LASTEXITCODE -ne 0) { Fail-Build "Unit tests failed in $proj" }
}

# 5️⃣ MERGE COVERAGE REPORTS
# Purpose: Combine coverage reports from multiple projects into one single report.
Write-Host "##teamcity[progressMessage 'Merging coverage reports...']"
dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.*   # Install ReportGenerator if not installed
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"

reportgenerator `
  -reports:"artifacts/coverage/**/coverage.opencover.xml" `
  -targetdir:"artifacts/coverage/merged" `
  -reporttypes:opencover

# 6️⃣ SONARQUBE ANALYSIS
# Purpose: Run SonarQube static code analysis and push results (including coverage) to Sonar dashboard.
Write-Host "##teamcity[progressMessage 'Running SonarQube Analysis...']"

dotnet sonarscanner begin `
  /k:"$SonarProjectKey" `
  /d:sonar.login="$SonarLoginToken" `
  /d:sonar.host.url="$SonarHostUrl" `
  /d:sonar.cs.opencover.reportsPaths="artifacts/coverage/merged/coverage.opencover.xml"

dotnet build $SolutionPath -c Release --no-incremental    # Required step for Sonar analysis

dotnet sonarscanner end /d:sonar.login="$SonarLoginToken"

# 7️⃣ PUBLISH PROJECTS
# Purpose: Publish selected projects (e.g., APIs, background services) for deployment.
Write-Host "##teamcity[progressMessage 'Publishing projects...']"
$publishProjectsArray = $PublishProjects -split ";"
foreach ($pubProj in $publishProjectsArray) {
    Write-Host "##teamcity[progressMessage 'Publishing: $pubProj']"
    dotnet publish $pubProj -c Release -o artifacts/publish/$pubProj --no-build
    if ($LASTEXITCODE -ne 0) { Fail-Build "Publishing failed for $pubProj" }
}

# 8️⃣ PUBLISH ARTIFACTS
# Purpose: Make coverage report and published outputs available in TeamCity artifacts tab.
Write-Host "##teamcity[progressMessage 'Publishing artifacts to TeamCity...']"
Write-Host "##teamcity[publishArtifacts 'artifacts/coverage/merged/coverage.opencover.xml => coverage']"
Write-Host "##teamcity[publishArtifacts 'artifacts/publish/**/* => published']"

# ✅ SUCCESS
Write-Host "##teamcity[progressMessage 'Build Pipeline Completed Successfully']"
Write-Host "##teamcity[buildStatus status='SUCCESS' text='Build + Tests + SonarQube Completed']"
