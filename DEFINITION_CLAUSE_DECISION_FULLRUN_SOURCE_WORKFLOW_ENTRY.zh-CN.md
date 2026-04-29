# DefinitionClauseDecision Full-Run Source Workflow Entry

## 目标

把 `DefinitionClauseDecision` 的真实 `fullrun-source` 导出路径收成一个统一入口。

这样后面接到 `Program.cs` 的 full-run 主循环时，不需要直接关心：

- artifact builder
- serializer
- validation
- manifest
- README

而只需要喂两类输入：

- `DefinitionClauseDecisionFullRunAssemblySource`
- `DefinitionClauseDecisionFullRunRepresentativePartSource`

---

## 当前统一入口

当前统一入口已经固定为：

- [DefinitionClauseDecisionFullRunSourceWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceWorkflow.cs>)

调用方式：

```csharp
var workflowResult = DefinitionClauseDecisionFullRunSourceWorkflow.Run(
    outputDirectory,
    assemblies,
    representativeParts);
```

返回值会直接给出：

- `JsonPath`
- `MarkdownPath`
- `ValidationJsonPath`
- `ValidationMarkdownPath`
- `ManifestJsonPath`
- `ReadmePath`

---

## 价值

这一步让下一轮接线继续收薄为：

1. 在 `Program.cs` 主循环里收集真实 source rows
2. 主循环结束后调一次 workflow
3. 先 review `fullrun-source`
4. 再把同一批 representative parts 投给既有 `snapshot -> sidecar`

也就是说，`Program.cs` 不需要直接知道这层到底写了几个文件、各自叫什么名字。
