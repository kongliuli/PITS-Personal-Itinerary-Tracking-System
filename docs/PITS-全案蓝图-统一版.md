# PITS - 个人行程日程记录系统全案蓝图
## Personal Itinerary Tracking System (Unified Roadmap)

> 以 .NET MAUI 移动端 App 为 MVP 起点，逐步演进为覆盖 CLI、TUI、Web API、AI Agent 的全平台个人数据管理系统的完整发展蓝图。所有阶段共用同一套 .NET 6.0 核心域与 SQLite 数据层。

---

## 目录

1. [项目总览](#一项目总览)
2. [统一技术架构](#二统一技术架构)
3. [数据模型与存储](#三数据模型与存储)
4. [MVP 阶段：App-First（第 1-6 周）](#四mvp-阶段app-first第-1-6-周)
5. [演进阶段：全平台扩展（第 7-20 周）](#五演进阶段全平台扩展第-7-20-周)
6. [功能模块详述](#六功能模块详述)
7. [AI 与 MCP 集成](#七ai-与-mcp-集成)
8. [安全与隐私](#八安全与隐私)
9. [开发计划总表](#九开发计划总表)
10. [部署与发布](#十部署与发布)
11. [下一步行动](#十一下一步行动)

---

## 一、项目总览

### 1.1 核心定位

**PITS** 是一个面向个人的时空数据管理系统，以"**记录你在何时、何地、做什么**"为核心，通过权限分级和图层机制管理不同粒度的位置与行程信息，并支持 AI 自然语言交互。

### 1.2 设计原则

| 原则 | 说明 |
|------|------|
| **App-First 起步** | MVP 以 .NET MAUI 移动端 App 为主入口，降低使用门槛 |
| **本地优先** | 数据主权归个人，SQLite 单文件存储，无需云端账户 |
| **渐进式复杂** | 从单机 App → 多端同步 → AI 原生 → 开源生态，按需扩展 |
| **隐私分级** | 采集端即按权限加密，服务端永不可见明文 |
| **.NET 统一** | 核心域、存储、AI 编排全部基于 .NET 6.0 生态 |

### 1.3 发展阶段概览

```
Phase 0 (MVP)          Phase 1              Phase 2              Phase 3              Phase 4
第 1-6 周              第 7-10 周           第 11-14 周          第 15-18 周          第 19-20 周+
┌──────────┐          ┌──────────┐          ┌──────────┐          ┌──────────┐          ┌──────────┐
│ MAUI App │    →    │ 后台自动 │    →    │ AI 深度  │    →    │ 多端协同 │    →    │ 开源生态 │
│ 快速记录 │          │ 采集+同步│          │ 洞察+预测│          │ CLI/TUI  │          │ 插件市场 │
│ 日历地图 │          │ 邮件日历 │          │ 向量检索 │          │ Web看板  │          │ 团队模式 │
│ AI 对话  │          │ 解析导入 │          │ 智能摘要 │          │ MCP完善  │          │ 社区发布 │
└──────────┘          └──────────┘          └──────────┘          └──────────┘          └──────────┘
```

---

## 二、统一技术架构

### 2.1 总体架构图

```
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│                                    交互层 (Presentation)                                    │
├──────────────────┬──────────────────┬──────────────────┬──────────────────┬─────────────────┤
│   App (MAUI)     │   CLI (.NET)     │   TUI (.NET)     │   Web (.NET)     │   AI Agent (SK) │
│   - 快速记录      │   Spectre.Console│   Terminal.Gui   │   ASP.NET Core   │   MCP Server    │
│   - 日历地图      │   System.CmdLine │                  │   Minimal API    │   Semantic      │
│   - AI 对话       │                  │                  │   SignalR        │   Kernel        │
│   - 后台定位      │                  │                  │                  │   Plugins       │
└────────┬─────────┴────────┬─────────┴────────┬─────────┴────────┬─────────┴────────┬────────┘
         │                  │                  │                  │                  │
┌────────▼──────────────────▼──────────────────▼──────────────────▼──────────────────▼────────┐
│                                    应用层 (Application)                                     │
│  ┌─────────────────────────────────────────────────────────────────────────────────────┐   │
│  │  PITS.Core (.NET 6.0 Class Library)                                                  │   │
│  │  - Entities: Trip, Place, TrackPoint, LayerPolicy, UserPreference                    │   │
│  │  - ValueObjects: GeoHash, TimeRange, BoundingBox, EncryptedPayload                    │   │
│  │  - Services: ITripService, IPlaceService, IGeocodingService, IIntentParser           │   │
│  │  - Interfaces: IVectorSearch, ISyncProvider, INotificationService                     │   │
│  └─────────────────────────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────────────────────────┐   │
│  │  PITS.Infrastructure (.NET 6.0 Class Library)                                        │   │
│  │  - EF Core 6.0 + SQLite + NetTopologySuite (空间数据)                                 │   │
│  │  - FTS5 全文搜索 (自定义 Migration + 触发器)                                          │   │
│  │  - AgeEncryptionInterceptor (classified 级自动加密)                                   │   │
│  │  - GeocodingClient (Photon/Nominatim/高德 HTTP 封装)                                  │   │
│  │  - VectorSearchClient (Chroma/Milvus Lite HTTP 封装)                                  │   │
│  └─────────────────────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────────────────────┘
         │
┌────────▼────────────────────────────────────────────────────────────────────────────────────┐
│                                    基础设施层 (Infrastructure)                              │
├──────────────────┬──────────────────┬──────────────────┬──────────────────┬──────────────────┤
│   本地存储        │   地理服务        │   AI 推理        │   加密/安全       │   同步/通信       │
│   SQLite (设备)   │   Photon/Nominatim│   Ollama (本地)   │   age-cli        │   Syncthing      │
│   ├─ pits.db      │   (Docker自托管)  │   ├─ qwen2.5     │   (外部调用)      │   (P2P文件同步)   │
│   ├─ MBTiles离线  │   ├─ 高德(备选)   │   ├─ BGE嵌入     │   + DataProtection│   + 冲突解决脚本   │
│   │   地图瓦片     │                  │   └─ SK Plugin   │   API            │                  │
│   └─ Syncthing    │                  │                  │                  │                  │
│       副本目录     │                  │                  │                  │                  │
└──────────────────┴──────────────────┴──────────────────┴──────────────────┴──────────────────┘
         │
┌────────▼────────────────────────────────────────────────────────────────────────────────────┐
│                                    采集层 (Collection)                                      │
├──────────────────┬──────────────────┬──────────────────┬──────────────────┬──────────────────┤
│   App 前台        │   App 后台        │   第三方数据       │   自动化          │   手动扩展        │
│   - GPS 一键标记   │   - 定时定位       │   - IMAP邮件      │   - n8n Webhook  │   - CLI手动录入  │
│   - 地图选点       │   - 地理围栏       │     (MailKit)     │   - 快捷指令      │   - Web表单      │
│   - 描述输入       │   - 运动检测       │   - ICS日历       │     (iOS/Android)│                  │
│                   │   - WiFi指纹       │     (Ical.Net)    │                  │                  │
│                   │                   │   - 聊天记录      │                  │                  │
└──────────────────┴──────────────────┴──────────────────┴──────────────────┴──────────────────┘
```

### 2.2 .NET 技术矩阵

| 层级 | 技术选型 | 作用 | 引入阶段 |
|------|---------|------|---------|
| **App 框架** | .NET MAUI | 跨平台移动端 UI (iOS/Android/Win/Mac) | MVP |
| **CLI 框架** | System.CommandLine + Spectre.Console | 命令行接口、富文本表格/树/进度条 | Phase 3 |
| **TUI 框架** | Terminal.Gui v2 | 交互式终端界面 | Phase 3 |
| **Web API** | ASP.NET Core Minimal API | 设备间 HTTP 通信、外部 AI 接入 | Phase 2 |
| **数据访问** | EF Core 6.0 + SQLite | ORM、迁移、复杂关系查询 | MVP |
| **空间数据** | NetTopologySuite | Point/Line/Polygon 类型、GeoJSON | MVP |
| **全文搜索** | SQLite FTS5 (自定义触发器) | 行程描述语义搜索 | Phase 1 |
| **AI 编排** | Semantic Kernel (Microsoft) | Plugin 架构、本地 LLM 连接 | Phase 1 |
| **本地 LLM** | Ollama + OllamaSharp | qwen2.5/BGE 本地推理 | Phase 1 |
| **向量检索** | Chroma (HTTP 模式) | 语义相似度搜索 | Phase 2 |
| **加密工具** | age-cli + DataProtection API | classified 级字段加密 | Phase 2 |
| **同步引擎** | Syncthing (P2P) | 多设备 SQLite 实时同步 | Phase 2 |
| **自动化** | n8n / Huginn (Docker) | 邮件/日历自动解析工作流 | Phase 2 |
| **邮件解析** | MailKit | IMAP 行程邮件自动导入 | Phase 2 |
| **日历解析** | Ical.Net | ICS 订阅双向同步 | Phase 2 |

---

## 三、数据模型与存储

### 3.1 核心实体 (C#)

```csharp
// ============================================
// 核心行程实体
// ============================================
public class Trip
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Timezone { get; set; } = "Asia/Shanghai";

    // 地理信息 (WGS84, NetTopologySuite)
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

    // 导航
    public Place? Place { get; set; }
}

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

// ============================================
// 地点/POI 库 (自维护，减少第三方依赖)
// ============================================
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
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}

public enum PlaceCategory
{
    Office, Home, Cafe, Station, ClientSite, Hotel, Restaurant, Gym, Other
}

// ============================================
// 轨迹点 (高频 GPS 采集，与 Trip 分离避免主表膨胀)
// ============================================
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

### 3.2 EF Core 配置 (Fluent API)

```csharp
public class TripContext : DbContext
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Place> Places => Set<Place>();
    public DbSet<TrackPoint> TrackPoints => Set<TrackPoint>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PITS", "pits.db");

        options.UseSqlite($"DataSource={dbPath};Cache=Shared", sqlite =>
        {
            sqlite.MigrationsAssembly("PITS.Infrastructure");
        });

        options.UseNetTopologySuite();
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.ActivityType);
            entity.HasIndex(e => e.Visibility);
            entity.HasIndex(e => e.GeoHash);
            entity.HasIndex(e => e.PlaceId);

            entity.Property(e => e.ActivityType).HasConversion<string>();
            entity.Property(e => e.Visibility).HasConversion<string>();
            entity.Property(e => e.Source).HasConversion<string>();
        });

        model.Entity<Place>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GeoHash);
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Category).HasConversion<string>();
        });

        model.Entity<TrackPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.TripId);
        });
    }
}
```

### 3.3 FTS5 全文搜索集成

```csharp
// 在 Migration 中手动创建 FTS5 虚拟表和触发器
public partial class AddFts5Search : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE VIRTUAL TABLE TripsFts USING fts5(
                Description, Tags, 
                content='Trips', content_rowid='rowid'
            );

            CREATE TRIGGER trips_ai AFTER INSERT ON Trips BEGIN
                INSERT INTO TripsFts(rowid, Description, Tags) 
                VALUES (new.rowid, new.Description, new.TagsJson);
            END;

            CREATE TRIGGER trips_au AFTER UPDATE ON Trips BEGIN
                INSERT INTO TripsFts(TripsFts, rowid, Description, Tags) 
                VALUES ('delete', old.rowid, old.Description, old.TagsJson);
                INSERT INTO TripsFts(rowid, Description, Tags) 
                VALUES (new.rowid, new.Description, new.TagsJson);
            END;

            CREATE TRIGGER trips_ad AFTER DELETE ON Trips BEGIN
                INSERT INTO TripsFts(TripsFts, rowid, Description, Tags) 
                VALUES ('delete', old.rowid, old.Description, old.TagsJson);
            END;
        ");
    }
}

// 查询封装
public class TripSearchService
{
    private readonly TripContext _db;

    public async Task<IEnumerable<Trip>> SearchAsync(string query, VisibilityLevel maxVisibility)
    {
        var sql = @"
            SELECT t.* FROM Trips t
            JOIN TripsFts f ON t.rowid = f.rowid
            WHERE TripsFts MATCH {0}
            AND t.Visibility <= {1}
            ORDER BY rank
            LIMIT 50";

        return await _db.Trips
            .FromSqlRaw(sql, query, (int)maxVisibility)
            .Include(t => t.Place)
            .ToListAsync();
    }
}
```

---

## 四、MVP 阶段：App-First（第 1-6 周）

### 4.1 MVP 目标

**一句话**：用户在手机上打开 App，一键记录当前地点和正在做的事，在日历和地图上回顾，通过 AI 对话自然语言查询。

### 4.2 App 页面架构

```
AppShell (底部 Tab 导航)
├── 📍 记录 (MainPage/RecordPage)
│   └── 自动定位 → 类型选择 → 保存
├── 📅 日历 (CalendarPage)
│   └── 月历视图 → 日期详情列表
├── 🗺️ 地图 (MapPage)
│   └── 轨迹打点 → 图层过滤 → 路径连线
├── 🏷️ 地点 (PlacePage)
│   └── POI 库管理 → 地理围栏设置
├── 🤖 AI (AIChatPage)
│   └── 自然语言对话 → 行程确认卡片
└── ⚙️ 设置 (SettingsPage)
    └── 图层权限 → 同步配置 → 数据导出
```

### 4.3 快速记录页 (RecordPage)

#### 4.3.1 XAML 界面

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             Title="记录行程">
    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="15">

            <!-- 当前位置卡片 -->
            <Frame BorderColor="#E0E0E0" Padding="15">
                <VerticalStackLayout>
                    <Label Text="📍 当前位置" FontAttributes="Bold" />
                    <Label Text="{Binding CurrentAddress}" 
                           TextColor="Gray" FontSize="14"/>
                    <Label Text="{Binding CurrentCoords}" 
                           TextColor="LightGray" FontSize="12"/>
                </VerticalStackLayout>
            </Frame>

            <!-- 活动类型选择（横向滑动） -->
            <CollectionView ItemsSource="{Binding ActivityTypes}"
                            SelectionMode="Single"
                            SelectedItem="{Binding SelectedActivity}">
                <CollectionView.ItemsLayout>
                    <LinearItemsLayout Orientation="Horizontal" ItemSpacing="10"/>
                </CollectionView.ItemsLayout>
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <Frame Padding="15,8" CornerRadius="20"
                               BackgroundColor="{Binding Color}">
                            <Label Text="{Binding Icon} {Binding Name}"
                                   TextColor="White" HorizontalOptions="Center"/>
                        </Frame>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>

            <!-- 时间选择 -->
            <Grid ColumnDefinitions="*,Auto,*">
                <DatePicker Date="{Binding StartDate}" />
                <TimePicker Time="{Binding StartTime}" Grid.Column="1"/>
                <Label Text="至" Grid.Column="2" VerticalOptions="Center"/>
            </Grid>
            <DatePicker Date="{Binding EndDate}" />
            <TimePicker Time="{Binding EndTime}" />

            <!-- 描述输入 -->
            <Editor Placeholder="描述这次行程..."
                    Text="{Binding Description}"
                    HeightRequest="80"/>

            <!-- 可见性/图层 -->
            <Picker Title="可见性级别"
                    ItemsSource="{Binding VisibilityLevels}"
                    SelectedItem="{Binding SelectedVisibility}"/>

            <!-- 保存按钮 -->
            <Button Text="保存记录"
                    Command="{Binding SaveCommand}"
                    BackgroundColor="#2196F3"
                    TextColor="White"
                    CornerRadius="8"/>

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

#### 4.3.2 ViewModel

```csharp
public partial class RecordViewModel : ObservableObject
{
    private readonly ITripService _tripService;
    private readonly IGeocodingService _geoService;

    [ObservableProperty] private string _currentAddress = "正在定位...";
    [ObservableProperty] private string _currentCoords = "";
    [ObservableProperty] private Location? _currentLocation;
    [ObservableProperty] private ActivityType _selectedActivity = ActivityType.Work;
    [ObservableProperty] private VisibilityLevel _selectedVisibility = VisibilityLevel.Private;
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _startTime = DateTime.Now.TimeOfDay;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _endTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromHours(1));

    public ObservableCollection<ActivityTypeModel> ActivityTypes { get; } = new()
    {
        new("🏢", "工作", ActivityType.Work, Colors.Blue),
        new("🚗", "通勤", ActivityType.Commute, Colors.Grey),
        new("☕", "私人", ActivityType.Personal, Colors.Green),
        new("✈️", "出差", ActivityType.Travel, Colors.Orange),
        new("📚", "学习", ActivityType.Study, Colors.Purple),
        new("🏃", "健康", ActivityType.Health, Colors.Red),
    };

    public List<VisibilityLevel> VisibilityLevels { get; } = 
        Enum.GetValues<VisibilityLevel>().ToList();

    public async Task InitializeAsync()
    {
        var location = await Geolocation.GetLocationAsync(
            new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));

        if (location != null)
        {
            CurrentLocation = location;
            CurrentCoords = $"{location.Latitude:F4}, {location.Longitude:F4}";
            CurrentAddress = await _geoService.ReverseGeocodeAsync(
                location.Latitude, location.Longitude) ?? "未知地点";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (CurrentLocation == null)
        {
            await Shell.Current.DisplayAlert("错误", "无法获取位置", "确定");
            return;
        }

        var startedAt = StartDate.Add(StartTime);
        var endedAt = EndDate.Add(EndTime);

        var trip = new Trip
        {
            StartedAt = startedAt,
            EndedAt = endedAt,
            Location = new Point(CurrentLocation.Longitude, CurrentLocation.Latitude) { SRID = 4326 },
            GeoHash = GeoHash.Encode(CurrentLocation.Latitude, CurrentLocation.Longitude, 8),
            ActivityType = SelectedActivity,
            Description = Description,
            Visibility = SelectedVisibility,
            Source = DataSource.Manual,
            Accuracy = CurrentLocation.Accuracy
        };

        await _tripService.AddAsync(trip);

        await NotificationService.ShowAsync("行程已记录", 
            $"{SelectedActivity} @ {CurrentAddress}");

        await Shell.Current.GoToAsync("//main");
    }
}

public record ActivityTypeModel(string Icon, string Name, ActivityType Type, Color Color);
```

### 4.4 日历视图页 (CalendarPage)

```xml
<ContentPage Title="日历">
    <Grid RowDefinitions="Auto,*">
        <!-- 头部导航 -->
        <Grid ColumnDefinitions="Auto,*,Auto" Padding="15">
            <Button Text="←" Command="{Binding PrevMonthCommand}"/>
            <Label Text="{Binding CurrentMonthLabel}" 
                   HorizontalOptions="Center" FontAttributes="Bold"/>
            <Button Text="→" Command="{Binding NextMonthCommand}" Grid.Column="2"/>
        </Grid>

        <!-- 日历主体 -->
        <CollectionView Grid.Row="1" ItemsSource="{Binding CalendarDays}">
            <CollectionView.ItemsLayout>
                <GridItemsLayout Orientation="Vertical" Span="7"/>
            </CollectionView.ItemsLayout>
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Padding="5" BorderColor="{Binding BorderColor}">
                        <VerticalStackLayout>
                            <Label Text="{Binding DayNumber}" 
                                   HorizontalOptions="End" FontSize="12"/>
                            <HorizontalStackLayout Spacing="2" 
                                                   HorizontalOptions="Center">
                                <!-- 彩色小点表示行程类型 -->
                                <BindableLayout.ItemsSource>
                                    <Binding Path="TripIndicators"/>
                                </BindableLayout.ItemsSource>
                                <BindableLayout.ItemTemplate>
                                    <DataTemplate>
                                        <BoxView Color="{Binding Color}" 
                                                 WidthRequest="6" HeightRequest="6"
                                                 CornerRadius="3"/>
                                    </DataTemplate>
                                </BindableLayout.ItemTemplate>
                            </HorizontalStackLayout>
                        </VerticalStackLayout>
                        <Frame.GestureRecognizers>
                            <TapGestureRecognizer Command="{Binding SelectDayCommand}"
                                                  CommandParameter="{Binding .}"/>
                        </Frame.GestureRecognizers>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- 底部选中日期详情 -->
        <Frame Grid.Row="1" VerticalOptions="End" 
               IsVisible="{Binding HasSelectedDay}">
            <CollectionView ItemsSource="{Binding SelectedDayTrips}"
                            HeightRequest="200">
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <Grid ColumnDefinitions="Auto,*,Auto" Padding="10">
                            <Label Text="{Binding StartedAt,StringFormat='{0:HH:mm}'}"/>
                            <Label Text="{Binding Description}" Grid.Column="1"/>
                            <Frame Grid.Column="2" Padding="5,2" 
                                   BackgroundColor="{Binding ActivityColor}">
                                <Label Text="{Binding ActivityType}" 
                                       TextColor="White" FontSize="12"/>
                            </Frame>
                        </Grid>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
        </Frame>
    </Grid>
</ContentPage>
```

### 4.5 地图轨迹页 (MapPage)

```xml
<ContentPage Title="轨迹地图">
    <Grid>
        <maps:Map x:Name="tripMap" IsShowingUser="True" MapType="Street"/>

        <!-- 悬浮图层选择器 -->
        <Frame VerticalOptions="Start" HorizontalOptions="End"
               Margin="10" Padding="10" BackgroundColor="White" Opacity="0.95">
            <VerticalStackLayout Spacing="5">
                <Label Text="图层" FontAttributes="Bold" FontSize="12"/>
                <Picker ItemsSource="{Binding LayerOptions}"
                        SelectedItem="{Binding SelectedLayer}"/>
                <Label Text="时间" FontAttributes="Bold" FontSize="12"/>
                <Picker ItemsSource="{Binding TimeOptions}"
                        SelectedItem="{Binding SelectedTimeRange}"/>
            </VerticalStackLayout>
        </Frame>

        <!-- 底部统计 -->
        <Frame VerticalOptions="End" Margin="10" Padding="10"
               BackgroundColor="White" Opacity="0.95">
            <Label Text="{Binding MapStatsLabel}" FontSize="12"/>
        </Frame>
    </Grid>
</ContentPage>
```

```csharp
public partial class MapPage : ContentPage
{
    private readonly MapViewModel _vm;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMapDataAsync();
    }

    private async Task LoadMapDataAsync()
    {
        var trips = await _vm.GetFilteredTripsAsync();

        tripMap.Pins.Clear();
        tripMap.MapElements.Clear();

        // 添加标记点
        foreach (var trip in trips.Where(t => t.Location != null))
        {
            var pin = new Pin
            {
                Label = trip.ActivityType.ToString(),
                Address = trip.Description,
                Location = new Microsoft.Maui.Devices.Sensors.Location(
                    trip.Location.Y, trip.Location.X),
                Type = PinType.Place
            };
            pin.MarkerClicked += (s, e) => ShowTripDetail(trip);
            tripMap.Pins.Add(pin);
        }

        // 绘制轨迹线
        var polyline = new Polyline { StrokeColor = Colors.Blue, StrokeWidth = 3 };
        foreach (var trip in trips.OrderBy(t => t.StartedAt))
        {
            if (trip.Location != null)
                polyline.Geopath.Add(new Microsoft.Maui.Devices.Sensors.Location(
                    trip.Location.Y, trip.Location.X));
        }
        if (polyline.Geopath.Count > 1)
            tripMap.MapElements.Add(polyline);

        // 自动缩放
        if (tripMap.Pins.Any())
        {
            var bounds = CalculateBounds(trips);
            tripMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                bounds.Center, bounds.Radius));
        }
    }
}
```

### 4.6 AI 对话页 (AIChatPage)

```xml
<ContentPage Title="AI 助手">
    <Grid RowDefinitions="*,Auto">
        <CollectionView ItemsSource="{Binding Messages}" Grid.Row="0">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Grid ColumnDefinitions="Auto,*"
                          HorizontalOptions="{Binding IsUser, 
                          Converter={StaticResource BoolToLayoutOptions}}">
                        <Frame Grid.Column="{Binding IsUser, 
                              Converter={StaticResource BoolToColumn}}"
                               BackgroundColor="{Binding IsUser, 
                               Converter={StaticResource BoolToColor}}"
                               Padding="12" Margin="8">
                            <VerticalStackLayout>
                                <Label Text="{Binding Content}"/>
                                <!-- 行程确认卡片 -->
                                <Frame IsVisible="{Binding HasTripCard}"
                                       BackgroundColor="#F5F5F5" Padding="10">
                                    <VerticalStackLayout>
                                        <Label Text="{Binding TripCard.Title}" 
                                               FontAttributes="Bold"/>
                                        <Label Text="{Binding TripCard.Time}" 
                                               FontSize="12" TextColor="Gray"/>
                                        <Button Text="确认添加" 
                                                Command="{Binding TripCard.ConfirmCommand}"/>
                                    </VerticalStackLayout>
                                </Frame>
                            </VerticalStackLayout>
                        </Frame>
                    </Grid>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <Grid Grid.Row="1" ColumnDefinitions="*,Auto" Padding="10">
            <Entry Placeholder="输入指令，如'记录刚才的会议'..."
                   Text="{Binding InputText}" Grid.Column="0"/>
            <Button Text="发送" Command="{Binding SendCommand}" 
                    Grid.Column="1"/>
        </Grid>
    </Grid>
</ContentPage>
```

```csharp
public partial class AIChatViewModel : ObservableObject
{
    private readonly Kernel _kernel;
    private readonly ITripService _tripService;

    [ObservableProperty] private ObservableCollection<ChatMessage> _messages = new();
    [ObservableProperty] private string _inputText = "";

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText)) return;

        Messages.Add(new ChatMessage { Content = InputText, IsUser = true });
        var userInput = InputText;
        InputText = "";

        // AI 解析意图
        var intent = await _kernel.InvokeAsync("TripLogPlugin", "ParseIntent",
            new() { ["input"] = userInput });

        if (intent.IntentType == IntentType.CreateTrip)
        {
            var card = new TripCard
            {
                Title = intent.Description,
                Time = $"{intent.StartTime:MM-dd HH:mm} - {intent.EndTime:HH:mm}",
                Location = intent.LocationName,
                ConfirmCommand = new Command(async () => await ConfirmTripAsync(intent))
            };

            Messages.Add(new ChatMessage 
            { 
                Content = "我解析到以下行程，请确认：",
                HasTripCard = true,
                TripCard = card
            });
        }
        else if (intent.IntentType == IntentType.Query)
        {
            var results = await _tripService.QueryAsync(intent.TimeRange, intent.ActivityType);
            var summary = await _kernel.InvokeAsync("TripAnalyzePlugin", "Summarize",
                new() { ["trips"] = results });
            Messages.Add(new ChatMessage { Content = summary.GetValue<string>(), IsUser = false });
        }
        else
        {
            // 通用对话
            var response = await _kernel.InvokeAsync("GeneralChat", "Respond",
                new() { ["input"] = userInput });
            Messages.Add(new ChatMessage { Content = response.GetValue<string>(), IsUser = false });
        }
    }

    private async Task ConfirmTripAsync(ParsedIntent intent)
    {
        var trip = new Trip
        {
            StartedAt = intent.StartTime,
            EndedAt = intent.EndTime,
            Description = intent.Description,
            ActivityType = intent.ActivityType,
            Source = DataSource.AiParse
        };
        await _tripService.AddAsync(trip);
        Messages.Add(new ChatMessage { Content = "✅ 行程已添加！", IsUser = false });
    }
}
```

### 4.7 后台定位服务

#### Android (FusedLocationProvider)

```csharp
#if ANDROID
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using AndroidX.Core.App;

[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeLocation)]
public class AndroidLocationService : Service
{
    private FusedLocationProviderClient? _fusedClient;
    private LocationCallback? _callback;
    private readonly TripRepository _repo;

    public override void OnCreate()
    {
        base.OnCreate();
        _fusedClient = LocationServices.GetFusedLocationProviderClient(this);
    }

    public override StartCommandResult OnStartCommand(Intent? intent, 
        StartCommandFlags flags, int startId)
    {
        var notification = new NotificationCompat.Builder(this, "pits_location")
            .SetContentTitle("PITS 后台定位")
            .SetContentText("正在记录位置轨迹...")
            .SetSmallIcon(Resource.Drawable.notification_icon)
            .Build();

        StartForeground(1001, notification);

        var request = LocationRequest.Create()
            .SetInterval(5 * 60 * 1000)
            .SetSmallestDisplacement(500)
            .SetPriority(LocationRequest.PriorityBalancedPowerAccuracy);

        _callback = new LocationCallbackImpl(async location =>
        {
            var point = new TrackPoint
            {
                Timestamp = DateTime.UtcNow,
                Location = new Point(location.Longitude, location.Latitude) { SRID = 4326 },
                Accuracy = location.Accuracy,
                Speed = location.HasSpeed ? location.Speed : null
            };
            await _repo.AddTrackPointAsync(point);
            await CheckGeofenceAsync(location);
        });

        _fusedClient?.RequestLocationUpdates(request, _callback, Looper.MainLooper!);
        return StartCommandResult.Sticky;
    }

    private async Task CheckGeofenceAsync(Android.Locations.Location location)
    {
        var nearby = await _repo.FindNearbyPlacesAsync(
            location.Latitude, location.Longitude, 200);

        foreach (var place in nearby)
        {
            await NotificationService.ShowAsync("地点检测", 
                $"您已进入 {place.Name} 范围，是否记录行程？",
                action: () => OpenRecordPage(place));
        }
    }
}
#endif
```

#### iOS (CLLocationManager)

```csharp
#if IOS
using CoreLocation;

public class iOSLocationService
{
    private readonly CLLocationManager _manager = new();

    public void Start()
    {
        _manager.DesiredAccuracy = CLLocation.AccuracyHundredMeters;
        _manager.DistanceFilter = 500;
        _manager.AllowsBackgroundLocationUpdates = true;
        _manager.PausesLocationUpdatesAutomatically = false;
        _manager.StartMonitoringSignificantLocationChanges();

        _manager.LocationsUpdated += async (sender, e) =>
        {
            var loc = e.Locations.LastOrDefault();
            if (loc == null) return;
            await SaveTrackPointAsync(loc.Coordinate.Latitude, loc.Coordinate.Longitude);
        };
    }
}
#endif
```

### 4.8 MVP 周计划

| 周 | 核心任务 | 交付物 |
|----|---------|--------|
| **W1** | 项目搭建 + 实体 + EF Core + 记录页 | App 可安装，打开定位，保存行程到 SQLite |
| **W2** | 日历视图 + 地图页 + 地点管理 | 月历彩色标记、地图打点、POI 库 |
| **W3** | 后台定位 + 地理围栏 + 通知 | 后台每 5 分钟记录、进入地点 200m 提醒 |
| **W4** | Semantic Kernel + Ollama 集成 + AI 对话页 | App 内自然语言解析、确认卡片 |
| **W5** | 搜索(FTS5) + 图层过滤 + 导出 | 按权限筛选、全文搜索、GeoJSON 导出 |
| **W6** | Syncthing 配置 + 冲突解决 + 打包 | 多设备同步、Android APK / iOS TestFlight |

---

## 五、演进阶段：全平台扩展（第 7-20 周）

### 5.1 Phase 1：自动化采集与同步（第 7-10 周）

**目标**：减少手动输入，覆盖 70% 行程自动记录。

| 周 | 模块 | 技术实现 |
|----|------|---------|
| W7 | 本地 API (ASP.NET Core) | Minimal API 绑定 `localhost:5728`，供外部调用 |
| W7 | iOS/Android 快捷指令 | Shortcuts/Intent 直接 HTTP POST 到本地 API |
| W8 | 邮件自动解析 (MailKit) | IMAP 读取 12306/携程/航旅纵横邮件，正则+LLM 提取 |
| W8 | 日历同步 (Ical.Net) | WebDAV/ICS 订阅，Google/Outlook 日历双向同步 |
| W9 | WiFi 指纹 + 地理围栏增强 | 连接特定 WiFi 自动标记地点，重复模式学习 |
| W9 | 轨迹点自动分段 | 基于停留时间自动将 TrackPoint 聚合为 Trip |
| W10 | Syncthing 深度集成 | 冲突解决脚本、加密传输、三端（手机/笔记本/台式机） |

### 5.2 Phase 2：AI 深度与向量检索（第 11-14 周）

**目标**：从"能查"到"能懂"，从"记录"到"洞察"。

| 周 | 模块 | 技术实现 |
|----|------|---------|
| W11 | 向量检索集成 (Chroma) | BGE 嵌入模型，行程语义搜索 "去年和客户吃饭" |
| W11 | 智能摘要与周报 | Local LLM 生成自然语言周报，含地图 + 统计 |
| W12 | 疲劳预警与异常检测 | 连续 5 天 > 10h 工作自动提醒，时间冲突检测 |
| W12 | 行程预测与草稿 | 基于历史模式预测下周行程，预生成待确认草稿 |
| W13 | 高级可视化 | TUI 地图 (Terminal.Gui)、Web 看板 (Svelte + Leaflet) |
| W13 | 静态地图生成 | 调用 Mapbox Static API / 自托管 TileServer |
| W14 | 数据洞察看板 | 时间地理分布、通勤优化建议、客户拜访热力图 |

### 5.3 Phase 3：CLI / TUI / MCP 完善（第 15-18 周）

**目标**：为开发者/高级用户提供终端原生体验，开放 AI 生态。

| 周 | 模块 | 技术实现 |
|----|------|---------|
| W15 | CLI 工具 (Spectre.Console) | `pits add`, `pits list --layer work`, `pits export` |
| W15 | TUI 界面 (Terminal.Gui) | 三栏式：日期树 + 详情 + ASCII 地图 |
| W16 | MCP Server 标准实现 | 暴露 `trip_create`, `trip_query`, `trip_summarize` Tool |
| W16 | AI 外部接入 | Claude Desktop / Cline / Continue 通过 MCP 调用 PITS |
| W17 | Web API 增强 | SignalR 实时推送、gRPC 高性能接口（可选） |
| W17 | 性能优化 | AOT 编译、Span<T>、批量写入、CLI 启动 < 200ms |
| W18 | 文档与示例 | 完整 API 文档、MCP Schema、社区示例仓库 |

### 5.4 Phase 4：生态与开源发布（第 19-20 周+）

**目标**：从个人工具进化为可扩展平台。

| 模块 | 说明 |
|------|------|
| **插件架构** | MEF / DI 插件加载，第三方扩展开发指南 |
| **n8n 工作流市场** | 预设自动化模板（邮件→行程、航班→日历） |
| **团队共享模式** | 端到端加密 + 群组权限，家庭/小团队共享图层 |
| **.NET MAUI 桌面端** | Windows/macOS 原生 App，作为辅助查看端 |
| **NuGet 分包发布** | `PITS.Core`、`PITS.Infrastructure` 独立 NuGet 包 |
| **开源社区** | GitHub 组织、Issue 模板、贡献指南、版本发布流程 |

---

## 六、功能模块详述

### 6.1 记录模块

| 功能 | MVP | Phase 1 | Phase 2 | 说明 |
|------|-----|---------|---------|------|
| 手动 GPS 记录 | ✅ | ✅ | ✅ | App 前台一键标记 |
| 后台定时定位 | ✅ | ✅ | ✅ | 5min/500m 策略 |
| 地理围栏触发 | ✅ | ✅ | ✅ | 进入 POI 200m 提醒 |
| 自然语言解析 | ✅ | ✅ | ✅ | AI 对话 "记录下午3点会议" |
| 邮件自动导入 | - | ✅ | ✅ | 12306/携程邮件解析 |
| 日历双向同步 | - | ✅ | ✅ | ICS/WebDAV 订阅 |
| WiFi 指纹匹配 | - | ✅ | ✅ | 连接公司 WiFi 自动标记 |
| 轨迹自动分段 | - | ✅ | ✅ | TrackPoint → Trip 聚合 |
| 语音输入 (Whisper) | - | - | ✅ | 本地语音转文字 |
| 照片 EXIF 导入 | - | - | ✅ | 读取照片 GPS 自动生成 |

### 6.2 查询与展示模块

| 功能 | MVP | Phase 1 | Phase 2 | 说明 |
|------|-----|---------|---------|------|
| 日历视图 | ✅ | ✅ | ✅ | 月历 + 彩色标记 |
| 地图轨迹 | ✅ | ✅ | ✅ | MAUI Maps 打点连线 |
| 列表筛选 | ✅ | ✅ | ✅ | 按图层/时间/类型 |
| 全文搜索 (FTS5) | ✅ | ✅ | ✅ | SQLite 原生全文索引 |
| 语义搜索 (向量) | - | - | ✅ | "去年和客户吃饭" |
| TUI 地图 | - | - | ✅ | Terminal.Gui ASCII 地图 |
| Web 看板 | - | - | ✅ | Svelte + Leaflet |
| 静态地图导出 | - | - | ✅ | 分享图片生成 |
| 3D 时空立方体 | - | - | - | 后期扩展 |

### 6.3 AI 模块

| 功能 | MVP | Phase 1 | Phase 2 | 说明 |
|------|-----|---------|---------|------|
| 自然语言解析意图 | ✅ | ✅ | ✅ | SK TripLogPlugin |
| 结构化确认流 | ✅ | ✅ | ✅ | 解析 → 预览 → 确认 |
| 语义查询 | ✅ | ✅ | ✅ | "上周去了哪些地方" |
| 智能摘要 | - | ✅ | ✅ | 周报/月报自动生成 |
| 疲劳预警 | - | - | ✅ | 连续高强度提醒 |
| 行程预测 | - | - | ✅ | 下周草稿预生成 |
| 冲突检测 | - | - | ✅ | 航班与会议重叠提醒 |
| MCP Tool 暴露 | - | - | ✅ | 外部 AI 调用接口 |

### 6.4 同步与导出模块

| 功能 | MVP | Phase 1 | Phase 2 | 说明 |
|------|-----|---------|---------|------|
| SQLite 本地存储 | ✅ | ✅ | ✅ | 单文件，App 私有目录 |
| Syncthing P2P 同步 | ✅ | ✅ | ✅ | 文件级同步 + 冲突解决 |
| GeoJSON 导出 | ✅ | ✅ | ✅ | 带 visibility 属性 |
| CSV / Markdown 导出 | ✅ | ✅ | ✅ | 表格/文本格式 |
| KML / PDF 导出 | - | - | ✅ | 地图/报表格式 |
| 增量同步 API | - | - | ✅ | 基于 WAL 的增量 |
| 端到端加密同步 | - | - | ✅ | 团队共享模式 |

### 6.5 安全与隐私模块

| 功能 | MVP | Phase 1 | Phase 2 | 说明 |
|------|-----|---------|---------|------|
| 四级权限 (Public/Work/Private/Classified) | ✅ | ✅ | ✅ | 写入时即分级 |
| 图层过滤查询 | ✅ | ✅ | ✅ | 无权限不返回 |
| GeoHash 精度降级 | ✅ | ✅ | ✅ | public 级仅 5 位 |
| Classified 加密存储 | - | - | ✅ | age 公钥加密 |
| 安全擦除 | - | - | ✅ | `pits purge --before` |
| 本地 API 仅 localhost | ✅ | ✅ | ✅ | 不外网暴露 |

---

## 七、AI 与 MCP 集成

### 7.1 Semantic Kernel 插件架构

```csharp
// 注册本地 LLM (Ollama) 和插件
var builder = Kernel.CreateBuilder();
builder.AddOllamaChatCompletion("qwen2.5:14b", "http://localhost:11434");
builder.AddOllamaTextEmbeddingGeneration("bge-large", "http://localhost:11434");

var kernel = builder.Build();
kernel.ImportPluginFromType<TripLogPlugin>();
kernel.ImportPluginFromType<TripQueryPlugin>();
kernel.ImportPluginFromType<TripAnalyzePlugin>();
```

### 7.2 TripLogPlugin（自然语言 → 结构化）

```csharp
public class TripLogPlugin
{
    [KernelFunction("parse_trip_intent")]
    [Description("将自然语言转换为结构化行程数据")]
    public async Task<string> ParseIntent([Description("用户输入")] string input)
    {
        var prompt = $@"解析以下行程描述，提取 JSON:
        {{
            ""time_expression"": ""开始和结束时间"",
            ""location_hint"": ""地点描述"",
            ""activity_type"": ""work|commute|personal|travel"",
            ""description"": ""原始描述"",
            ""confidence"": 0.0-1.0
        }}
        输入: {input}";

        var result = await _kernel.InvokePromptAsync(prompt);
        return result.GetValue<string>()!;
    }
}
```

### 7.3 TripQueryPlugin（语义查询）

```csharp
public class TripQueryPlugin
{
    [KernelFunction("query_trips")]
    [Description("按条件查询行程")]
    public async Task<string> QueryTrips(
        [Description("时间范围")] string timeRange,
        [Description("地点关键词")] string? location = null,
        [Description("活动类型")] string? activityType = null)
    {
        // 解析时间 → 构建 LINQ 查询 → 返回 JSON
    }

    [KernelFunction("semantic_search")]
    [Description("语义搜索行程记录")]
    public async Task<string> SemanticSearch([Description("自然语言查询")] string query)
    {
        var embedding = await _vector.EmbedAsync(query);
        var similar = await _vector.SearchAsync(embedding, topK: 10);
        return JsonSerializer.Serialize(similar);
    }
}
```

### 7.4 TripAnalyzePlugin（统计与洞察）

```csharp
public class TripAnalyzePlugin
{
    [KernelFunction("summarize_period")]
    [Description("汇总指定时间段的行程统计")]
    public async Task<string> Summarize(string period, string? groupBy = null)
    {
        // 返回工时、通勤、地点分布等
    }

    [KernelFunction("detect_anomalies")]
    [Description("检测异常模式")]
    public async Task<string> DetectAnomalies(string period)
    {
        // 时间冲突、过度疲劳检测
    }
}
```

### 7.5 MCP Server 标准接口

```json
{
  "name": "pits",
  "description": "Personal Itinerary Tracking System",
  "tools": [
    {
      "name": "trip_create",
      "parameters": {
        "time_expression": "string",
        "location_hint": "string",
        "activity_type": "enum",
        "description": "string",
        "visibility": "enum"
      }
    },
    {
      "name": "trip_query",
      "parameters": {
        "time_range": "string",
        "geo_bounds": "object",
        "activity_filter": ["string"],
        "visibility_layer": "enum",
        "output_format": "enum"
      }
    },
    {
      "name": "trip_summarize",
      "parameters": {
        "period": "string",
        "group_by": "enum",
        "include_stats": "boolean"
      }
    },
    {
      "name": "trip_export",
      "parameters": {
        "layer": "string",
        "format": "enum",
        "date_range": "string"
      }
    }
  ]
}
```

---

## 八、安全与隐私

### 8.1 权限分级实现

```
采集端                    存储端                    消费端
┌─────────┐              ┌─────────────┐          ┌─────────────┐
│ 明文输入  │───[ACL检查]──▶│  SQLite       │◀────[ACL过滤]──│  App/CLI/AI  │
│         │              │  ├─公开表      │          │             │
│         │───[age加密]──▶│  ├─工作表      │          │             │
│         │   (classified)│  └─加密表      │          │             │
└─────────┘              └─────────────┘          └─────────────┘
```

### 8.2 隐私自检清单

- [ ] `Classified` 级数据使用 `age` 公钥加密，私钥存储在系统密钥环
- [ ] EF Core 拦截器自动处理加密/解密，业务代码无感知
- [ ] API 端点默认仅绑定 `localhost`，无外部暴露风险
- [ ] 导出功能按请求图层自动降级 GeoHash 精度（public 级仅 5 位 ≈ 2.4km×4.8km）
- [ ] Syncthing 配置为仅已知设备连接，启用 TLS
- [ ] 移动端快捷指令使用本地网络，不经过公网
- [ ] 提供 `pits purge --before 2025-01-01 --visibility classified` 安全擦除
- [ ] 定期备份加密后的 SQLite 文件到离线介质

---

## 九、开发计划总表

### 9.1 项目结构

```
PITS/
├── src/
│   ├── PITS.Core/                    # 共享类库（所有阶段共用）
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Services/
│   │   └── Geo/
│   ├── PITS.Infrastructure/            # EF Core + SQLite（所有阶段共用）
│   │   ├── Data/
│   │   ├── Migrations/
│   │   ├── Encryption/
│   │   └── Geocoding/
│   ├── PITS.App/                       # .NET MAUI 移动端 (MVP 主入口)
│   │   ├── Platforms/
│   │   ├── Resources/
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── Services/
│   │   └── MauiProgram.cs
│   ├── PITS.CLI/                       # 命令行工具 (Phase 3)
│   │   ├── Commands/
│   │   └── Program.cs
│   ├── PITS.TUI/                       # 终端界面 (Phase 3)
│   ├── PITS.API/                       # Web API (Phase 1-3)
│   │   ├── Endpoints/
│   │   ├── Hubs/
│   │   └── Program.cs
│   └── PITS.AI/                        # AI 插件 + MCP (MVP 起逐步完善)
│       ├── Plugins/
│       ├── MCP/
│       └── KernelConfig.cs
├── tests/
│   ├── PITS.Core.Tests/
│   └── PITS.Integration.Tests/
├── docs/
│   ├── ARCHITECTURE.md
│   ├── MCP_SCHEMA.json
│   └── API.md
├── scripts/
│   ├── setup-nominatim.sh
│   ├── setup-ollama.sh
│   └── sync-hook.sh
├── docker/
│   ├── docker-compose.yml
│   └── Dockerfile.nominatim
└── PITS.sln
```

### 9.2 20 周里程碑总表

| 阶段 | 周 | 里程碑 | 核心交付 |
|------|----|--------|---------|
| **MVP** | 1-3 | M0 | App 可记录、日历、地图、后台定位 |
| **MVP** | 4-6 | M1 | AI 对话、搜索导出、Syncthing 同步、APK 发布 |
| **Phase 1** | 7-10 | M2 | 本地 API、邮件/日历自动采集、WiFi 指纹、三端同步 |
| **Phase 2** | 11-14 | M3 | 向量检索、智能摘要、疲劳预警、TUI/Web 可视化 |
| **Phase 3** | 15-18 | M4 | CLI/TUI、MCP Server、外部 AI 接入、性能优化 |
| **Phase 4** | 19-20+ | M5 | 插件架构、团队模式、NuGet 发布、开源社区 |

---

## 十、部署与发布

### 10.1 Android

```bash
# 调试安装
dotnet build -t:Run -f net6.0-android

# 发布 APK
dotnet publish -f net6.0-android -c Release   -p:AndroidPackageFormat=apk   -p:AndroidKeyStore=true

# 发布 AAB (Google Play)
dotnet publish -f net6.0-android -c Release   -p:AndroidPackageFormat=aab
```

### 10.2 iOS

```bash
# 需要 Mac + Apple Developer 账号
dotnet build -t:Run -f net6.0-ios

# TestFlight 分发
dotnet publish -f net6.0-ios -c Release
# 使用 Transporter 或 xcrun altool 上传
```

### 10.3 配套服务部署 (Docker)

```yaml
# docker-compose.yml
version: '3.8'
services:
  photon:
    image: komoot/photon:latest
    ports:
      - "2322:2322"
    volumes:
      - ./photon_data:/photon/photon_data

  n8n:
    image: n8nio/n8n:latest
    ports:
      - "5678:5678"
    volumes:
      - ./n8n_data:/home/node/.n8n

  ollama:
    image: ollama/ollama:latest
    ports:
      - "11434:11434"
    volumes:
      - ./ollama_data:/root/.ollama
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
```

---

## 十一步行动

### 今天即可开始

1. **创建仓库**
   ```bash
   mkdir PITS && cd PITS
   dotnet new maui -n PITS.App
   dotnet new classlib -n PITS.Core
   dotnet new classlib -n PITS.Infrastructure
   dotnet new sln -n PITS
   dotnet sln add PITS.App/PITS.App.csproj
   dotnet sln add PITS.Core/PITS.Core.csproj
   dotnet sln add PITS.Infrastructure/PITS.Infrastructure.csproj
   ```

2. **添加核心依赖**
   ```bash
   cd PITS.Infrastructure
   dotnet add package Microsoft.EntityFrameworkCore.Sqlite
   dotnet add package Microsoft.EntityFrameworkCore.Sqlite.NetTopologySuite
   dotnet add package NetTopologySuite

   cd ../PITS.App
   dotnet add package CommunityToolkit.Mvvm
   dotnet add package CommunityToolkit.Maui
   dotnet add package Microsoft.SemanticKernel
   dotnet add package OllamaSharp
   ```

3. **实现第一个功能**
   - 在 `PITS.Core` 创建 `Trip.cs` 实体
   - 在 `PITS.Infrastructure` 创建 `TripContext.cs`
   - 在 `PITS.App` 创建 `RecordPage.xaml` + `RecordViewModel.cs`
   - 运行 → 打开 App → 获取 GPS → 保存行程

4. **启动配套服务**
   ```bash
   docker run -d -p 2322:2322 komoot/photon
   docker run -d -p 11434:11434 ollama/ollama
   ollama pull qwen2.5:14b
   ollama pull bge-large
   ```

---

*文档版本: v4.0 (Unified Edition)*
*生成日期: 2026-05-01*
*目标框架: .NET 6.0 / .NET MAUI*
*协议: 设计蓝图，CC0 1.0 Universal*
