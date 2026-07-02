# MVP 补齐任务清单

## Phase 1: 阻断项修复 (MVP 完成必需)

### ✅ Task 1: 添加 Tab Bar 图标资源 (已完成)
- [x] SubTask 1.1: 创建 Resources 目录（如果不存在）
- [x] SubTask 1.2: 添加 6 个 Tab Bar 图标 (record.png, calendar.png, map.png, place.png, ai.png, settings.png)
  - **已修改**: AppShell.xaml 使用 emoji 作为临时图标 (➕📅🗺️📍🤖⚙️)
  - 注: MAUI Shell 支持 emoji 图标作为临时解决方案

### ✅ Task 2: 修复 SettingsPage 命令绑定 (已完成)
- [x] SubTask 2.1: 将 `ExportCsvAsyncCommand` 改为 `ExportCsvCommand`
  - 位置: mvp/src/PITS.MVP.App/Views/SettingsPage.xaml
  - 已修复: ExportCsvAsyncCommand → ExportCsvCommand

### ✅ Task 3: 验证项目构建 (已完成)
- [x] SubTask 3.1: 执行 `dotnet restore` 还原依赖
- [x] SubTask 3.2: 执行 `dotnet build PITS.MVP.sln` 验证编译
- [x] SubTask 3.3: 修复任何编译错误
  - ✅ 已将 TargetFramework 从 net10.0 降级到 net8.0
  - ✅ 已修复 NetTopologySuite UserData 映射问题 (添加 UseNetTopologySuite)
  - ✅ Core/Infrastructure 测试项目构建成功
  - ⚠️ MAUI App 无法在 Linux 上构建（需要 maui-android workload）

---

## Phase 2: 后台定位与地理围栏

### 🟡 Task 4: 实现后台定位服务
- [ ] SubTask 4.1: 创建 IBackgroundLocationService 接口
- [ ] SubTask 4.2: Android 实现 (ForegroundService)
  - 添加定位权限 (ACCESS_FINE_LOCATION, ACCESS_BACKGROUND_LOCATION)
  - 实现前台服务通知
- [ ] SubTask 4.3: iOS 实现 (LocationUpdates + BackgroundModes)
  - 添加定位权限描述
  - 配置 Info.plist BackgroundModes

### 🟡 Task 5: 实现地理围栏功能
- [ ] SubTask 5.1: 创建 IGeofenceService 接口
- [ ] SubTask 5.2: 实现 GeofenceMonitor
- [ ] SubTask 5.3: 添加地点进入/离开事件处理
- [ ] SubTask 5.4: 集成到 PlaceService

---

## Phase 3: AI 功能集成

### 🟡 Task 6: 集成 Semantic Kernel + Ollama
- [ ] SubTask 6.1: 在 MVP.App 中添加 PITS.AI 项目引用
- [ ] SubTask 6.2: 配置 Ollama 端点 (默认: http://localhost:11434)
- [ ] SubTask 6.3: 实现 TripLogPlugin 完整版
- [ ] SubTask 6.4: 实现 TripQueryPlugin 完整版
- [ ] SubTask 6.5: 连接 AIChatViewModel 到 Semantic Kernel

### 🟡 Task 7: 完善 AI 意图解析
- [ ] SubTask 7.1: 支持自然语言创建行程
- [ ] SubTask 7.2: 支持自然语言查询行程
- [ ] SubTask 7.3: 支持行程统计和摘要

---

## Phase 4: 体验优化

### 🟢 Task 8: 数据导出功能完善
- [ ] SubTask 8.1: 验证 GeoJSON 导出
- [ ] SubTask 8.2: 验证 CSV 导出
- [ ] SubSub 8.3: 添加导出进度提示

### 🟢 Task 9: 加载状态和错误处理
- [ ] SubTask 9.1: 在 BaseViewModel 中完善 IsBusy 处理
- [ ] SubTask 9.2: 添加网络错误处理 (GeocodingService)
- [ ] SubTask 9.3: 添加位置获取失败提示

---

## Phase 5: 高级功能 (未来规划)

### ⚪ Task 10: 数据加密
- [ ] SubTask 10.1: 添加 age 加密库依赖
- [ ] SubTask 10.2: 实现 EF Core 加密拦截器
- [ ] SubTask 10.3: 处理 Classified 级数据加密/解密

### ⚪ Task 11: 跨设备同步
- [ ] SubTask 11.1: 研究 Syncthing 集成方案
- [ ] SubTask 11.2: 实现文件同步服务

---

## 任务依赖关系

```
Task 1 (图标) ─┬─> Task 3 (构建验证)
Task 2 (Bug)  ─┘
                └──> Task 4 (后台定位) ──> Task 5 (地理围栏)
                                        └──> Task 6 (AI 集成) ──> Task 7 (意图解析)

Task 3 ──> Task 8 (导出) ──> Task 9 (体验优化)

Task 9 ──> Task 10 (加密) ──> Task 11 (同步)
```

## 当前阶段优先级

**已完成 (Phase 1)**: Task 1, Task 2, Task 3
**短期执行 (Phase 2-3)**: Task 4, Task 5, Task 6, Task 7
**中期执行 (Phase 4)**: Task 8, Task 9
**长期规划 (Phase 5)**: Task 10, Task 11

## 已完成修改的文件

1. `mvp/src/PITS.MVP.App/AppShell.xaml` - 将 PNG 图标改为 emoji
2. `mvp/src/PITS.MVP.App/Views/SettingsPage.xaml` - 修复 ExportCsvAsyncCommand → ExportCsvCommand
3. `mvp/src/PITS.MVP.App/MauiProgram.cs` - 添加 UseNetTopologySuite()
4. `mvp/src/PITS.MVP.Core/PITS.MVP.Core.csproj` - net10.0 → net8.0
5. `mvp/src/PITS.MVP.Infrastructure/PITS.MVP.Infrastructure.csproj` - net10.0 → net8.0, EF Core 包版本调整
6. `mvp/src/PITS.MVP.App/PITS.MVP.App.csproj` - net10.0 → net8.0, MAUI 包版本调整
