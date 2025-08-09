# Bulk Editor Launcher Script
# This script launches the Bulk Editor application

# Navigate to the application directory
Set-Location -Path "$PSScriptRoot\bin\Release\net8.0-windows"

# Launch the application
Start-Process -FilePath "Bulk_Editor.exe"