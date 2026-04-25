# Body Candidate Partition Conservative Fixture Package Audit Validation Workflow

## 目标

把阶段 2 `body-candidate partition conservative fixture package audit` 的执行结果进一步收口成统一 `validation.json / validation.md` 工件，避免后续接 command、bundle、smoke 或 sidecar 时重复翻译“package audit 是否完整”。

当前该 workflow 既可独立按 `outputRootDirectory` 运行，也可直接消费已有的 `BodyCandidatePartitionConservativeFixturePackageAuditCommandResult`，以便复用到现有命令链里而不重复跑两遍 fixture。

## 当前调用链

1. `BodyCandidatePartitionConservativeFixturePackageAuditExportService.Export(...)`
2. `BodyCandidatePartitionConservativeFixturePackageAuditValidator.Validate(...)`
3. `BodyCandidatePartitionConservativeFixturePackageAuditValidationArtifactBuilder.Build(...)`
4. `BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflow.Run(...)`

## 冻结的 validation 输出契约

输出目录沿用 package-audit 结果里的 `OutputDirectory`，并在目录下额外写出：

- `validation.json`
- `validation.md`

## 成功判定

同时满足以下条件才视为成功：

- fixture 至少执行了一条
- `FailedCount == 0`
- `PackageIsComplete == true`
- `MissingFiles` 为空
- 归一化后的 `ExitCode == 0`

## 失败语义

以下任一情况会进入失败态并写入 `Issues`：

- 没有执行任何 fixture
- fixture 存在失败
- package audit 不完整
- 缺失必需工件

## 设计约束

- 当前层不直接改动旧 dispatcher / Program。
- 该层是阶段 2 的统一 validation artifact 真源，后续 command、bundle 或 smoke 若需要读取校验结果，应优先复用该层，而不是各自再拼装一份 `validation.json / validation.md`。
