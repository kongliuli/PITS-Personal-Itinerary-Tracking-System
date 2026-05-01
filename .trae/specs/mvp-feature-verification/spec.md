# MVP 功能验证与集成开发规范

## Why

PITS MVP 项目需要逐步验证各个核心功能的可行性，包括数据存储、地理位置、AI 集成等关键技术点。通过建立独立的测试项目和渐进式集成，确保最终交付一个稳定可用的 MAUI 应用。

## What Changes

- 在 `mvp/tests/` 目录下建立分功能测试项目
- 按优先级逐步验证和实现核心功能
- 创建独立的 POC 项目验证技术选型
- 最终集成到完整的 MAUI MVP 应用中
- 提供可运行的 Android APK 进行实际测试

## Impact

- Affected specs: `create-mvp-prototype` (MVP 原型开发)
- Affected code:
  - `mvp/tests/` - 单元测试和集成测试
  - `mvp/poc/` - 技术验证 POC 项目
  - `mvp/src/PITS.MVP.App/` - 最终集成应用

## ADDED Requirements

### Requirement: 核心功能验证

系统 SHALL 提供独立的功能验证流程，确保每个核心模块在集成前经过充分测试。

#### Scenario: 数据存储验证
- **WHEN** 开发者运行数据存储测试
- **THEN** SQLite 数据库应能正常创建、读写、迁移
- **AND** 空间数据 (Point) 应能正确存储和查询

#### Scenario: 地理位置验证
- **WHEN** 开发者运行地理位置测试
- **THEN** GPS 定位应能在 Android/iOS 上正常工作
- **AND** 反向地理编码应能返回有效地址
- **AND** GeoHash 编码/解码应准确

#### Scenario: 地图集成验证
- **WHEN** 开发者运行地图测试
- **THEN** MAUI Maps 控件应能正常显示
- **AND** 标记点和轨迹线应能正确渲染

#### Scenario: AI 功能验证
- **WHEN** 开发者运行 AI 功能测试
- **THEN** Ollama 本地推理应能正常连接
- **AND** 语义 Kernel 插件应能正确解析意图

### Requirement: 测试项目结构

系统 SHALL 提供完整的测试项目结构，覆盖核心功能。

#### Scenario: 单元测试覆盖
- **WHEN** 运行单元测试
- **THEN** 核心业务逻辑应有 80%+ 覆盖率
- **AND** 所有测试应通过

#### Scenario: 集成测试覆盖
- **WHEN** 运行集成测试
- **THEN** 数据库操作应有完整的集成测试
- **AND** API 端点应有端到端测试

### Requirement: 最终集成应用

系统 SHALL 提供一个完整的、可运行的 MAUI MVP 应用。

#### Scenario: 应用可构建
- **WHEN** 执行 `dotnet build` 命令
- **THEN** 项目应能成功编译无错误

#### Scenario: 应用可运行
- **WHEN** 在 Android 设备/模拟器上安装 APK
- **THEN** 应用应能正常启动
- **AND** 基础功能（记录行程）应能正常工作

## MODIFIED Requirements

无修改的需求。

## REMOVED Requirements

无移除的需求。
