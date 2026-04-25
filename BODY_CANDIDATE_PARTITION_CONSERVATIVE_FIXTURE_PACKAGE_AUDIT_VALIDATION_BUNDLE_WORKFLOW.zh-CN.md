# Body Candidate Partition Conservative Fixture Package Audit Validation Bundle Workflow

## 目标

在阶段 2 已经拥有独立 validation 命令和 contribution 层的前提下，继续补一层真正的统一 bundle workflow，使这些结果不再只是“可被汇总”，而是已经有稳定的 `summary / README / manifest` 出口。

## 当前 bundle 输出

bundle 目录：

- `body-candidate-partition-conservative-fixture-package-audit-validation-bundle`

当前 bundle 至少写出：

- `summary.md`
- `README.md`
- `manifest.json`

并在结果对象中继续显式回传：

- validation 输出目录
- `validation.json`
- `validation.md`
- `ValidationSucceeded`

## 当前命令入口

- `--body-candidate-partition-conservative-fixture-package-audit-validation-bundle`

## 设计约束

- 该 bundle workflow 当前只消费阶段 2 validation contribution。
- 它的价值不是替代已有 validation 命令，而是把“可汇总接口”变成“已汇总出口”，给后续统一 validation / audit / summary 聚合留出稳定接入点。
