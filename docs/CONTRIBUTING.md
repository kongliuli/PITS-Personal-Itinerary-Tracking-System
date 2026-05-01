# PITS 项目贡献指南

## 开发环境设置

### 必要条件

- .NET 6.0 SDK 或更高版本
- Git
- Visual Studio 2022 / VS Code / Rider

### 克隆与构建

```bash
git clone https://github.com/your-repo/PITS.git
cd PITS
dotnet restore
dotnet build
```

### 运行测试

```bash
dotnet test
```

## 代码规范

### 命名规范

- 类名: PascalCase (如 `TripService`)
- 方法名: PascalCase (如 `GetTripById`)
- 变量名: camelCase (如 `tripList`)
- 常量: PascalCase (如 `DefaultTimeZone`)
- 枚举值: PascalCase (如 `ActivityType.Work`)

### 代码风格

- 使用 4 空格缩进
- 表达式主体优先（当逻辑简单时）
- 避免不必要的 `this.` 引用
- 使用空行分隔逻辑块

### 文档注释

- 公开 API 必须添加 XML 文档注释
- 使用 `<summary>`, `<param>`, `<returns>` 等标签

## 分支管理

### 分支命名

- 功能分支: `feature/功能名称`
- 修复分支: `fix/问题描述`
- 文档分支: `docs/文档更新`

### 提交规范

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

**Type 类型**：

- `feat`: 新功能
- `fix`: 修复 bug
- `docs`: 文档更新
- `style`: 代码格式（不影响功能）
- `refactor`: 重构
- `test`: 测试
- `chore`: 构建或辅助工具

## 项目结构说明

```
src/
├── PITS.Core/           # 核心域模型，不依赖其他项目
├── PITS.Infrastructure/ # 数据访问层，依赖 Core
├── PITS.App/            # MAUI 应用，依赖 Core 和 Infrastructure
├── PITS.CLI/            # 命令行工具
├── PITS.TUI/            # 终端界面
├── PITS.API/            # Web API
└── PITS.AI/             # AI 集成
```

## 测试策略

- 单元测试: `PITS.Core.Tests`
- 集成测试: `PITS.Integration.Tests`
- 所有公开方法应编写单元测试
- 测试覆盖率目标: 80%+

## 许可证

本项目采用 CC0 1.0 Universal 许可证。
