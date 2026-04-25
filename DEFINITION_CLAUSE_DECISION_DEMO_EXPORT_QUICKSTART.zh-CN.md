# DefinitionClauseDecision Demo Export Quickstart

## 目的

把当前 `DefinitionClauseDecision` 的 demo 最小闭环再往前推一步，固定成一个可直接调用的导出服务：

- [DefinitionClauseDecisionDemoExportService.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionDemoExportService.cs)

这样后续无论从 `Program.cs`、临时命令入口，还是交付包脚本里接入，都只需要挂一个调用点：

- `DefinitionClauseDecisionDemoExportService.Export(outputDirectory)`

## 当前输出

调用后会返回一个带 manifest 的结果对象，能够直接拿到：

- `definition-clause-decision-summary.json`
- `definition-clause-decision-summary.zh-CN.md`
- `definition-clause-decision-fixture-report.json`
- `definition-clause-decision-fixture-report.md`

## 当前定位

这条路径仍然是 demo 闭环，不替代真实结果链接线。

它的作用是先证明：

- bridge 判定
- source row 适配
- mapper 聚合
- sidecar workflow 落盘

这四层已经能用一个统一导出服务串起来。

## 后续接线建议

### 方案 A

先在 `Program.cs` 挂一个临时开关：

- `--definition-clause-decision-demo-output <dir>`

### 方案 B

先在内部开发脚本里挂一个调用点，单独跑 demo sidecar。

### 方案 C

等真实结果链接完后，把这条 demo 服务保留为 smoke/self-check 入口。

## 当前最小结论

如果后面任一入口能成功调用：

- `DefinitionClauseDecisionDemoExportService.Export(outputDirectory)`

并落出四个 sidecar 文件，就说明 `DefinitionClauseDecision` 的 demo 闭环已经真正可执行。
## 补充输出

当前导出服务还会额外落出：

- `definition-clause-decision-snapshot.json`
- `definition-clause-decision-snapshot-validation.json`
- `definition-clause-decision-snapshot-validation.md`
- `definition-clause-decision-snapshot-roundtrip.json`
- `definition-clause-decision-snapshot-roundtrip.md`
- `definition-clause-decision-demo-manifest.json`
- `definition-clause-decision-demo-validation.json`
- `definition-clause-decision-demo-validation.md`
- `README.md`
