# Principles (.NET MAUI)

Native .NET MAUI client for the Principles habit/goal app.

Official install page (Android, iOS): [principles.top](https://principles.top)

A Flutter rewrite lives in a sibling repo; this MAUI app remains the shipping Android/iOS client (`com.set.principles`).

## Core loop

1. Define a **goal**
2. Attach **habits** that automate progress (optionally via AI recommendations)
3. Complete habits; inspect detail (progress, streaks, stability)
4. Use **Tasks** for one-off work and to keep today's habits visible
5. Use **AI Helper** for support — not the primary recommendation entry point
6. Settings / profile (slogan, mission) feed AI habit recommendations

## Shell

Tab order: Helper · Progress · **Tasks** · Profile

- Pre-login: App Benefits carousel
- Post-login: SyncGate bootstrap, then main tabs
- Settings, Edit Habit, Habit Detail, auth screens are pushed routes (not tabs)
- Daily habits report reminder and local notifications for tasks/habits
- AdMob interstitial ads (Android / iOS); Android in-app updates

## Architecture

Solution: `Principles.App.sln`

| Project | Role |
|---------|------|
| `Principles` | MAUI UI (Views, ViewModels, Shell, platform code) |
| `Principles.Core` | Domain models, services, SQLite migrations, sync |
| `Principles.UnitTests` | Unit tests |

MVVM with CommunityToolkit.Mvvm. DI registration: `RegisterAppCore` → `RegisterMauiServices` → `RegisterViewModels` → `RegisterViews` in `MauiProgram`.

Local encrypted SQLite (`sqlite-net-sqlcipher`) plus a sync queue to the .NET WebAPI. SQL migrations live under `Principles.Core/Migrations/` and are embedded resources.

## Features

- Auth (email, Apple, Google) and startup / SyncGate bootstrap
- Goals and habits on the Progress tab; habit detail and edit
- Tasks with filters (Inbox, Today, Upcoming, Overdue, Someday, Completed)
- AI Helper chat and habit recommendations
- Local notifications (`Plugin.LocalNotification`)
- Themes, EN localization resources, charts (LiveCharts / SkiaSharp)
- DevExpress MAUI controls for lists/editors

## Tech stack

- .NET SDK `10.0.101` (`global.json`; roll-forward `latestMajor`)
- .NET MAUI `10` — targets `net10.0-android` and `net10.0-ios`
- CommunityToolkit.Maui / Mvvm, DIPS.Mobile.UI, DevExpress.Maui
- Serilog, Plugin.LocalNotification, Plugin.AdMob, LiveChartsCore
- Backend: .NET WebAPI (separate repo; see `run-backend-and-frontend.ps1`)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MAUI workloads for Android and/or iOS
- Android SDK / Xcode for the platforms you run
- Optional: Visual Studio 2022 or JetBrains Rider

```bash
dotnet workload install maui
```

## Getting started

```bash
dotnet restore Principles.App.sln
dotnet build Principles.App.sln -f net10.0-android
dotnet build Principles.App.sln -f net10.0-ios
```

Run on a device or emulator (examples):

```bash
dotnet build Principles/Principles.csproj -t:Run -f net10.0-android
dotnet build Principles/Principles.csproj -t:Run -f net10.0-ios
```

Configurations: `Debug`, `Release`, `LocalDebug` (`LOCALDEBUG` hits a local API when configured).

Optional helper to start backend + MAUI together (Windows PowerShell):

```powershell
.\run-backend-and-frontend.ps1
# or
.\run-backend-and-frontend.ps1 -BackendPath "C:\path\to\backend"
```

Backend path resolution: `-BackendPath` → `BACKEND_PATH` → sibling folders named `backend-of-principles-app`, `backend`, `api`, or `Principles.Server`.

## Testing

```bash
dotnet test Principles.UnitTests/Principles.UnitTests.csproj
```

## Project structure

```text
Principles.App.sln
Principles/                 # MAUI app
  Views/
  ViewModels/
  Platforms/                # Android, iOS
  Resources/
  MauiProgram.cs
  AppShell.xaml
Principles.Core/            # Shared logic
  Services/
  Models/
  Migrations/               # Embedded SQL
  Extensions/
Principles.UnitTests/
```

## Notes

- App ID: `com.set.principles`
- Min OS: Android 21+, iOS 15+
- Do not commit secrets; keep API keys and encryption settings out of source control where possible
- Design-time builds target Android only to avoid transient iOS load failures in the IDE language server
