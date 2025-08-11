# Bulk Editor - Standalone Deployment Guide

## Overview

This guide explains how to create a completely standalone version of the Bulk Editor that doesn't require any in-folder dependencies.

## Architecture Changes Made

### 1. Self-Contained Deployment

- Configured project to bundle .NET 8.0 runtime
- Single-file deployment with all dependencies included
- Windows x64 target platform
- No external runtime requirements

### 2. Embedded Configuration

- Default `appsettings.json` embedded as resource
- Fallback configuration loading hierarchy:
  1. External `appsettings.json` (if exists)
  2. External `Publish/appsettings.json` (if exists)
  3. Embedded default configuration
  4. Environment variables (highest priority)

### 3. Configuration Loading Strategy

- **External files take priority** for customization
- **Embedded defaults ensure the app always runs**
- Automatic creation of external config file from embedded defaults
- Maintains backward compatibility

## Build Methods

### Method 1: PowerShell Script (Recommended)

```powershell
# Release build (optimized)
.\build-standalone.ps1 -Configuration Release

# Debug build (with symbols)
.\build-standalone.ps1 -Configuration Debug

# Clean build
.\build-standalone.ps1 -Configuration Release -Clean
```

### Method 2: Batch File (Simple)

```cmd
# Run the batch file
build-standalone.bat
```

### Method 3: Manual dotnet CLI

```cmd
# Clean and restore
dotnet clean
dotnet restore --runtime win-x64

# Publish standalone
dotnet publish -c Release --self-contained true --runtime win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=false -p:IncludeNativeLibrariesForSelfExtract=true -o bin\Standalone
```

### Method 4: Using Publish Profiles

```cmd
# Using the predefined publish profile
dotnet publish -p:PublishProfile=StandaloneWindows

# Debug version
dotnet publish -p:PublishProfile=StandaloneDebug
```

## Output Locations

| Build Type         | Output Directory       | Description                      |
| ------------------ | ---------------------- | -------------------------------- |
| Standalone Release | `bin\Standalone\`      | Optimized single-file executable |
| Standalone Debug   | `bin\StandaloneDebug\` | Debug version with symbols       |

## File Structure After Build

```
bin\Standalone\
└── Bulk_Editor.exe    # Single standalone executable (~150-200MB)
```

## Key Features

### ✅ Completely Standalone

- No .NET runtime installation required
- No external DLL dependencies
- Single executable file
- No configuration files required (embedded defaults)

### ✅ Runtime Flexibility

- External configuration files can override defaults
- Environment variables supported
- Settings can be exported/imported
- Maintains all existing functionality

### ✅ Deployment Benefits

- Zero-dependency distribution
- Simplified deployment process
- Reduced support overhead
- Version consistency

## Configuration Behavior

### Default Operation (No External Files)

1. Application starts with embedded configuration
2. Creates default `appsettings.json` in application directory
3. All features work with reasonable defaults

### With External Configuration

1. Loads external `appsettings.json` if present
2. Falls back to embedded defaults for missing sections
3. Environment variables override both file sources

## Troubleshooting

### Build Issues

- **Trimming errors**: Disabled for Windows Forms compatibility
- **Runtime not found**: Ensure RuntimeIdentifiers property is set
- **Resource not found**: Verify embedded resource configuration

### Runtime Issues

- **Configuration not loading**: Check file permissions and paths
- **Missing features**: Verify all NuGet packages are included
- **Performance**: Consider ReadyToRun compilation for startup speed

## Technical Details

### Project Configuration

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishTrimmed>false</PublishTrimmed>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>
```

### Embedded Resources

```xml
<ItemGroup>
  <EmbeddedResource Include="appsettings.json">
    <LogicalName>appsettings.default.json</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

## Size Optimization

The standalone executable will be approximately 150-200MB due to:

- Embedded .NET 8.0 runtime
- Windows Forms framework
- All NuGet dependencies
- Application code and resources

This is normal for self-contained .NET applications and ensures complete portability.

## Distribution

The standalone executable can be distributed as a single file with no additional setup or installation requirements. Users simply run `Bulk_Editor.exe` on any Windows x64 system.
