@echo off

cd SmartBlaze.Backend
start SmartBlaze.Backend.exe

timeout /t 2 >nul

cd ..
cd SmartBlaze.Frontend
start SmartBlaze.Frontend.exe

timeout /t 2 >nul

start "" https://localhost:7040