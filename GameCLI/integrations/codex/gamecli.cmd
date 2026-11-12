@echo off
node "%~dp0cli\gamecli.mjs" %*
exit /b %errorlevel%
