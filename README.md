# Shadr

[![Release](https://img.shields.io/github/v/release/aherrick/Shadr)](https://github.com/aherrick/Shadr/releases)

A lightweight Windows application to control your screen brightness beyond your monitor's native range. Perfect for reducing eye strain during late-night sessions or boosting visibility in bright environments.

## Features

- **Quick Access**: System tray icon for instant brightness control
- **Flexible Brightness Levels**:
  - 25% - Heavy dimming (overlay)
  - 50% - Light dimming (gamma)
  - 75% - Slight dimming (gamma)
  - 100% - Normal brightness
  - 125% - Brightness boost (gamma)
  - 150% - Maximum brightness boost (gamma)
- **Gamma Ramp Technology**: Uses Windows gamma adjustment for natural-looking brightness changes (50-150%)
- **Overlay Dimming**: Black overlay for deep dimming below 50%
- **Auto-Updates**: Automatically checks for and installs new versions from GitHub
- **Click-Through**: The overlay doesn't interfere with your work
- **Always On Top**: Stays on top of all windows
- **Lightweight**: Minimal resource usage, single-file executable
- **Self-Contained**: No .NET runtime installation required

## Installation

1. Download `Shadr-vX.X.X-win-x64.zip` from the [latest release](https://github.com/aherrick/Shadr/releases/latest)
2. Extract to any folder
3. Run `Shadr.exe`
4. Shadr will auto-update when new versions are available

## Usage

1. After launching, Shadr appears as an icon in your system tray
2. Right-click the tray icon to access the menu:
   - Select brightness levels (25%, 50%, 75%, 100%, 125%, 150%)
   - **Check for Updates** - Manually check for new versions
   - **About** - Opens the GitHub page
   - **Exit** - Close the application

### How It Works

- **50% and above**: Uses gamma ramp adjustment - modifies the display's color curve for natural brightness changes
- **Below 50%**: Uses a black overlay for deeper dimming with true blacks

Gamma changes are automatically reset when the app exits.

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
- Gamma adjustment via Windows GDI32 `SetDeviceGammaRamp`
- Auto-updates powered by [Updatum](https://github.com/sn4k3/Updatum)
- GitHub Actions CI/CD for automated releases

## License

This project is open source. Feel free to use and modify as needed.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
