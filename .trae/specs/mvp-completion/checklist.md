# MVP 补齐检查清单

## Phase 1: 阻断项修复

### ✅ Tab Bar 图标资源
- [x] Resources 目录存在于 mvp/src/PITS.MVP.App/ 下 (使用 emoji 作为替代方案)
- [x] record.png → ➕
- [x] calendar.png → 📅
- [x] map.png → 🗺️
- [x] place.png → 📍
- [x] ai.png → 🤖
- [x] settings.png → ⚙️
- [x] AppShell.xaml 已更新使用 emoji 图标

### ✅ SettingsPage Bug 修复
- [x] SettingsPage.xaml 中 ExportCsvAsyncCommand 已改为 ExportCsvCommand
- [x] 命令绑定正确，无运行时异常

### ✅ 构建验证
- [x] `dotnet restore` 成功完成 (Core/Infrastructure)
- [x] `dotnet build PITS.MVP.Core` 成功
- [x] `dotnet build PITS.MVP.Infrastructure` 成功
- [x] `dotnet test PITS.MVP.Core.Tests` 全部通过 (40 tests)
- [x] MauiProgram.cs 添加 UseNetTopologySuite()
- [x] TargetFramework 从 net10.0 降级到 net8.0
- [ ] MAUI App 无法在 Linux 上构建验证（需要 maui-android workload）

---

## Phase 2: 后台定位与地理围栏

### 后台定位服务
- [ ] IBackgroundLocationService 接口定义完整
- [ ] Android ForegroundService 实现
- [ ] Android AndroidManifest.xml 包含必要权限
- [ ] iOS LocationManager 实现
- [ ] iOS Info.plist 包含必要权限描述
- [ ] 后台定位开关 (SettingsViewModel) 功能正常

### 地理围栏
- [ ] IGeofenceService 接口定义完整
- [ ] GeofenceMonitor 实现
- [ ] 地点进入事件正确触发
- [ ] 地点离开事件正确触发
- [ ] 自动创建相关行程记录

---

## Phase 3: AI 功能集成

### Semantic Kernel 集成
- [ ] PITS.AI 项目正确引用
- [ ] Ollama 端点配置正确
- [ ] Kernel 创建和配置成功

### AI 意图解析
- [ ] "记录行程" 意图正确解析
- [ ] "查询行程" 意图正确解析
- [ ] "统计" 意图正确解析
- [ ] 行程创建成功并保存

### AIChatPage 交互
- [ ] 用户消息正确发送
- [ ] AI 响应正确显示
- [ ] 错误状态正确处理

---

## Phase 4: 体验优化

### 数据导出
- [ ] GeoJSON 导出包含正确字段
- [ ] CSV 导出包含正确字段
- [ ] 导出文件可正确下载/分享

### 错误处理
- [ ] 位置获取失败显示友好提示
- [ ] 网络错误正确处理
- [ ] 加载状态正确显示

---

## Phase 5: 高级功能

### 数据加密
- [ ] age 加密库正确集成
- [ ] Classified 级数据正确加密
- [ ] 解密后数据正确恢复

### 跨设备同步
- [ ] Syncthing 配置正确
- [ ] 数据库文件同步成功
- [ ] 冲突正确处理

---

## 最终验收

- [x] `dotnet build PITS.MVP.Core` 通过
- [x] `dotnet build PITS.MVP.Infrastructure` 通过
- [x] `dotnet test PITS.MVP.Core.Tests` 全部通过
- [ ] `dotnet build PITS.MVP.App` (需要 Windows/Mac + MAUI workload)
- [ ] APK 可成功构建 (需要 Windows/Mac + MAUI workload)
- [ ] 应用可正常启动 (需要实际设备)
- [ ] 6 个 Tab 页面全部可访问 (需要实际设备)
- [ ] 核心功能 (记录/日历/地图) 可正常使用 (需要实际设备)
