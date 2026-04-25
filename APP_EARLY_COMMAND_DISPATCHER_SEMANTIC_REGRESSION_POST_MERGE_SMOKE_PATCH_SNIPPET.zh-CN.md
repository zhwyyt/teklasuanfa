# AppEarlyCommandDispatcher Semantic Regression Post-Merge Smoke Patch Snippet

## 目的

在真正编辑 `AppEarlyCommandDispatcher.cs` 之前，先把“最小插入片段”冻结下来，避免下一轮再次围绕：

- 放在哪个位置
- 需要哪些局部变量
- 命中后如何短路返回

重新思考。

## 推荐插入位置

放在 `AppEarlyCommandDispatcher` 已有 demo / diagnostic / smoke 类命令判断区域内，且位于主识别流程之前。

## 最小插入片段

```csharp
if (DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeDispatcherAdapter.TryRun(
        args,
        defaultOutputRootDirectory,
        out var exitCode,
        out var message,
        out var outputDirectory))
{
    // 若 dispatcher 现有模式支持消息回传，则沿用既有消息字段
    // 若 dispatcher 现有模式只关心 exit code，则至少保证 exitCode 被正确短路返回
    return /* 复用现有早期命令短路返回模式 */;
}
```

## 命中后的最小要求

- 不进入主识别流程
- `exitCode`
  - `0`：smoke 导出成功且自校验通过
  - `1`：smoke 导出完成但存在缺失工件
- 若 dispatcher 有统一消息/日志回传位：
  - 直接传递 adapter 返回的 `message`
- 若 dispatcher 有统一输出目录回传位：
  - 直接传递 adapter 返回的 `outputDirectory`

## 最小实跑顺序

1. 先在 dispatcher 中插入上面的 `TryRun(...)`
2. 通过命令：

```text
--definition-clause-validation-bundle-semantic-regression-post-merge-smoke --output-root <path>
```

进行最小实跑

3. 优先检查：
   - 命令是否被 dispatcher 成功短路
   - `validation.md` 是否生成
   - `validation.md` 中是否仍出现缺失工件

## 下一步

1. 将本片段落实到 `AppEarlyCommandDispatcher.cs`
2. 若实跑稳定，再评估是否把 post-merge 路线直接并回旧 validation bundle 主入口
