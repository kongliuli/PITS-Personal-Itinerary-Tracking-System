# PITS - Personal Itinerary Tracking System

个人行程追踪系统 - 一个以本地优先、隐私保护的时空数据管理平台。

## 项目简介

PITS 是一个面向个人的时空数据管理系统，以"**记录你在何时、何地、做什么**"为核心，通过权限分级和图层机制管理不同粒度的位置与行程信息，并支持 AI 自然语言交互。

## 核心特性

- **本地优先** - SQLite 单文件存储，数据主权归个人
- **隐私分级** - 四级权限控制（Public/Work/Private/Classified）
- **AI 赋能** - 自然语言解析意图，智能摘要与洞察
- **全平台覆盖** - 移动端、CLI、TUI、Web API 多端协同
- **跨设备同步** - Syncthing P2P 加密同步

## 技术架构

```
├── src/
│   ├── PITS.Core/           # 核心域模型与服务接口
│   ├── PITS.Infrastructure/ # EF Core + SQLite 数据层
│   ├── PITS.App/            # .NET MAUI 移动端应用
│   ├── PITS.CLI/             # 命令行工具
│   ├── PITS.TUI/             # 终端界面
│   ├── PITS.API/             # Web API
│   └── PITS.AI/              # AI 插件与 MCP 集成
├── tests/
└── docs/
```

## 快速开始

### 环境要求

- .NET 6.0 SDK+
- Android SDK / Xcode (iOS 开发)
- SQLite

### 构建项目

```bash
# 克隆项目
git clone https://github.com/your-repo/PITS.git
cd PITS

# 还原依赖
dotnet restore

# 构建解决方案
dotnet build
```

### 运行应用

```bash
# 运行 MAUI 应用
cd src/PITS.App
dotnet run

# 或构建发布版本
dotnet publish -c Release
```

## 文档

- [架构文档](docs/ARCHITECTURE.md) - 详细技术架构说明
- [API 文档](docs/API.md) - Web API 接口规范
- [MCP Schema](docs/MCP_SCHEMA.md) - AI Agent 接口定义

## 开发阶段

| 阶段 | 周期 | 目标 |
|------|------|------|
| MVP | 1-6 周 | MAUI App 基础功能：记录、日历、地图、AI 对话 |
| Phase 1 | 7-10 周 | 自动化采集：邮件解析、日历同步、WiFi 指纹 |
| Phase 2 | 11-14 周 | AI 深度：向量检索、智能摘要、疲劳预警 |
| Phase 3 | 15-18 周 | 全平台：CLI、TUI、MCP Server |
| Phase 4 | 19-20 周+ | 生态：插件市场、团队模式、开源发布 |

## 许可证

本项目采用 CC0 1.0 Universal 许可证。
