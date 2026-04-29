# P1 牛腿样本 6 行速填

如果你现在只愿意先补最关键的 `6` 个牛腿样本，直接填这张表：

- [priority-bracket-answers.p1.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-bracket-answers.p1.csv>)

这张 `6` 行表现在已经自带：

- `PredictedBracketProfiles`
- `PatternHint`
- `ReviewPromptZh`

也就是填表时不必再来回对照 `p1-review-summary`，可以直接在表内完成最小判断。

填完以后直接跑：

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Evaluate-P1BracketAnswers.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv `
  -P1BracketAnswersCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-bracket-answers.p1.csv
```

这条命令现在会同时产出：

- `priority-bracket-answers.p1.merged.csv`
- `priority-review-pack.p1.merged.csv`
- `p1-pattern-decision-summary.md`
- `p1-pattern-decision-summary.csv`

最后这对 `p1-pattern-decision-summary.*` 会把 `6` 行样本自动卷成模式级建议，例如“某一类可以安全压制”或“某一类不要压制”。

## 可接受的填写值

`HumanHasBracket` 可填：

- `yes/no`
- `是/否`
- `有/无`
- `有牛腿/无牛腿`

`HumanBracketPartGroups` 可填：

- part id 直接用逗号分隔，例如 `262317231,262317238`
- 也支持这些分隔符：`，`、`、`、`；`、`;`、`|`

`HumanBodyFamily` 可选填，不填也可以继续评估：

- `BuiltUpH`
- `BuiltUpBox`
- `BuiltUpT`
- `BuiltUpCross`
- `Irregular`
- 或中文别名：`焊接H`、`箱型`、`T型`、`十字型`、`异形`

## 这 6 个样本

- `262948748`
- `262960295`
- `262970541`
- `260925586`
- `260925609`
- `260925691`

前 `3` 个是 `GKZ` 真牛腿优先样本，后 `3` 个是 `BuiltUpT` 硬负例优先样本。
