# BuiltUpT 3 行优先速填

如果你现在只愿意先补最低风险、最关键的一小部分，先填这张 `3` 行表：

- [priority-bracket-answers.p1.builtupt-first.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-bracket-answers.p1.builtupt-first.csv>)

这 `3` 条样本全部属于 `BuiltUpT` 上的 `angle_short_plate_cluster` 模式，也就是当前最适合先确认的“角钢/短板簇是否只是局部附件”问题。

填完以后直接跑：

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Evaluate-BuiltUpTReviewBundle.ps1 `
  -ResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow
```

这条命令会继续产出：

- `priority-bracket-answers.p1.builtupt-first.merged.csv`
- `priority-review-pack.p1.builtupt-first.merged.csv`
- `builtupt-pattern-decision-summary.md`
- `builtupt-pattern-decision-summary.csv`

如果这 `3` 条都被确认成“无牛腿”，那么当前 `angle_short_plate_cluster` 这一桶就很接近可以做窄幅压制。

这 `3` 个 assembly 是：

- `260925586`
- `260925609`
- `260925691`
