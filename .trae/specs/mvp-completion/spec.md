# MVP 补齐计划 Spec

## Why

PITS MVP 项目当前完成度约 75-80%，核心功能已实现但存在 UI 资源缺失、已知 bug 和不完整的功能（后台定位、AI 集成等）。需要系统性地补齐这些缺失，确保项目可以成功编译运行。

## What Changes

### 必须修复 (MVP 完成阻断项)
- 添加 6 个 Tab Bar 图标文件 (record.png, calendar.png, map.png, place.png, ai.png, settings.png)
- 修复 SettingsPage ExportCsvAsyncCommand 命令绑定错误
- 统一解决方案结构（决定使用 mvp/src/ 还是根 src/）

### 高优先级 (功能完整)
- 实现后台定位服务 (Android ForegroundService / iOS LocationUpdates)
- 实现地理围栏触发器
- AI 对话功能集成 Ollama 本地 LLM

### 中优先级 (体验优化)
- 数据导出功能完善
- App Shell 样式优化
- 错误处理和加载状态

### 低优先级 (未来规划)
- 数据加密 (age 加密 Classified 级数据)
- 跨设备同步 (Syncthing P2P)
- CLI/TUI/API 完整实现

## Impact

- Affected specs: create-mvp-prototype, mvp-feature-verification
- Affected code:
  - mvp/src/PITS.MVP.App/ (UI 层)
  - mvp/src/PITS.MVP.App/ViewModels/ (ViewModel 层)
  - mvp/src/PITS.MVP.App/Resources/ (资源文件)
  - PITS.sln (解决方案结构)

## ADDED Requirements

### Requirement: 关键资源完整性
系统 SHALL 提供完整的 UI 资源，确保应用可以正常显示和运行。

#### Scenario: Tab Bar 图标
- **WHEN** 用户打开应用
- **THEN** 底部 Tab Bar 应显示 6 个带图标的 Tab (记录、日历、地图、地点、AI、设置)
- **AND** 每个 Tab 点击应正确导航到对应页面

#### Scenario: 解决方案构建
- **WHEN** 开发者执行 `dotnet build`
- **THEN** PITS.MVP.App 项目应成功编译无错误
- **AND** 所有依赖项应正确解析

### Requirement: 核心功能可用性
系统 SHALL 提供完整的后台定位和地理围栏功能。

#### Scenario: 后台定位
- **WHEN** 用户开启后台定位开关
- **THEN** 应用应在后台持续获取 GPS 位置
- **AND** 位置数据应自动记录为新行程

#### Scenario: 地理围栏
- **WHEN** 用户进入/离开已保存的地点
- **THEN** 系统应自动记录到达/离开事件
- **AND** 应触发相应的通知或自动记录

### Requirement: AI 功能集成
系统 SHALL 提供基于本地 LLM 的自然语言行程管理。

#### Scenario: 自然语言记录
- **WHEN** 用户说"记录今天下午3点到5点在公司开会"
- **THEN** AI 应解析意图并创建相应行程
- **AND** 返回确认消息给用户

#### Scenario: 行程统计查询
- **WHEN** 用户问"上周去了哪些地方"
- **THEN** AI 应查询数据库并返回行程列表

## MODIFIED Requirements

### Requirement: AIChatPage 功能增强
当前 AIChatPage 仅支持关键词匹配，未来版本应集成 Semantic Kernel + Ollama。

**Original**: 基于关键词匹配的自然语言解析
**New**: 基于本地 LLM 的意图解析和行程管理

## REMOVED Requirements

无移除的需求。

## 阶段划分

| 阶段 | 目标 | 优先级 |
|------|------|--------|
| Phase 1 | 阻断项修复 (图标、bug、构建) | 🔴 必须 |
| Phase 2 | 后台定位 + 地理围栏 | 🟡 高 |
| Phase 3 | AI LLM 集成 | 🟡 高 |
| Phase 4 | 体验优化 | 🟢 中 |
| Phase 5 | 高级功能 | ⚪ 低 |

## 风险与依赖

1. **图标资源** - 需要设计或获取 6 个 PNG 图标
2. **后台定位** - 需要处理 Android/iOS 权限和平台差异
3. **Ollama 集成** - 需要本地运行 Ollama 服务
