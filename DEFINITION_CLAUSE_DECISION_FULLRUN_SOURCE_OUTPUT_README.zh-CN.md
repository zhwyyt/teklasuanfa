# DefinitionClauseDecision Full-Run Source Output README

## 目标

把 `fullrun-source` 导出目录也做成自描述的，这样后面一旦接到 `Program.cs` 的 full-run 主循环，拿到导出目录的人不需要再回到源码里猜每个文件是什么。

## 当前目录约定

当前 `DefinitionClauseDecisionFullRunSourceExportService.Export(...)` 会额外导出：

- `definition-clause-decision-fullrun-source-manifest.json`
- `README.md`

其中：

- `manifest.json` 面向脚本消费；
- `README.md` 面向人工 review；
- 二者都围绕：
  - `fullrun-source.json`
  - `fullrun-source.zh-CN.md`
  - `fullrun-source-validation.json`
  - `fullrun-source-validation.md`

## 价值

这一步不改变业务判定，但能确保下一轮真实接线后，导出目录本身就能解释：

- 哪些文件是输入工件
- 哪些文件是校验工件
- 它们的绝对路径在哪里
