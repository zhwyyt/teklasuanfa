# AppEarlyCommandDispatcher 契约

## 目的

把应用启动阶段的“早期命令处理”统一收敛到一个 dispatcher，避免后续在 `Program.cs` 中直接散落多种特判入口。

## 当前入口

- [AppEarlyCommandDispatcher.cs](./src/TeklaBodyBracketRecognition.App/AppEarlyCommandDispatcher.cs)

当前它只代理一条早期命令：

- `DefinitionClauseDecisionDemoAppEntry`

## 当前行为

调用：

- `AppEarlyCommandDispatcher.TryRun(args, standardOutput, standardError, out exitCode)`

语义：

- 返回 `true`：说明命中了某条早期命令，主程序应立即返回
- 返回 `false`：说明没有命中，主程序继续原有主流程

## 当前价值

这层虽然很薄，但它固定了一个重要约束：

- `Program.cs` 以后只接 dispatcher
- 各种 demo / 自检 / 旁路导出命令不直接写进主程序

## 下一步

后续把 `DefinitionClauseDecisionDemo` 挂进主程序时，建议只做下面这一层：

1. 程序启动最前面调 `AppEarlyCommandDispatcher.TryRun(...)`
2. 若返回 `true`，直接按 `exitCode` 退出
3. 若返回 `false`，继续原有主流程

## 未来扩展

后续如果还需要加入：

- 其它 sidecar demo
- 自检命令
- fixture 命令

都优先挂在 dispatcher 后面，而不是直接改 `Program.cs` 的主体流程。
