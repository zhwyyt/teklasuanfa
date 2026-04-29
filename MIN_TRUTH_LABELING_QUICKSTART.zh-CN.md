# 最小真值回填速查

## 最省事的做法

优先只填这两张小表：

- [priority-bracket-answers.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-bracket-answers.csv>)：`10` 条牛腿判断
- [priority-body-answers.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-body-answers.csv>)：`8` 条主体家族判断

填完以后，直接跑：

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Evaluate-PriorityAnswerSheets.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv `
  -BracketAnswersCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-bracket-answers.csv `
  -BodyAnswersCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-body-answers.csv
```

## 可接受的填写值

`HumanHasBracket` 可填：

- `yes/no`
- `是/否`
- `有/无`
- `有牛腿/无牛腿`

`HumanBodyFamily` 可填：

- 英文：`BuiltUpH`、`BuiltUpBox`、`BuiltUpT`、`BuiltUpCross`、`Irregular`
- 中文：`焊接H`、`箱型`、`T型`、`十字型`、`异形`

`HumanBracketPartGroups` 可填：

- part id 直接用逗号分隔，例如 `263024889,263025492`
- 也支持这些分隔符：`，`、`、`、`；`、`;`、`|`

## 如果你想先看上下文再填

先看这两份：

- [priority-review-checklist.md](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-review-checklist.md>)
- [priority-review-bundle](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-review-bundle/README.md>)

其中：

- checklist 把 `18` 个代表样本拆成 `10` 个牛腿优先确认和 `8` 个主体优先确认
- review bundle 带了样本源 `member JSON` 以及对应的 `input/recognition JSON`

## 如果你不想人工回填

那我现在最需要的下一批数据是：

- `6-10` 个明确有牛腿的 `GKZ` 紧凑双板对样本
- `6-10` 个明确没有牛腿、但接近 `GL / BuiltUpT + 角钢短板簇` 的硬负例

## 当前目标

当前离线交付版已经能稳定回放 `246` 个真实 assembly，并通过周一 `2026-04-20` 的打包验收脚本。现在继续收敛准确率的关键，就是这批最小真值。
