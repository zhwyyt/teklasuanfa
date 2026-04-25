# Body Candidate Partition Conservative Fixture Package Audit Validation Contribution

## 目标

把阶段 2 已经跑通的 `package-audit + validation` 命令链继续收口成可被统一 summary / bundle / readme 汇总消费的最小 contribution 层，而不是长期停留在“单独命令 + 单独 workflow”。

## 当前最小组成

- `BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSection`
- `BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSectionBuilder`
- `BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSectionMarkdownBuilder`
- `BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachment`
- `BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachmentBuilder`
- `BodyCandidatePartitionConservativeFixturePackageAuditValidationContribution`
- `BodyCandidatePartitionConservativeFixturePackageAuditValidationContributionBuilder`

## 当前输入来源

当前 contribution builder 直接复用：

- `BodyCandidatePartitionConservativeFixturePackageAuditExportService.Export(...)`

也就是它消费的是已经稳定跑通的“package-audit + validation 双输出”主链，而不是再绕回底层 fixture/workflow 自己重复拼装。

## 当前输出意图

- `Section`：用于后续统一 bundle / summary 汇总时携带稳定字段
- `SummaryMarkdownBlock`：用于后续统一 summary/readme 直接拼接
- `ReadmeMarkdownBlock`：当前先与 summary 复用同一块说明，后续若 bundle/readme 分流再拆

## 设计约束

- 这一层先只做“可汇总消费的薄封装”，暂不强行并入某个既有 validation bundle workflow。
- 等后续 stage-2 还有更多 validation/audit 子入口时，再决定是合并为统一 bundle workflow，还是维持 contribution 合流。
