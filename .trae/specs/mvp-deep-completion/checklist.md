# MVP 深度补齐检查清单

## 优先级 1: 平台配置

### Android 配置
- [x] AndroidManifest.xml 存在于 mvp/src/PITS.MVP.App/Platforms/Android/
- [x] 声明 ACCESS_FINE_LOCATION 权限
- [x] 声明 ACCESS_COARSE_LOCATION 权限
- [x] 声明 INTERNET 权限
- [x] 声明 ACCESS_NETWORK_STATE 权限
- [x] 声明 FOREGROUND_SERVICE 权限
- [x] uses-sdk 配置正确 (minSdkVersion 21)

### iOS 配置
- [x] Info.plist 存在于 mvp/src/PITS.MVP.App/Platforms/iOS/
- [x] 包含 NSLocationWhenInUseUsageDescription
- [x] 包含 NSLocationAlwaysAndWhenInUseUsageDescription
- [x] 包含 UIBackgroundModes: location
- [x] MinimumOSVersion 为 14.2

### MAUI 资源
- [x] Resources/Fonts/ 目录存在
- [x] OpenSans-Regular.ttf 字体文件存在（或 MauiProgram.cs 引用已移除）
- [x] OpenSans-Semibold.ttf 字体文件存在（或 MauiProgram.cs 引用已移除）
- [x] Resources/Images/ 目录存在
- [x] Resources/Splash/ 目录存在

---

## 优先级 2: 测试修复

- [x] InfrastructureTests.cs 中 DbContext 配置包含 UseNetTopologySuite()
- [x] `dotnet test PITS.MVP.Infrastructure.Tests` 全部通过
- [x] `dotnet test PITS.MVP.Core.Tests` 全部通过

---

## 优先级 3: 代码 Bug 修复

### BaseViewModel
- [x] ExecuteAsync 不再静默吞掉异常
- [x] 异常被正确记录或传播

### AIChatPage Converter
- [x] Converter 在 App.xaml 中全局注册
- [x] AIChatPage.xaml.cs 不再局部注册 Converter
- [x] AIChatPage.xaml 的 StaticResource 引用正常工作

### CalendarViewModel
- [x] 周起始日计算正确（周一为一周开始）
- [x] 日历视图在周日显示正确

### SettingsViewModel
- [x] DefaultVisibility 持久化到 Preferences
- [x] EnableBackgroundLocation 持久化到 Preferences
- [x] GeofenceRadius 持久化到 Preferences
- [x] 应用重启后设置值正确恢复

---

## 优先级 4: 项目结构治理

- [x] PITS.sln 正确引用 mvp/ 下的实际项目
- [x] 根目录 src/ 下空项目已处理（升级/删除/标记占位）
- [x] 所有 .csproj 文件 TargetFramework 一致 (net8.0)
- [x] `dotnet restore` 在根目录成功

---

## 优先级 5: 代码质量

### GeocodingService
- [x] catch 块不再为空（至少记录日志）
- [x] HttpClient 有超时配置

### GeoJSON 导出
- [x] GenerateGeoJson 使用 System.Text.Json 序列化
- [x] Description 等字段正确转义

---

## 最终验收

- [x] 所有 .csproj TargetFramework 为 net8.0
- [x] `dotnet build PITS.MVP.Core` 通过
- [x] `dotnet build PITS.MVP.Infrastructure` 通过
- [x] `dotnet test` 全部通过
- [x] Android 平台配置完整
- [x] iOS 平台配置完整
- [x] MAUI 资源目录结构完整
- [x] 无静默吞异常的代码
- [x] 设置可持久化
