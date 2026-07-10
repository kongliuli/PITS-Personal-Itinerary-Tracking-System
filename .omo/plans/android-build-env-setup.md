# android-build-env-setup — 设置 Android 构建环境变量

## TL;DR (For humans)

**背景**: Android SDK 和 JDK 已通过 VS Installer 安装在非标准路径：
- Android SDK: `D:\program\apps\vs\sdk\Android\android-sdk`
- JDK 21: `D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8`

在命令行中用 `dotnet build` 编译 `net10.0-android` 目标时，需要 `ANDROID_HOME` 和 `JAVA_HOME` 环境变量才能找到 SDK/JDK。初始化已通过临时设变量验证编译成功。

**要做的事**: 把这两个环境变量设为**持久化用户级环境变量**，使未来的所有 `dotnet build` 命令都能自动找到 Android SDK 和 JDK。

**风险**: 极低 — 仅设环境变量，不改任何代码或项目文件。

## Scope

### Must have
- 设置用户级环境变量 `ANDROID_HOME` = `D:\program\apps\vs\sdk\Android\android-sdk`
- 设置用户级环境变量 `JAVA_HOME` = `D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8`
- 将 JDK `bin` 目录追加到 `PATH`：`D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8\bin`
- 验证：新开 PowerShell 窗口运行 `dotnet build -f net10.0-android` 成功

### Must NOT have
- **不得**修改任何 `.csproj`、`.sln`、或源码文件
- **不得**删除或修改现有 `PATH` 条目
- **不得**使用系统级环境变量（避免权限问题）

## Verification strategy
- `[System.Environment]::GetEnvironmentVariable("ANDROID_HOME", "User")` 返回正确路径
- `[System.Environment]::GetEnvironmentVariable("JAVA_HOME", "User")` 返回正确路径
- 新 PowerShell 进程中 `dotnet build -f net10.0-android` exit code = 0

## Execution strategy
- 需要**管理员身份**？不需要，用户级变量无需管理员
- 执行后将写入证据到 `.omo/evidence/android-build-env.md`

## Todos
> Implementation + Test = ONE todo. Never separate.

- [x] 1. 设置持久用户级环境变量 ANDROID_HOME 和 JAVA_HOME
  What to do / Must NOT do:
  1. 使用 `[Environment]::SetEnvironmentVariable("ANDROID_HOME", "D:\program\apps\vs\sdk\Android\android-sdk", "User")` 设 ANDROID_HOME
  2. 使用 `[Environment]::SetEnvironmentVariable("JAVA_HOME", "D:\program\apps\vs\sdk\Android\openjdk\jdk-21.0.8", "User")` 设 JAVA_HOME
  3. 获取当前用户 PATH，追加 `%JAVA_HOME%\bin`（如果还不存在的话），写回
  4. 不得使用系统级 ("Machine") 范围

- [x] 2. 验证 — 新进程构建 Android 目标
  What to do / Must NOT do:
  1. 启动新 PowerShell 进程（确保读到新变量）
  2. 运行 `dotnet build mvp/src/PITS.MVP.App/PITS.MVP.App.csproj -f net10.0-android`
  3. exit code 必须为 0
  4. 写入证据到 `.omo/evidence/android-build-env.md`

## Commit strategy
- 环境变量变更不产生 git 变更，无需 commit
- 证据文件 `.omo/evidence/android-build-env.md` 记录结果
