param(
    [string]$Configuration = "Debug",
    [string]$AndroidSdkDirectory = "D:\program\apps\vs\sdk\Android\android-sdk",
    [string]$JavaSdkDirectory = "D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8",
    [string]$DeviceSerial = "",
    [string]$PackageName = "PITS.MVP.App",
    [int]$LaunchWaitSeconds = 20,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$adb = Join-Path $AndroidSdkDirectory "platform-tools\adb.exe"
if (!(Test-Path $adb)) {
    throw "adb not found: $adb"
}

if (!$SkipBuild) {
    & (Join-Path $PSScriptRoot "build-android.ps1") `
        -Configuration $Configuration `
        -AndroidSdkDirectory $AndroidSdkDirectory `
        -JavaSdkDirectory $JavaSdkDirectory
}

if ([string]::IsNullOrWhiteSpace($DeviceSerial)) {
    $deviceLines = @(& $adb devices | Where-Object { $_ -match "`tdevice$" })
    if (!$deviceLines) {
        throw "No Android device or emulator is online."
    }
    $DeviceSerial = ($deviceLines[0] -split "`t")[0]
}

$apk = Join-Path $PSScriptRoot "src\PITS.MVP.App\bin\$Configuration\net10.0-android\PITS.MVP.App-Signed.apk"
if (!(Test-Path $apk)) {
    throw "APK not found: $apk"
}

& $adb -s $DeviceSerial install -r $apk
& $adb -s $DeviceSerial logcat -c
& $adb -s $DeviceSerial shell am force-stop $PackageName
Start-Sleep -Seconds 1
& $adb -s $DeviceSerial shell monkey -p $PackageName 1
Start-Sleep -Seconds $LaunchWaitSeconds

$appPid = (& $adb -s $DeviceSerial shell pidof $PackageName).Trim()
if ([string]::IsNullOrWhiteSpace($appPid)) {
    throw "$PackageName is not running after launch."
}

$fatal = & $adb -s $DeviceSerial logcat -d -t 1000 |
    Select-String -Pattern "FATAL EXCEPTION|AndroidRuntime: FATAL|ANR in $PackageName|Unhandled exception|Force finishing activity $PackageName"

if ($fatal) {
    $fatal
    throw "Android smoke failed: fatal log lines detected."
}

"Android smoke passed: $PackageName pid=$appPid on $DeviceSerial"
