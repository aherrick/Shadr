# Shadr

[![Release](https://img.shields.io/github/v/release/aherrick/Shadr)](https://github.com/aherrick/Shadr/releases)

A lightweight Windows application that adds a customizable overlay to dim your screen. Perfect for reducing eye strain during late-night work sessions or in low-light environments.

## Features

- **Quick Access**: System tray icon for easy access
- **Multiple Dimming Levels**: Choose from preset brightness levels:
  - 25% dimming
  - 50% dimming
  - 75% dimming
  - 100% (no dimming)
- **Click-Through Overlay**: The dimming layer doesn't interfere with your work - click through it to interact with applications normally
- **Always On Top**: Overlay stays on top of all windows
- **Lightweight**: Minimal resource usage

## Installation

1. Download the latest release from the [Releases](https://github.com/aherrick/Shadr/releases) page
2. Extract the files to your desired location
3. Run `Shadr.exe`

## Usage

1. After launching, Shadr will appear as an icon in your system tray
2. Right-click the tray icon to access the brightness menu
3. Select your preferred dimming level:
   - **25%** - Light dimming
   - **50%** - Medium dimming
   - **75%** - Heavy dimming
   - **100%** - Full brightness (no overlay)
4. To exit the application, right-click the tray icon and select "Exit"

## Requirements

- Windows 10 or later
- .NET 10.0 Runtime

## Building from Source

```powershell
# Clone the repository
git clone https://github.com/aherrick/Shadr.git

# Navigate to the project directory
cd Shadr

# Build the project
dotnet build

# Run the application
dotnet run
```

## License

This project is open source. Feel free to use and modify as needed.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
