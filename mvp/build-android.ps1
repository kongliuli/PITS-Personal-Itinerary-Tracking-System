param(
    [string]$Configuration = "Debug",
    [string]$AndroidSdkDirectory = "D:\program\apps\vs\sdk\Android\android-sdk",
    [string]$JavaSdkDirectory = "D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8"
)

$project = Join-Path $PSScriptRoot "src\PITS.MVP.App\PITS.MVP.App.csproj"

dotnet build $project `
    -f net10.0-android `
    -c $Configuration `
    --no-restore `
    -p:AndroidSdkDirectory="$AndroidSdkDirectory" `
    -p:JavaSdkDirectory="$JavaSdkDirectory"
