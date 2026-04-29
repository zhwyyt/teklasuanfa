# DefinitionClauseDecision Fixture 契约

## 目的

定义桥接层 fixture 旁路的最小产物结构，使 `DefinitionClauseDecisionBridge` 在尚未完全接进主结果链时，也能独立产出：

- `definition-clause-decision-fixture-report.json`
- `definition-clause-decision-fixture-report.md`

## JSON 顶层结构

顶层对象固定包含：

- `Results`
- `TotalCount`
- `PassedCount`
- `FailedCount`

## Results

每个元素至少包含：

- `Name`
- `Passed`
- `ActualClauseVerdictCode`
- `ExpectedClauseVerdictCode`
- `ActualClausePromotionReadinessCode`
- `ExpectedClausePromotionReadinessCode`

## 生成顺序

推荐顺序：

1. 先跑 `DefinitionClauseDecisionBridgeFixtureRunner.RunDefault()`
2. 用 `DefinitionClauseDecisionFixtureArtifactBuilder` 生成 artifact
3. 用 `DefinitionClauseDecisionFixtureSerializer` 写出 JSON
4. 调用 `Build-DefinitionClauseDecisionFixtureReport.ps1` 重建 Markdown

## 当前组件

- 夹具集：
  [DefinitionClauseDecisionBridgeFixtures.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeFixtures.cs)
- runner：
  [DefinitionClauseDecisionBridgeFixtureRunner.cs](./src/TeklaBodyBracketRecognition.Core/Algorithms/DefinitionClauseDecisionBridgeFixtureRunner.cs)
- artifact builder：
  [DefinitionClauseDecisionFixtureArtifactBuilder.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFixtureArtifactBuilder.cs)
- serializer：
  [DefinitionClauseDecisionFixtureSerializer.cs](./src/TeklaBodyBracketRecognition.App/DefinitionClauseDecisionFixtureSerializer.cs)
- Markdown 重建脚本：
  [Build-DefinitionClauseDecisionFixtureReport.ps1](./tools/Build-DefinitionClauseDecisionFixtureReport.ps1)

## 首轮要求

首轮旁路自检完成后，至少应满足：

- `GKZ` -> `CLAUSE_SATISFIED_WITH_REWRITE`
- `HXZ` -> `CLAUSE_SATISFIED_WITH_REWRITE`
- `MJ` -> `CLAUSE_BROKEN_DIRECT`
- `GL` -> `CLAUSE_REVIEW_REQUIRED`
- `YPGL` -> `CLAUSE_REVIEW_REQUIRED`

并且：

- `PassedCount = TotalCount`
- `FailedCount = 0`
