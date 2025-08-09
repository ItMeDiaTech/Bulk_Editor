#!/usr/bin/env pwsh
# Bulk Editor - Build, Publish, and Run Script
# This script automates the complete build and deployment process

param(
    [switch]$SkipRun,
    [switch]$SkipClean,
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "=== Bulk Editor - Build and Deploy Script ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Green
Write-Host "Runtime: $Runtime" -ForegroundColor Green
Write-Host ""

try {
    # Step 1: Clean previous builds (unless skipped)
    if (-not $SkipClean) {
        Write-Host "Step 1: Cleaning previous builds..." -ForegroundColor Yellow
        if (Test-Path "bin") {
            Remove-Item -Recurse -Force "bin"
            Write-Host "  ✓ Removed bin directory" -ForegroundColor Green
        }
        if (Test-Path "obj") {
            Remove-Item -Recurse -Force "obj"
            Write-Host "  ✓ Removed obj directory" -ForegroundColor Green
        }
        if (Test-Path "Publish") {
            Remove-Item -Recurse -Force "Publish"
            Write-Host "  ✓ Removed Publish directory" -ForegroundColor Green
        }
        Write-Host ""
    }

    # Step 2: Restore dependencies
    Write-Host "Step 2: Restoring dependencies..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore dependencies"
    }
    Write-Host "  ✓ Dependencies restored successfully" -ForegroundColor Green
    Write-Host ""

    # Step 3: Build the project
    Write-Host "Step 3: Building project..." -ForegroundColor Yellow
    dotnet build --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    Write-Host "  ✓ Build completed successfully" -ForegroundColor Green
    Write-Host ""

    # Step 4: Run tests (if any exist)
    if (Test-Path "*Tests*.csproj") {
        Write-Host "Step 4: Running tests..." -ForegroundColor Yellow
        dotnet test --configuration $Configuration --no-build
        if ($LASTEXITCODE -ne 0) {
            throw "Tests failed"
        }
        Write-Host "  ✓ All tests passed" -ForegroundColor Green
        Write-Host ""
    }

    # Step 5: Publish the application
    Write-Host "Step 5: Publishing application..." -ForegroundColor Yellow
    $PublishArgs = @(
        "publish"
        "--configuration", $Configuration
        "--runtime", $Runtime
        "--self-contained", "true"
        "--output", "Publish"
        "/p:PublishSingleFile=true"
        "/p:PublishTrimmed=false"
        "--no-build"
    )

    dotnet @PublishArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed"
    }
    Write-Host "  ✓ Application published successfully" -ForegroundColor Green
    Write-Host ""

    # Step 6: Verify published files
    Write-Host "Step 6: Verifying published files..." -ForegroundColor Yellow
    $ExePath = Join-Path "Publish" "Bulk_Editor.exe"
    if (Test-Path $ExePath) {
        $FileSize = (Get-Item $ExePath).Length / 1MB
        Write-Host "  ✓ Executable found: $ExePath ($([math]::Round($FileSize, 2)) MB)" -ForegroundColor Green

        # Check file version
        $FileVersion = (Get-ItemProperty $ExePath).VersionInfo.FileVersion
        Write-Host "  ✓ File version: $FileVersion" -ForegroundColor Green
    } else {
        throw "Published executable not found at $ExePath"
    }
    Write-Host ""

    # Step 7: Run the application (unless skipped)
    if (-not $SkipRun) {
        Write-Host "Step 7: Launching application..." -ForegroundColor Yellow
        Write-Host "  → Starting Bulk_Editor.exe..." -ForegroundColor Cyan
        Write-Host "  → Press Ctrl+C to stop this script (application will continue running)" -ForegroundColor Gray

        # Start the application in a new process
        $Process = Start-Process -FilePath $ExePath -PassThru
        Write-Host "  ✓ Application launched (PID: $($Process.Id))" -ForegroundColor Green

        # Wait a moment to ensure it started successfully
        Start-Sleep -Seconds 2
        if (-not $Process.HasExited) {
            Write-Host "  ✓ Application is running successfully" -ForegroundColor Green
        } else {
            Write-Host "  ⚠ Application exited immediately (Exit Code: $($Process.ExitCode))" -ForegroundColor Yellow
        }
    }

    Write-Host ""
    Write-Host "=== Build and Deploy Completed Successfully ===" -ForegroundColor Green
    Write-Host "Published to: $(Join-Path $PWD 'Publish')" -ForegroundColor Cyan

    # Display summary
    Write-Host ""
    Write-Host "Summary:" -ForegroundColor Cyan
    Write-Host "  • Configuration: $Configuration" -ForegroundColor Gray
    Write-Host "  • Runtime: $Runtime" -ForegroundColor Gray
    Write-Host "  • Output: $(Join-Path $PWD 'Publish')" -ForegroundColor Gray
    if (-not $SkipRun) {
        Write-Host "  • Application: Running" -ForegroundColor Gray
    }

} catch {
    Write-Host ""
    Write-Host "=== Build Failed ===" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Instructions
Write-Host ""
Write-Host "Instructions:" -ForegroundColor Cyan
Write-Host "  • To run manually: .\Publish\Bulk_Editor.exe" -ForegroundColor Gray
Write-Host "  • To rebuild: .\build-and-run.ps1" -ForegroundColor Gray
Write-Host "  • To build without running: .\build-and-run.ps1 -SkipRun" -ForegroundColor Gray
Write-Host "  • To build Debug version: .\build-and-run.ps1 -Configuration Debug" -ForegroundColor Gray