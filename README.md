# MdViewer

A lightweight desktop application for viewing Markdown files with a clean, distraction-free interface.

## Overview

**MdViewer** is a Windows desktop application built with WPF and .NET 8.0 that provides a dedicated viewer for Markdown (.md) files. It automatically associates itself with `.md` file extensions on first launch, allowing seamless viewing by double-clicking Markdown files in Windows Explorer.

## Features

### Core Functionality
- **Markdown Rendering**: Full support for standard Markdown syntax with beautiful formatting
- **File Association**: Automatic registration of `.md` file extension on first run
- **Multi-Window Support**: Open multiple Markdown files simultaneously
- **Smart Window Management**: Opening the same file twice focuses the existing window instead of creating duplicates
- **Internal Link Navigation**: Click links to other `.md` files to open them in new windows
- **External Link Support**: Web links (http/https) open in your default browser

### UI & Typography
- **Customized Styling**:
  - Body text: 18px with Malgun Gothic/Segoe UI fonts
  - Headings: H1-H6 with progressive sizing (28px - 18px)
  - Code blocks: Fixed-width fonts (D2Coding, Consolas, etc.) at 13px
- **Syntax Highlighting**: Code blocks displayed with distinct formatting
- **Responsive Layout**: Window size and position are remembered across sessions

### Shortcuts
| Shortcut | Action |
|----------|--------|
| **Esc** | Close window |
| **F5** | Refresh current file |
| **Alt+F4** | Close window (standard) |

## Tech Stack

- **Framework**: WPF (.NET 8.0 Windows)
- **Markdown Engine**: [MdXaml](https://github.com/whistyun/MdXaml) 1.27.0
- **Language**: C#
- **Build System**: .NET CLI / Visual Studio
- **Configuration**: JSON (stored in `%AppData%/MdViewer/settings.json`)

## Installation

### Prerequisites
- Windows 7 or later
- .NET 8.0 Runtime (if running pre-built executable)
- Visual Studio 2022 or .NET 8.0 SDK (if building from source)

### From Release
Download the latest release executable and run it. File association will be registered on first launch.

### Build from Source
```bash
# Clone the repository
git clone https://github.com/seeper0/MdViewer.git
cd MdViewer

# Build the project
dotnet build -c Release

# Run the application
dotnet run

# Or publish a standalone executable
dotnet publish -c Release -o ./publish
```

## Usage

### Opening Files
- **From Explorer**: Double-click any `.md` file (after first launch)
- **From Command Line**: `MdViewer.exe "path/to/file.md"`
- **From Within App**: Click links to other `.md` files

### Window State
The application automatically saves and restores:
- Window width and height
- Window position on screen
- These settings persist between sessions in `%AppData%/MdViewer/settings.json`

### File Management
- Each `.md` file opens in a single window (duplicates prevented)
- Opening the same file twice focuses the existing window
- Different files each open in new windows
- Inter-process communication uses Named Pipes (MdViewer_Pipe)

## Project Structure

```
MdViewer/
├── App.xaml              # Application root (resources, styles)
├── App.xaml.cs           # App startup, single-instance management, pipe server
├── MainWindow.xaml       # Main viewer window UI
├── MainWindow.xaml.cs    # Window logic, file loading, rendering, keyboard shortcuts
├── AssemblyInfo.cs       # Assembly metadata
├── MdViewer.csproj       # Project configuration
├── MdViewer.sln          # Solution file
│
├── Models/
│   └── AppSettings.cs    # Window state model (Width, Height, Left, Top)
│
├── Services/
│   ├── FileAssociationService.cs  # Registry-based .md file association
│   ├── PipeService.cs             # Named Pipe client/server communication
│   ├── SettingsService.cs         # Settings persistence (JSON)
│   └── WindowManager.cs           # Multi-window tracking and coordination
│
└── docs/
    ├── MdViewer.md       # Complete specification document
    ├── TaskPlan.md       # Implementation task checklist
    ├── HeadingTest.md    # Test markdown files
    ├── LinkTest.md
    ├── TableTest.md
    └── DiagramTest.md
```

## Architecture

### Key Components

**App.xaml.cs**
- Entry point for the application
- Handles command-line arguments
- Manages single-instance behavior via Named Pipes
- Registers file association on first run
- Creates and shows the first window

**MainWindow.xaml.cs**
- Renders Markdown content using MdXaml's MarkdownScrollViewer
- Loads `.md` files and applies custom styling
- Handles keyboard shortcuts (Esc, F5)
- Processes link clicks (internal `.md` and external web links)
- Saves/restores window state through SettingsService

**WindowManager.cs**
- Maintains a registry of open files and their windows
- Prevents duplicate windows for the same file
- Routes new file requests to existing windows or creates new ones

**PipeService.cs**
- Named Pipe server: Listens for file open requests from new instances
- Named Pipe client: Communicates with existing instance
- Allows the app to act as a single-instance application with inter-process coordination

**FileAssociationService.cs**
- Registers `.md` extension with Windows registry on first run
- Uses HKEY_CURRENT_USER (no admin rights required)
- Registry path: `Software\Classes\.md`

**SettingsService.cs**
- Saves window state to JSON file
- Location: `%AppData%/MdViewer/settings.json`
- Loads and applies settings on application startup

## Configuration

### Default Settings
```json
{
  "WindowWidth": 900.0,
  "WindowHeight": 600.0,
  "WindowLeft": 100.0,
  "WindowTop": 100.0
}
```

Settings are automatically saved when closing any window and restored on next launch.

## Registry Integration

On first run, the following registry entries are created:
```
HKEY_CURRENT_USER\Software\Classes\.md
  (Default) = "MdViewer.md"

HKEY_CURRENT_USER\Software\Classes\MdViewer.md
  (Default) = "Markdown File"

HKEY_CURRENT_USER\Software\Classes\MdViewer.md\shell\open\command
  (Default) = "C:\path\to\MdViewer.exe" "%1"
```

No administrator privileges are required as changes are made to HKEY_CURRENT_USER.

## Error Handling

- **File Not Found**: Shows error message and closes the window
- **File Read Error**: Displays error message with details
- **Registry Error**: Displays warning but continues running normally

## Performance

- **Lightweight**: Minimal memory footprint
- **Fast Startup**: Instant window opening
- **Smooth Rendering**: MdXaml provides efficient Markdown rendering
- **Smart Font Selection**: Automatically uses available system fonts (fallback chain)

## Future Enhancement Ideas

- Dark mode / theme switching
- Font size adjustment (Ctrl+Mouse wheel)
- File explorer sidebar for navigation
- Print support
- Bookmark/history management
- Search/find functionality
- Right-to-left text support

## Building & Development

### Requirements
- Visual Studio 2022 (or VS Code with C# extension)
- .NET 8.0 SDK
- Windows development tools

### Build Commands
```bash
# Debug build
dotnet build

# Release build with optimizations
dotnet build -c Release

# Run tests (when added)
dotnet test

# Publish standalone executable
dotnet publish -c Release --self-contained -r win-x64 -o ./publish
```

### NuGet Dependencies
```xml
<PackageReference Include="MdXaml" Version="1.27.0" />
<PackageReference Include="MdXaml.Plugins" Version="1.27.0" />
<PackageReference Include="System.Text.Json" Version="8.0.5" />
```

## Contributing

Feel free to submit issues and pull requests. For major changes, please open an issue first to discuss proposed changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025 seeper0

## Acknowledgments

- [MdXaml](https://github.com/whistyun/MdXaml) - Excellent WPF Markdown rendering library
- Windows Presentation Foundation (WPF) team for the powerful UI framework
- .NET community for making .NET 8.0 available

---

**Made with ❤️ for Markdown lovers**
