# Definition Clause Decision Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Required Artifact Audit Artifact

## 目标

把已经冻结下来的 `audit decision` 再推进成可直接落盘的统一工件层，让未来 smoke validator 或最小实跑入口不必再自己拼：

- `validation.json`
- `validation.md`

## 代码入口

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifact`
- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactBuilder`
- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactSerializer`

## 当前工件职责

### JSON

落到固定路径：

- `validation.json`

当前包含：

- `IsComplete`
- `ExitCode`
- `ConsoleMessage`
- `ManifestPath`
- `ReadmePath`
- `ValidationJsonPath`
- `ValidationMarkdownPath`
- `SemanticRegressionBundleSectionMarkdownPath`
- `MissingArtifacts`

### Markdown

落到固定路径：

- `validation.md`

当前包含：

- 标题 `# Post-Merge Smoke Validation`
- `ExitCode`
- 来自共享 formatter 的 `Required Artifacts` Markdown block

## 使用方式

```csharp
var decision =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditDecisionBuilder
        .BuildFromOutputDirectory(outputDirectory);

var artifact =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactBuilder
        .Build(decision);

DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactSerializer
    .WriteJson(artifact);

DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactSerializer
    .WriteMarkdown(artifact);
```

## 当前价值

- 继续沿“输出契约清理”主线推进，把 post-merge smoke 的 validation 工件收敛到共享生成方式
- 为未来 smoke validator / dispatcher 最小实跑替换现有 validation 输出提供直接落点
- 让 `validation.json / validation.md` 与共享 audit decision 保持一致
