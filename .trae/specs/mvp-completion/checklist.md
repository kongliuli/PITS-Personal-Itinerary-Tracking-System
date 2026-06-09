# MVP 补齐检查清单

## Phase 1: 阻断项修复 ✅ 全部完成

### Tab Bar 图标资源
- [x] Resources 目录存在于 mvp/src/PITS.MVP.App/ 下 (使用 emoji 作为替代方案)
- [x] AppShell.xaml 已更新使用 emoji 图标 (➕📅🗺️📍🤖📊⚙️📥)

### SettingsPage Bug 修复
- [x] SettingsPage.xaml 中 ExportCsvAsyncCommand 已改为 ExportCsvCommand

### 构建验证
- [x] `dotnet build PITS.MVP.Core` 成功 (0 Error)
- [x] `dotnet build PITS.MVP.Infrastructure` 成功 (0 Error, 1 Warning)
- [x] `dotnet test PITS.MVP.Core.Tests` 全部通过 (40 tests)
- [x] `dotnet test PITS.MVP.Infrastructure.Tests` 全部通过 (20 tests)
- [x] MauiProgram.cs 添加 UseNetTopologySuite()
- [x] TargetFramework 统一为 net8.0
- [x] SpatiaLite 已安装，空间数据测试通过
- [ ] MAUI App 无法在 Linux 上构建验证（需 Windows/Mac + maui workload）

---

## Phase 2: 后台定位与地理围栏 ⚠️ 部分完成（平台配置已就绪，服务实现待做）

### 后台定位服务
- [x] Android AndroidManifest.xml 包含必要权限 (ACCESS_FINE_LOCATION, ACCESS_COARSE_LOCATION, FOREGROUND_SERVICE)
- [x] iOS Info.plist 包含必要权限描述 (NSLocationWhenInUseUsageDescription, NSLocationAlwaysAndWhenInUseUsageDescription, UIBackgroundModes:location)
- [x] 后台定位开关 (SettingsViewModel.EnableBackgroundLocation) 已持久化
- [ ] IBackgroundLocationService 接口定义
- [ ] Android ForegroundService 实现
- [ ] iOS LocationManager 实现

### 地理围栏
- [x] 地理围栏半径设置 (SettingsViewModel.GeofenceRadius) 已持久化
- [x] IPlaceClusterService 实现常去地点识别（部分覆盖地理围栏需求）
- [ ] IGeofenceService 接口定义
- [ ] GeofenceMonitor 实现
- [ ] 地点进入/离开事件自动触发

---

## Phase 3: AI 功能集成 ⚠️ 部分完成（关键词匹配已有，LLM 集成待做）

### Semantic Kernel 集成
- [ ] PITS.AI 项目引用到 MVP.App
- [ ] Ollama 端点配置
- [ ] Kernel 创建和配置

### AI 意图解析
- [x] AIChatPage 关键词匹配已有基本功能
- [x] IStatsService 提供统计查询能力（可供 AI 调用）
- [x] ITransportModeDetector 提供出行方式检测（可供 AI 调用）
- [ ] "记录行程" 自然语言意图解析
- [ ] "查询行程" 自然语言意图解析
- [ ] "统计" 自然语言意图解析

### AIChatPage 交互
- [x] 用户消息正确发送
- [x] AI 响应正确显示
- [x] 错误状态正确处理
- [x] Converter 全局注册

---

## Phase 4: 体验优化 ✅ 全部完成

### 数据导出
- [x] GeoJSON 导出使用 System.Text.Json 序列化（安全）
- [x] CSV 导出功能
- [x] GPX 导出功能（新增）

### 错误处理
- [x] BaseViewModel 异常不再被静默吞掉
- [x] GeocodingService 网络错误正确处理（具体异常捕获+日志）
- [x] GeocodingService HttpClient 超时配置 (10秒)

---

## Phase 5: 高级功能 ❌ 未实现（未来规划）

### 数据加密
- [ ] age 加密库集成
- [ ] Classified 级数据加密
- [ ] 解密后数据正确恢复

### 跨设备同步
- [ ] Syncthing 集成方案
- [ ] 数据库文件同步
- [ ] 冲突处理

---

## 最终验收

### 已验证通过 ✅
- [x] `dotnet build PITS.MVP.Core` 通过
- [x] `dotnet build PITS.MVP.Infrastructure` 通过
- [x] `dotnet test` 全部通过 (Core: 40, Infrastructure: 20)
- [x] Android 平台配置完整 (AndroidManifest.xml)
- [x] iOS 平台配置完整 (Info.plist)
- [x] MAUI 资源目录结构完整 (Fonts/Images/Splash)
- [x] 无静默吞异常的代码
- [x] 设置可持久化 (Preferences API)
- [x] 所有服务在 MauiProgram.cs 注册
- [x] 所有 Converter 在 App.xaml 全局注册
- [x] AppShell 包含 8 个 Tab (记录/日历/地图/地点/AI/统计/设置/导入)
- [x] PITS.sln 正确引用 mvp/ 下项目
- [x] 所有 .csproj TargetFramework 为 net8.0

### 环境限制 ⚠️
- [ ] `dotnet build PITS.MVP.App` (需 Windows/Mac + MAUI workload)
- [ ] APK 构建 (需 Windows/Mac)
- [ ] 应用实际运行测试 (需设备)

### 未闭环项 ❌
- [ ] IBackgroundLocationService 后台定位服务实现
- [ ] IGeofenceService 地理围栏服务实现
- [ ] Semantic Kernel + Ollama LLM 集成
- [ ] 数据加密 (age)
- [ ] 跨设备同步 (Syncthing)
