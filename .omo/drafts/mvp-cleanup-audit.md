# PITS MVP 清理、未完成功能 & AI 方案调研

> 调研日期: 2026-07-10
> 状态: ✅ 已审批通过

---

## 审批记录

用户于 2026-07-10 确认：
1. ✅ **删除** 根 `src/` + `tests/` 的 8 个空壳项目
2. ✅ **删除并 .gitignore** `dist/` 发布产物
3. ✅ **保留** 12 张 `pits-ui-*.png` UI 截图
4. ✅ **接受** "ONNX 本地主推 + API 云端备用" 混合 AI 策略
5. ✅ **优先执行** 目录清理任务

---

## 一、目录清理建议

### 1.1 安全可清理（无实际代码贡献，仅为占位/历史遗留）

| 目录/文件 | 内容 | 建议 |
|-----------|------|------|
| `src/PITS.Core/` | 仅含 `Class1.cs`（空类），target net8.0 | **删除项目 + 目录** — MVP 的 Core 在 `mvp/src/PITS.MVP.Core` |
| `src/PITS.Infrastructure/` | 仅含 `Class1.cs`（空类），target net8.0 | **删除项目 + 目录** — MVP 的 Infrastructure 在 `mvp/src/PITS.MVP.Infrastructure` |
| `src/PITS.AI/` | 仅含 `Class1.cs` + 引用了 SemanticKernel + OllamaSharp，target net8.0 | **删除项目 + 目录** — AI POC 在 `mvp/poc/PITS.POC.AI`，且不依赖 SK |
| `src/PITS.API/` | 仅含 `var builder = WebApplication.CreateBuilder(args); app.Run();` | **删除项目 + 目录** — API 功能尚未开发，属于 Phase 2 范围 |
| `src/PITS.CLI/` | 仅含 `Console.WriteLine("PITS CLI - Coming Soon");` | **删除项目 + 目录** — CLI 属于 Phase 3 范围 |
| `src/PITS.TUI/` | 仅含 `Console.WriteLine("PITS TUI - Coming Soon");` | **删除项目 + 目录** — TUI 属于 Phase 3 范围 |
| `tests/PITS.Core.Tests/` | 仅含 `UnitTest1.cs` 空壳 | **删除项目 + 目录** — 真正的测试在 `mvp/tests/` |
| `tests/PITS.Integration.Tests/` | 仅含 `UnitTest1.cs` 空壳 | **删除项目 + 目录** — 真正的测试在 `mvp/tests/` |
| `Dosc/` | 仅含 `Readme.md` 内容为 "123"；但有对应蓝图文档 `PITS-全案蓝图-统一版.md` | **保留蓝图文档，删除空壳 Readme.md**，考虑将蓝图文档移至 `docs/` |
| `.uploads/` | 空目录 | **删除** |
| `.agents/` | 空目录 | **删除** |
| `dist/` | 包含 `PITS-MVP-20260702.zip` 和展开的发布文件 | ✅ 已确认删除，加入 .gitignore |

### 1.2 已确认保留

| 目录/文件 | 说明 |
|-----------|------|
| `mvp/poc/` | 4 个 POC 子项目（AI/Geolocation/Maps/Storage），均被 PITS.sln 引用 |
| `.trae/` | Trae IDE 的 5 组 spec 文件 |
| `mvp-art/`, `svp-art/` | 设计文档 + 测试脚本 |
| `.vs/` | Visual Studio 用户设置 |
| `.codex/`, `.codegraph/` | Agent 配置/索引 |
| `pits-ui-*.png` (12张) | UI 设计截图，已确认保留 |

### 1.3 清理工作要点

1. **先删除根 `src/` 的 6 个空壳项目** — 它们都不在 PITS.sln 中引用（PITS.sln 只引用 mvp 下的项目）
2. **删除根 `tests/` 的 2 个空壳项目** — 不在 PITS.sln 中
3. **删除 `.uploads/` 和 `.agents/`** 空目录
4. **修复 `Dosc/`** — 将蓝图文档移到 `docs/`，删除空 Readme
5. **删除 `dist/` 并加入 .gitignore**
6. **保留根目录的 png 文件**

---

## 二、未完成功能梳理

### 2.1 MVP 计划功能 vs 实际实现

#### ✅ 已实现的 MVP 核心功能

| 功能 | 说明 | 验证路径 |
|------|------|----------|
| 核心实体 | Trip, Place, TrackPoint, Enums | `mvp/src/PITS.MVP.Core/Entities/` |
| 扩展实体 | TripPlan, TrackingProfile, ImportStagingItem | `mvp/src/PITS.MVP.Core/Entities/` |
| 值对象 | GeoHash, TimeRange, BoundingBox | 代码中引用 |
| 服务接口 | 18 个服务接口 | `mvp/src/PITS.MVP.Core/Services/` |
| 服务实现 | 17 个服务实现（含 TripService, PlaceService, StatsService, GeocodingService 等） | `mvp/src/PITS.MVP.Infrastructure/Services/` |
| EF Core DbContext | TripContext 含完整 Fluent API 配置 | `mvp/src/PITS.MVP.Infrastructure/Data/` |
| 9 个 MAUI 页面 | Record, Calendar, Map, Place, AI Chat, Stats, Settings, Import, More | `mvp/src/PITS.MVP.App/Views/` |
| 9 个 ViewModel | 对应上述页面 | `mvp/src/PITS.MVP.App/ViewModels/` |
| 单元测试 | Core Tests + Infrastructure Tests（含 TransportModeDetector 等） | `mvp/tests/` |
| AI 对话 | 基于规则的关键词匹配（无 LLM 依赖） | `AIChatViewModel.cs` |
| MQTT 位置发布 | `MqttLocationPublisher` | Infrastructure 层 |
| 隐私导出 | `PrivacyExportService` | Infrastructure 层 |
| 备份服务 | `BackupService` | Infrastructure 层 |
| 交通方式识别 | `TransportModeDetector` + 测试 | Infrastructure 层 |
| 行程分段分析 | `TripSegmentAnalyzer` | Infrastructure 层 |
| 地点聚类 | `PlaceClusterService` | Infrastructure 层 |
| 日历+黄历服务 | `AlmanacService` | Infrastructure 层 |
| 照片服务 | `PhotoService` | Infrastructure 层 |

#### ⚠️ 已实现但有缺口的功能

| 功能 | 现状 | 缺少什么 |
|------|------|----------|
| **AI 对话** | 基础规则引擎，仅支持数种固定模式（周/月统计、最近查询、创建计划） | 缺少 LLM 集成；自然语言理解能力非常有限；无 Semantic Kernel 集成 |
| **地图页** | View + ViewModel 存在，加载标记和轨迹线 | 需要验证实际地图渲染效果；缺少图层过滤联动 |
| **日历页** | View + ViewModel 存在，月历视图 | 缺少计划 vs 实际行程对比显示 |
| **后台定位** | 接口和 Android/iOS 平台代码就绪 | `AndroidLocationTrackingService` 在 `#if ANDROID` 条件编译中，实际运行需验证；缺少地理围栏的生产级实现 |
| **导入功能** | View + ViewModel + ImportService | 需要验证导入格式和用户确认流程 |
| **统计页** | StatsViewModel 就绪 | 需要验证图表呈现 |
| **地点管理** | PlaceViewModel 就绪 | 地理围栏设置 UI |
| **设置页** | SettingsViewModel 就绪 | 导出功能 UI 集成 |

#### ❌ 文档计划但尚未实现

参照 `Dosc/PITS-全案蓝图-统一版.md` 和 `mvp/docs/ARCHITECTURE.md`：

| 功能 | 计划阶段 | 状态 | 说明 |
|------|---------|------|------|
| **FTS5 全文搜索** | MVP/Phase 1 | ❌ 未实现 | 文档中有完整 SQL，但代码中未发现对应 Migration 或 Service |
| **Semantic Kernel 集成** | MVP（W4）/Phase 1 | ❌ 未集成 | POC 中有演示代码，但 MVP App 未引入 SK 依赖 |
| **Ollama 本地 LLM** | MVP（W4）/Phase 1 | ❌ 未实现 | POC 中有代码，App 未使用 |
| **Syncthing 同步** | MVP（W6）/Phase 2 | ❌ 未实现 | 仅文档提及 |
| **本地 Web API** | Phase 1 | ❌ 未实现 | `src/PITS.API/` 仅为空壳 |
| **邮件自动解析** | Phase 1 | ❌ 未实现 | 远期功能 |
| **日历双向同步** | Phase 1 | ❌ 未实现 | 远期功能 |
| **WiFi 指纹匹配** | Phase 1 | ❌ 未实现 | 远期功能 |
| **向量检索** | Phase 2 | ❌ 未实现 | 远期功能 |
| **MCP Server** | Phase 3 | ❌ 未实现 | 远期功能 |
| **CLI/TUI** | Phase 3 | ❌ 未实现 | `src/PITS.CLI/` 和 `src/PITS.TUI/` 为空壳 |

### 2.2 MVP 当前真正缺失的关键功能（建议优先补齐）

按优先级排列：

| 优先级 | 功能 | 原因 |
|--------|------|------|
| **P0** | **AI 依赖升级（当前焦点）** | 用户明确要求调研 |
| P1 | 全功能 RecordPage + 后台定位验证 | 核心体验闭环 |
| P1 | FTS5 全文搜索 | 搜索是 MVP 核心功能之一，SQL 已写好但未部署 |
| P2 | 导入导出验证 | 数据互通能力 |
| P2 | 隐私导出/备份 UI 联动 | 安全合规 |
| P3 | 日历-计划联动 | 规划 vs 实际对比 |
| P3 | 统计图表 | 数据可视化 |

---

## 三、ONNX vs API 调研报告

### 3.1 核心判断

**建议采用混合策略：ONNX Runtime GenAI（本地）为主 + API（云端）为辅**

### 3.2 方案对比总表

| 维度 | ONNX Runtime GenAI | API (Ollama/OpenAI) |
|------|-------------------|---------------------|
| **运行方式** | 应用进程内直接推理 | 外部 HTTP 请求（本地或云端） |
| **冷启动** | ~1-2s（加载模型） | ~3-5s（Ollama）/ 无冷启动（OpenAI） |
| **首 Token 延迟** | ~30-50ms | ~50-100ms（Ollama）/ ~300-800ms（OpenAI API） |
| **离线能力** | ✅ 完全离线 | ❌ 需要网络 |
| **隐私性** | ✅ 数据不离设备 | ⚠️ 取决于 API 商 |
| **每次调用成本** | ¥0（电费/电池） | ¥0（Ollama 本地）/ 按 token 计费（云 API） |
| **模型大小** | ~2GB（Phi-3 INT4） | ~2-4GB（Ollama）/ 无本地存储（云 API） |
| **移动端支持** | ✅ 支持 Android/iOS（需验证） | ❌ Ollama 非移动原生；云 API 需网络 |
| **MAUI 集成** | NuGet: `Microsoft.ML.OnnxRuntimeGenAI` | NuGet: `Microsoft.Extensions.AI.Ollama` / `Azure.AI.OpenAI` |
| **模型质量** | Phi-3/Phi-4 小模型（3.8B） | 可选任何模型（Ollama）/ 前沿模型（云 API） |
| **GPU 加速** | DirectML / CoreML / CUDA | Ollama: Metal/CUDA；云 API: 服务端 |
| **内存占用** | ~2-4GB（模型加载后） | Ollama 驻留额外进程 / 云 API 无本地占用 |
| **每秒 Token** | CPU: ~10 tok/s, GPU: 80-150 tok/s | Ollama GPU: 45-60 tok/s / 云 API 取决于后端 |
| **部署复杂度** | ⭐⭐⭐ 较高：需下载模型、管理路径 | ⭐ 简单：`ollama pull` 或一行 API key |
| **功能完整性** | 基础生成（function calling 有限） | OpenAI 兼容（完整 function calling） |

### 3.3 推荐方案：ONNX Runtime GenAI（主） + API 备用

```
用户输入 → [本地 ONNX 推理] → 成功 → 返回结果
                        ↓ 失败/超纲
                   [云端 API 回退] → 返回结果
```

#### 为什么 ONNX 更适合 PITS MVP

1. **原生移动端支持** — ONNX Runtime GenAI 0.5.1+ 已支持 Android/iOS，作为 MAUI App 这是关键优势
2. **零网络依赖** — PITS 的隐私优先理念与本地推理完全一致，数据不离设备
3. **低延迟** — 进程内推理，无 HTTP 往返，首 token 30-50ms
4. **零 API 成本** — 适合 MVP 阶段无需为每次 AI 调用付费
5. **Microsoft 生态** — Phi-3/Phi-4 小模型以 ONNX 格式原生发布，与 .NET 生态完美契合

#### 推荐的模型选择

| 场景 | 推荐模型 | 大小 | 硬件要求 |
|------|---------|------|---------|
| MVP 核心推理 | **Phi-3-mini-4k-instruct (INT4)** | ~2GB | 4GB RAM，CPU 可运行 |
| 升级选项 | **Phi-4-mini (3.8B, INT4)** | ~2.5GB | 4GB VRAM 或 CPU |
| 云端备用 | **GPT-4o-mini** 或 **DeepSeek** | - | 需网络 |

#### 需要注意的风险

1. **模型大小**：~2GB 的模型在移动端下载和存储需要用户接受
2. **CPU 推理速度**：手机上约 5-10 tok/s，对实时对话偏慢
3. **Android 兼容性**：需要验证 `libonnxruntime-genai.so` 在目标 Android 版本的兼容性（GitHub issue #1081 显示有库加载问题）
4. **功能限制**：Phi 系列 model 的 function calling 支持有限
5. **电池消耗**：持续推理会显著增加设备发热和耗电

### 3.4 替代方案的取舍

| 方案 | 适用场景 | 不适用场景 |
|------|---------|-----------|
| **Ollama** | 开发测试、桌面端、服务端 | 移动端 App（需后台常驻进程） |
| **OpenAI API** | 高复杂度任务、需要最新模型 | 离线场景、隐私敏感场景、高频小调用 |
| **LLamaSharp** | 桌面 App，GGUF 模型 | 性能低于 ONNX，移动端支持待验证 |
| **Azure Foundry Local** | 已使用 Azure 生态的团队 | 引入额外 Azure 依赖，不适合 MVP |

### 3.5 实施建议

**MVP 阶段建议**：先采用**轻量规则引擎（已实现）+ OpenAI API（按需调用）**，ONNX 集成作为 Phase 1 重点推进。

原因：
1. 规则引擎已经覆盖了查询统计、创建计划等简单场景
2. ONNX 模型 ~2GB 下载在 MVP 阶段增加用户门槛
3. API 方案可以逐步引入，先验证用户对 AI 功能的需求
4. 架构上使用 `IChatClient` 抽象，未来切换到 ONNX 无需改 ViewModel

**架构设计方案**：
```
IChatClient（抽象接口）
├── LocalRuleChatClient（现有规则引擎，默认）
├── OnnxChatClient（本地 ONNX 推理，Phase 1）
└── OpenAiChatClient（云端 API，可配置备用）
```
