# DefinitionClauseDecision Full-Run Source Workflow

## 目标

把 `DefinitionClauseDecision` 在真实 full-run 结果侧的第一层落盘路径固定下来：

1. 真实结果
2. representative-part source
3. fullrun-source artifact
4. snapshot
5. sidecar

本文只覆盖第 `2 -> 3` 步，也就是：

- 如何把真实 `representative-part source` 聚成可落盘 artifact
- 如何稳定输出 `json + markdown`

---

## 最小落盘文件

真实结果一旦接入，应优先落出：

- `definition-clause-decision-fullrun-source.json`
- `definition-clause-decision-fullrun-source.zh-CN.md`

它们的职责是：

- 在进入 snapshot 前，先让人看到“哪些代表零件会被送进 `DefinitionClauseDecision`”
- 让脚本也能消费这批代表零件，不需要直接碰复杂 full-run 原始对象

---

## 当前代码入口

当前这一层已具备：

- 中间模型：
  - [DefinitionClauseDecisionFullRunSourceModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceModels.cs>)
- artifact 模型：
  - [DefinitionClauseDecisionFullRunSourceArtifactModels.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactModels.cs>)
- artifact 构建器：
  - [DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactBuilder.cs>)
- artifact serializer：
  - [DefinitionClauseDecisionFullRunSourceArtifactSerializer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceArtifactSerializer.cs>)

---

## 推荐接线顺序

1. 在 `Program.cs` 收集真实 full-run 结果时，先投成：
   - `DefinitionClauseDecisionFullRunAssemblySource`
   - `DefinitionClauseDecisionFullRunRepresentativePartSource`
2. 用 `DefinitionClauseDecisionFullRunSourceArtifactBuilder.Build(...)` 聚成 artifact
3. 用 `DefinitionClauseDecisionFullRunSourceArtifactSerializer` 输出：
   - `definition-clause-decision-fullrun-source.json`
   - `definition-clause-decision-fullrun-source.zh-CN.md`
4. 再把这批 representative parts 投给后续 snapshot / sidecar 链

---

## 为什么要先有这一层

原因很直接：

- 如果直接从 full-run 复杂对象跳到 snapshot，很难定位是“真实输入错了”还是“snapshot 适配错了”
- 有了 `fullrun-source` 这层，就能先在真实结果侧做一次人工复核
- 后续排查顺序会变成：
  1. `fullrun-source`
  2. `snapshot validation`
  3. `snapshot roundtrip`
  4. `summary sidecar`

---

## 当前最近一步

下一步不是再补文档，而是：

- 把这条 `fullrun-source artifact` 真正接到 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 的 full-run 结果落盘点；
- 先跑出第一版真实：
  - `definition-clause-decision-fullrun-source.json`
  - `definition-clause-decision-fullrun-source.zh-CN.md`
