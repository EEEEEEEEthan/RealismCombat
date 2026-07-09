@echo off
setlocal
cd /d "%~dp0"

call .engine-prepare.bat
if errorlevel 1 exit /b 1

if not exist ".logs\regression" mkdir ".logs\regression"

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'"') do set TIMESTAMP=%%i
set LOG=.logs\regression\%TIMESTAMP%.log

echo 回归测试日志: %CD%\%LOG%

echo [%date% %time%] 开始回归测试 > "%LOG%"
echo 日志: %LOG% >> "%LOG%"
echo. >> "%LOG%"

python tests\run_tests.py %* >> "%LOG%" 2>&1
set EXIT_CODE=%errorlevel%

if %EXIT_CODE% equ 0 (
    echo. >> "%LOG%"
    echo 回归测试通过 >> "%LOG%"
) else (
    echo. >> "%LOG%"
    echo 回归测试失败 exit=%EXIT_CODE% >> "%LOG%"
)

type "%LOG%"
exit /b %EXIT_CODE%
