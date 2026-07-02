# MVP 深度补齐任务清单

## 🔴 优先级 1: 平台配置（App 无法运行的关键缺失）

### ✅ Task 1: 创建 Android 平台配置
- [x] SubTask 1.1: 创建 `mvp/src/PITS.MVP.App/Platforms/Android/AndroidManifest.xml`
  - 声明权限: ACCESS_FINE_LOCATION, ACCESS_COARSE_LOCATION, ACCESS_NETWORK_STATE, INTERNET, FOREGROUND_SERVICE
  - 设置 application 标签和 uses-sdk
- [x] SubTask 1.2: 验证 MainActivity.cs 和 MainApplication.cs 与 Manifest 一致

### ✅ Task 2: 创建 iOS 平台配置
- [x] SubTask 2.1: 创建 `mvp/src/PITS.MVP.App/Platforms/iOS/Info.plist`
  - 添加 NSLocationWhenInUseUsageDescription
  - 添加 NSLocationAlwaysAndWhenInUseUsageDescription
  - 添加 UIBackgroundModes: location
  - 设置 MinimumOSVersion 为 14.2

### ✅ Task 3: 创建 MAUI 资源目录结构
- [x] SubTask 3.1: 创建 `Resources/Fonts/` 目录并添加字体文件占位
  - MauiProgram.cs 引用了 OpenSans-Regular.ttf 和 OpenSans-Semibold.ttf
  - 需要添加这些字体文件或移除引用
- [x] SubTask 3.2: 创建 `Resources/Splash/` 目录（splash screen 配置）
- [x] SubTask 3.3: 创建 `Resources/Images/` 目录（app icon 等占位）

---

## 🔴 优先级 2: 测试修复

### ✅ Task 4: 修复 Infrastructure Tests 失败
- [x] SubTask 4.1: 在测试项目中配置 UseNetTopologySuite()
  - InfrastructureTests.cs 构造函数中 DbContextOptionsBuilder 缺少 UseNetTopologySuite()
  - 添加 `optionsBuilder.UseNetTopologySuite()` 到测试的 DbContext 配置
- [x] SubTask 4.2: 运行 `dotnet test` 验证所有测试通过

---

## 🟡 优先级 3: 代码 Bug 修复

### ✅ Task 5: 修复 BaseViewModel 异常吞没问题
- [x] SubTask 5.1: 修改 ExecuteAsync 方法，添加异常传播或至少日志记录
  - 当前 finally 块只设置 IsBusy=false，异常被静默吞掉
  - 应添加 try-catch-finally，在 catch 中记录或重新抛出

### ✅ Task 6: 修复 AIChatPage Converter 注册方式
- [x] SubTask 6.1: 将 Converter 从 AIChatPage.xaml.cs 局部注册移到 App.xaml 全局注册
  - 移除 AIChatPage.xaml.cs 中的 Resources.Add 代码
  - 在 App.xaml 的 ResourceDictionary 中添加 BoolToColorConverter, BoolToLayoutConverter, BoolToTextColorConverter
- [x] SubTask 6.2: 更新 AIChatPage.xaml 的 StaticResource 引用方式

### ✅ Task 7: 修复 CalendarViewModel 周日计算
- [x] SubTask 7.1: 修正 DayOfWeek 偏移计算
  - .NET DayOfWeek.Sunday = 0，中国习惯周一为一周开始
  - 当前 `now.AddDays(-(int)now.DayOfWeek)` 在周日会跳到上一周
  - 修正为考虑周一为起始日

### ✅ Task 8: SettingsViewModel 设置持久化
- [x] SubTask 8.1: 使用 Preferences API 持久化 DefaultVisibility
- [x] SubTask 8.2: 使用 Preferences API 持久化 EnableBackgroundLocation
- [x] SubTask 8.3: 使用 Preferences API 持久化 GeofenceRadius
- [x] SubTask 8.4: 在构造函数中从 Preferences 恢复设置值

---

## 🟡 优先级 4: 项目结构治理

### ✅ Task 9: 统一根目录解决方案
- [x] SubTask 9.1: 重写 `/workspace/PITS.sln`，使其引用 mvp/ 下的实际项目
  - 当前引用 src/ 下的空项目（只有 csproj 无源码）
  - 应引用 mvp/src/ 和 mvp/tests/ 下的项目
- [x] SubTask 9.2: 更新根目录 src/ 下空项目的 csproj
  - 将 TargetFramework 从 net6.0 升级到 net8.0
  - 添加与 mvp/ 项目一致的项目引用
  - 或者标记为未来 Phase 3-4 的占位项目

### ✅ Task 10: 清理根目录空项目
- [x] SubTask 10.1: 决定根目录 src/ 下空项目的处理方式
  - 方案 A: 删除空项目，统一使用 mvp/ 目录
  - 方案 B: 将 mvp/ 代码复制到根目录 src/ 下，删除 mvp/ 目录
  - 方案 C: 保留空项目作为占位，但更新 csproj 使其可构建
- [x] SubTask 10.2: 执行选定的方案

---

## 🟢 优先级 5: 代码质量改进

### ✅ Task 11: GeocodingService 错误处理改进
- [x] SubTask 11.1: 将空 catch 块改为记录日志或返回更具体的错误信息
- [x] SubTask 11.2: 添加超时配置（HttpClient.Timeout）

### ✅ Task 12: GeoJSON 导出安全性
- [x] SubTask 12.1: 修复 SettingsViewModel.GenerateGeoJson 中的 JSON 注入风险
  - Description 字段未转义，可能包含引号等特殊字符
  - 使用 System.Text.Json 序列化替代字符串拼接

---

## 任务依赖关系

```
Task 1 (Android) ─┬─> Task 3 (MAUI 资源)
Task 2 (iOS)     ─┘
Task 4 (测试修复) ──> 独立
Task 5 (BaseVM)  ──> 独立
Task 6 (Converter)──> 独立
Task 7 (Calendar)──> 独立
Task 8 (Settings)──> 独立
Task 9 (Sln)     ──> Task 10 (空项目)
Task 11 (Geo)    ──> 独立
Task 12 (JSON)   ──> 独立
```

## 可并行执行的任务

- Task 1 + Task 2 + Task 3 (平台配置，互不依赖)
- Task 4 (测试修复，独立)
- Task 5 + Task 6 + Task 7 + Task 8 (代码 Bug，互不依赖)
- Task 11 + Task 12 (代码质量，互不依赖)
