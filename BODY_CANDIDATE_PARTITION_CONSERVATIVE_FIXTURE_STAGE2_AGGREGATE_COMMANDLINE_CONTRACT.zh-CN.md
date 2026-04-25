# Body Candidate Partition Conservative Fixture Stage-2 Aggregate Commandline Contract

## 目标

把阶段 2 已经跑通的三条入口：

- `package-audit`
- `package-audit-validation`
- `package-audit-validation-bundle`

继续收口成一个更高层 aggregate 命令，让阶段 2 不再只是“多条平行命令都能跑”，而是拥有统一的顶层出口。

## 命令名

- `--body-candidate-partition-conservative-fixture-stage2-aggregate`

## 可选参数

- `--output-root <directory>`

## 当前 aggregate 输出

aggregate 目录至少写出：

- `summary.md`
- `README.md`
- `manifest.json`

并显式回传：

- package 输出目录和主工件路径
- validation 工件路径
- validation-bundle 输出目录和主工件路径
- `ValidationSucceeded`

## 设计约束

- aggregate workflow 只跑一次 `BodyCandidatePartitionConservativeFixturePackageAuditExportService.Export(...)`
- validation-bundle workflow 通过 overload 复用这次 command result，避免重复跑底层 fixture
- 该命令的价值在于给阶段 2 提供统一顶层出口，后续更容易再并入更高层的 summary / audit / release checklist
