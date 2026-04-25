# Definition Clause Decision Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Required Artifact Audit Artifact Workflow

## 目标

把已经冻结下来的：

- `OutputContract`
- `OutputPathResolver`
- `RequiredArtifactInspector`
- `RequiredArtifactSummaryFormatter`
- `RequiredArtifactAuditBuilder`
- `RequiredArtifactAuditDecisionBuilder`
- `RequiredArtifactAuditArtifactBuilder`
- `RequiredArtifactAuditArtifactSerializer`

再收口成一个真正可调用的 workflow，供未来 smoke validator、dispatcher 回查或最小实跑入口直接重建：

- `validation.json`
- `validation.md`

## 代码入口

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflow`
- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflowResult`

## 调用方式

### 1. 从 output root directory

```csharp
var result =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflow
        .RunFromOutputRootDirectory(outputRootDirectory);
```

### 2. 从 output directory

```csharp
var result =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflow
        .RunFromOutputDirectory(outputDirectory);
```

### 3. 已有共享 decision

```csharp
var result =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeRequiredArtifactAuditArtifactWorkflow
        .Run(decision);
```

## 当前返回结果

- `OutputDirectory`
- `IsComplete`
- `ExitCode`
- `ConsoleMessage`
- `ValidationJsonPath`
- `ValidationMarkdownPath`

## 当前价值

- 继续沿“输出契约清理”主线推进，把 post-merge smoke 的 validation 输出最终收敛成单次 workflow 调用
- 为未来切换现有 smoke validator 的输出逻辑提供直接落点
- 让最小 smoke 实跑后的 validation 重建不再依赖分散 helper 串接
