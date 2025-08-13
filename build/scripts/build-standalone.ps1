#!/usr/bin/env pwsh

# Build Standalone Bulk Editor
# This script creates a self-contained single-file executable for Windows x64

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [Parameter(Mandatory=$false)]
    [switch]$Clean
)

Write-Host "Building Bulk Editor Standalone for Windows x64..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Clean previous builds if requested
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force }
    if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force }
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "Package restore failed!"
    exit 1
}

# Build the application
Write-Host "Building application..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

# Publish standalone executable
$PublishProfile = if ($Configuration -eq "Debug") { "StandaloneDebug" } else { "StandaloneWindows" }
$OutputDir = if ($Configuration -eq "Debug") { "bin\StandaloneDebug" } else { "bin\Standalone" }

Write-Host "Publishing standalone executable using profile: $PublishProfile..." -ForegroundColor Yellow
dotnet publish --configuration $Configuration --no-build -p:PublishProfile=$PublishProfile

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed!"
    exit 1
}

# Display results
Write-Host "`nBuild completed successfully!" -ForegroundColor Green
Write-Host "Standalone executable location: $OutputDir" -ForegroundColor Cyan

if (Test-Path "$OutputDir\Bulk_Editor.exe") {
    $FileInfo = Get-Item "$OutputDir\Bulk_Editor.exe"
    $FileSizeMB = [math]::Round($FileInfo.Length / 1MB, 2)
    Write-Host "Executable size: $FileSizeMB MB" -ForegroundColor Cyan
    Write-Host "Created: $($FileInfo.CreationTime)" -ForegroundColor Cyan
} else {
    Write-Warning "Executable not found at expected location!"
}

Write-Host "`nTo test the standalone executable:" -ForegroundColor Yellow
Write-Host "  cd $OutputDir" -ForegroundColor White
Write-Host "  .\Bulk_Editor.exe" -ForegroundColor White