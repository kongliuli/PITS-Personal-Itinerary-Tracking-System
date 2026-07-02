# PITS 竞品分析与特性吸收 Spec

## Why

PITS 作为一个本地优先的个人行程追踪系统，在开源领域存在多个成熟的竞品（Dawarich、Reitti、GeoPulse、OwnTracks、Colota、OpenTracks 等）。通过分析这些竞品的优秀特性，可以识别 PITS 的差异化优势和需要补齐的关键能力，避免重复造轮子，同时找到独特的定位。

## 竞品全景

### 第一梯队：功能完整的自托管平台

| 项目 | 技术栈 | Stars | 核心定位 |
|------|--------|-------|----------|
| **Dawarich** | Ruby/Rails + PostgreSQL + Sidekiq | 3.5k+ | Google Timeline 替代，Web 优先 |
| **Reitti** | Java/Spring Boot + PostgreSQL | 2.1k+ | 个人行程分析，自动识别常去地点 |
| **GeoPulse** | Java/Quarkus + PostGIS + Vue3 | 586 | 隐私优先的时间线分析平台 |

### 第二梯队：轻量追踪工具

| 项目 | 技术栈 | 核心定位 |
|------|--------|----------|
| **OwnTracks** | C/ObjC/Java + MQTT + LMDB | 轻量 MQTT 位置发布，Home Assistant 集成 |
| **Colota** | Kotlin/Android + MapLibre | Android 原生追踪，多后端同步 |
| **OpenTracks** | Java/Android | 运动轨迹记录，隐私优先 |

### 第三梯队：特殊定位

| 项目 | 核心定位 |
|------|----------|
| **Grid** | 端到端加密位置共享（Matrix 协议） |
| **AdventureLog** | 旅行规划+记录，SvelteKit |
| **Traccar** | 企业级 GPS 设备追踪（200+协议） |

## PITS 的差异化优势

| 维度 | PITS | 竞品 |
|------|------|------|
| **架构** | 本地优先，SQLite 单文件 | 服务器依赖（PostgreSQL/MySQL） |
| **AI** | 本地 LLM (Ollama) | 无/云端 API (GeoPulse 用 OpenAI) |
| **隐私** | 数据不出设备 | 自托管但需服务器 |
| **权限** | 四级分层 (Public/Work/Private/Classified) | 无/简单公开-私有 |
| **离线** | 完全离线可用 | 大多需网络 |
| **跨平台** | MAUI (Android/iOS/Windows/macOS) | 多为 Web 或 Android only |

## 竞品优秀特性分析（可吸收）

### 来自 Dawarich
1. **Google Takeout 导入** - 支持从 Google 位置历史导入数据，降低迁移成本
2. **热力图可视化** - 在地图上显示位置密度热力图
3. **统计仪表盘** - 访问国家/城市数量、总旅行距离、时间分布
4. **多数据源导入** - GPX、GeoJSON、OwnTracks JSON 等

### 来自 Reitti
1. **自动出行方式检测** - 基于速度/加速度自动识别步行/骑车/开车
2. **常去地点自动识别** - 基于停留时间和频率自动聚类
3. **照片地理信息整合** - 与 Immich 集成，在时间线上显示照片
4. **旅行回忆功能** - "去年今日" 式的回忆推送

### 来自 GeoPulse
1. **Stay/Trip 自动分类** - 将 GPS 点自动归类为"停留"和"出行"
2. **AI 聊天助手** - 自然语言查询旅行模式（但用云端 API）
3. **数据缺口检测** - 识别 GPS 数据缺失的时间段
4. **可调节时间线灵敏度** - 用户可调整停留/出行检测阈值
5. **MQTT 实时数据接收** - 支持 OwnTracks MQTT 模式

### 来自 Colota
1. **追踪配置文件** - 多种 GPS 配置自动切换（充电时/驾驶时/静止时）
2. **离线地图** - 下载地图区域供离线使用
3. **速度着色轨迹** - 根据速度用不同颜色渲染轨迹
4. **灵活同步策略** - 即时发送/批量同步/WiFi only/完全离线
5. **地理围栏暂停区** - 在特定 WiFi 附近自动暂停追踪
6. **AES-256-GCM 加密** - 数据传输加密

### 来自 OwnTracks
1. **Home Assistant 集成** - 位置驱动的家庭自动化
2. **极低资源占用** - 50MB RAM，LMDB 存储
3. **区域围栏自动化触发** - 进入/离开区域触发事件

### 来自 Grid
1. **端到端加密** - 基于 Matrix 协议的 E2EE
2. **Protomaps 自托管地图** - 不依赖 Google/Apple Maps
3. **无需 Google Play Services** - 兼容 GrapheneOS

### 来自 OpenTracks
1. **传感器融合** - 加速度计+陀螺仪提高室内精度
2. **多运动模式** - 针对不同活动优化性能
3. **海拔图表** - 海拔变化可视化

## What Changes

基于竞品分析，建议 PITS 吸收以下特性（按优先级排列）：

### 高优先级（MVP 差异化核心）
- **出行方式自动检测** - 基于速度阈值识别步行/骑车/开车/公共交通
- **Stay/Trip 自动分类** - 将 GPS 点自动归类为停留和出行
- **Google Takeout 导入** - 从 Google 位置历史迁移数据
- **统计仪表盘** - 旅行距离、常去地点、时间分布统计
- **速度着色轨迹** - 地图上根据速度用不同颜色渲染

### 中优先级（体验提升）
- **热力图可视化** - 地图上显示位置密度
- **常去地点自动识别** - 基于停留时间和频率聚类
- **照片地理信息整合** - 与本地相册关联
- **数据缺口检测** - 识别 GPS 缺失时段
- **GPX 完整导入导出** - 标准格式互操作
- **可调节检测阈值** - 用户自定义停留/出行灵敏度

### 低优先级（未来规划）
- **Home Assistant 集成** - MQTT 位置发布
- **端到端加密共享** - 位置共享功能
- **离线地图** - 下载地图区域
- **追踪配置文件** - 场景化 GPS 配置
- **传感器融合** - 加速度计辅助定位
- **旅行回忆** - "去年今日"推送

## Impact

- Affected specs: mvp-completion, mvp-deep-completion
- Affected code:
  - `PITS.MVP.Core/` - 新增实体和值对象 (TripSegment, TransportMode)
  - `PITS.MVP.Infrastructure/` - 新增服务 (TripAnalyzer, ImportService)
  - `PITS.MVP.App/ViewModels/` - 新增 ViewModel (StatsViewModel)
  - `PITS.MVP.App/Views/` - 新增页面 (StatsPage, ImportPage)
  - `PITS.MVP.App/AppShell.xaml` - 添加新 Tab

## ADDED Requirements

### Requirement: 出行方式自动检测
系统 SHALL 基于速度和加速度数据自动检测出行方式。

#### Scenario: 步行检测
- **WHEN** GPS 轨迹平均速度 < 8 km/h
- **THEN** 系统应将出行方式标记为步行

#### Scenario: 骑车检测
- **WHEN** GPS 轨迹平均速度 8-25 km/h
- **THEN** 系统应将出行方式标记为骑车

#### Scenario: 驾车检测
- **WHEN** GPS 轨迹平均速度 25-120 km/h
- **THEN** 系统应将出行方式标记为驾车

#### Scenario: 公共交通检测
- **WHEN** GPS 轨迹平均速度 25-120 km/h 且有频繁停靠
- **THEN** 系统应将出行方式标记为公共交通

### Requirement: Stay/Trip 自动分类
系统 SHALL 将 GPS 轨迹点自动分类为"停留"和"出行"。

#### Scenario: 停留检测
- **WHEN** GPS 点在半径 X 米内停留超过 Y 分钟
- **THEN** 系统应将其标记为"停留"事件
- **AND** 应自动关联或创建 Place 记录

#### Scenario: 出行检测
- **WHEN** GPS 点持续移动且速度超过阈值
- **THEN** 系统应将其标记为"出行"事件
- **AND** 应自动计算距离和持续时间

### Requirement: 数据导入
系统 SHALL 支持从外部数据源导入位置历史。

#### Scenario: Google Takeout 导入
- **WHEN** 用户选择 Google Takeout JSON 文件
- **THEN** 系统应解析位置历史并创建 Trip 和 TrackPoint 记录
- **AND** 应显示导入进度和结果统计

#### Scenario: GPX 导入
- **WHEN** 用户选择 GPX 文件
- **THEN** 系统应解析轨迹点并创建 Trip 和 TrackPoint 记录

### Requirement: 统计仪表盘
系统 SHALL 提供旅行统计和可视化。

#### Scenario: 旅行统计
- **WHEN** 用户查看统计页面
- **THEN** 应显示总旅行距离、总行程数、常去地点排名
- **AND** 应显示时间分布图表（按小时/按星期/按月份）

#### Scenario: 速度着色轨迹
- **WHEN** 用户在地图页面查看轨迹
- **THEN** 轨迹线应根据速度用不同颜色渲染（慢=蓝，中=绿，快=红）

## MODIFIED Requirements

### Requirement: Trip 实体增强
**Original**: Trip 包含 ActivityType 枚举 (Walking, Running, Cycling, Driving, Transit, Flying, Other)
**New**: ActivityType 增加自动检测逻辑，新增 TransportMode 值对象包含置信度

### Requirement: MapPage 增强
**Original**: 地图显示 Pin 和 Polyline
**New**: 地图支持速度着色 Polyline、热力图层、Stay/Trip 分段显示

## REMOVED Requirements

无移除的需求。
