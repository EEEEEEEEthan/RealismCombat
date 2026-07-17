@echo off
setlocal
cd /d "%~dp0"

if /I "%~1"=="--update" (
    python -m pip install --upgrade --force-reinstall "egent @ git+https://github.com/EEEEEEEEthan/egent.git"
    if errorlevel 1 exit /b 1
    python -m pip install -e ".[dev]" --upgrade
    if errorlevel 1 exit /b 1
) else (
    python -c "import egent, prompt_toolkit" >nul 2>nul
    if errorlevel 1 (
        python -m pip install -e .
        if errorlevel 1 exit /b 1
    )
)

if not exist ".egent\.model.toml" (
    echo 缺少 .egent\.model.toml，请先复制 .egent\.model.toml.example 并填写模型配置。
    exit /b 1
)

python ".egent\main.py"
exit /b %errorlevel%
