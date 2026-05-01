# Tasks

## Phase 1: 测试项目结构搭建

- [x] Task 1.1: 创建测试项目目录结构
  - [x] 创建 mvp/tests/ 目录
  - [x] 创建 mvp/poc/ 目录
  - [x] 配置 xUnit 测试框架

## Phase 2: 数据存储功能验证 (POC + 测试)

- [x] Task 2.1: SQLite 数据库基础功能验证
  - [x] 创建数据库 POC 项目
  - [x] 验证 CRUD 操作
  - [x] 验证事务处理

- [x] Task 2.2: EF Core + SQLite 集成测试
  - [x] 配置 EF Core 上下文
  - [x] 验证数据库迁移
  - [x] 验证数据持久化

- [x] Task 2.3: 空间数据 (NetTopologySuite) 测试
  - [x] Point 类型存储测试
  - [x] GeoHash 编码/解码测试
  - [x] 空间查询测试

## Phase 3: 地理位置功能验证 (POC + 测试)

- [x] Task 3.1: GPS 定位功能验证
  - [x] 创建定位 POC 项目
  - [x] 验证 MAUI Geolocation API
  - [x] 测试 Android/iOS 定位权限

- [x] Task 3.2: 反向地理编码测试
  - [x] 集成 Nominatim API
  - [x] 验证地址解析
  - [x] 测试离线缓存

- [x] Task 3.3: GeoHash 功能测试
  - [x] 编码精度测试
  - [x] 解码准确性测试
  - [x] 边界情况测试

## Phase 4: 地图集成功能验证 (POC + 测试)

- [x] Task 4.1: MAUI Maps 基础测试
  - [x] 创建地图 POC 项目
  - [x] 验证地图显示
  - [x] 测试缩放和平移

- [x] Task 4.2: 地图标记点功能测试
  - [x] Pin 添加和移除
  - [x] 点击事件响应
  - [x] 自定义标记样式

- [x] Task 4.3: 轨迹线和多边形测试
  - [x] Polyline 绘制
  - [x] Polygon 绘制
  - [x] 动态更新

## Phase 5: 用户界面功能验证 (POC + 测试)

- [x] Task 5.1: MVVM 架构测试
  - [x] ViewModel 绑定测试
  - [x] 命令执行测试
  - [x] 属性通知测试

- [x] Task 5.2: 页面导航测试
  - [x] Shell 导航
  - [x] 参数传递
  - [x] 返回处理

- [x] Task 5.3: 数据绑定和转换器测试
  - [x] Converter 实现
  - [x] 绑定模式测试

## Phase 6: AI 功能验证 (POC + 测试)

- [x] Task 6.1: Semantic Kernel 集成测试
  - [x] SK 基础配置
  - [x] Plugin 注册测试
  - [x] Kernel 调用测试

- [x] Task 6.2: Ollama 本地推理测试
  - [x] Ollama 连接测试
  - [x] Chat Completion 测试
  - [x] Embedding 测试

- [x] Task 6.3: 意图解析功能测试
  - [x] TripLogPlugin 测试
  - [x] 时间表达式解析
  - [x] 地点识别测试

## Phase 7: 后台服务功能验证 (POC + 测试)

- [ ] Task 7.1: 后台定位服务测试
  - [ ] Android Foreground Service
  - [ ] iOS Background Location
  - [ ] 权限处理

- [ ] Task 7.2: 本地通知测试
  - [ ] 通知权限
  - [ ] 通知发送/接收
  - [ ] 点击处理

- [ ] Task 7.3: 地理围栏检测测试
  - [ ] 圆形区域检测
  - [ ] 进入/离开事件

## Phase 8: 最终集成

- [ ] Task 8.1: 整合所有验证模块
  - [ ] 合并到主项目
  - [ ] 解决依赖冲突
  - [ ] 优化构建配置

- [ ] Task 8.2: 应用配置和资源
  - [ ] Android 配置 (Manifest, 权限)
  - [ ] iOS 配置 (Info.plist, 权限)
  - [ ] 应用图标和启动页

- [ ] Task 8.3: 构建和打包验证
  - [ ] Android Debug 构建
  - [ ] Android Release 构建
  - [ ] 生成 APK 测试包

## Phase 9: 文档和交付

- [ ] Task 9.1: 验证测试文档
  - [ ] 每个 POC 的测试结果
  - [ ] 已知问题和限制

- [ ] Task 9.2: 集成使用指南
  - [ ] 开发环境配置
  - [ ] 构建和运行指南
  - [ ] 功能测试清单

# Task Dependencies

- Phase 2 依赖 Phase 1 完成 ✅
- Phase 3, 4, 5 可并行执行（在 Phase 2 完成后） ✅
- Phase 6 依赖 Phase 2, 3 完成 ✅
- Phase 7 依赖 Phase 2 完成
- Phase 8 依赖 Phase 3, 4, 5, 6, 7 完成
- Phase 9 依赖 Phase 8 完成

# Verification Criteria

- [ ] 所有 POC 项目可编译运行
- [ ] 所有单元测试通过
- [ ] 所有集成测试通过
- [ ] Android Debug APK 可正常安装和运行
- [ ] 基础功能（记录、查看、日历）可正常使用
