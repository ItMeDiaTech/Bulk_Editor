# Bulk Editor - Document Hyperlink Processor

## Overview

The Bulk Editor is a modern Windows Forms application designed to process Word documents (.docx files) and fix various hyperlink issues. It provides a user-friendly interface for selecting files or folders, applying various fixes, and generating detailed changelogs.

## 🚀 Features

- **Fix Source Hyperlinks**: Update hyperlinks with current URLs and Content IDs
- **Append Content ID**: Add Content IDs to hyperlink display text
- **Fix Internal Hyperlinks**: Validate and repair internal document links
- **Fix Titles**: Clean up hyperlink titles and remove status markers
- **Fix Double Spaces**: Remove multiple consecutive spaces
- **Replace Hyperlinks**: Bulk replace hyperlinks based on configured rules
- **Export Detailed Changelogs**: Generate comprehensive reports of changes
- **Version Checking**: Automatic update notifications

## 🏗️ Architecture

The application follows modern software architecture principles:

### Project Structure

```
Bulk_Editor/
├── Configuration/          # Application settings and configuration
├── Models/                 # Data models and DTOs
├── Services/              # Business logic and processing services
├── MainForm.cs            # Main UI form (UI concerns only)
├── Program.cs             # Application entry point
└── README.md             # This file
```

### Key Components

#### Models

- [`HyperlinkData.cs`](Models/HyperlinkData.cs) - Hyperlink data structure
- [`ProcessingResult.cs`](Models/ProcessingResult.cs) - Result handling with success/failure states
- [`HyperlinkReplacementRule.cs`](HyperlinkReplacementRule.cs) - Replacement rule definitions

#### Services

- [`WordDocumentProcessor.cs`](Services/WordDocumentProcessor.cs) - Document processing and API communication
- [`ProcessingService.cs`](Services/ProcessingService.cs) - Core business logic operations
- [`ValidationService.cs`](Services/ValidationService.cs) - Input validation and security

#### Configuration

- [`AppSettings.cs`](Configuration/AppSettings.cs) - Centralized configuration management

## 🎨 UI/UX Improvements

### Modern Design

- **Responsive Layout**: TableLayoutPanel-based design that adapts to window size
- **Modern Color Scheme**: Professional color palette with proper accessibility
- **Visual Hierarchy**: Clear typography and spacing for better usability
- **Intuitive Icons**: Emoji-based icons for visual clarity
- **DPI Scaling**: Proper high-DPI support for modern displays

### User Experience

- **Progress Feedback**: Real-time progress bars and status updates
- **Error Handling**: User-friendly error messages and validation
- **File Management**: Drag-and-drop support and easy file selection
- **Changelog Viewing**: Integrated changelog viewer with file-specific details

## 🔒 Security & Validation

### Input Validation

- File path validation and sanitization
- URL format validation (HTTPS enforcement)
- Content ID format validation
- File size and extension restrictions
- Path traversal attack prevention

### Security Measures

- HTTPS-only API communication
- Input sanitization for all user data
- File system access controls
- Backup integrity validation

## ⚙️ Configuration

The application uses a centralized configuration system with the following settings:

### API Settings

```json
{
  "Api": {
    "BaseUrl": "https://prod-00.eastus.logic.azure.com:443/workflows/",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "UserAgent": "Bulk-Editor/2.1",
    "ValidateSsl": true
  }
}
```

### Processing Settings

```json
{
  "Processing": {
    "MaxFileSizeBytes": 52428800,
    "MaxConcurrentFiles": 5,
    "CreateBackups": true,
    "BackupFolderName": "Backup",
    "ValidateDocuments": true,
    "AllowedExtensions": [".docx"]
  }
}
```

## 🚧 Implementation Status

### ✅ Completed

- **Architecture Refactoring**: Separated concerns into proper layers
- **Modern UI Design**: Responsive, accessible interface
- **Configuration Management**: Centralized settings system
- **Validation Framework**: Comprehensive input validation
- **Error Handling**: Structured result handling
- **Security Measures**: Input sanitization and path validation

### 🔧 In Progress / TODO

- **Document Processing**: Replace placeholder with actual .docx parsing using DocumentFormat.OpenXml
- **API Integration**: Implement real Power Automate Flow communication
- **Logging Framework**: Add structured logging (Serilog recommended)
- **Unit Testing**: Comprehensive test coverage
- **Performance Optimization**: Memory management and async improvements

## 🛠️ Building the Application

### Prerequisites

- .NET 8.0 or later
- Windows 10/11
- Visual Studio 2022 or VS Code

### Build Commands

```bash
# Build the application
dotnet build

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true

# Run the application
dotnet run
```

### Dependencies

The application currently uses minimal dependencies:

- .NET 8.0 Windows Forms
- System.Text.Json for configuration
- System.Text.RegularExpressions for pattern matching

### Recommended Additions for Production

```xml
<PackageReference Include="DocumentFormat.OpenXml" Version="3.0.0" />
<PackageReference Include="Serilog" Version="3.1.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
```

## 📊 Code Quality Metrics

### Architecture Quality

- **Separation of Concerns**: ✅ Properly implemented
- **Single Responsibility**: ✅ Each class has a clear purpose
- **Dependency Management**: ⚠️ Could benefit from DI container
- **Error Handling**: ✅ Structured with ProcessingResult pattern

### Security Assessment

- **Input Validation**: ✅ Comprehensive validation implemented
- **Path Security**: ✅ Path traversal protection
- **Network Security**: ✅ HTTPS enforcement
- **File System Security**: ✅ Access controls and sanitization

### Performance Considerations

- **Memory Management**: ⚠️ Could benefit from streaming for large files
- **Async Operations**: ✅ Proper async/await patterns
- **UI Responsiveness**: ✅ Non-blocking operations
- **Resource Disposal**: ✅ Using statements implemented

## 🐛 Known Issues & Limitations

### Current Limitations

1. **Document Processing**: Uses placeholder text parsing instead of proper .docx handling
2. **API Integration**: Placeholder implementation for Power Automate Flow
3. **Error Recovery**: Limited retry mechanisms for failed operations
4. **Large File Handling**: May have memory issues with very large documents

### Future Enhancements

1. **Real Document Processing**: Implement DocumentFormat.OpenXml integration
2. **Batch Processing**: Improved parallel processing capabilities
3. **Plugin Architecture**: Support for custom processing rules
4. **Multi-language Support**: Internationalization framework

## 📝 Change Log

### Version 2.1 (Current)

- **Major Architecture Refactor**: Separated into proper layers
- **Modern UI Design**: Complete interface redesign
- **Enhanced Validation**: Comprehensive input validation
- **Configuration System**: Centralized settings management
- **Security Improvements**: Input sanitization and path validation

### Previous Versions

- **Version 2.0**: Original monolithic implementation
- **Version 1.0**: Initial release

## 🤝 Contributing

When contributing to this project:

1. **Follow Architecture**: Maintain separation of concerns
2. **Add Tests**: Include unit tests for new functionality
3. **Document Changes**: Update this README and inline documentation
4. **Validate Input**: Use ValidationService for all inputs
5. **Handle Errors**: Use ProcessingResult pattern for operations

## 📞 Support

For issues or questions:

1. Check the [`ANALYSIS.md`](ANALYSIS.md) file for detailed technical analysis
2. Review the validation error messages for common issues
3. Check the generated changelogs for processing details
4. Create an issue in the GitHub repository

## 📄 License

This project is part of an internal document processing workflow. Please ensure compliance with organizational policies when using or modifying this code.

---

**Note**: This application is production-ready in terms of architecture and security, but requires completion of the core document processing functionality for full operational use.
