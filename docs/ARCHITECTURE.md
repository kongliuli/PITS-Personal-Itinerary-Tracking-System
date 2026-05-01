# PITS 架构文档

## 1. 系统概述

PITS (Personal Itinerary Tracking System) 是一个面向个人的时空数据管理系统，采用本地优先架构，以 SQLite 单文件存储为核心，通过权限分级和图层机制管理位置与行程信息。

## 2. 技术架构

### 2.1 总体架构

```
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│                                    交互层 (Presentation)                                    │
├──────────────────┬──────────────────┬──────────────────┬──────────────────┬─────────────────┤
│   App (MAUI)     │   CLI (.NET)     │   TUI (.NET)     │   Web (.NET)     │   AI Agent (SK) │
└────────┬─────────┴────────┬─────────┴────────┬─────────┴────────┬─────────┴────────┬────────┘
         │                  │                  │                  │                  │
┌────────▼──────────────────▼──────────────────▼──────────────────▼──────────────────▼────────┐
│                                    应用层 (Application)                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────────┐   │
│  │  PITS.Core (.NET 6.0 Class Library)                                                  │   │
│  └─────────────────────────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────────────────────────┐   │
│  │  PITS.Infrastructure (.NET 6.0 Class Library)                                        │   │
│  └─────────────────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────────────────┘
         │
┌────────▼────────────────────────────────────────────────────────────────────────────────────┐
│                                    基础设施层 (Infrastructure)                              │
├──────────────────┬──────────────────┬──────────────────┬──────────────────┬──────────────────┤
│   本地存储        │   地理服务        │   AI 推理        │   加密/安全       │   同步/通信       │
└──────────────────┴──────────────────┴──────────────────┴──────────────────┴──────────────────┘
```

### 2.2 项目结构

```
PITS/
├── src/
│   ├── PITS.Core/                    # 共享类库（所有阶段共用）
│   │   ├── Entities/                  # 核心实体 (Trip, Place, TrackPoint)
│   │   ├── ValueObjects/             # 值对象 (GeoHash, TimeRange, BoundingBox)
│   │   ├── Services/                 # 服务接口 (ITripService, IPlaceService)
│   │   └── Geo/                      # 地理工具类
│   ├── PITS.Infrastructure/          # EF Core + SQLite 实现
│   │   ├── Data/                     # DbContext 和配置
│   │   ├── Migrations/               # 数据库迁移
│   │   ├── Encryption/               # 加密实现
│   │   └── Geocoding/                # 地理编码客户端
│   ├── PITS.App/                     # .NET MAUI 移动端 (MVP 主入口)
│   │   ├── Platforms/               # 平台特定代码
│   │   ├── Resources/                # 资源文件
│   │   ├── Views/                    # 页面视图
│   │   ├── ViewModels/               # 视图模型
│   │   └── Services/                 # App 层服务
│   ├── PITS.CLI/                     # 命令行工具 (Phase 3)
│   │   └── Commands/                 # CLI 命令
│   ├── PITS.TUI/                     # 终端界面 (Phase 3)
│   ├── PITS.API/                     # Web API (Phase 1-3)
│   │   ├── Endpoints/                # API 端点
│   │   └── Hubs/                     # SignalR Hub
│   └── PITS.AI/                      # AI 插件 + MCP
│       ├── Plugins/                  # Semantic Kernel 插件
│       └── MCP/                      # MCP Server 实现
├── tests/
│   ├── PITS.Core.Tests/              # 单元测试
│   └── PITS.Integration.Tests/       # 集成测试
├── docs/                             # 项目文档
├── scripts/                          # 脚本工具
├── docker/                           # Docker 配置
└── PITS.sln                          # 解决方案文件
```

## 3. 核心模块

### 3.1 PITS.Core - 核心域

**实体定义**：

- `Trip` - 行程实体，记录时间、地点、活动类型
- `Place` - 地点/POI 实体，自维护地点库
- `TrackPoint` - 轨迹点，用于高频 GPS 采集

**值对象**：

- `GeoHash` - 地理哈希编码
- `TimeRange` - 时间范围
- `BoundingBox` - 地理边界框

**服务接口**：

- `ITripService` - 行程服务
- `IPlaceService` - 地点服务
- `IGeocodingService` - 地理编码服务
- `IIntentParser` - 意图解析服务

### 3.2 PITS.Infrastructure - 基础设施

**数据层**：

- `TripContext` - EF Core DbContext
- SQLite + NetTopologySuite (空间数据)
- FTS5 全文搜索集成

**加密**：

- age 加密集成
- Classified 级数据自动加密

**地理服务**：

- Photon/Nominatim/高德 HTTP 封装

### 3.3 PITS.App - MAUI 移动端

**页面结构**：

- `RecordPage` - 快速记录页
- `CalendarPage` - 日历视图页
- `MapPage` - 地图轨迹页
- `PlacePage` - 地点管理页
- `AIChatPage` - AI 对话页
- `SettingsPage` - 设置页

### 3.4 PITS.AI - AI 集成

**Semantic Kernel 插件**：

- `TripLogPlugin` - 自然语言解析
- `TripQueryPlugin` - 行程查询
- `TripAnalyzePlugin` - 统计分析

**MCP Server**：

- `trip_create` - 创建行程
- `trip_query` - 查询行程
- `trip_summarize` - 行程摘要
- `trip_export` - 导出行程

## 4. 数据模型

### 4.1 Trip 实体

```csharp
public class Trip
{
    public string Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Timezone { get; set; }
    public Point? Location { get; set; }        // WGS84 坐标
    public string? GeoHash { get; set; }
    public double? Accuracy { get; set; }
    public string? Address { get; set; }
    public string? PlaceId { get; set; }
    public ActivityType ActivityType { get; set; }
    public string? Description { get; set; }
    public string? TagsJson { get; set; }
    public VisibilityLevel Visibility { get; set; }
    public byte[]? EncryptedPayload { get; set; }
    public DataSource Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Place? Place { get; set; }
}
```

### 4.2 枚举定义

```csharp
public enum ActivityType
{
    Work, Commute, Personal, Health, Travel, Study, Entertainment, Other
}

public enum VisibilityLevel
{
    Public = 1, Work = 2, Private = 3, Classified = 4
}

public enum DataSource
{
    Manual, GpsAuto, AiParse, CalendarSync, EmailParse, Import
}

public enum PlaceCategory
{
    Office, Home, Cafe, Station, ClientSite, Hotel, Restaurant, Gym, Other
}
```

## 5. 安全与隐私

### 5.1 权限分级

| 级别 | 值 | 说明 |
|------|-----|------|
| Public | 1 | 公开级别，可导出 |
| Work | 2 | 工作相关 |
| Private | 3 | 私人隐私 |
| Classified | 4 | 高度机密，age 加密 |

### 5.2 隐私保护措施

- Classified 级数据使用 age 公钥加密
- EF Core 拦截器自动处理加密/解密
- API 端点默认仅绑定 localhost
- 导出功能按图层自动降级 GeoHash 精度

## 6. 部署架构

### 6.1 移动端

- Android: APK / AAB (Google Play)
- iOS: TestFlight / App Store

### 6.2 配套服务 (Docker)

- Photon: 自托管地理编码
- Ollama: 本地 LLM 推理
- n8n: 工作流自动化

## 7. 开发计划

| 阶段 | 周数 | 里程碑 | 核心交付 |
|------|------|--------|---------|
| MVP | 1-6 | M0-M1 | App 可记录、日历、地图、后台定位、AI 对话 |
| Phase 1 | 7-10 | M2 | 本地 API、邮件/日历自动采集、WiFi 指纹 |
| Phase 2 | 11-14 | M3 | 向量检索、智能摘要、疲劳预警 |
| Phase 3 | 15-18 | M4 | CLI/TUI、MCP Server |
| Phase 4 | 19-20+ | M5 | 插件架构、团队模式、开源发布 |
