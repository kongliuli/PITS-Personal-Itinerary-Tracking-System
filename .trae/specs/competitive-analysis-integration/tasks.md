# 竞品特性吸收任务清单

## Phase 1: 核心分析能力（高优先级）

### ✅ Task 1: 实现出行方式自动检测
- [x] SubTask 1.1: 在 Core 中创建 TransportModeDetector 服务
  - 基于速度阈值检测: 步行(<8km/h), 骑车(8-25), 驾车(25-120), 公共交通(25-120+频繁停靠), 飞行(>200)
  - 支持置信度评分 (0-1)
- [x] SubTask 1.2: 在 Core 中创建 ITransportModeDetector 接口
- [x] SubTask 1.3: 在 Infrastructure 中实现 TransportModeDetector
  - 使用 TrackPoint 序列计算速度和加速度
  - 公共交通 vs 驾车区分: 检测频繁停靠模式
- [x] SubTask 1.4: 集成到 TripService.CreateTripAsync 自动设置 ActivityType
- [x] SubTask 1.5: 添加单元测试

### ✅ Task 2: 实现 Stay/Trip 自动分类
- [x] SubTask 2.1: 在 Core 中创建 TripSegment 值对象
  - 类型: Stay / Trip / Gap
  - 属性: StartTime, EndTime, Location/Route, Duration, Distance
- [x] SubTask 2.2: 在 Core 中创建 ITripSegmentAnalyzer 接口
- [x] SubTask 2.3: 在 Infrastructure 中实现 TripSegmentAnalyzer
  - 停留检测: 半径内停留超过阈值时间（默认 5 分钟，半径 50 米）
  - 出行检测: 持续移动且速度超过阈值
  - 缺口检测: GPS 数据缺失超过阈值时间
- [x] SubTask 2.4: 添加可调节检测阈值到 SettingsViewModel
  - StayRadius (默认 50m), StayDuration (默认 5min), GapThreshold (默认 30min)
- [x] SubTask 2.5: 添加单元测试

### ✅ Task 3: 实现速度着色轨迹
- [x] SubTask 3.1: 在 MapViewModel 中创建速度着色 Polyline 逻辑
  - 根据 TrackPoint 速度计算颜色: 慢=蓝, 中=绿, 快=红
  - 将轨迹分段为不同颜色的 Polyline
- [x] SubTask 3.2: 更新 MapPage.xaml 支持多段 Polyline 显示
- [x] SubTask 3.3: 添加图例说明颜色含义

### ✅ Task 4: 实现统计仪表盘
- [x] SubTask 4.1: 在 Core 中创建 IStatsService 接口
  - GetTotalDistanceAsync, GetTripCountAsync, GetTopPlacesAsync
  - GetTimeDistributionAsync (按小时/星期/月份)
  - GetTransportModeDistributionAsync
- [x] SubTask 4.2: 在 Infrastructure 中实现 StatsService
- [x] SubTask 4.3: 创建 StatsViewModel
- [x] SubTask 4.4: 创建 StatsPage.xaml
  - 统计卡片: 总距离、总行程、常去地点
  - 时间分布图表 (使用 MAUI 图表或简单条形图)
  - 出行方式饼图
- [x] SubTask 4.5: 在 AppShell.xaml 添加统计 Tab

---

## Phase 2: 数据互操作（中优先级）

### ✅ Task 5: 实现 Google Takeout 导入
- [x] SubTask 5.1: 在 Core 中创建 IImportService 接口
  - ImportFromGoogleTakeoutAsync(Stream jsonStream)
  - ImportFromGpxAsync(Stream gpxStream)
- [x] SubTask 5.2: 在 Infrastructure 中实现 GoogleTakeoutParser
  - 解析 Google Takeout JSON 格式 (locations array)
  - 批量创建 TrackPoint 和 Trip 记录
  - 支持进度回调
- [x] SubTask 5.3: 在 Infrastructure 中实现 GpxParser
  - 解析 GPX 1.0/1.1 格式 (trkpt 元素)
  - 提取坐标、时间、海拔、速度
- [x] SubTask 5.4: 创建 ImportViewModel
- [x] SubTask 5.5: 创建 ImportPage.xaml
  - 文件选择器
  - 导入进度条
  - 结果统计 (导入点数、创建行程数、跳过数)
- [x] SubTask 5.6: 添加 GPX 导出功能 (已有 GeoJSON/CSV，补充 GPX)

### ✅ Task 6: 实现热力图可视化
- [x] SubTask 6.1: 在 MapViewModel 中添加热力图数据计算
  - 将 TrackPoint 按 GeoHash 网格聚合
  - 计算每个网格的密度
- [x] SubTask 6.2: 在 MapPage.xaml 添加热力图图层切换
  - 普通轨迹视图 / 热力图视图
- [x] SubTask 6.3: 实现热力图渲染 (使用 Circle 标记 + 透明度)

### ✅ Task 7: 实现常去地点自动识别
- [x] SubTask 7.1: 在 Core 中创建 IPlaceClusterService 接口
- [x] SubTask 7.2: 在 Infrastructure 中实现 PlaceClusterService
  - 基于 DBSCAN 或简单 GeoHash 聚类
  - 识别停留时间最长和频率最高的地点
  - 自动创建 Place 记录（如果不存在）
- [x] SubTask 7.3: 集成到 TripSegmentAnalyzer
  - Stay 事件自动关联或创建 Place
- [x] SubTask 7.4: 在 PlacePage 添加"自动识别"按钮

### ✅ Task 8: 实现数据缺口检测
- [x] SubTask 8.1: 在 TripSegmentAnalyzer 中添加 Gap 检测
  - 两个连续 TrackPoint 间隔超过阈值 (默认 30 分钟) 标记为 Gap
- [x] SubTask 8.2: 在 MapPage 和 CalendarPage 中显示缺口标记
- [x] SubTask 8.3: 在统计页面显示缺口统计

---

## Phase 3: 体验增强（低优先级）

### ✅ Task 9: 照片地理信息整合
- [x] SubTask 9.1: 在 Core 中创建 IPhotoService 接口
- [x] SubTask 9.2: 在 Infrastructure 中实现 PhotoService
  - 读取照片 EXIF GPS 数据
  - 按时间匹配到 Trip
- [x] SubTask 9.3: 在 MapPage 和 CalendarPage 显示关联照片缩略图

### ✅ Task 10: Home Assistant MQTT 集成
- [x] SubTask 10.1: 添加 MQTTnet NuGet 包
- [x] SubTask 10.2: 在 Core 中创建 IMqttLocationPublisher 接口
- [x] SubTask 10.3: 在 Infrastructure 中实现 MqttLocationPublisher
  - 发布 OwnTracks 格式位置消息
- [x] SubTask 10.4: 在 SettingsPage 添加 MQTT 配置项

### ✅ Task 11: 追踪配置文件
- [x] SubTask 11.1: 在 Core 中创建 TrackingProfile 实体
  - 名称、GPS 间隔、距离过滤器、触发条件
- [x] SubTask 11.2: 实现自动切换逻辑
  - 充电时: 高频追踪
  - 静止时: 低频追踪
  - 驾驶时: 中频追踪

### ✅ Task 12: 旅行回忆功能
- [x] SubTask 12.1: 在 Core 中创建 IReminderService 接口
- [x] SubTask 12.2: 实现"去年今日"查询
- [x] SubTask 12.3: 添加本地通知推送

---

## 任务依赖关系

```
Task 1 (出行检测) ──> Task 2 (Stay/Trip 分类) ──> Task 7 (常去地点)
Task 2 ──> Task 3 (速度着色)
Task 2 ──> Task 8 (缺口检测)
Task 4 (统计仪表盘) ──> 独立（但依赖 Task 1,2 的数据）
Task 5 (数据导入) ──> 独立
Task 6 (热力图) ──> 独立

Phase 3 (Task 9-12) ──> 依赖 Phase 1-2 完成
```

## 可并行执行的任务

- Task 1 + Task 5 (分析能力 + 数据导入，互不依赖)
- Task 3 + Task 4 + Task 6 (UI 层，互不依赖)
- Task 9 + Task 10 + Task 11 + Task 12 (Phase 3，互不依赖)

## PITS 独特价值主张（竞品对比后确认）

PITS 的核心差异化在于：
1. **完全本地优先** - 唯一不需要服务器的全功能方案
2. **本地 AI** - 唯一使用本地 LLM (Ollama) 的方案
3. **四级权限** - 唯一支持 Classified 加密的方案
4. **MAUI 跨平台** - 唯一原生支持 Windows/macOS 的方案

这些差异化优势应在 UI 和文档中突出展示。
