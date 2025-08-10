# Bulk Editor - Build and Deploy Script
# This script builds the project and copies the .exe to standard locations

Write-Host "🚀 Building Bulk Editor..." -ForegroundColor Green

# Build the project
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green

# Publish the project
Write-Host "📦 Publishing..." -ForegroundColor Yellow
dotnet publish --configuration Release --output ./Publish --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Publish successful!" -ForegroundColor Green

# Copy .exe to standard locations
Write-Host "📋 Copying .exe to standard locations..." -ForegroundColor Yellow

# Copy to project root
Copy-Item ".\Publish\Bulk_Editor.exe" ".\Bulk_Editor.exe" -Force
Write-Host "✅ Copied to project root: .\Bulk_Editor.exe" -ForegroundColor Green

Write-Host ""
Write-Host "🎉 BUILD AND DEPLOY COMPLETE!" -ForegroundColor Cyan
Write-Host "📍 Your .exe files are available at:" -ForegroundColor White
Write-Host "   🖥️  Desktop: $desktopPath" -ForegroundColor Yellow
Write-Host "   📁 Project: .\Bulk_Editor.exe" -ForegroundColor Yellow
Write-Host "   📦 Publish: .\Publish\Bulk_Editor.exe" -ForegroundColor Yellow
Write-Host ""