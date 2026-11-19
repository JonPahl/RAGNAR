@echo off
set PROJECT_NAME=Ragnar
set SONAR_HOST=http://localhost:9000
set SONAR_TOKEN=sqp_6e956a6f1b12b79a369a2df793fd8c409a12cb0b

echo [1/4] Starting SonarScanner...
dotnet sonarscanner begin /k:"%PROJECT_NAME%" /d:sonar.host.url="%SONAR_HOST%"  /d:sonar.token="%SONAR_TOKEN%"  /d:sonar.cs.vscoveragexml.reportsPaths="coverage.xml"

if %errorlevel% neq 0 exit /b %errorlevel%

echo [2/4] Building and running tests...
dotnet build --no-incremental

echo [3/4] Building and running tests...
dotnet-coverage collect "dotnet test" -f xml -o "coverage.xml"


echo [4/4] Finishing analysis and uploading results...
dotnet sonarscanner end /d:sonar.token="%SONAR_TOKEN%"
pause
