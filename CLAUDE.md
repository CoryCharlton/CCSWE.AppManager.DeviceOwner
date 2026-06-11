# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Project

A small cross-platform helper that makes the **App Manager** Android app the **device owner** on a connected
device. It does exactly one thing: list the online `adb` devices, let you pick one, and run
`adb -s <serial> shell dpm set-device-owner com.ccswe.appmanager.deviceowner/com.ccswe.appmanager.receivers.DeviceAdminReceiver`,
then report success or failure. The device-owner component is **fixed** to App Manager's — this is not a general
`dpm`/shell command runner.

It re-writes a much older Windows-only WinForms utility (which bundled its own `adb.exe`) as a modern .NET 10 /
Avalonia app that finds `adb` the normal way (and offers to download Google's Platform Tools when it can't). It
also runs read-only pre-flight checks before `dpm set-device-owner` and turns dpm failures into friendly,
actionable messages. All functionality lives in a shared
**`CCSWE.AppManager.DeviceOwner.Core`** library exposed through two front-ends: a desktop GUI
(**`CCSWE.AppManager.DeviceOwner.Desktop`**, **Avalonia 12** + **MVVM** via CommunityToolkit.Mvvm) and a CLI
(**`CCSWE.AppManager.DeviceOwner.Console`**). The architecture mirrors the sibling **Remote.Adb** project.

## Status

The app is feature-complete for its single purpose. Completed work is evidenced in git history and the code
itself — there is no tracked roadmap.

## Git Commits

Do not include co-author trailers or any mention of Claude in commit messages.

## Build & Run Commands

All projects live under `src/`; the solution is `src/CCSWE.AppManager.DeviceOwner.slnx`.

```bash
# Build
dotnet build src/CCSWE.AppManager.DeviceOwner.slnx --configuration Release

# Run the desktop app
dotnet run --project src/CCSWE.AppManager.DeviceOwner.Desktop

# Run the console app
dotnet run --project src/CCSWE.AppManager.DeviceOwner.Console -- [--serial <serial>] [--yes]

# Run all tests
dotnet test src/CCSWE.AppManager.DeviceOwner.slnx

# Run a specific test
dotnet test src/CCSWE.AppManager.DeviceOwner.slnx --filter "FullyQualifiedName~ClassName"
```

The SDK is pinned to `10.0.0` (`rollForward: latestMinor`) via `global.json`. `src/Directory.Build.props`
applies `LangVersion=default`, `ImplicitUsings=enable`, and `Nullable=enable` solution-wide, and references
JetBrains.Annotations and Nerdbank.GitVersioning (version derived from git history).

## Package management

Package versions are centrally managed via **Central Package Management** (`src/Directory.Packages.props`,
`ManagePackageVersionsCentrally=true`). To add or update a dependency, add a `<PackageVersion Include="X"
Version="N" />` in `Directory.Packages.props` and a version-less `<PackageReference Include="X" />` in the
project. Never put `Version=` on a `<PackageReference>` — it errors with NU1008.

## Architecture

Projects in `src/CCSWE.AppManager.DeviceOwner.slnx`:

- **`CCSWE.AppManager.DeviceOwner.Core`** — class library holding all domain logic and the
  `AddDeviceOwnerCore()` DI registration. No UI dependency. Feature-first folders:
  - `Common/` — process execution (`IProcessRunner`/`ProcessRunner`, run-to-completion only), `adb` location
    (`IAdbLocator`/`AdbLocator`, with `IsAvailable` for the missing-adb path), `ExecutableLocator`/`IExecutableFinder`,
    `IEnvironment` (mockable env-var/known-folder lookups), `ProcessResult`/`ProcessLaunchException`.
  - `Adb/` — `AdbDevice`/`AdbOwner`, `AdbOutputParser` (span-based parsing of `adb devices -l`, `dpm list-owners`,
    `pm list users`, `dumpsys account`), `IDeviceService`/`DeviceService`.
  - `DeviceOwner/` — `IDeviceOwnerService`/`DeviceOwnerService` (the one command, with a timeout, mapping failures
    to friendly copy), `IDeviceOwnerPreflight`/`DeviceOwnerPreflight` (read-only readiness checks before running),
    `DeviceOwnerError`/`DeviceOwnerMessages` (failure-string → user copy, mirrored from the ccswe.com guidance),
    `DeviceOwnerResult`/`DeviceOwnerReadiness`/`PreflightBlocker`.
  - `PlatformTools/` — `IPlatformToolsInstaller`/`PlatformToolsInstaller` (downloads & extracts Google's SDK
    Platform Tools when `adb` is missing, via `IHttpClientFactory`), `DownloadProgress`/`DownloadError`.
  - `Settings/` — persisted settings (`adb` path override, theme/density) under the app-data folder.
- **`CCSWE.AppManager.DeviceOwner.Desktop`** — the Avalonia desktop GUI (`WinExe`), a **single window** (no
  navigation). `Shell/MainWindow` composes small cards from `DeviceOwner/` (`DevicePickerCard`,
  `SetDeviceOwnerCard`) bound to one `MainWindowViewModel`. `Common/` holds the modal-dialog infrastructure
  (`DialogHost`/`IDialogViewModel`, `ConfirmDialog` for Try-anyway, `MessageDialog` for long failure text),
  notifications, the `ITimerFactory`/`IDispatcherTimer` auto-refresh seam, and `ObservableCollectionMergeExtensions`
  (in-place list reconcile so the selection survives a refresh); `PlatformTools/` holds the download-progress dialog.
- **`CCSWE.AppManager.DeviceOwner.Console`** — the CLI; interactive prompts by default, with `--serial`/`--yes`
  for scripting.
- **`*.UnitTests`** — NUnit 4 tests for Core and Desktop.

Both front-ends compose a `Microsoft.Extensions.DependencyInjection` provider and call `AddDeviceOwnerCore()`
(the Desktop head adds `AddDeviceOwnerDesktop()` on top). Keep logic in Core; the GUI and CLI are thin shells.

The desktop app follows Avalonia's **MVVM** conventions:

- `Program.Main` builds a **.NET Generic Host** via `DesktopApplication.CreateBuilder<App>` (from
  **CCSWE.Avalonia.Hosting**), registers services, and runs it. `BuildAvaloniaApp()` mirrors the same Avalonia
  configuration (minus the host) for the XAML previewer; `WithDeveloperTools()` is added only in `DEBUG`.
- `App.OnFrameworkInitializationCompleted` is the composition root — the host injects the service provider, the
  app applies the persisted theme/density, then resolves `MainWindow` from DI.
- `ViewModelBase` derives from CommunityToolkit.Mvvm's `ObservableObject`. Use the toolkit's source generators
  (`[ObservableProperty]`, `[RelayCommand]`) for bindable state and commands.
- **Compiled bindings are on by default** (`AvaloniaUseCompiledBindingsByDefault=true`) — XAML bindings need a
  declared `x:DataType`, and binding errors surface at compile time.

> This is a single-window app, so there is **no** `ViewLocator` / VM→View routing (unlike Remote.Adb). The
> window's cards are plain `UserControl`s that inherit the window's `MainWindowViewModel` as their `DataContext`.

### View models and services stay UI-free

A view model or service must **not** depend directly on an Avalonia UI control or threading primitive. Introduce
a thin abstraction in the relevant `Common/` folder and a UI-side **adapter** that implements it. Example:
`INotificationService` (a domain-only seam) is implemented over a `WindowNotificationManager` by
`WindowNotificationManagerSink` (and, likewise, `ITimerFactory`/`IDispatcherTimer` over Avalonia's
`DispatcherTimer`); the adapter — not the consumer — owns the Avalonia type and UI-thread marshaling. This keeps
view models unit-testable with plain NUnit + Moq (no Avalonia.Headless harness).

# Testing

Tests use **NUnit 4**.

## Class organization

- Outer class name: `<ClassUnderTest>Tests`, decorated with `[SuppressMessage("ReSharper", "InconsistentNaming")]`.
  The outer class is NOT `sealed` — nested classes inherit from it.
- Nested classes group tests by method or scenario: `When_<MethodName>_Is_Called`, inheriting the outer class.
- Test methods describe expected behavior: `It_<expected_behavior>`.

## Arrange-Act-Assert

Follow the AAA pattern. Use blank lines to separate sections — do **not** use `// Arrange`, `// Act`,
`// Assert` comments.

## Mocking

- Use **Moq** for mocking.
- `ILogger` should be mocked using the `LoggerFake` class, not `new Mock<ILogger>()`.
- Prefer `ReturnsAsync(...)` and `ThrowsAsync(...)` over manually setting up async mock methods.

# Coding Standards

Follow standard C# conventions.

## Formatting

- 4-space indentation (no tabs); Allman braces; always use curly braces for control flow.
- One statement/declaration per line; one blank line between members; no consecutive blank lines.

## Naming

- PascalCase types/methods/properties/constants; camelCase locals/parameters; `_camelCase` private fields.
- `I` prefix for interfaces. Two-character acronyms uppercase (`IO`, `UI`); longer ones PascalCase (`Adb`, `Json`).
- Use `nameof()` instead of string literals for member names.

## File organization

- One type per file, file named `{TypeName}.cs`. File-scoped namespaces aligned with folder structure.
- `using` directives outside the namespace, `System` first, then third-party, then project namespaces.
- A per-project `Usings.cs` holds global usings (test projects: global `using NUnit.Framework;`).

## Access modifiers & language style

- Always explicit. `internal` for implementation details; `[PublicAPI]` on intentional public API;
  `[ExcludeFromCodeCoverage]` on composition-only types (the DI registrations, the Avalonia `App`).
- Use `var` where the type is inferable; language keywords for built-in types; string interpolation; `async`/`await`.
- Nullable reference types are enabled solution-wide; respect the annotations.

## Class member order

Group members by kind in this order, alphabetized by name within each group regardless of access modifier:
constants/`static readonly` fields → instance fields → constructors → properties → methods. Nested types last.

## Comments

Default to **no** comments — the code should speak for itself. The only justified comment is a short note on a
genuinely non-obvious *why* (a subtle rationale, a workaround, a gotcha). When in doubt, leave it out.

## Prefer spans for multi-step string manipulation

When a method performs more than one manipulation over the same string source (splitting, trimming, slicing,
scanning), operate over `ReadOnlySpan<char>` instead of allocating intermediate strings. `AdbOutputParser` is the
canonical example — take `source.AsSpan()`, iterate with `EnumerateLines()`, tokenize/slice with span operations,
and call `.ToString()` only on the values you keep.
