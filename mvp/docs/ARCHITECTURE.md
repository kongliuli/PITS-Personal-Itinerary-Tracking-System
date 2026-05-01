# PITS MVP 技术架构文档

## 1. 概述

### 1.1 项目目标

PITS MVP (Minimum Viable Product) 是个人行程追踪系统的最小可行产品原型，用于验证核心技术架构和用户体验。主要目标包括：

- 验证 .NET MAUI 跨平台移动端开发能力
- 验证 SQLite 本地数据存储方案
- 验证地图集成和位置服务
- 验证 AI 自然语言交互能力

### 1.2 技术选型

| 技术领域 | 选型 | 版本 |
|---------|------|------|
| 框架 | .NET MAUI | 8.0 |
| 语言 | C# | 12.0 |
| 数据库 | SQLite | 3.x |
| ORM | Entity Framework Core | 8.0 |
| 空间数据 | NetTopologySuite | 2.5.0 |
| MVVM | CommunityToolkit.Mvvm | 8.2.2 |
| 地图 | Microsoft.Maui.Controls.Maps | 8.0.0 |

## 2. 项目结构

```
mvp/
├── src/
│   ├── PITS.MVP.sln              # 解决方案文件
│   ├── PITS.MVP.Core/            # 核心域模型
│   │   ├── Entities/             # 实体类
│   │   │   ├── Enums.cs          # 枚举定义
│   │   │   ├── Trip.cs           # 行程实体
│   │   │   ├── Place.cs          # 地点实体
│   │   │   └── TrackPoint.cs     # 轨迹点实体
│   │   ├── ValueObjects/         # 值对象
│   │   │   ├── GeoHash.cs        # 地理哈希
│   │   │   ├── TimeRange.cs      # 时间范围
│   │   │   └── BoundingBox.cs    # 边界框
│   │   └── Services/             # 服务接口
│   │       ├── ITripService.cs
│   │       ├── IPlaceService.cs
│   │       └── IGeocodingService.cs
│   ├── PITS.MVP.Infrastructure/  # 基础设施层
│   │   ├── Data/
│   │   │   └── TripContext.cs    # EF Core DbContext
│   │   └── Services/             # 服务实现
│   │       ├── TripService.cs
│   │       ├── PlaceService.cs
│   │       └── GeocodingService.cs
│   └── PITS.MVP.App/             # MAUI 应用
│       ├── Views/                # 页面视图
│       ├── ViewModels/           # 视图模型
│       ├── Converters/           # 值转换器
│       ├── Platforms/            # 平台特定代码
│       └── Resources/            # 资源文件
└── docs/                         # 文档
```

## 3. 核心模块设计

### 3.1 核心域 (PITS.MVP.Core)

#### 3.1.1 实体设计

**Trip (行程)**

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string | ULID 主键 |
| StartedAt | DateTime | 开始时间 |
| EndedAt | DateTime? | 结束时间 |
| Location | Point? | WGS84 坐标 |
| GeoHash | string? | 地理哈希编码 |
| ActivityType | enum | 活动类型 |
| Visibility | enum | 可见性级别 |
| Description | string? | 描述 |

**Place (地点)**

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string | ULID 主键 |
| Name | string | 地点名称 |
| Location | Point? | 坐标位置 |
| Category | enum | 地点类别 |
| Radius | double? | 地理围栏半径 |

#### 3.1.2 服务接口

```csharp
public interface ITripService
{
    Task<Trip?> GetByIdAsync(string id);
    Task<IEnumerable<Trip>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task AddAsync(Trip trip);
    Task UpdateAsync(Trip trip);
    Task DeleteAsync(string id);
    Task<IEnumerable<Trip>> SearchAsync(string query, VisibilityLevel maxVisibility);
}
```

### 3.2 基础设施层 (PITS.MVP.Infrastructure)

#### 3.2.1 数据库配置

```csharp
public class TripContext : DbContext
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Place> Places => Set<Place>();
    public DbSet<TrackPoint> TrackPoints => Set<TrackPoint>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        // 实体映射配置
        model.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.GeoHash);
            // ...
        });
    }
}
```

#### 3.2.2 地理编码服务

使用 Nominatim (OpenStreetMap) 提供反向地理编码服务：

```csharp
public interface IGeocodingService
{
    Task<string?> ReverseGeocodeAsync(double latitude, double longitude);
    Task<(double Latitude, double Longitude)?> GeocodeAsync(string address);
}
```

### 3.3 MAUI 应用层 (PITS.MVP.App)

#### 3.3.1 页面架构

```
AppShell (底部 Tab 导航)
├── RecordPage     # 快速记录页
├── CalendarPage   # 日历视图页
├── MapPage        # 地图轨迹页
├── PlacePage      # 地点管理页
├── AIChatPage     # AI 对话页
└── SettingsPage   # 设置页
```

#### 3.3.2 依赖注入配置

```csharp
builder.Services.AddDbContext<TripContext>(options =>
    options.UseSqlite($"DataSource=pits_mvp.db"));

builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddSingleton<IGeocodingService, GeocodingService>();

builder.Services.AddTransient<RecordPage>();
builder.Services.AddTransient<RecordViewModel>();
// ...
```

## 4. 功能模块

### 4.1 快速记录功能

**流程**：
1. 打开 App → 自动获取 GPS 位置
2. 反向地理编码获取地址
3. 选择活动类型
4. 设置时间范围
5. 输入描述
6. 保存到 SQLite

**关键代码**：
```csharp
public async Task InitializeAsync()
{
    var location = await Geolocation.GetLocationAsync(
        new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));
    
    if (location != null)
    {
        CurrentAddress = await _geoService.ReverseGeocodeAsync(
            location.Latitude, location.Longitude) ?? "未知地点";
    }
}
```

### 4.2 日历视图功能

**功能**：
- 月历视图展示
- 日期行程标记（彩色小点）
- 点击日期查看详情

**数据结构**：
```csharp
public class CalendarDayModel
{
    public DateTime Date { get; set; }
    public int DayNumber { get; set; }
    public bool IsToday { get; set; }
    public IList<Trip> Trips { get; set; }
    public IList<TripIndicator> Indicators { get; set; }
}
```

### 4.3 地图轨迹功能

**功能**：
- 地图标记点显示
- 轨迹连线
- 时间范围过滤

**实现**：
```csharp
private async Task LoadMapDataAsync()
{
    var trips = await _viewModel.GetFilteredTripsAsync();
    
    foreach (var trip in trips.Where(t => t.Location != null))
    {
        var pin = new Pin
        {
            Location = new Location(trip.Location!.Y, trip.Location.X),
            Label = trip.ActivityType.ToString()
        };
        TripMap.Pins.Add(pin);
    }
}
```

### 4.4 AI 对话功能

**当前实现**：
- 基础关键词匹配
- 行程统计查询
- 最近行程查询

**扩展计划**：
- 集成 Semantic Kernel
- 本地 LLM (Ollama)
- 自然语言意图解析

## 5. 数据安全

### 5.1 可见性级别

| 级别 | 值 | 说明 |
|------|-----|------|
| Public | 1 | 公开，可导出 |
| Work | 2 | 工作相关 |
| Private | 3 | 私人隐私 |
| Classified | 4 | 高度机密，加密存储 |

### 5.2 数据存储

- SQLite 本地数据库
- 敏感数据加密（Classified 级别）
- 数据库文件存储在应用私有目录

## 6. 部署

### 6.1 Android

```bash
cd mvp/src/PITS.MVP.App
dotnet build -f net8.0-android
dotnet publish -f net8.0-android -c Release
```

### 6.2 iOS

```bash
cd mvp/src/PITS.MVP.App
dotnet build -f net8.0-ios
dotnet publish -f net8.0-ios -c Release
```

## 7. 后续演进

MVP 完成后，将逐步实现：

1. **Phase 1**: 后台定位、地理围栏、邮件/日历同步
2. **Phase 2**: 向量检索、智能摘要、疲劳预警
3. **Phase 3**: CLI/TUI、MCP Server
4. **Phase 4**: 插件架构、团队模式
