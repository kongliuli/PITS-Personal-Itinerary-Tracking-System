# 竞品特性吸收检查清单

## Phase 1: 核心分析能力

### 出行方式自动检测
- [x] ITransportModeDetector 接口定义完整
- [x] TransportModeDetector 实现步行/骑车/驾车/公共交通/飞行检测
- [x] 置信度评分功能正常
- [x] TripService.CreateTripAsync 自动设置 ActivityType
- [x] 单元测试覆盖所有出行方式

### Stay/Trip 自动分类
- [x] TripSegment 值对象定义完整 (Stay/Trip/Gap)
- [x] ITripSegmentAnalyzer 接口定义完整
- [x] TripSegmentAnalyzer 正确检测停留事件
- [x] TripSegmentAnalyzer 正确检测出行事件
- [x] TripSegmentAnalyzer 正确检测数据缺口
- [x] 检测阈值可在 SettingsPage 调节
- [x] 单元测试覆盖所有分类场景

### 速度着色轨迹
- [x] MapViewModel 生成速度着色 Polyline
- [x] 慢速=蓝色, 中速=绿色, 快速=红色
- [x] MapPage 显示多段着色轨迹
- [x] 图例说明颜色含义

### 统计仪表盘
- [x] IStatsService 接口定义完整
- [x] StatsService 正确计算总距离
- [x] StatsService 正确计算行程数
- [x] StatsService 正确计算常去地点排名
- [x] StatsService 正确计算时间分布
- [x] StatsService 正确计算出行方式分布
- [x] StatsPage 显示统计卡片
- [x] StatsPage 显示时间分布图表
- [x] StatsPage 显示出行方式饼图
- [x] AppShell 包含统计 Tab

---

## Phase 2: 数据互操作

### Google Takeout 导入
- [x] IImportService 接口定义完整
- [x] GoogleTakeoutParser 正确解析 JSON 格式
- [x] GpxParser 正确解析 GPX 1.0/1.1 格式
- [x] 批量导入支持进度回调
- [x] ImportPage 包含文件选择器
- [x] ImportPage 显示导入进度
- [x] ImportPage 显示结果统计
- [x] GPX 导出功能正常

### 热力图可视化
- [x] MapViewModel 计算热力图数据
- [x] MapPage 支持轨迹/热力图视图切换
- [x] 热力图渲染正确（密度越高颜色越深）

### 常去地点自动识别
- [x] IPlaceClusterService 接口定义完整
- [x] PlaceClusterService 正确聚类地点
- [x] Stay 事件自动关联 Place
- [x] PlacePage 包含"自动识别"按钮

### 数据缺口检测
- [x] TripSegmentAnalyzer 检测 Gap 事件
- [x] MapPage 显示缺口标记
- [x] CalendarPage 显示缺口标记
- [x] 统计页面显示缺口统计

---

## Phase 3: 体验增强

### 照片地理信息整合
- [x] IPhotoService 接口定义完整
- [x] PhotoService 正确读取 EXIF GPS 数据
- [x] 照片按时间匹配到 Trip
- [x] MapPage 显示关联照片缩略图

### Home Assistant MQTT 集成
- [x] MQTTnet NuGet 包已添加
- [x] IMqttLocationPublisher 接口定义完整
- [x] MqttLocationPublisher 发布 OwnTracks 格式消息
- [x] SettingsPage 包含 MQTT 配置项

### 追踪配置文件
- [x] TrackingProfile 实体定义完整
- [x] 充电时自动切换高频追踪
- [x] 静止时自动切换低频追踪
- [x] 驾驶时自动切换中频追踪

### 旅行回忆功能
- [x] IReminderService 接口定义完整
- [x] "去年今日"查询正确
- [x] 本地通知推送正常

---

## 竞品差异化验证

- [x] PITS 可完全离线运行（无需服务器）
- [x] PITS 支持本地 LLM (Ollama) AI 对话
- [x] PITS 支持四级权限（含 Classified 加密）
- [x] PITS 原生支持 Windows/macOS（MAUI）
- [x] 差异化优势在 UI 和文档中突出展示
