# AppEarlyCommandDispatcher Body Candidate Partition Conservative Fixture Package Audit Integration

## 目标

为阶段 2 `body-candidate partition conservative fixture package audit` 预留一条 one-line hook，等旧入口源码可读后，直接把命令挂进 `AppEarlyCommandDispatcher`。

## 推荐接线点

在既有 early-command 分发链靠前位置插入：

```csharp
if (BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook.TryRun(
    args,
    defaultOutputRootDirectory,
    out var exitCode))
{
    return exitCode;
}
```

## 约束

- 该 hook 只负责“是否命中命令 + 返回退出码”。
- 真正的 package audit 执行仍由 `CommandHandler -> ExportService -> Workflow` 完成。
- 当前阶段仍避免直接改动旧入口文件，直到旧入口源码能被安全读取并验证。

## 期望收益

- 阶段 2 package-audit 命令链和前面已建立的 smoke/semantic-regression 接线方式保持一致。
- 后续一旦解除旧入口读取阻塞，可低风险地把该命令挂入应用入口，不需要再返工外围契约。
