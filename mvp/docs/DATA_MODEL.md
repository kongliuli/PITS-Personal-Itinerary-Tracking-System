# MVP 数据模型设计

## 1. 实体关系图

```
┌─────────────────┐       ┌─────────────────┐
│      Trip       │       │      Place      │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ StartedAt       │       │ Name            │
│ EndedAt         │       │ Location        │
│ Location        │◄──────│ GeoHash         │
│ GeoHash         │       │ Category        │
│ ActivityType    │       │ VisitCount      │
│ Visibility      │       │ Radius          │
│ Description     │       └─────────────────┘
│ PlaceId (FK)    │──────►
│ Source          │
└─────────────────┘

┌─────────────────┐
│   TrackPoint    │
├─────────────────┤
│ Id (PK)         │
│ TripId (FK)     │
│ Timestamp       │
│ Location        │
│ Accuracy        │
│ Speed           │
└─────────────────┘
```

## 2. 实体详细设计

### 2.1 Trip (行程)

```csharp
public class Trip
{
    // 主键
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    
    // 时间信息
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Timezone { get; set; } = "Asia/Shanghai";
    
    // 地理信息 (WGS84)
    public Point? Location { get; set; }
    public string? GeoHash { get; set; }
    public double? Accuracy { get; set; }
    public string? Address { get; set; }
    public string? PlaceId { get; set; }
    
    // 内容与分类
    public ActivityType ActivityType { get; set; }
    public string? Description { get; set; }
    public string? TagsJson { get; set; }
    
    // 权限与加密
    public VisibilityLevel Visibility { get; set; } = VisibilityLevel.Private;
    public byte[]? EncryptedPayload { get; set; }
    
    // 元数据
    public DataSource Source { get; set; } = DataSource.Manual;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public Place? Place { get; set; }
}
```

### 2.2 Place (地点)

```csharp
public class Place
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string Name { get; set; } = string.Empty;
    public Point? Location { get; set; }
    public string? GeoHash { get; set; }
    public PlaceCategory Category { get; set; }
    public int VisitCount { get; set; }
    public DateTime? LastVisited { get; set; }
    public string? MetadataJson { get; set; }
    public double? Radius { get; set; } = 200;
    
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
```

### 2.3 TrackPoint (轨迹点)

```csharp
public class TrackPoint
{
    public long Id { get; set; }
    public string? TripId { get; set; }
    public DateTime Timestamp { get; set; }
    public Point Location { get; set; } = null!;
    public double? Accuracy { get; set; }
    public double? Speed { get; set; }
    public double? Altitude { get; set; }
}
```

## 3. 枚举定义

### 3.1 ActivityType (活动类型)

| 值 | 说明 | 颜色 |
|---|------|------|
| Work | 工作 | Blue |
| Commute | 通勤 | Grey |
| Personal | 私人 | Green |
| Health | 健康 | Red |
| Travel | 出差 | Orange |
| Study | 学习 | Purple |
| Entertainment | 娱乐 | Pink |
| Other | 其他 | DarkGray |

### 3.2 VisibilityLevel (可见性级别)

| 值 | 数值 | 说明 |
|---|------|------|
| Public | 1 | 公开，可导出分享 |
| Work | 2 | 工作相关 |
| Private | 3 | 私人隐私 |
| Classified | 4 | 高度机密，加密存储 |

### 3.3 DataSource (数据来源)

| 值 | 说明 |
|---|------|
| Manual | 手动录入 |
| GpsAuto | GPS 自动采集 |
| AiParse | AI 解析 |
| CalendarSync | 日历同步 |
| EmailParse | 邮件解析 |
| Import | 外部导入 |

### 3.4 PlaceCategory (地点类别)

| 值 | 说明 |
|---|------|
| Office | 办公室 |
| Home | 家 |
| Cafe | 咖啡馆 |
| Station | 车站 |
| ClientSite | 客户现场 |
| Hotel | 酒店 |
| Restaurant | 餐厅 |
| Gym | 健身房 |
| Other | 其他 |

## 4. 索引设计

### 4.1 Trip 表索引

| 索引名 | 字段 | 类型 |
|--------|------|------|
| PK_Trip | Id | 主键 |
| IX_Trip_StartedAt | StartedAt | 普通 |
| IX_Trip_ActivityType | ActivityType | 普通 |
| IX_Trip_Visibility | Visibility | 普通 |
| IX_Trip_GeoHash | GeoHash | 普通 |
| IX_Trip_PlaceId | PlaceId | 外键 |

### 4.2 Place 表索引

| 索引名 | 字段 | 类型 |
|--------|------|------|
| PK_Place | Id | 主键 |
| IX_Place_GeoHash | GeoHash | 普通 |
| IX_Place_Name | Name | 普通 |

## 5. 值对象

### 5.1 GeoHash

地理哈希编码，用于快速地理位置查询。

```csharp
public static class GeoHash
{
    public static string Encode(double latitude, double longitude, int precision = 8);
    public static (double Latitude, double Longitude) Decode(string geohash);
}
```

### 5.2 TimeRange

时间范围值对象。

```csharp
public record TimeRange(DateTime Start, DateTime End)
{
    public TimeSpan Duration => End - Start;
    public bool Contains(DateTime point);
    public bool Overlaps(TimeRange other);
    
    public static TimeRange Today { get; }
    public static TimeRange ThisWeek { get; }
    public static TimeRange ThisMonth { get; }
}
```

### 5.3 BoundingBox

地理边界框。

```csharp
public record BoundingBox(double North, double South, double East, double West)
{
    public Point Center { get; }
    public bool Contains(Point point);
    public Polygon ToPolygon();
    public static BoundingBox FromCenter(Point center, double radiusMeters);
}
```
