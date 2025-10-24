@echo off
setlocal

pushd "%~dp0.McpServer"

dotnet run -c Release -- %*
set EXITCODE=%ERRORLEVEL%

popd
exit /b %EXITCODE%

