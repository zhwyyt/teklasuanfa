# DefinitionClauseDecision Full-Run Source Collector

## 目标

把 `Program.cs` 里已经有的真实 full-run 结果，先收成两批稳定输入：

- `DefinitionClauseDecisionFullRunAssemblySource`
- `DefinitionClauseDecisionFullRunRepresentativePartSource`

这样下一步接 `fullrun-source workflow` 时，主循环不需要直接手写字段映射。

---

## 当前 collector

当前 collector 已固定在：

- [DefinitionClauseDecisionFullRunSourceCollector.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceCollector.cs>)

它当前按以下来源收集：

1. `BodyMaterialSummary`
2. `CoreBodyProofViewOutput (real_input)`

### CollectAssemblies(...)

负责生成构件级输入：

- `SourceFile / MemberId / AssemblyId / InputMainPartId`
- `BodyDescriptorFamily / BodyDescriptorSectionType`
- `HasTopologyRewrite`
- `LeadClauseCode / LabelZh / Share`
- `ClauseMix`

### CollectRepresentativeParts(...)

负责生成代表零件输入：

- 只收 `core` 或 `review` 零件
- 同时带上：
  - `Pattern`
  - `ControllerRole`
  - `ShapeRole`
  - `FamilyRisk`
  - `ProofType`
  - `FamilyProofTarget`
  - `DefinitionClause`
  - `LeadClause`
  - `ReviewPromptZh`

---

## 下一步接线

下一轮在 [Program.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/Program.cs>) 里应走：

1. full-run 主循环结束
2. 调 `DefinitionClauseDecisionFullRunSourceCollector.CollectAssemblies(...)`
3. 调 `DefinitionClauseDecisionFullRunSourceCollector.CollectRepresentativeParts(...)`
4. 把结果喂给：
   - [DefinitionClauseDecisionFullRunSourceWorkflow.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFullRunSourceWorkflow.cs>)

这样就能先稳定落出第一版真实：

- `definition-clause-decision-fullrun-source.json`
- `definition-clause-decision-fullrun-source.zh-CN.md`
- `definition-clause-decision-fullrun-source-validation.json`
- `definition-clause-decision-fullrun-source-validation.md`
- `definition-clause-decision-fullrun-source-manifest.json`
- `README.md`
