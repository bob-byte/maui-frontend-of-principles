# Запуск бекенду і фронтенду (MAUI) одночасно.
# Використання:
#   .\run-backend-and-frontend.ps1
#   .\run-backend-and-frontend.ps1 -BackendPath "C:\path\to\backend"
#
# Якщо бекенд у іншій папці — передай шлях першим аргументом або змінною середовища BACKEND_PATH.

param(
    [string] $BackendPath = $env:BACKEND_PATH
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# Шлях до бекенду: аргумент > BACKEND_PATH > папка-брат "backend" або "api"
if (-not $BackendPath) {
    foreach ($name in @("backend-of-principles-app", "backend", "api", "Principles.Server")) {
        $candidate = Join-Path (Split-Path $root -Parent) $name
        if (Test-Path $candidate) {
            $BackendPath = $candidate
            break
        }
    }
}

if (-not $BackendPath -or -not (Test-Path $BackendPath)) {
    Write-Host "Бекенд не знайдено. Вкажи шлях одним із способів:" -ForegroundColor Yellow
    Write-Host '  1. .\run-backend-and-frontend.ps1 -BackendPath "C:\шлях\до\бекенду"' -ForegroundColor Cyan
    Write-Host '  2. $env:BACKEND_PATH = "C:\шлях\до\бекенду"; .\run-backend-and-frontend.ps1' -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Запускаю тільки фронтенд (MAUI)..." -ForegroundColor Yellow
    $BackendPath = $null
} else {
    Write-Host "Бекенд: $BackendPath" -ForegroundColor Green
    $backendJob = Start-Job -ScriptBlock {
        Set-Location $using:BackendPath
        dotnet run
    }
    Start-Sleep -Seconds 2
}

# Фронтенд: збірка і запуск на Android (можна змінити на -f net10.0-ios)
Write-Host "Фронтенд: збірка та запуск MAUI (Android)..." -ForegroundColor Green
Set-Location $root
dotnet build Principles\Principles.csproj -p:Configuration=Debug -p:TargetFramework=net10.0-android
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet build Principles\Principles.csproj -p:Configuration=Debug -p:TargetFramework=net10.0-android -t:Run

if ($BackendPath) {
    Write-Host "Зупинити бекенд: Stop-Job -Id $($backendJob.Id); Remove-Job -Id $($backendJob.Id)" -ForegroundColor Gray
}
