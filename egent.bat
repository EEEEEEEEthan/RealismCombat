@echo off
setlocal
cd /d "%~dp0"

if /I "%~1"=="--update" goto update
if /I "%~1"=="-u" goto update
goto run

:update
git submodule update --init addons/godot_runtime_mcp
if errorlevel 1 exit /b 1

python -m pip install --force-reinstall "egent @ git+https://github.com/EEEEEEEEthan/egent.git"
if errorlevel 1 exit /b 1

python -m pip install -e ".[dev]" --upgrade
if errorlevel 1 exit /b 1

:run
python .egent\main.py
exit /b %errorlevel%
