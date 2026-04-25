# DefinitionClauseDecision Demo CommandLine 契约

## 目的

把当前已经具备的 `DefinitionClauseDecision` demo 导出服务，进一步包装成一个可挂到 `Program.cs` 的命令处理器，避免后续接入口时再次从零设计参数约定。

## 当前处理器

- [DefinitionClauseDecisionDemoCommandHandler.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoCommandHandler.cs)
- [DefinitionClauseDecisionDemoCommandResult.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoCommandResult.cs)

## 命令参数

当前固定参数名：

- `--definition-clause-decision-demo-output`

用法：

```powershell
TeklaBodyBracketRecognition.App.exe --definition-clause-decision-demo-output <输出目录>
```

## 成功后固定导出文件

处理成功后，固定导出四个文件：

- `definition-clause-decision-snapshot.json`
- `definition-clause-decision-snapshot-roundtrip.json`
- `definition-clause-decision-snapshot-roundtrip.md`
- `definition-clause-decision-summary.json`
- `definition-clause-decision-summary.zh-CN.md`
- `definition-clause-decision-fixture-report.json`
- `definition-clause-decision-fixture-report.md`
- `definition-clause-decision-demo-manifest.json`
- `definition-clause-decision-demo-validation.json`
- `definition-clause-decision-demo-validation.md`

## 命令处理结果

`DefinitionClauseDecisionDemoCommandResult` 当前固定包含：

- `Handled`
- `Succeeded`
- `Message`
- `OutputDirectory`
- `SnapshotJsonPath`
- `SnapshotValidationJsonPath`
- `SnapshotValidationMarkdownPath`
- `SnapshotRoundTripJsonPath`
- `SnapshotRoundTripMarkdownPath`
- `SummaryJsonPath`
- `SummaryMarkdownPath`
- `FixtureJsonPath`
- `FixtureMarkdownPath`
- `ManifestJsonPath`
- `ReadmePath`
- `ValidationJsonPath`
- `ValidationMarkdownPath`

## 接线建议

后续接入 `Program.cs` 时，只需要做一层很薄的处理：

1. 调 `DefinitionClauseDecisionDemoCommandHandler.TryHandle(args)`
2. 如果 `Handled = false`，继续走原有主流程
3. 如果 `Handled = true` 且 `Succeeded = true`，输出 `Message` 和四个导出路径后返回
4. 如果 `Handled = true` 且 `Succeeded = false`，输出错误信息并返回非零状态

## 当前边界

这条命令行契约只负责 demo sidecar 导出，不负责：

- 真实 full-run
- 主摘要更新
- 交付包刷新

它的作用是先把 `DefinitionClauseDecision` 最小闭环变成一个真正可调用的入口。
