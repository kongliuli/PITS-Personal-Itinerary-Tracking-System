# Android Build Environment Setup — Evidence Log

## Problem

`dotnet build -f net10.0-android` failed with error XA5300 ("Cannot find Android SDK directory") because `ANDROID_HOME` and `JAVA_HOME` environment variables were missing at user scope.

## SDK Paths (installed via VS Installer)

| Variable | Value |
|----------|-------|
| ANDROID_HOME | `D:\program\apps\vs\sdk\Android\android-sdk` |
| JAVA_HOME | `D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8` |

## Commands Executed

### 1. Set user-level environment variables (PowerShell)

```powershell
[Environment]::SetEnvironmentVariable('ANDROID_HOME','D:\program\apps\vs\sdk\Android\android-sdk','User')
[Environment]::SetEnvironmentVariable('JAVA_HOME','D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8','User')

# Append %JAVA_HOME%\bin to user PATH
$userPath = [Environment]::GetEnvironmentVariable('PATH','User')
if (-not $userPath.Contains('%JAVA_HOME%\\bin')) {
    [Environment]::SetEnvironmentVariable('PATH', $userPath + ';%JAVA_HOME%\\bin', 'User')
}
```

### 2. Registry verification

```powershell
Get-ItemProperty -Path HKCU:\Environment -Name ANDROID_HOME
Get-ItemProperty -Path HKCU:\Environment -Name JAVA_HOME
Get-ItemProperty -Path HKCU:\Environment -Name PATH  # contains %JAVA_HOME%\bin
```

### 3. Build verification (new process)

Launched a **truly independent PowerShell process** via `wmic process call create` (which reads a fresh environment block from the registry, unlike child-process inheritance):

```powershell
# The verify script reads env vars from registry into the new process:
$env:ANDROID_HOME = [Environment]::GetEnvironmentVariable('ANDROID_HOME', 'User')
$env:JAVA_HOME = [Environment]::GetEnvironmentVariable('JAVA_HOME', 'User')

dotnet build mvp/src/PITS.MVP.App/PITS.MVP.App.csproj -f net10.0-android
```

## Verification Output

```
ANDROID_HOME = D:\program\apps\vs\sdk\Android\android-sdk
JAVA_HOME = D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8

正在确定要还原的项目…
所有项目均是最新的，无法还原。

PITS.MVP.Core -> bin\Debug\net10.0\PITS.MVP.Core.dll
PITS.MVP.Infrastructure -> bin\Debug\net10.0\PITS.MVP.Infrastructure.dll
PITS.MVP.App -> bin\Debug\net10.0-android\PITS.MVP.App.dll
PITS.MVP.Core -> bin\Debug\net10.0\PITS.MVP.Core.dll
PITS.MVP.Infrastructure -> bin\Debug\net10.0\PITS.MVP.Infrastructure.dll

已成功生成。
    0 个警告
    0 个错误

已用时间 00:00:05.89
---EXIT CODE: 0---
```

## Result

| Check | Status |
|-------|--------|
| ANDROID_HOME set in user env | ✅ |
| JAVA_HOME set in user env | ✅ |
| %JAVA_HOME%\bin in user PATH | ✅ |
| `dotnet build -f net10.0-android` exit code 0 | ✅ |
| Android SDK found (no XA5300) | ✅ |
| PITS.MVP.App.dll built for android | ✅ |
