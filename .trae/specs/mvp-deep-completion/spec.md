# MVP 深度补齐与项目治理 Spec

## Why

Phase 1 仅修复了表面阻断项，但深入审查发现项目存在更深层的问题：平台配置文件完全缺失（AndroidManifest.xml、Info.plist）、MAUI 必需资源目录不存在、根目录空项目与 MVP 项目结构分裂、Infrastructure 测试因 NetTopologySuite 映射问题全部失败、以及多个代码层面的 bug 和不一致。这些问题不解决，项目在任何平台上都无法真正运行。

## What Changes

### 平台配置（MAUI App 运行必需）
- 创建 AndroidManifest.xml（位置权限、网络权限、前台服务权限）
- 创建 iOS Info.plist（位置权限描述、后台模式）
- 创建 MAUI Resources 目录结构（字体、图片占位）

### 代码 Bug 修复
- 修复 Infrastructure Tests 的 Point.UserData 映射问题（UseNetTopologySuite 未在测试中配置）
- 修复 CalendarPage.xaml 中 DayOfWeek 周日=0 导致的日期计算偏移（中国习惯周一为一周开始）
- 修复 AIChatPage.xaml 引用 BoolToColorConverter 等但未在 App.xaml 全局注册（仅在 AIChatPage 局部注册）
- 修复 RecordViewModel 中 GeoHash.Encode 的 using 引用缺失

### 项目结构治理
- **BREAKING** 清理根目录空项目（src/, tests/），统一到 mvp/ 目录结构
- 更新根目录 PITS.sln 指向 mvp/ 下的实际项目
- 统一所有项目 TargetFramework 到 net8.0

### 代码质量
- BaseViewModel 缺少异常传播机制（ExecuteAsync 吞掉异常）
- GeocodingService 的 catch 块过于宽泛
- SettingsViewModel 设置项未持久化（Preferences）

## Impact

- Affected specs: mvp-completion (Phase 1 已完成，本 spec 是 Phase 1.5/2 的深度补充)
- Affected code:
  - `mvp/src/PITS.MVP.App/Platforms/Android/` - 新增 AndroidManifest.xml
  - `mvp/src/PITS.MVP.App/Platforms/iOS/` - 新增 Info.plist
  - `mvp/src/PITS.MVP.App/Resources/` - 新增资源目录
  - `mvp/src/PITS.MVP.App/App.xaml` - 注册全局 Converter
  - `mvp/src/PITS.MVP.App/ViewModels/BaseViewModel.cs` - 异常处理
  - `mvp/src/PITS.MVP.Infrastructure/Data/TripContext.cs` - 可能需要调整
  - `mvp/tests/PITS.MVP.Infrastructure.Tests/` - 修复测试
  - `PITS.sln` - 重写指向 mvp/ 项目
  - `src/`, `tests/` - 清理或整合空项目

## ADDED Requirements

### Requirement: 平台配置完整性
系统 SHALL 提供完整的 Android 和 iOS 平台配置，确保应用可在设备上安装和运行。

#### Scenario: Android 权限声明
- **WHEN** 应用安装到 Android 设备
- **THEN** 应声明 ACCESS_FINE_LOCATION、ACCESS_COARSE_LOCATION 权限
- **AND** 应声明 INTERNET 权限（用于地理编码）
- **AND** 应声明 FOREGROUND_SERVICE 权限（为后台定位做准备）

#### Scenario: iOS 权限声明
- **WHEN** 应用安装到 iOS 设备
- **THEN** Info.plist 应包含 NSLocationWhenInUseUsageDescription 描述
- **AND** 应包含 UIBackgroundModes 的 location 模式

#### Scenario: MAUI 资源完整性
- **WHEN** MAUI 应用启动
- **THEN** 应有正确的字体资源引用
- **AND** 应有 splash screen 配置

### Requirement: 测试可运行性
系统 SHALL 确保所有测试项目可以在 CI 环境中成功运行。

#### Scenario: Infrastructure 测试通过
- **WHEN** 执行 `dotnet test PITS.MVP.Infrastructure.Tests`
- **THEN** 所有测试应通过
- **AND** NetTopologySuite 空间数据应正确映射

### Requirement: 项目结构一致性
系统 SHALL 维护统一的项目结构和 TargetFramework 版本。

#### Scenario: TargetFramework 一致
- **WHEN** 检查所有 .csproj 文件
- **THEN** 所有 MVP 项目应使用 net8.0
- **AND** 根目录解决方案应正确引用 MVP 项目

### Requirement: 代码健壮性
系统 SHALL 正确处理异常和错误情况。

#### Scenario: BaseViewModel 异常处理
- **WHEN** ExecuteAsync 中的操作抛出异常
- **THEN** 异常应被记录或传播，而非静默吞掉

#### Scenario: 设置持久化
- **WHEN** 用户修改设置（默认可见性、后台定位开关等）
- **THEN** 设置应持久化到 Preferences 存储
- **AND** 应用重启后设置应恢复

## MODIFIED Requirements

### Requirement: AIChatPage Converter 注册
**Original**: 在 AIChatPage.xaml.cs 中局部注册 Converter
**New**: 在 App.xaml 中全局注册 Converter，所有页面可复用

## REMOVED Requirements

无移除的需求。
