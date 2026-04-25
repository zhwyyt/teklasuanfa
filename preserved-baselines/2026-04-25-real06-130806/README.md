# Preserved Stable Baseline

- Baseline label: `2026-04-25 13:08:06`
- Source artifacts copied from:
  - `I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_06_current_check`
- User confirmation:
  - this run was manually checked and considered the basically correct version to preserve

## What is preserved here

- `body-family-proof.*`
- `definition-clause-decision-fullrun-source.*`
- `body-profile-resolution.*`

These files preserve the audited output surface that existed at the time the stable Excel was generated.

## Important boundary

The exact source-code snapshot that produced this run was **not** recoverable from Git history:

- current repository history only contains the initial commit
- there was no same-day commit, stash, or reflog entry for the `13:08:06` run
- the older `I:\autoteklasuanfa.rar` archive is an earlier heuristic-era codebase and does not reproduce this baseline

Because of that, this branch preserves the stable **artifacts** exactly, while source reconstruction continues separately.
