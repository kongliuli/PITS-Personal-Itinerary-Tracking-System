# MVP 原型开发规范

## Why

PITS 项目需要一个独立的 MVP 原型目录，用于快速验证核心技术架构和用户体验，降低主项目开发风险。MVP 将实现最小可行产品，验证 .NET MAUI 移动端开发、SQLite 数据存储、地图集成、AI 对话等核心功能。

## What Changes

- 创建独立的 `mvp/` 目录结构，与主项目解耦
- 实现完整的 .NET MAUI 移动端应用框架
- 实现核心数据模型和服务层
- 实现六大核心页面：记录、日历、地图、地点、AI 对话、设置
- 集成后台定位服务和地理围栏功能
- 集成 Semantic Kernel + Ollama AI 能力

## Impact

- Affected specs: 新增 MVP 原型目录，不影响主项目结构
- Affected code: 
  - `mvp/src/PITS.MVP.sln` - MVP 解决方案
  - `mvp/src/PITS.MVP.Core/` - 核心域模型
  - `mvp/src/PITS.MVP.Infrastructure/` - 基础设施层
  - `mvp/src/PITS.MVP.App/` - MAUI 移动端应用

## ADDED Requirements

### Requirement: MVP 项目结构

系统 SHALL 提供独立的 MVP 项目目录结构，包含完整的 .NET MAUI 应用框架。

#### Scenario: MVP 目录创建成功
- **WHEN** 开发者查看 `mvp/` 目录
- **THEN** 应看到完整的解决方案结构，包含 Core、Infrastructure、App 三个核心项目

### Requirement: 核心数据模型

系统 SHALL 提供完整的行程追踪数据模型，包括 Trip、Place、TrackPoint 实体。

#### Scenario: Trip 实体创建成功
- **WHEN** 用户创建新的行程记录
- **THEN** 系统应保存包含时间、位置、活动类型、描述等完整信息的 Trip 实体

#### Scenario: 地理位置编码正确
- **WHEN** 保存带有 GPS 坐标的行程
- **THEN** 系统应自动生成 GeoHash 编码

### Requirement: 快速记录功能

系统 SHALL 提供一键记录当前地点和活动的能力。

#### Scenario: GPS 定位记录成功
- **WHEN** 用户点击记录按钮
- **THEN** 系统应获取当前 GPS 位置，反向地理编码为地址，显示在界面上

#### Scenario: 活动类型选择
- **WHEN** 用户选择活动类型
- **THEN** 系统应提供工作、通勤、私人、出差、学习、健康等预设类型

### Requirement: 日历视图功能

系统 SHALL 提供月历视图展示行程记录。

#### Scenario: 月历显示行程标记
- **WHEN** 用户查看日历页面
- **THEN** 有行程的日期应显示彩色小点标记，不同活动类型对应不同颜色

#### Scenario: 日期详情查看
- **WHEN** 用户点击某个日期
- **THEN** 系统应显示该日期所有行程的详细列表

### Requirement: 地图轨迹功能

系统 SHALL 提供地图视图展示行程轨迹。

#### Scenario: 地图打点显示
- **WHEN** 用户查看地图页面
- **THEN** 系统应在地图上显示所有行程位置的标记点

#### Scenario: 轨迹连线显示
- **WHEN** 存在多个行程位置
- **THEN** 系统应按时间顺序连接各点形成轨迹线

### Requirement: AI 对话功能

系统 SHALL 提供自然语言交互能力，支持语音/文字输入创建和查询行程。

#### Scenario: 自然语言创建行程
- **WHEN** 用户输入 "记录今天下午3点到5点在公司开会"
- **THEN** 系统应解析意图，生成行程确认卡片

#### Scenario: 行程确认流程
- **WHEN** AI 解析出行程信息
- **THEN** 系统应显示确认卡片，用户确认后保存行程

### Requirement: 后台定位服务

系统 SHALL 支持后台持续定位和地理围栏触发。

#### Scenario: 后台定位记录
- **WHEN** 用户开启后台定位
- **THEN** 系统应每 5 分钟或移动 500 米记录一次位置

#### Scenario: 地理围栏触发
- **WHEN** 用户进入已保存地点 200 米范围内
- **THEN** 系统应发送通知询问是否记录行程

### Requirement: 数据持久化

系统 SHALL 使用 SQLite 本地数据库存储所有数据。

#### Scenario: 数据库初始化
- **WHEN** 应用首次启动
- **THEN** 系统应创建 SQLite 数据库文件和必要的表结构

#### Scenario: 数据加密存储
- **WHEN** 保存 Classified 级别的行程
- **THEN** 系统应对敏感字段进行加密后存储

## MODIFIED Requirements

无修改的需求。

## REMOVED Requirements

无移除的需求。
