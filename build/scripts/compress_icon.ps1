Add-Type -AssemblyName System.Drawing

# Load the original image
$originalImage = [System.Drawing.Image]::FromFile('White_Settings_Icon_25x25.png')
Write-Host "Original dimensions: $($originalImage.Width) x $($originalImage.Height) pixels"
Write-Host "Original file size: $((Get-Item 'White_Settings_Icon_25x25.png').Length / 1KB) KB"

# Create a new 25x25 bitmap
$newImage = New-Object System.Drawing.Bitmap(25, 25)
$graphics = [System.Drawing.Graphics]::FromImage($newImage)

# Set high quality resize settings
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

# Draw the original image resized to 25x25
$graphics.DrawImage($originalImage, 0, 0, 25, 25)

# Clean up graphics object
$graphics.Dispose()
$originalImage.Dispose()

# Backup the original file
Copy-Item 'White_Settings_Icon_25x25.png' 'White_Settings_Icon_25x25_backup.png' -Force

# Save the new compressed image
$newImage.Save('White_Settings_Icon_25x25.png', [System.Drawing.Imaging.ImageFormat]::Png)
$newImage.Dispose()

Write-Host "New file size: $((Get-Item 'White_Settings_Icon_25x25.png').Length) bytes"
Write-Host "Image compressed and saved successfully!"