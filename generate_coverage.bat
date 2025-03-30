@echo off
setlocal

set TEST_PROJECT=BankingApp.Tests
set RESULTS_DIR=%TEST_PROJECT%\TestResults
set COVERAGE_FILE_NAME=coverage.cobertura.xml
set REPORT_DIR=CoverageReport
set MIN_COVERAGE=70

echo 🧪 Running tests with coverage...
dotnet test %TEST_PROJECT% --collect:"XPlat Code Coverage" --results-directory %RESULTS_DIR%

:: Find the latest coverage file dynamically
for /f "delims=" %%f in ('dir /s /b %RESULTS_DIR%\%COVERAGE_FILE_NAME%') do set "COVERAGE_PATH=%%f"

if not exist "%COVERAGE_PATH%" (
    echo ❌ Could not find coverage file.
    exit /b 1
)

echo 📊 Generating coverage report...
reportgenerator -reports:"%COVERAGE_PATH%" -targetdir:"%REPORT_DIR%" -reporttypes:HtmlSummary;TextSummary

:: Read coverage % from summary file
for /f "tokens=3 delims= " %%a in ('findstr /i "Line coverage" %REPORT_DIR%\Summary.txt') do set COVERAGE=%%a
set COVERAGE=%COVERAGE:~0,-1%

echo.
echo 📈 Line coverage: %COVERAGE%%

:: Validate minimum coverage threshold
if %COVERAGE% LSS %MIN_COVERAGE% (
    echo ❌ Coverage is below %MIN_COVERAGE%%. Please improve your tests!
    exit /b 1
)

echo ✅ Coverage is sufficient (%COVERAGE%%). Opening report...
start "" "%REPORT_DIR%\\index.html"

endlocal
