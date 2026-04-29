# Offline Test Workflow

Quick Chinese guide for the smallest human-labeling path:

- [MIN_TRUTH_LABELING_QUICKSTART.zh-CN.md](</I:/autoteklasuanfa/MIN_TRUTH_LABELING_QUICKSTART.zh-CN.md>)
- [NEXT_EXPORT_REQUEST.zh-CN.md](</I:/autoteklasuanfa/NEXT_EXPORT_REQUEST.zh-CN.md>)
- [NEXT_EXPORT_REQUEST.csv](</I:/autoteklasuanfa/NEXT_EXPORT_REQUEST.csv>)
- [NEXT_EXPORT_REQUEST_P1_ONLY.zh-CN.md](</I:/autoteklasuanfa/NEXT_EXPORT_REQUEST_P1_ONLY.zh-CN.md>)
- [NEXT_EXPORT_REQUEST_P1_ONLY.csv](</I:/autoteklasuanfa/NEXT_EXPORT_REQUEST_P1_ONLY.csv>)
- [P1_BRACKET_QUICKSTART.zh-CN.md](</I:/autoteklasuanfa/P1_BRACKET_QUICKSTART.zh-CN.md>)
- [p1-review-bundle](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/p1-review-bundle/README.md>)
- [p1-review-summary.md](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/p1-review-bundle/p1-review-summary.md>)

One-command artifact builder:

- [Build-ReviewArtifacts.ps1](</I:/autoteklasuanfa/tools/Build-ReviewArtifacts.ps1>)

Recommended when you already have a new result directory and want to rebuild the review queue, comparison, priority pack, answer sheets, and P1 bundle in one step:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Build-ReviewArtifacts.ps1 `
  -ResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_latest `
  -BaselineResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow `
  -MemberCacheDirectory I:\xingcaisuanfa\cache\run_body_bracket_real_01\members
```

If you omit `-BaselineResultDirectory`, it defaults to the same directory as `-ResultDirectory`. That is fine when you only want regenerated review artifacts for one run, but use a stable baseline directory when you want meaningful `run-comparison.md` and body-change summaries.
The one-command builder now also generates `pattern-bucket-summary.md`, which estimates the blast radius for each remaining candidate bucket.

## 1. Run recognition on exported Tekla cache

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Run-OfflineRecognition.ps1 `
  -InputPath I:\xingcaisuanfa\cache\run_body_bracket_real_01\members `
  -OutputPath I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_latest `
  -GenerateReviewQueue
```

This will:

- build the offline recognizer in workspace-local paths
- analyze every `member_*.json`
- write `assembly-*.input.json` and `assembly-*.recognition.json`
- write `batch-summary.json`
- write `review-queue.csv`
- write `review-queue-summary.md`
- after running `Build-ReviewArtifacts.ps1`, also write `pattern-bucket-summary.md`

## 2. Fill human labels in `review-queue.csv`

Columns to fill:

- `HumanHasBracket`: `yes/no` are supported, and Chinese aliases such as `是/否`, `有/无`, `有牛腿/无牛腿` are also accepted
- `HumanBodyFamily`: for example `BuiltUpH`, `BuiltUpT`, `Irregular`; Chinese aliases such as `焊接H`, `箱型`, `T型`, `十字型`, `异形` are also accepted
- `HumanBracketPartGroups`: use the exact part-id groups if known, for example `261594522,261594528`
- `HumanVerdict`: short final judgment, for example `true-positive`, `false-positive`, `body-wrong`
- `Notes`: optional remarks

## 3. Evaluate against human labels

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Evaluate-ReviewQueue.ps1 `
  -ReviewQueueCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_latest\review-queue.csv
```

This prints:

- bracket precision / recall / F1 / accuracy
- body-family accuracy
- exact bracket-part-group match rate
- false positive list
- false negative list

## 4. Review the clustered candidate summary

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Summarize-ReviewQueue.ps1 `
  -ReviewQueueCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_latest\review-queue.csv
```

This writes `review-queue-summary.md`, which groups repeated candidate patterns.
For the current dataset, the `16` remaining candidate assemblies collapse to `9` unique pattern groups, so you can prioritize one representative per group first.

## 5. Compare two recognition runs

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Compare-RecognitionRuns.ps1 `
  -BaselineResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01 `
  -CandidateResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_latest
```

This writes `run-comparison.md`, which summarizes:

- bracket total reduction
- positive-assembly reduction
- max bracket-count reduction
- largest bracket reductions
- newly introduced positives
- body-family changes between two runs

## 6. Build a minimum review pack

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Build-PriorityReviewPack.ps1 `
  -ReviewQueueCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_latest\review-queue.csv `
  -BaselineResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01 `
  -CandidateResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_latest `
  -MemberCacheDirectory I:\xingcaisuanfa\cache\run_body_bracket_real_01\members
```

This writes `priority-review-pack.md`, which merges:

- remaining bracket-pattern representatives
- body-family change representatives
- combined unique assemblies that should be checked first

For the current dataset, the combined review pack is `18` representative assemblies.

## 7. Use the smallest current review sheet

Preferred current artifact:

- `priority-review-pack.inline.csv`

This file is the fastest manual review entry point for the current dataset because it flattens the `18` representative assemblies into one sheet with:

- `AssemblyId`
- source member file
- predicted body family and confidence
- predicted bracket count / bracket part groups
- body and bracket review reasons
- empty human-label columns for direct fill-in

For the current run, use:

- [priority-review-pack.inline.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-review-pack.inline.csv>)
- [priority-review-checklist.md](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-review-checklist.md>)
- [priority-bracket-answers.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-bracket-answers.csv>)
- [priority-body-answers.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-body-answers.csv>)
- [priority-review-bundle](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-review-bundle/README.md>)

If you prefer browsing files instead of only filling the sheet, build or open the review bundle:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Build-PriorityReviewBundle.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv `
  -ResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow `
  -OutputDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-bundle
```

If you want the smallest possible manual-input files, generate:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Build-PriorityAnswerSheets.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv `
  -OutputDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow
```

After filling those two smaller CSVs, merge them back into the main 18-row sheet:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Merge-PriorityAnswerSheets.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv `
  -BracketAnswersCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-bracket-answers.csv `
  -BodyAnswersCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-body-answers.csv `
  -OutputCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.merged.csv
```

Or do the merge and direct evaluation in one step:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Evaluate-PriorityAnswerSheets.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv `
  -BracketAnswersCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-bracket-answers.csv `
  -BodyAnswersCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-body-answers.csv
```

If you only want to fill the top `6` bracket-critical samples first, use:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Evaluate-P1BracketAnswers.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv `
  -P1BracketAnswersCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-bracket-answers.p1.csv
```

## 8. Evaluate the filled 18-row review sheet

If you filled `priority-review-pack.inline.csv`, you can evaluate it directly:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Evaluate-PriorityReviewPack.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv
```

The evaluation scripts now accept both the original English labels and common Chinese aliases, so you can fill the review sheets more naturally.

If you also want to copy the filled labels back into `review-queue.csv`:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Evaluate-PriorityReviewPack.ps1 `
  -PriorityReviewCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\priority-review-pack.inline.csv `
  -ReviewQueueCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\review-queue.csv `
  -OutputMergedCsv I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow\review-queue.merged.csv
```

## 9. Run acceptance checks on a result directory

To verify that a result directory still matches the current Monday baseline:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\tools\Assert-RecognitionResult.ps1 `
  -ResultDirectory I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_workflow
```

For the packaged build, there is also a one-command acceptance wrapper:

```powershell
powershell -ExecutionPolicy Bypass -File I:\autoteklasuanfa\.deliverables\tekla-body-bracket-testable-latest\tools\Run-MondayAcceptance.ps1 `
  -InputPath I:\xingcaisuanfa\cache\run_body_bracket_real_01\members
```

## Current known limitation

The remaining uncertain cases are mostly concentrated in:

- `GKZ` samples with compact plate-pair appendages
- some `GL` / `BuiltUpT` samples with angle-steel plus plate clusters

Those need human confirmation before further rule tightening, otherwise recall may be damaged.
