param(
    [string]$SolutionFile = "%Solution.File%",
    [string]$Configuration = "%Build.Configuration%",
    [string]$TestProjectsPattern = "%Test.Projects.Pattern%",
    [string]$ExcludeTestClasses = "%Test.Exclude.Classes%",
    [string]$CoverageDirectory = "%Coverage.Output.Directory%",
    [string]$SonarQubeProjectKey = "%Sonar.Project.Key%",
    [string]$SonarQubeHostUrl = "%Sonar.Host.Url%",
    [string]$SonarQubeToken = "%Sonar.Token%"
)

Write-Host "##teamcity[progressMessage 'Starting Build and Test Pipeline']"

# ------------------------------
# Step 1: Clean & Restore
# ------------------------------
Write-Host "##teamcity[progressMessage 'Cleaning solution...']"
dotnet clean $SolutionFile -c $Configuration

Write-Host "##teamcity[progressMessage 'Restoring NuGet packages...']"
dotnet restore $SolutionFile

# ------------------------------
# Step 2: Build
# ------------------------------
Write-Host "##teamcity[progressMessage 'Building solution ($Configuration)...']"
dotnet build $SolutionFile -c $Configuration --no-restore

# ------------------------------
# Step 3: SonarQube Begin
# ------------------------------
Write-Host "##teamcity[progressMessage 'Starting SonarQube Analysis...']"
dotnet sonarscanner begin `
    /k:"$SonarQubeProjectKey" `
    /d:sonar.host.url="$SonarQubeHostUrl" `
    /d:sonar.login="$SonarQubeToken" `
    /d:sonar.cs.opencover.reportsPaths="$CoverageDirectory\**\coverage.opencover.xml"

# ------------------------------
# Step 4: Discover Test Projects
# ------------------------------
Write-Host "##teamcity[progressMessage 'Discovering Test Projects...']"
$testProjects = Get-ChildItem -Recurse -Path . -Filter *.csproj |
    Where-Object { $_.FullName -match $TestProjectsPattern }

if (-not $testProjects) {
    Write-Host "##teamcity[buildProblem description='No test projects found matching $TestProjectsPattern']"
    exit 1
}

Write-Host "Discovered Test Projects:"
$testProjects | ForEach-Object { Write-Host " - $($_.FullName)" }

# ------------------------------
# Step 5: Run Tests with Coverage
# ------------------------------
foreach ($project in $testProjects) {
    Write-Host "##teamcity[progressMessage 'Running tests in $($project.FullName)...']"

    $excludeFilter = ""
    if ($ExcludeTestClasses) {
        $classes = $ExcludeTestClasses -split ","
        $filterExpr = $classes | ForEach-Object { "FullyQualifiedName!~$_" }
        $excludeFilter = "--filter `"$($filterExpr -join '&')`""
    }

    dotnet test $project.FullName `
        -c $Configuration `
        --no-build `
        --collect:"XPlat Code Coverage" `
        $excludeFilter `
        --logger:"trx;LogFileName=$($project.BaseName)-TestResults.trx"

    # Publish TRX to TeamCity
    $trxPath = Get-ChildItem -Recurse -Filter "$($project.BaseName)-TestResults.trx" | Select-Object -First 1
    if ($trxPath) {
        Write-Host "##teamcity[importData type='vstest' path='$($trxPath.FullName)']"
    }
}

# ------------------------------
# Step 6: SonarQube End
# ------------------------------
Write-Host "##teamcity[progressMessage 'Completing SonarQube Analysis...']"
dotnet sonarscanner end /d:sonar.login="$SonarQubeToken"

Write-Host "##teamcity[progressMessage 'Build & Test pipeline completed successfully']"
