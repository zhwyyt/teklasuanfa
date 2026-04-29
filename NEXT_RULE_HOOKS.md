# Next Rule Hooks

This note records where the next bracket-pattern rules should land once the `P1` truth labels are confirmed.

## Current pattern buckets

Current remaining bracket candidates on the `246`-assembly workflow run fall into four buckets:

- `compact_gkz_plate_pair`: `9` assemblies
  `262970541, 262971960, 262939757, 262960295, 262938086, 262948748, 262974096, 262968948, 262967188`
- `angle_short_plate_cluster`: `3` assemblies
  `260925691, 260925586, 260925609`
- `single_plate_appendage`: `2` assemblies
  `263004765, 263112220`
- `irregular_misc`: `2` assemblies
  `260923860, 260923319`

The most important implication is blast radius:

- the `BuiltUpT` angle-short-plate pattern currently affects only `3` assemblies, so it is a good low-risk first suppression candidate if those `3` are confirmed false positives
- the `GKZ` compact-plate-pair pattern affects `9` assemblies, so it should not be suppressed without truth labels

The current lowest-cost truth path is now aligned to that blast radius:

- confirm the `3` `BuiltUpT` assemblies first:
  `260925586, 260925609, 260925691`
- then confirm the `3` `GKZ` assemblies:
  `262948748, 262960295, 262970541`

## Recommended hook points

Preferred rule-insertion points in the current code:

- cluster-level decision gate:
  [BracketRecognizer.TryClassifyCluster](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BracketRecognizer.cs:101>)
- candidate prefilter:
  [BracketRecognizer.IsAppendageCandidate](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BracketRecognizer.cs:213>)
- current review/suppression signals:
  [BracketRecognizer.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Algorithms/BracketRecognizer.cs:129>)
- result payload fields:
  [Results.cs](</I:/autoteklasuanfa/src/TeklaBodyBracketRecognition.Core/Domain/Results.cs:31>)

Preferred strategy:

- put pattern-specific suppression or confidence shaping inside `TryClassifyCluster`
- avoid putting these pattern rules into `IsAppendageCandidate` unless a pattern is globally safe to discard before clustering
- keep the pattern decision explainable by appending a dedicated reason into `ClassificationReasons` or `ReviewReasons`

## Recommended next rules

### 1. BuiltUpT angle-short-plate cluster

Pattern:

- body family is `BuiltUpT`
- bracket cluster part count is `>= 4`
- part-name summary is dominated by `MQMJ` or `MQB-`
- profile summary contains `CC52.5-4-15-34` and `PL12*130`

Current coverage:

- `260925586`
- `260925609`
- `260925691`

Recommended action after truth confirmation:

- if these are confirmed false positives, add a guarded suppression branch inside `TryClassifyCluster` before the current `isBracket` check
- emit a dedicated reason such as `PATTERN_ANGLE_SHORT_PLATE_CLUSTER`
- keep the rule narrow to `BuiltUpT + MQMJ/MQB + CC52.5-4-15-34 + PL12*130`

Why this is a good first rule:

- it only affects `3` currently predicted positives
- it is structurally different from the `GKZ` plate-pair group

### 2. GKZ compact plate pair

Pattern:

- source/member pattern is `GKZ`
- predicted body family is `BuiltUpH`
- cluster part count is exactly `2`
- name summary is `GKZ1325x2`
- profile summary looks like `PLxx*100 + PLxx*250/300/400`

Current coverage:

- `262970541`
- `262971960`
- `262939757`
- `262960295`
- `262938086`
- `262948748`
- `262974096`
- `262968948`
- `262967188`

Recommended action after truth confirmation:

- if these are confirmed true positives, do not suppress them
- optionally add a confidence boost or a keep-alive rule inside `TryClassifyCluster`
- emit a dedicated reason such as `PATTERN_COMPACT_GKZ_PLATE_PAIR`

Why this must wait for truth:

- the blast radius is `9` assemblies
- a wrong suppression here would remove most of the remaining bracket candidates at once

### 3. Single-plate appendage / irregular misc

Current assemblies:

- single plate: `263004765`, `263112220`
- irregular misc: `260923860`, `260923319`

Recommended action:

- keep these under manual review for now
- do not generalize rules from them until more truth is available

## Practical next step

Still the most valuable next input:

- lowest-cost first: confirm these `3` `BuiltUpT` assemblies:
  `260925586, 260925609, 260925691`
- then confirm the remaining `3` `GKZ` `P1` assemblies:
  `262948748, 262960295, 262970541`

Once those are confirmed, the safest implementation order is:

1. decide whether `angle_short_plate_cluster` can be suppressed
2. decide whether `compact_gkz_plate_pair` should be protected/boosted
3. leave the remaining `4` non-P1 assemblies on manual review unless new truth expands coverage
