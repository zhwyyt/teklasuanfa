# AppEarlyCommandDispatcher Body Candidate Partition Conservative Fixture Package Audit Validation Integration

## 目标

为阶段 2 `package-audit validation` 提供一条可直接接入 `AppEarlyCommandDispatcher` 的 one-line hook，使 validation 工件链不再只能经由内部 workflow 被动调用。

## 推荐接线点

在 `AppEarlyCommandDispatcher` 中，保留 demo 命令优先级后，插入：

```csharp
if (BodyCandidatePartitionConservativeFixturePackageAuditValidationAppEarlyCommandDispatcherHook.TryRun(
    args ?? [],
    ResolveDefaultOutputRootDirectory(),
    out exitCode))
{
    return true;
}
```

## 约束

- 该 hook 只负责“命中命令 + 输出用户可读结果 + 返回退出码”。
- 真正执行仍由 `ValidationCommandHandler -> ValidationExportService -> ValidationWorkflow` 完成。
- 当前与 `package-audit` 主命令并存，后续若进入统一 validation/bundle 收口，再决定是否合并或重定向。
