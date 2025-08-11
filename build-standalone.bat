@echo off
REM Build Standalone Bulk Editor for Windows x64
REM Simple batch file alternative to PowerShell script

echo Building Bulk Editor Standalone for Windows x64...
echo.

REM Restore packages
echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo Package restore failed!
    pause
    exit /b 1
)

REM Build the application
echo Building application...
dotnet build --configuration Release --no-restore
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)

REM Publish standalone executable
echo Publishing standalone executable...
dotnet publish --configuration Release --no-build -p:PublishProfile=StandaloneWindows
if errorlevel 1 (
    echo Publish failed!
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo Standalone executable location: bin\Standalone\
echo.
echo To test the standalone executable:
echo   cd bin\Standalone
echo   Bulk_Editor.exe
echo.
pause