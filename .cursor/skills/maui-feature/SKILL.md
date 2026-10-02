---
name: maui-feature
description: >-
  Implements MAUI features in Principles following MVVM folders, Core vs app
  split, DI registration, and EN UI strings. Use when adding or changing
  screens, viewmodels, Core services, platform services, or running build/tests.
---

# Implement a MAUI feature

## Layout

- UI / ViewModels / Shell: `Principles/` (`Views/`, `ViewModels/`, `Services/` for MAUI-bound impls)
- Domain / sync / SQLite: `Principles.Core/` (plain `net10.0` class library — no `UseMaui`)
- Tests: `Principles.UnitTests/`

## Rules of thumb

1. Keep MAUI Essentials / Shell / notifications / WebAuthenticator in `Principles/`; keep interfaces (and pure logic) in `Principles.Core/`.
2. Register Core services in `RegisterAppCore`; register platform impls in `RegisterMauiServices` (`MauiProgram`).
3. Prefer injecting abstractions (`IPreferencesService`, `ISecureStorageService`, `IReminderService`, …) over calling Essentials from Core.
4. UI copy: `Principles/Resources/AppStrings/` (`LocStrings`); chat replies to the user stay English.
5. After substantive edits: `dotnet build Principles.Core` then `dotnet build Principles/Principles.csproj -p:TargetFramework=net10.0-android` (or iOS when needed).

## Do not

- Add `Microsoft.Maui.*` or `Plugin.LocalNotification` references back into `Principles.Core`
- Commit secrets, keystores, or real `.env` values
