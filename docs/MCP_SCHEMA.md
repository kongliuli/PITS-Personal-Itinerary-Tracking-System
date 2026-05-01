# PITS MCP Schema

## 概述

PITS MCP (Model Context Protocol) Server 提供标准化的 AI Agent 接口，允许外部 AI 系统（如 Claude Desktop、Cline、Continue）通过 MCP 协议调用 PITS 功能。

## 连接信息

```json
{
  "name": "pits",
  "description": "Personal Itinerary Tracking System - Access and manage your personal location and schedule data",
  "version": "1.0.0"
}
```

## 工具定义

### trip_create

创建新的行程记录。

**参数**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| time_expression | string | 是 | 时间表达式，如 "今天下午3点到5点" |
| location_hint | string | 否 | 地点描述 |
| activity_type | enum | 否 | 活动类型: work, commute, personal, health, travel, study, entertainment, other |
| description | string | 否 | 行程描述 |
| visibility | enum | 否 | 可见性: public, work, private, classified |
| tags | string[] | 否 | 标签列表 |

**返回**：

```json
{
  "success": true,
  "trip": {
    "id": "01AR5Z1N7VX2V0V0V0V0V0V0V",
    "startedAt": "2026-05-01T15:00:00Z",
    "endedAt": "2026-05-01T17:00:00Z",
    "activityType": "Work",
    "description": "客户会议",
    "visibility": "Private"
  }
}
```

### trip_query

查询行程记录。

**参数**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| time_range | string | 否 | 时间范围，如 "上周"、"本月" |
| geo_bounds | object | 否 | 地理边界 |
| activity_filter | string[] | 否 | 活动类型过滤 |
| visibility_layer | enum | 否 | 可见性图层 |
| output_format | enum | 否 | 输出格式: json, text, summary |
| limit | int | 否 | 返回数量限制 (默认 50) |

**geo_bounds 对象**：

```json
{
  "north": 31.3,
  "south": 31.2,
  "east": 121.5,
  "west": 121.4
}
```

**返回**：

```json
{
  "trips": [...],
  "totalCount": 100,
  "query": {
    "timeRange": "last_week",
    "activityFilter": ["work"],
    "visibility": "private"
  }
}
```

### trip_summarize

生成行程摘要。

**参数**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| period | string | 是 | 时间段，如 "上周"、"本月" |
| group_by | enum | 否 | 分组方式: day, activity_type, location |
| include_stats | boolean | 否 | 是否包含统计信息 |

**返回**：

```json
{
  "period": "2026-04-01 to 2026-04-30",
  "summary": "本月共记录 45 次行程，总计 120 小时。其中工作行程 25 次（80 小时），私人行程 20 次（40 小时）。",
  "stats": {
    "totalTrips": 45,
    "totalHours": 120,
    "topActivity": "Work",
    "topLocation": "公司"
  },
  "breakdown": {
    "byActivityType": {...},
    "byDay": {...}
  }
}
```

### trip_export

导出行程数据。

**参数**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| layer | string | 否 | 可见性图层 |
| format | enum | 是 | 导出格式: geojson, csv, markdown |
| date_range | string | 否 | 日期范围 |
| include_attachments | boolean | 否 | 是否包含附件数据 |

**返回**：

导出文件内容或下载链接。

### trip_delete

删除行程记录。

**参数**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | string | 是 | 行程 ID |
| confirm | boolean | 是 | 确认删除 |

### place_create

创建或更新地点。

**参数**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 是 | 地点名称 |
| latitude | number | 是 | 纬度 |
| longitude | number | 是 | 经度 |
| category | enum | 否 | 地点类别 |
| metadata | object | 否 | 元数据 |

### place_query

查询地点。

**参数**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 否 | 地点名称（模糊搜索） |
| category | enum | 否 | 地点类别 |
| near | object | 否 | 附近位置 |
| radius | number | 否 | 搜索半径（米） |

**near 对象**：

```json
{
  "latitude": 31.2304,
  "longitude": 121.4737
}
```

## 资源定义

### trip:///{id}

获取单个行程的完整信息。

### place:///{id}

获取单个地点的详细信息。

### stats://summary

获取当前统计摘要。

## 提示模板

### 行程记录

```
# trip_record_prompt
你是一个行程记录助手。当用户提供行程相关信息时，帮助他们记录行程。
支持的格式：
- "记录今天下午3点在公司的会议"
- "添加一个从家到公司的通勤记录"
- "记录一下上周去北京出差的情况"
```

### 行程查询

```
# trip_query_prompt
你是一个行程查询助手。帮助用户查找和分析行程记录。
支持的查询：
- "上周去了哪些地方？"
- "这个月工作了多少小时？"
- "找出所有和客户相关的行程"
```

### 智能摘要

```
# trip_summary_prompt
你是一个行程分析助手。生成行程报告和洞察。
可以提供：
- 日/周/月报
- 时间分配分析
- 地点热力图
- 通勤优化建议
```

## 错误处理

所有工具调用可能返回以下错误：

```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Trip with id xxx not found"
  }
}
```

**错误代码**：

| 代码 | 说明 |
|------|------|
| INVALID_PARAMS | 参数无效 |
| NOT_FOUND | 资源不存在 |
| PERMISSION_DENIED | 权限不足 |
| DATABASE_ERROR | 数据库错误 |
| NETWORK_ERROR | 网络错误 |
