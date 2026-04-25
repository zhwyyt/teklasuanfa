# DefinitionClauseDecision Full-Run Source Export Quickstart

## 目标

把真实 full-run 结果里的 `representative-part source` 先稳定落成两份工件：

- `definition-clause-decision-fullrun-source.json`
- `definition-clause-decision-fullrun-source.zh-CN.md`

这样下一步即使还没完全接入 snapshot / sidecar，也能先独立 review：

- 哪些构件进入了 `DefinitionClauseDecision`
- 哪些代表零件被送入了旁路
- 每个代表零件当前带着哪些 `Pattern / DefinitionClause / LeadClause / core-review role`

---

## 当前代码入口

当前统一入口已经固定为：

- [DefinitionClauseDecisionFullRunSourceExportService.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceExportService.cs>)

调用方式：

```csharp
var exportResult = DefinitionClauseDecisionFullRunSourceExportService.Export(
    outputDirectory,
    assemblies,
    representativeParts);
```

其中：

- `assemblies` 是 `DefinitionClauseDecisionFullRunAssemblySource` 列表
- `representativeParts` 是 `DefinitionClauseDecisionFullRunRepresentativePartSource` 列表

返回值会直接给出：

- `JsonPath`
- `MarkdownPath`

---

## 推荐接线顺序

1. 在 `Program.cs` 的 full-run 主循环里收集：
   - `DefinitionClauseDecisionFullRunAssemblySource`
   - `DefinitionClauseDecisionFullRunRepresentativePartSource`
2. 在主循环结束后调用：
   - `DefinitionClauseDecisionFullRunSourceExportService.Export(...)`
3. 先 review `fullrun-source json/md`
4. 再把这批 representative parts 投进后续 `snapshot -> sidecar`

---

## 价值

这一步把下一轮的改动收敛成一个很薄的接线动作：

- `Program.cs` 只需要负责收集真实 source rows
- 文件名、编码、artifact 聚合、Markdown 形态都交给 `ExportService`

也就是说，下一轮接线时不需要再临时补：

- serializer
- artifact builder
- 文件路径常量
- Markdown 输出顺序
