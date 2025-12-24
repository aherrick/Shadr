# Shadr

[![Release](https://img.shields.io/github/v/release/aherrick/Shadr)](https://github.com/aherrick/Shadr/releases)

A lightweight Windows application that adds a customizable overlay to control your screen brightness. Perfect for reducing eye strain during late-night sessions or boosting brightness when needed.

## Features

- **Quick Access**: System tray icon for instant brightness control
- **Flexible Brightness Levels**:
  - 25% - Light dimming
  - 50% - Medium dimming
  - 75% - Heavy dimming
  - 100% - Normal brightness (no overlay)
  - 125% - Brightness boost (white overlay)
- **Auto-Updates**: Automatically checks for and installs new versions from GitHub
- **Click-Through Overlay**: The overlay doesn't interfere with your work - click through it to interact with applications normally
- **Always On Top**: Overlay stays on top of all windows
- **Lightweight**: Minimal resource usage, single-file executable
- **Self-Contained**: No .NET runtime installation required

## Installation

1. Download `Shadr_win-x64.exe` from the [latest release](https://github.com/aherrick/Shadr/releases/latest)
2. Rename it to `Shadr.exe` and place it anywhere you like
3. Run `Shadr.exe`
4. (Optional) Drag to your Startup folder for auto-start with Windows

## Usage

1. After launching, Shadr appears as an icon in your system tray
2. Right-click the tray icon to access the menu:
   - Select brightness levels (25%, 50%, 75%, 100%, 125%)
   - **Check for Updates** - Manually check for new versions
   - **About** - Opens the GitHub page
   - **Exit** - Close the application

### Auto-Updates

Shadr automatically checks for updates on startup. When a new version is available:
- You'll see a notification with version details
- Click "Yes" to download and install
- The app will restart with the new version

### Run on Startup

To make Shadr start automatically with Windows:
1. Press `Win + R`
2. Type `shell:startup` and press Enter
3. Create a shortcut to `Shadr.exe` in that folder

## Requirements

- Windows 10 or later
- No additional runtime required (self-contained)

## Building from Source

```powershell
# Clone the repository
git clone https://github.com/aherrick/Shadr.git
cd Shadr

# Publish single-file executable
dotnet publish -c Release

# Output will be in: bin\Release\net10.0-windows\win-x64\publish\
```

## Technical Details

- Built with .NET 10 and Windows Forms
- Single-file deployment with native libraries embedded
- Auto-updates powered by [Updatum](https://github.com/sn4k3/Updatum)
- GitHub Actions CI/CD for automated releases

## License

This project is open source. Feel free to use and modify as needed.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
