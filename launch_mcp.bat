@echo off
setlocal

REM ==============================================
REM 启动脚本: 编译并运行 .McpServer 项目 (Release)
REM 用法: 双击或在命令行运行本脚本，参数将透传给程序
REM 依赖: 需要已安装 .NET 8 SDK
REM ==============================================

chcp 65001 >nul

where dotnet >nul 2>nul
if errorlevel 1 (
  echo 未检测到 .NET SDK，请先安装 .NET 8 SDK: https://dotnet.microsoft.com/download
  exit /b 1
)

if not exist "%~dp0.McpServer" (
  echo 未找到目录: %~dp0.McpServer
  exit /b 1
)

pushd "%~dp0.McpServer"

echo [MCP] 还原依赖...
dotnet restore
if errorlevel 1 goto :fail

echo [MCP] 编译 Release...
dotnet build -c Release --no-restore
if errorlevel 1 goto :fail

echo [MCP] 运行...
dotnet run -c Release --no-build -- %*
set EXITCODE=%ERRORLEVEL%

popd
exit /b %EXITCODE%

:fail
echo 构建失败，已中止。
popd
exit /b 1

