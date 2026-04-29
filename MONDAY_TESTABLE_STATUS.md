# Monday Testable Status

## Current state

- Offline recognizer can batch-run against `xingcai` exported `member_*.json`
- Current best workflow output: [run_body_bracket_real_01_iter_beam_column_v1](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/batch-summary.json>)
- Verified packaged handoff: [tekla-body-bracket-testable-latest](</I:/autoteklasuanfa/.deliverables/tekla-body-bracket-testable-latest/README.md>)
- Main workflow doc: [OFFLINE_TEST_WORKFLOW.md](</I:/autoteklasuanfa/OFFLINE_TEST_WORKFLOW.md>)
- Current review queue: [review-queue.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/review-queue.csv>)
- Current pattern bucket summary: [pattern-bucket-summary.md](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/pattern-bucket-summary.md>)
- Current comparison vs previous baseline: [run-comparison.md](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/run-comparison.md>)
- Current compact representative audit sheet: [priority-review-pack.inline.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/priority-review-pack.inline.csv>)
- Current compact bracket answer sheet: [priority-bracket-answers.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/priority-bracket-answers.csv>)
- Current compact P1 answer sheet: [priority-bracket-answers.p1.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/priority-bracket-answers.p1.csv>)
- Current representative review bundle: [priority-review-bundle](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/priority-review-bundle/README.md>)
- Current compact P1 review bundle: [p1-review-bundle](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_iter_beam_column_v1/p1-review-bundle/README.md>)
- One-command rerun: [Run-OfflineRecognition.ps1](</I:/autoteklasuanfa/tools/Run-OfflineRecognition.ps1>)
- One-command artifact builder: [Build-ReviewArtifacts.ps1](</I:/autoteklasuanfa/tools/Build-ReviewArtifacts.ps1>)
- One-command packaged acceptance: [Run-MondayAcceptance.ps1](</I:/autoteklasuanfa/tools/Run-MondayAcceptance.ps1>)
- Acceptance guard defaults have been updated to the converged real-sample baseline in [Assert-RecognitionResult.ps1](</I:/autoteklasuanfa/tools/Assert-RecognitionResult.ps1>)

## Current measured improvement

- Assemblies: `246`
- Previous retained brackets: `16`
- Current retained brackets: `7`
- Previous retained positive assemblies: `16`
- Current retained positive assemblies: `7`
- Previous max bracket count per assembly: `1`
- Current max bracket count per assembly: `1`

## What changed in this pass

- Suppressed the remaining beam-side false positives on the current `GL` / horizontal-member bucket
- Suppressed the remaining `GKZ-15` / `GKZ-16` centered vertical plate-pair false positives
- Kept the `7` confirmed `GKZ` column positives:
  - `T2-3GKZ-17`
  - `T2-3GKZ-3`
  - `T2-3GKZ-4`
  - `T2-3GKZ-5`
  - `T2-3GKZ-6`
  - `T2-3GKZ-7`
  - `T2-3GKZ-8`

## Ready for Monday

- One-command offline batch run
- One-command packaged batch run
- One-command Monday acceptance
- One-command run comparison
- One-command review artifact generation for any future rerun
- Packaged build output refreshed against the converged `7 / 7` real-sample baseline

## Remaining blocker

- None for the current `246`-assembly real-sample set
- Optional next work is broader validation on new exports / new models, not more cleanup on this set
