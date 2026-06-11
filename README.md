# CCSWE.AppManager.DeviceOwner

[![CI](https://img.shields.io/github/actions/workflow/status/CoryCharlton/CCSWE.AppManager.DeviceOwner/ci.yml?branch=master&label=build)](https://github.com/CoryCharlton/CCSWE.AppManager.DeviceOwner/actions/workflows/ci.yml)
[![CodeQL](https://img.shields.io/github/actions/workflow/status/CoryCharlton/CCSWE.AppManager.DeviceOwner/codeql.yml?branch=master&label=codeql)](https://github.com/CoryCharlton/CCSWE.AppManager.DeviceOwner/actions/workflows/codeql.yml)
[![Release](https://img.shields.io/github/v/release/CoryCharlton/CCSWE.AppManager.DeviceOwner?display_name=tag&sort=semver)](https://github.com/CoryCharlton/CCSWE.AppManager.DeviceOwner/releases/latest)
[![License: GPL v3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)

A small cross-platform helper (desktop app + matching console) that makes the **App Manager** Android app the
**device owner** on a connected device — it picks a device, runs `adb shell dpm set-device-owner`, and reports
whether it took.

## Why this exists

App Manager needs to be the device owner to do its job, and `dpm set-device-owner` only works over `adb` from a
computer. This is the tool that runs that one command. It replaces a much older Windows-only WinForms utility
that bundled its own `adb.exe`; this re-write is .NET 10 / Avalonia, cross-platform, and finds `adb` the normal
way instead of shipping a copy.

## What it does

1. Lists the online devices the local `adb` server sees (`adb devices -l`), refreshing as you plug them in. If
   `adb` isn't installed, it offers to download Google's SDK Platform Tools for you.
2. You pick one.
3. Before running, it checks the device is ready — no blocking accounts, a single user, App Manager installed, no
   existing device owner — and explains anything you need to fix first (with a **Try anyway** override). If App
   Manager is already the device owner, it just tells you so.
4. It runs `adb -s <serial> shell dpm set-device-owner com.ccswe.appmanager.deviceowner/com.ccswe.appmanager.receivers.DeviceAdminReceiver`.
5. It reports success or failure with actionable guidance — including the cases where `dpm` exits cleanly but
   refused (e.g. *"there are already some accounts on the device"*).

That's the whole app. The device-owner component is fixed to App Manager's; this isn't a general `dpm` runner.

## Requirements

- **[.NET 10 runtime](https://dotnet.microsoft.com/download)** to run it (the **SDK** to build from source).
- **`adb`** — found via the Settings `adb`-path override, `ANDROID_HOME`/`ANDROID_SDK_ROOT`, the platform-default
  SDK location (e.g. `%LOCALAPPDATA%\Android\Sdk`), or `PATH`. If none of those have it, the helper offers to
  download Google's SDK Platform Tools and uses that copy (no manual SDK setup needed).
- A device with **USB debugging** enabled and **authorized**, in a state that allows setting a device owner
  (typically freshly set up, no added accounts). The helper checks this for you and explains what to fix.

## Install & run

Download the latest build for your platform — each archive contains both the desktop app and the console.
The builds are framework-dependent, so you'll need the [.NET 10 runtime](https://dotnet.microsoft.com/download):

- **Windows (x64):** [`CCSWE.AppManager.DeviceOwner-win-x64.zip`](https://github.com/CoryCharlton/CCSWE.AppManager.DeviceOwner/releases/latest/download/CCSWE.AppManager.DeviceOwner-win-x64.zip)
- **Linux (x64):** [`CCSWE.AppManager.DeviceOwner-linux-x64.tar.gz`](https://github.com/CoryCharlton/CCSWE.AppManager.DeviceOwner/releases/latest/download/CCSWE.AppManager.DeviceOwner-linux-x64.tar.gz)
- **macOS (Apple Silicon):** [`CCSWE.AppManager.DeviceOwner-osx-arm64.tar.gz`](https://github.com/CoryCharlton/CCSWE.AppManager.DeviceOwner/releases/latest/download/CCSWE.AppManager.DeviceOwner-osx-arm64.tar.gz)
- **macOS (Intel):** [`CCSWE.AppManager.DeviceOwner-osx-x64.tar.gz`](https://github.com/CoryCharlton/CCSWE.AppManager.DeviceOwner/releases/latest/download/CCSWE.AppManager.DeviceOwner-osx-x64.tar.gz)

(Or browse the [Releases page](https://github.com/CoryCharlton/CCSWE.AppManager.DeviceOwner/releases).) Or build from source:

```bash
# Build (Release)
dotnet build src/CCSWE.AppManager.DeviceOwner.slnx --configuration Release

# Run the desktop app
dotnet run --project src/CCSWE.AppManager.DeviceOwner.Desktop

# Run the console
dotnet run --project src/CCSWE.AppManager.DeviceOwner.Console
```

## Using it

### Desktop

Launch it, pick a device from the list (it auto-refreshes; **Refresh** forces it), then **Set device owner**. If
the device isn't ready it explains what to fix first (with a **Try anyway** override); on success a toast confirms
it, and any failure — with the full reason — opens in a dialog.

### Console

```bash
# Interactive: pick a device, confirm, set the owner
dotnet run --project src/CCSWE.AppManager.DeviceOwner.Console

# Non-interactive (scripting): target a serial and skip the confirmation
dotnet run --project src/CCSWE.AppManager.DeviceOwner.Console -- --serial <serial> --yes
```

`--help` lists the options.

---

## Development

### Tech stack

- **.NET 10** / C# (`net10.0`), SDK pinned to `10.0.0` via `global.json` (`rollForward: latestMinor`)
- **[Avalonia 12](https://avaloniaui.net/)** for the cross-platform desktop UI
- **MVVM** via [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) (`[ObservableProperty]`, `[RelayCommand]`)
- **[.NET Generic Host](https://learn.microsoft.com/dotnet/core/extensions/generic-host)** — the desktop head boots through **CCSWE.Avalonia.Hosting**; **CCSWE.Avalonia.Material** supplies the Material 3 theme
- **[Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)** wiring the shared Core services into both front-ends
- [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management) — versions live in `src/Directory.Packages.props`
- Versioning derived from git history via [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)

### Projects

The solution is `src/CCSWE.AppManager.DeviceOwner.slnx`:

- **`CCSWE.AppManager.DeviceOwner.Core`** — class library with all domain logic (process plumbing, `adb`
  location, device listing, the device-owner command and its pre-flight checks, the Platform Tools downloader,
  settings). No UI dependency.
- **`CCSWE.AppManager.DeviceOwner.Desktop`** — the Avalonia desktop GUI (`WinExe`), a single window.
- **`CCSWE.AppManager.DeviceOwner.Console`** — the command-line front-end.
- **`*.UnitTests`** — NUnit 4 tests (plain NUnit + Moq; UI types sit behind seams, so no Avalonia.Headless).

```bash
dotnet test src/CCSWE.AppManager.DeviceOwner.slnx
```

### Architecture

All functionality lives in **`CCSWE.AppManager.DeviceOwner.Core`**; the desktop GUI and console are thin
front-ends that compose a DI service provider (`AddDeviceOwnerCore()`) and drive the same services. Both are
organized **feature-first** (`Adb/`, `DeviceOwner/`, `PlatformTools/`, `Settings/`), with `Common/` for
cross-feature plumbing.
View models and services stay UI-free: anything that would touch an Avalonia control goes behind a `Common/`
abstraction with a UI-side adapter (e.g. `INotificationService`/`INotificationSink`), keeping them unit-testable
with plain NUnit + Moq.

### Releasing

Versioning is [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) — `version.json` sets
the major.minor; the patch comes from git height. To cut a release, let NBGV derive the tag:

```bash
dotnet tool restore
dotnet tool run nbgv tag      # creates v<major>.<minor>.<patch> for HEAD
git push origin <tag>          # e.g. git push origin v0.1.0
```

Pushing the `v*` tag triggers `.github/workflows/release.yml`, which publishes one framework-dependent archive
per RID (`win-x64`/`linux-x64`/`osx-x64`/`osx-arm64`), each containing both the Desktop and Console, and creates
a GitHub Release with auto-generated notes. Bump `version.json` (and commit) when you want a new major/minor.

## License

Copyright © 2026 Cory Charlton.

This program is free software: you can redistribute it and/or modify it under the terms of the **GNU General
Public License v3.0 or later** as published by the Free Software Foundation. It is distributed WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See
[LICENSE](LICENSE) for the full text.

The **App Manager** and **CCSWE** names, logos, and icons are trademarks of Cory Charlton and are **not** licensed
under the GPL — a fork must use its own distinct name and branding.
