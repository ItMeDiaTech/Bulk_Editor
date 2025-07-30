# Bulk Editor Application

## Overview

The Bulk Editor application is a Windows Forms application designed to process Word documents (.docx files) and fix various hyperlink issues. It provides a user-friendly interface for selecting files or folders, applying various fixes, and generating detailed changelogs.

## Features

- Fix source hyperlinks
- Append Content ID to hyperlinks
- Fix internal hyperlinks
- Fix titles
- Fix double spaces
- Export detailed changelogs
- Version checking and update notifications

## Building the Application

To build the application, run:

```batch
dotnet build
```

To publish the application:

```batch
dotnet publish -c Release -r win-x64 --self-contained true
```

## Support

For issues or questions, please create an issue in the GitHub repository.
