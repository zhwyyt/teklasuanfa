# Program Early Command Hook Plan

## 目的

在真正修改 `Program.cs` 之前，把最后一层“怎么接、接完先验什么”固定下来，避免主程序接 dispatcher 时变成一次无回归保护的硬改。

## 当前推荐接法

在 `Program.cs` 启动最前面插入一层很薄的早期命令 hook：

1. 调用 `AppEarlyCommandDispatcher.TryRun(args, Console.Out, Console.Error, out var exitCode)`
2. 若返回 `true`，立即 `return exitCode`
3. 若返回 `false`，继续当前原有主流程

## 当前依赖

- [AppEarlyCommandDispatcher.cs](./src/TeklaBodyBracketRecognition.App/AppEarlyCommandDispatcher.cs)
- [DefinitionClauseDecisionDemoAppEntry.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoAppEntry.cs)
- [DefinitionClauseDecisionDemoCommandHandler.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoCommandHandler.cs)

## 接入后最小 smoke 计划

### 1. 未命中参数

命令：

- 正常主流程命令

预期：

- dispatcher 返回 `false`
- 主程序继续原有逻辑
- 不影响当前离线识别

### 2. 命中 demo 参数但缺少目录

命令：

- `--definition-clause-decision-demo-output`

预期：

- dispatcher 返回 `true`
- 主程序直接退出
- 返回非零码
- 标准错误中出现缺少输出目录的提示

### 3. 命中 demo 参数且目录合法

命令：

- `--definition-clause-decision-demo-output <dir>`

预期：

- dispatcher 返回 `true`
- 主程序直接退出
- 返回码 `0`
- 成功落出四个 sidecar 文件

## 当前策略

在 shell 仍受环境层阻挡的情况下，先把这份 hook 计划冻结；一旦能执行真实命令或能安全 patch `Program.cs`，就按这份计划接入并做 smoke。
