@echo off
setlocal
cd /d "%~dp0"

call .engine-prepare.bat
if errorlevel 1 exit /b 1

if not exist ".logs\regression" mkdir ".logs\regression"

for /f %%i in ('python -c "import datetime; print(datetime.datetime.now().strftime('%%Y-%%m-%%d_%%H-%%M-%%S'))"') do set "TIMESTAMP=%%i"
set "LOG=.logs\regression\%TIMESTAMP%.log"

echo Regression log: %CD%\%LOG%

echo [%date% %time%] Start regression > "%LOG%"
echo Log: %LOG% >> "%LOG%"
echo. >> "%LOG%"

python tests\run_tests.py %* >> "%LOG%" 2>&1
set EXIT_CODE=%errorlevel%

if %EXIT_CODE% equ 0 (
    echo. >> "%LOG%"
    echo Regression passed >> "%LOG%"
) else (
    echo. >> "%LOG%"
    echo Regression failed exit=%EXIT_CODE% >> "%LOG%"
)

type "%LOG%"
exit /b %EXIT_CODE%
