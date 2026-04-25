# Definition Clause Decision Bridge Validation Bundle Workflow Semantic Regression Post-Merge Smoke Output Path Resolver

## 目标

在已经冻结输出目录名与关键文件名之后，再把“如何从 output root / output directory 解析出全部路径”统一收口，避免：

- workflow 自己拼一套
- validator 再拼一套
- dispatcher smoke 或后续最小实跑说明再写第三套

## 代码入口

- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPaths`
- `DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver`

## 解析方式

### 1. 从 output root directory 解析

```csharp
var paths =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver
        .ResolveFromOutputRootDirectory(outputRootDirectory);
```

适用于：

- export service
- command handler
- smoke app entry

### 2. 从已经存在的 output directory 解析

```csharp
var paths =
    DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeOutputPathResolver
        .ResolveFromOutputDirectory(outputDirectory);
```

适用于：

- validator
- dispatcher 最小实跑后的回查
- sidecar/readme/manifest 汇总复核

## 当前输出对象字段

- `OutputRootDirectory`
- `OutputDirectory`
- `ManifestPath`
- `ReadmePath`
- `ValidationJsonPath`
- `ValidationMarkdownPath`
- `SemanticRegressionBundleSectionMarkdownPath`

## 当前价值

- 继续推进“输出契约清理”，把路径模型也纳入单一真源
- 为下一轮安全接入旧 dispatcher / validator / workflow 提供统一路径对象
- 减少后续最小 smoke 实跑时的路径硬编码漂移
