@echo off

SET ASPNETCORE_ENVIRONMENT=Development

cd SmartBlaze.Backend
start SmartBlaze.Backend.exe

timeout /t 5 >nul

cd ..
cd SmartBlaze.Frontend
start SmartBlaze.Frontend.exe