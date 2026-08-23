# PocketLink Monitor

[![Build and Test](https://github.com/henryZhouLikeStudy/pocketlink-monitor/actions/workflows/build.yml/badge.svg)](https://github.com/henryZhouLikeStudy/pocketlink-monitor/actions/workflows/build.yml)

**Windows .NET 8 WPF tray monitor for portable Wi-Fi devices**

A read-only monitoring prototype built with mock-first design and safe boundaries. The current version does not access physical devices, perform configuration writes, firmware modifications, or reboots.

## Features

- **System tray integration** with real-time status display
- **Dashboard UI** showing throughput trends, signal metrics, network/battery status
- **Mock device adapter** for offline testing and development
- **WPF with modern libraries**: H.NotifyIcon.Wpf, LiveChartsCore, CommunityToolkit.Mvvm

## Requirements

- Windows 10/11
- .NET 8.0 SDK (for building from source)
- **Or download the pre-built executable** (no .NET runtime installation required)

## Getting Started

### Download Pre-built Executable (Recommended)

1. Go to the [Actions tab](https://github.com/henryZhouLikeStudy/pocketlink-monitor/actions/workflows/build.yml) in this repository
2. Click on the latest successful workflow run
3. Download the **PocketLink-Monitor-win-x64** artifact
4. Extract the ZIP file
5. Run `PocketLink.App.exe` directly—no .NET runtime installation required

The self-contained executable includes all necessary .NET runtime components.

### Build from Source

#### Build

```powershell
dotnet restore PocketLink.sln
dotnet build PocketLink.sln --configuration Release
```

#### Run

```powershell
dotnet run --project src/PocketLink.App/PocketLink.App.csproj
```

#### Test

```powershell
dotnet test PocketLink.sln
```

## Project Structure

```
src/
├── PocketLink.App/              # Main WPF application with tray icon
├── PocketLink.Contracts/        # Interfaces and data contracts
└── PocketLink.Infrastructure/   # Mock and disabled device adapters

tests/
└── PocketLink.Tests/            # Infrastructure tests
```

## Architecture

**Trust Boundary**: The `MockDeviceAdapter` (in `PocketLink.Infrastructure`) generates deterministic offline samples only. The `DisabledZteDeviceAdapter` explicitly disables live device transport—all methods throw `NotSupportedException`. Missing fields remain `null` and display as “Unknown” in the UI, never interpreted as zero.

**Current Scope**: Read-only monitoring. No device writes, no configuration changes, no firmware operations.

## CI/CD

GitHub Actions workflow builds the solution on Windows runners, restores NuGet dependencies, and runs all test projects.

## License

MIT License - see [LICENSE](LICENSE) for details

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Disclaimer

This is a development prototype. The mock adapter does not interact with real devices. Real device support would require explicit authentication, safety validation, and user consent before any device operation.
