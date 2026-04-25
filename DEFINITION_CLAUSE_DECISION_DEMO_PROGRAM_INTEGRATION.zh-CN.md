# DefinitionClauseDecision Demo Program Integration

## 目的

把当前已经准备好的 demo 导出链，收成一个几乎可直接挂到 `Program.cs` 的应用入口接口，避免真正接入主程序时再重复设计：

- 怎么判断是否命中 demo 参数
- 命中后往标准输出/标准错误写什么
- 返回码用多少

## 当前应用入口

- [DefinitionClauseDecisionDemoAppEntry.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoAppEntry.cs)

核心方法：

- `DefinitionClauseDecisionDemoAppEntry.TryRun(args, standardOutput, standardError, out exitCode)`

## 当前行为

### 未命中参数

- `return false`
- `exitCode = 0`
- 主程序继续走原有路径

### 命中但失败

- `return true`
- 写标准错误
- `exitCode = 2`

### 命中且成功

- `return true`
- 写标准输出
- `exitCode = 0`
- 输出：
  - `OutputDirectory`
  - `SummaryJsonPath`
  - `SummaryMarkdownPath`
  - `FixtureJsonPath`
  - `FixtureMarkdownPath`

## Program.cs 推荐接法

只需要一层很薄的入口：

1. 程序启动最前面调用 `DefinitionClauseDecisionDemoAppEntry.TryRun(...)`
2. 如果返回 `true`，直接按 `exitCode` 返回
3. 如果返回 `false`，继续原有主流程

## 当前边界

这个应用入口只负责 demo sidecar，不替代：

- 真实 full-run
- 主摘要更新
- 交付包刷新

它的作用是先把 `DefinitionClauseDecision` 的最小闭环真正挂到应用入口附近。
