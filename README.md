# PITS - Personal Itinerary Tracking System

PITS 是一个本地优先、隐私优先的个人行程追踪系统。当前分支收口的是
.NET MAUI MVP：记录行程、查看日历和地图、管理地点、导入数据、查看统计，
以及一个轻量 AI 查询助手。

## 当前入口

```
PITS.sln                 # PR/CI 验证入口
mvp/PITS.MVP.sln         # MVP 本地开发入口
mvp/src/PITS.MVP.Core    # 领域实体、值对象、服务接口
mvp/src/PITS.MVP.Infrastructure
                         # SQLite、EF Core、位置/统计/导入服务
mvp/src/PITS.MVP.App     # .NET MAUI App
mvp/tests                # MVP 单元测试
mvp/poc                  # 可选 POC，不作为产品入口
mvp-art, svp-art         # 展示资产
```

根目录 `src/` 和 `tests/` 保留为后续多端蓝图/占位，不是当前 MVP 验证路径。

## 环境要求

- .NET 10 SDK
- .NET MAUI workload
- Windows 目标构建可直接验证
- Android/iOS 构建需要本机安装对应 SDK

## 验证命令

```bash
dotnet restore PITS.sln
dotnet build mvp/src/PITS.MVP.App/PITS.MVP.App.csproj -f net10.0-windows10.0.19041.0 --no-restore
dotnet test mvp/tests/PITS.MVP.Core.Tests/PITS.MVP.Core.Tests.csproj --no-restore
dotnet test mvp/tests/PITS.MVP.Infrastructure.Tests/PITS.MVP.Infrastructure.Tests.csproj --no-restore
dotnet list PITS.sln package --vulnerable --include-transitive
```

`dotnet build PITS.sln --no-restore` 会同时构建 Android 目标；如果本机没有
Android SDK，会在 MAUI Android 项目上失败。

## MVP 功能

- 本地 SQLite 存储和 NetTopologySuite 空间数据
- 行程、地点、轨迹点、跟踪配置、提醒等核心实体
- 手动记录、日历、地图、地点、导入、统计、设置页面
- 交通方式识别、行程分段、地点聚类、统计服务
- 轻量 AI 查询助手：统计周期、最近行程、记录意图提示
- MQTT 位置发布服务接口和实现

## 文档

- [MVP 架构](mvp/docs/ARCHITECTURE.md)
- [MVP 数据模型](mvp/docs/DATA_MODEL.md)
- [PR 收口设计](docs/MVP_PR_READINESS.md)
- [全案蓝图](Dosc/PITS-全案蓝图-统一版.md)

## 许可证

本项目采用 CC0 1.0 Universal 许可证。
