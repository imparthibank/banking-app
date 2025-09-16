param(
    [string] $SolutionPath = "%SolutionPath%",                    # Solution path
    [string] $TestProjects = "%TestProjects%",                    # Comma-separated test csproj paths
    [string] $PublishProjects = "%PublishProjects%",              # Comma-separated publish csproj paths
    [string] $SonarQubeProjectKey = "%SonarQubeProjectKey%",      # SonarQube Project Key
    [string] $SonarQubeHostUrl = "%SonarQubeHostUrl%",            # SonarQube Server URL
    [string] $SonarQubeToken = "%SonarQubeToken%"                 # SonarQube Authentication Token
)

# ---------------------------
# Step 1: Announce Build Start
# ---------------------------
Write-Host "##teamcity[progressMessage 'Starting CI Build Pipeline with SonarQube Analysis...']"

# ---------------------------
# Step 2: Clear and Restore NuGet packages with Retry Logic
# ---------------------------
Write-Host "##teamcity[progressMessage 'Clearing NuGet cache (initial)']"
dotnet nuget locals all --clear

$maxRetries = 2
$retryCount = 0
$restoreSucceeded = $false

while (-not $restoreSucceeded -and $retryCount -lt $maxRetries) {
    $retryCount++
    Write-Host "##teamcity[progressMessage 'Restoring NuGet packages (Attempt $retryCount of $maxRetries)']"

    dotnet restore $SolutionPath --force --no-cache
    if ($LASTEXITCODE -eq 0) {
        $restoreSucceeded = $true
        Write-Host "##teamcity[progressMessage 'NuGet restore completed successfully (Attempt $retryCount)']"
    }
    else {
        Write-Host "##teamcity[buildProblem description='NuGet restore failed on attempt $retryCount']"
        if ($retryCount -lt $maxRetries) {
            Write-Host "##teamcity[progressMessage 'Retrying NuGet restore after clearing cache again...']"
            dotnet nuget locals all --clear
            Start-Sleep -Seconds 5
        }
    }
}

if (-not $restoreSucceeded) {
    Write-Host "##teamcity[buildStatus status='FAILURE' text='NuGet restore failed after $maxRetries attempts']"
    exit 1
}

# ---------------------------
# Step 3: SonarQube Begin Analysis
# ---------------------------
Write-Host "##teamcity[progressMessage 'Starting SonarQube Analysis (Begin Step)']"

dotnet sonarscanner begin `
    /k:"$SonarQubeProjectKey" `
    /d:sonar.host.url="$SonarQubeHostUrl" `
    /d:sonar.login="$SonarQubeToken" `
    /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml" `
    /d:sonar.coverage.exclusions="**/*.cshtml,**/Migrations/**,**/bin/**,**/obj/**"

if ($LASTEXITCODE -ne 0) {
    Write-Host "##teamcity[buildStatus status='FAILURE' text='SonarQube begin step failed. Stopping pipeline.']"
    exit 1
}

# ---------------------------
# Step 4: Build Solution
# ---------------------------
Write-Host "##teamcity[progressMessage 'Building Solution in Release Mode']"
dotnet build $SolutionPath -c Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "##teamcity[buildStatus status='FAILURE' text='Build failed. Stopping pipeline.']"
    exit 1
}

# ---------------------------
# Step 5: Run Unit Tests with Coverage
# ---------------------------
Write-Host "##teamcity[progressMessage 'Running Unit Tests with Coverage']"

$TestProjectList = $TestProjects -split ","
foreach ($testProject in $TestProjectList) {
    $trimmedTestProject = $testProject.Trim()
    if ($trimmedTestProject) {
        Write-Host "##teamcity[progressMessage 'Running tests for $trimmedTestProject']"

        dotnet test $trimmedTestProject `
            -c Release `
            --no-build `
            --logger:"trx;LogFileName=TestResults.trx" `
            --collect:"XPlat Code Coverage" `
            --results-directory "./TestResults" `
            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

        if ($LASTEXITCODE -ne 0) {
            Write-Host "##teamcity[buildStatus status='FAILURE' text='Unit tests failed in $trimmedTestProject']"
            exit 1
        }
    }
}

# ---------------------------
# Step 6: Publish Projects
# ---------------------------
Write-Host "##teamcity[progressMessage 'Publishing Projects to Artifacts']"

$PublishProjectList = $PublishProjects -split ","
foreach ($publishProject in $PublishProjectList) {
    $trimmedPublishProject = $publishProject.Trim()
    if ($trimmedPublishProject) {
        $publishOutput = "publish/$([System.IO.Path]::GetFileNameWithoutExtension($trimmedPublishProject))"
        Write-Host "##teamcity[progressMessage 'Publishing $trimmedPublishProject to $publishOutput']"

        dotnet publish $trimmedPublishProject -c Release -o $publishOutput --no-build

        if ($LASTEXITCODE -ne 0) {
            Write-Host "##teamcity[buildStatus status='FAILURE' text='Publish failed for $trimmedPublishProject']"
            exit 1
        }

        Write-Host "##teamcity[publishArtifacts '$publishOutput => PublishedArtifacts.zip']"
    }
}

# ---------------------------
# Step 7: SonarQube End Analysis
# ---------------------------
Write-Host "##teamcity[progressMessage 'Completing SonarQube Analysis (End Step)']"
dotnet sonarscanner end /d:sonar.login="$SonarQubeToken"

if ($LASTEXITCODE -ne 0) {
    Write-Host "##teamcity[buildStatus status='FAILURE' text='SonarQube end step failed. Check logs.']"
    exit 1
}

# ---------------------------
# Step 8: Build Completed
# ---------------------------
Write-Host "##teamcity[buildStatus status='SUCCESS' text='Build, Test, SonarQube Analysis, and Publish completed successfully!']"
Write-Host "##teamcity[progressMessage 'CI Pipeline Finished Successfully']"
