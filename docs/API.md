# PITS API 文档

## 概述

PITS Web API 基于 ASP.NET Core Minimal API 构建，提供设备间 HTTP 通信和外部 AI 接入能力。默认仅绑定 `localhost`，不暴露到外网。

## 基础信息

- **基础 URL**: `http://localhost:5728`
- **内容类型**: `application/json`
- **字符编码**: UTF-8

## 认证

当前版本使用本地 API，暂不需认证。未来版本将支持 API Key 认证。

## 端点列表

### 行程 (Trips)

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/trips` | 获取行程列表 |
| GET | `/api/trips/{id}` | 获取单个行程 |
| POST | `/api/trips` | 创建行程 |
| PUT | `/api/trips/{id}` | 更新行程 |
| DELETE | `/api/trips/{id}` | 删除行程 |
| GET | `/api/trips/search` | 搜索行程 |

### 地点 (Places)

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/places` | 获取地点列表 |
| GET | `/api/places/{id}` | 获取单个地点 |
| POST | `/api/places` | 创建地点 |
| PUT | `/api/places/{id}` | 更新地点 |
| DELETE | `/api/places/{id}` | 删除地点 |
| GET | `/api/places/nearby` | 获取附近地点 |

### 统计 (Statistics)

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/stats/summary` | 获取统计摘要 |
| GET | `/api/stats/heatmap` | 获取热力图数据 |

### 导出 (Export)

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/export/geojson` | 导出 GeoJSON |
| GET | `/api/export/csv` | 导出 CSV |
| GET | `/api/export/markdown` | 导出 Markdown |

## API 详细说明

### 行程接口

#### GET /api/trips

获取行程列表。

**查询参数**：

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| startDate | date | 否 | 开始日期 |
| endDate | date | 否 | 结束日期 |
| activityType | string | 否 | 活动类型 |
| visibility | int | 否 | 可见性级别 |
| page | int | 否 | 页码 (默认 1) |
| pageSize | int | 否 | 每页数量 (默认 20) |

**响应示例**：

```json
{
  "items": [
    {
      "id": "01AR5Z1N7VX2V0V0V0V0V0V0V",
      "startedAt": "2026-05-01T09:00:00Z",
      "endedAt": "2026-05-01T12:00:00Z",
      "location": {
        "latitude": 31.2304,
        "longitude": 121.4737
      },
      "address": "上海市黄浦区人民广场",
      "activityType": "Work",
      "description": "产品评审会议",
      "visibility": "Private",
      "source": "Manual",
      "createdAt": "2026-05-01T09:05:00Z"
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20
}
```

#### POST /api/trips

创建行程。

**请求体**：

```json
{
  "startedAt": "2026-05-01T09:00:00Z",
  "endedAt": "2026-05-01T12:00:00Z",
  "latitude": 31.2304,
  "longitude": 121.4737,
  "activityType": "Work",
  "description": "产品评审会议",
  "visibility": "Private",
  "tags": ["会议", "产品"]
}
```

**响应**：返回创建的行程对象，HTTP 201 Created。

#### GET /api/trips/search

全文搜索行程。

**查询参数**：

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| q | string | 是 | 搜索关键词 |
| maxVisibility | int | 否 | 最大可见性级别 |

### 地点接口

#### GET /api/places/nearby

获取附近地点。

**查询参数**：

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| latitude | double | 是 | 纬度 |
| longitude | double | 是 | 经度 |
| radius | int | 否 | 半径(米)，默认 1000 |

### 统计接口

#### GET /api/stats/summary

获取统计摘要。

**查询参数**：

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| startDate | date | 是 | 开始日期 |
| endDate | date | 是 | 结束日期 |

**响应示例**：

```json
{
  "totalTrips": 45,
  "totalHours": 120.5,
  "byActivityType": {
    "Work": { "count": 25, "hours": 80 },
    "Commute": { "count": 10, "hours": 5 },
    "Personal": { "count": 10, "hours": 35.5 }
  },
  "topPlaces": [
    { "name": "公司", "count": 20 },
    { "name": "家", "count": 15 }
  ]
}
```

### 导出接口

#### GET /api/export/geojson

导出 GeoJSON 格式。

**查询参数**：

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| startDate | date | 否 | 开始日期 |
| endDate | date | 否 | 结束日期 |
| visibility | int | 否 | 可见性级别 |
| includeTrackPoints | bool | 否 | 包含轨迹点 |

## 错误响应

错误响应格式：

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid date range",
    "details": [
      {
        "field": "startDate",
        "message": "Start date must be before end date"
      }
    ]
  }
}
```

**错误代码**：

| 代码 | HTTP 状态 | 说明 |
|------|-----------|------|
| VALIDATION_ERROR | 400 | 参数验证错误 |
| NOT_FOUND | 404 | 资源不存在 |
| UNAUTHORIZED | 401 | 未授权 |
| INTERNAL_ERROR | 500 | 服务器内部错误 |

## SignalR 实时通知

支持通过 SignalR 接收实时更新：

- 连接地址: `/hubs/trips`
- 事件: `TripCreated`, `TripUpdated`, `TripDeleted`
