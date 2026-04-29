param(
    [string]$PackageRoot,

    [string]$ReferenceResultDirectory = "I:\autoteklasuanfa\.tmpresults\run_body_bracket_real_01_iter_beam_column_v1"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$packageDate = Get-Date -Format "yyyyMMdd-HHmmss"
if ([string]::IsNullOrWhiteSpace($PackageRoot))
{
    $PackageRoot = Join-Path $repoRoot ".deliverables\tekla-body-bracket-testable-$packageDate"
}

$packageRoot = [System.IO.Path]::GetFullPath($PackageRoot)
$appOutput = Join-Path $packageRoot "app"
$toolsOutput = Join-Path $packageRoot "tools"
$docsOutput = Join-Path $packageRoot "docs"
$resultsOutput = Join-Path $packageRoot "results"
$tmpAppData = Join-Path $repoRoot "tmpdata"
$tmpPackages = Join-Path $repoRoot ".nuget\packages"
$nugetConfig = Join-Path $repoRoot "NuGet.Config"
$appProject = Join-Path $repoRoot "src\TeklaBodyBracketRecognition.App\TeklaBodyBracketRecognition.App.csproj"

New-Item -ItemType Directory -Force -Path $packageRoot, $appOutput, $toolsOutput, $docsOutput, $resultsOutput, $tmpAppData, $tmpPackages | Out-Null

$env:APPDATA = $tmpAppData
$env:NUGET_PACKAGES = $tmpPackages

dotnet build $appProject `
    -c Release `
    -o $appOutput `
    --configfile $nugetConfig `
    -p:CodexWorkspaceBuildPaths=true

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

$toolFiles = @(
    "Run-PackagedRecognition.ps1",
    "Run-OfflineRecognition.ps1",
    "Assert-RecognitionResult.ps1",
    "Run-MondayAcceptance.ps1",
    "Build-ReviewArtifacts.ps1",
    "New-ReviewQueue.ps1",
    "Apply-PriorityReviewLabels.ps1",
    "Merge-PriorityAnswerSheets.ps1",
    "Evaluate-ReviewQueue.ps1",
    "Evaluate-PriorityReviewPack.ps1",
    "Evaluate-PriorityAnswerSheets.ps1",
    "Evaluate-P1BracketAnswers.ps1",
    "Evaluate-BuiltUpTReviewBundle.ps1",
    "Smoke-TestReviewFlows.ps1",
    "Summarize-P1PatternDecisions.ps1",
    "Build-BuiltUpTReviewBundle.ps1",
    "Build-PriorityReviewCsv.ps1",
    "Build-PriorityAnswerSheets.ps1",
    "Build-P1BracketAnswerSheet.ps1",
    "Build-PatternBucketSummary.ps1",
    "Build-TopologyRewriteSummary.ps1",
    "Build-PriorityReviewChecklist.ps1",
    "Build-PriorityReviewBundle.ps1",
    "Build-P1ReviewBundle.ps1",
    "Build-P1ReviewSummary.ps1",
    "Summarize-ReviewQueue.ps1",
    "Compare-RecognitionRuns.ps1",
    "Summarize-BodyChanges.ps1",
    "Build-PriorityReviewPack.ps1"
)

foreach ($toolFile in $toolFiles)
{
    Copy-Item (Join-Path $repoRoot "tools\$toolFile") (Join-Path $toolsOutput $toolFile) -Force
}

$docFiles = @(
    "OFFLINE_TEST_WORKFLOW.md",
    "MIN_TRUTH_LABELING_QUICKSTART.zh-CN.md",
    "NEXT_EXPORT_REQUEST.zh-CN.md",
    "NEXT_EXPORT_REQUEST.csv",
    "NEXT_EXPORT_REQUEST_P1_ONLY.zh-CN.md",
    "NEXT_EXPORT_REQUEST_P1_ONLY.csv",
    "NEXT_RULE_HOOKS.md",
    "BUILTUPT_P1_QUICKSTART.zh-CN.md",
    "P1_BRACKET_QUICKSTART.zh-CN.md",
    "tekla_body_bracket_algorithm_design.md"
)

foreach ($docFile in $docFiles)
{
    Copy-Item (Join-Path $repoRoot $docFile) (Join-Path $docsOutput $docFile) -Force
}

if (Test-Path $ReferenceResultDirectory)
{
    $referenceResultDirectory = (Resolve-Path $ReferenceResultDirectory).Path
    $buildPriorityReviewBundleScript = Join-Path $repoRoot "tools\Build-PriorityReviewBundle.ps1"
    $buildP1ReviewBundleScript = Join-Path $repoRoot "tools\Build-P1ReviewBundle.ps1"
    $buildBuiltUpTReviewBundleScript = Join-Path $repoRoot "tools\Build-BuiltUpTReviewBundle.ps1"
    $buildTopologyRewriteSummaryScript = Join-Path $repoRoot "tools\Build-TopologyRewriteSummary.ps1"
    $referenceFiles = @(
        "batch-summary.json",
        "review-queue.csv",
        "review-queue-summary.md",
        "body-change-summary.md",
        "run-comparison.md",
        "pattern-bucket-summary.md",
        "topology-rewrite-summary.zh-CN.md",
        "priority-review-pack.md",
        "priority-review-pack.inline.csv",
        "priority-review-checklist.md",
        "priority-bracket-answers.csv",
        "priority-bracket-answers.p1.csv",
        "priority-bracket-answers.p1.builtupt-first.csv",
        "priority-body-answers.csv"
    )

    foreach ($name in $referenceFiles)
    {
        $source = Join-Path $referenceResultDirectory $name
        if (Test-Path $source)
        {
            Copy-Item $source (Join-Path $resultsOutput $name) -Force
        }
    }

    $coreBodyProofRealInputJson = Join-Path $referenceResultDirectory "core-body-proof-real-input.json"
    if ((Test-Path $buildTopologyRewriteSummaryScript) -and (Test-Path $coreBodyProofRealInputJson))
    {
        & $buildTopologyRewriteSummaryScript `
            -CoreBodyProofRealInputJson $coreBodyProofRealInputJson `
            -OutputMarkdown (Join-Path $resultsOutput "topology-rewrite-summary.zh-CN.md") | Out-Null
    }

    $priorityBundleDirectory = Join-Path $referenceResultDirectory "priority-review-bundle"
    if (Test-Path $priorityBundleDirectory)
    {
        $priorityBundleOutput = Join-Path $resultsOutput "priority-review-bundle"
        if (Test-Path $priorityBundleOutput)
        {
            Remove-Item $priorityBundleOutput -Recurse -Force
        }

        Copy-Item $priorityBundleDirectory $priorityBundleOutput -Recurse -Force
    }

    $p1BundleDirectory = Join-Path $referenceResultDirectory "p1-review-bundle"
    if (Test-Path $p1BundleDirectory)
    {
        $p1BundleOutput = Join-Path $resultsOutput "p1-review-bundle"
        if (Test-Path $p1BundleOutput)
        {
            Remove-Item $p1BundleOutput -Recurse -Force
        }

        Copy-Item $p1BundleDirectory $p1BundleOutput -Recurse -Force
    }

    $priorityReviewCsv = Join-Path $referenceResultDirectory "priority-review-pack.inline.csv"
    if ((Test-Path $buildPriorityReviewBundleScript) -and (Test-Path $priorityReviewCsv))
    {
        & $buildPriorityReviewBundleScript `
            -PriorityReviewCsv $priorityReviewCsv `
            -ResultDirectory $referenceResultDirectory `
            -OutputDirectory (Join-Path $resultsOutput "priority-review-bundle") | Out-Null
    }

    $p1BracketAnswersCsv = Join-Path $referenceResultDirectory "priority-bracket-answers.p1.csv"
    $p1Rows = @()
    if (Test-Path $p1BracketAnswersCsv)
    {
        $p1Rows = @(Import-Csv $p1BracketAnswersCsv)
    }

    if ((Test-Path $buildP1ReviewBundleScript) -and ($p1Rows.Count -gt 0))
    {
        & $buildP1ReviewBundleScript `
            -P1BracketAnswersCsv $p1BracketAnswersCsv `
            -ResultDirectory $referenceResultDirectory `
            -OutputDirectory (Join-Path $resultsOutput "p1-review-bundle") | Out-Null
    }

    $builtUpTBracketAnswersCsv = Join-Path $referenceResultDirectory "priority-bracket-answers.p1.builtupt-first.csv"
    $builtUpTRows = @()
    if (Test-Path $builtUpTBracketAnswersCsv)
    {
        $builtUpTRows = @(Import-Csv $builtUpTBracketAnswersCsv)
    }

    if ((Test-Path $buildBuiltUpTReviewBundleScript) -and ($builtUpTRows.Count -gt 0))
    {
        & $buildBuiltUpTReviewBundleScript `
            -BuiltUpTBracketAnswersCsv $builtUpTBracketAnswersCsv `
            -ResultDirectory $referenceResultDirectory `
            -OutputDirectory (Join-Path $resultsOutput "builtupt-review-bundle") | Out-Null
    }
}

$readmePath = Join-Path $packageRoot "README.md"
$readmeLines = @(
    "# Tekla Body/Bracket Testable Package",
    "",
    "## Quick start",
    "",
    "Run packaged recognition:",
    "",
    '```powershell',
    "powershell -ExecutionPolicy Bypass -File .\\tools\\Run-PackagedRecognition.ps1 -InputPath I:\\xingcaisuanfa\\cache\\run_body_bracket_real_01\\members -OutputPath .\\results\\run_body_bracket_real_01_retest -GenerateReviewQueue",
    '```',
    "",
    "Run one-command Monday acceptance:",
    "",
    '```powershell',
    "powershell -ExecutionPolicy Bypass -File .\\tools\\Run-MondayAcceptance.ps1 -InputPath I:\\xingcaisuanfa\\cache\\run_body_bracket_real_01\\members",
    '```',
    "",
    "Optional deeper review docs:",
    "",
    '- `docs\\PACKAGE_LOCAL_QUICKSTART.zh-CN.md`: package-local commands for rerun, acceptance, and current-result audit',
    '- `docs\\OFFLINE_TEST_WORKFLOW.md`: offline rerun / compare / review workflow reference',
    '- `docs\\MIN_TRUTH_LABELING_QUICKSTART.zh-CN.md`: archived minimal-truth guide for future model expansion',
    '- `docs\\NEXT_RULE_HOOKS.md`: current rule hook notes and future extension points',
    "",
    "## Included content",
    "",
    '- `app\\`: packaged recognizer binaries',
    '- `tools\\`: run / review / compare scripts',
    '- `docs\\`: workflow and design references',
    '- `results\\`: current best offline baseline summaries',
    '- `tools\\Assert-RecognitionResult.ps1`: acceptance guard for key summary invariants',
    '- `tools\\Run-MondayAcceptance.ps1`: one-command packaged run plus acceptance check',
    '- `tools\\Build-ReviewArtifacts.ps1`: one-command generation of review queue, priority pack, answer sheets, and P1 bundle for a result directory',
    '- `tools\\Merge-PriorityAnswerSheets.ps1`: merge bracket/body answer sheets back into the inline review sheet when future truth is added',
    '- `tools\\Evaluate-PriorityAnswerSheets.ps1`: merge the two smallest answer sheets and evaluate in one step',
    '- `tools\\Evaluate-P1BracketAnswers.ps1`: merge the compact P1 bracket sheet into the full review flow and evaluate in one step',
    '- `tools\\Smoke-TestReviewFlows.ps1`: smoke-test the review/evaluation flows when compact review sheets are present',
    '- `tools\\Summarize-P1PatternDecisions.ps1`: roll filled compact P1 sheets up to pattern-level recommendations',
    '- `tools\\Build-PriorityReviewCsv.ps1`: convert the priority review markdown pack into the editable inline CSV',
    '- `tools\\Build-P1BracketAnswerSheet.ps1`: filter the bracket answer sheet down to the current compact P1 subset',
    '- `tools\\Build-PatternBucketSummary.ps1`: group remaining candidates into pattern buckets and estimate suppression blast radius',
    '- `tools\\Build-TopologyRewriteSummary.ps1`: rebuild stage-5 `TopologyRewrite` evidence into a standalone review summary',
    '- `tools\\Build-P1ReviewBundle.ps1`: rebuild the compact P1 review bundle from the current inline review set',
    '- `tools\\Build-P1ReviewSummary.ps1`: extract member hints and recognition reasons into a compact P1 comparison table',
    '- `docs\\MIN_TRUTH_LABELING_QUICKSTART.zh-CN.md`: archived minimal truth-labeling guide for future extension work',
    '- `docs\\NEXT_RULE_HOOKS.md`: note for the current rule hook points and future extension work',
    '- `docs\\PACKAGE_LOCAL_QUICKSTART.zh-CN.md`: package-local quickstart for the copied result CSVs and tools',
    '- `results\\review-queue.csv`: current retained positives after convergence on the 246-assembly real set',
    '- `results\\priority-review-pack.inline.csv`: current compact representative audit sheet',
    '- `results\\pattern-bucket-summary.md`: current pattern-bucket summary with blast-radius estimates',
    '- `results\\topology-rewrite-summary.zh-CN.md`: current stage-5 主截面改写模式摘要，现已同时展示 `ControllerRole / ShapeRole / FamilyRisk / ProofType / FamilyProofTarget / DefinitionClause`，便于区分 GKZ / HXZ 的改写类型、主轮廓证明对象、下一步定义验证目标与更具体的条款提示',
    '- `results\\priority-review-checklist.md`: current compact audit checklist generated from the representative review set',
    '- `results\\priority-bracket-answers.csv`: compact bracket answer sheet generated from the current representative review set',
    '- `results\\priority-bracket-answers.p1.csv`: compact P1 answer sheet generated from the current representative review set',
    '- `results\\priority-body-answers.csv`: compact body-family answer sheet when body changes are present',
    '- `results\\p1-review-bundle\\`: compact P1 review bundle generated from the current representative review set',
    '- `results\\priority-review-bundle\\`: representative review bundle generated from the current representative review set',
    '- review/evaluation scripts accept both English labels and common Chinese aliases such as `是/否`, `有/无`, `焊接H`, `箱型`, `异形`',
    "",
    "## Current validation state",
    "",
    '- No open blocker remains for the current 246-assembly real-sample set',
    '- Current retained positives: `7` assemblies / `7` brackets, all on `GKZ` column samples',
    '- The previously remaining `GL` beam-side candidates and `GKZ-15` / `GKZ-16` have been suppressed in this package baseline',
    '- Optional next work is broader model validation on new exports, not more cleanup on the current set'
)

Set-Content -Path $readmePath -Value $readmeLines -Encoding UTF8

$packageLocalQuickstartPath = Join-Path $docsOutput "PACKAGE_LOCAL_QUICKSTART.zh-CN.md"
$packageLocalQuickstartLines = @(
    "# 交付包本地速用",
    "",
    "下面这些命令都以交付包根目录为当前目录来执行，不依赖工作区源码路径。",
    "",
    "## 1. 当前包内基线已经收敛",
    "",
    "当前 results 目录已经对应最新收敛版真实样本结果：246 个 assembly 里保留 7 个牛腿正例，全部来自 GKZ 柱样本。",
    "如果你只是要复核当前结论，优先看：",
    "",
    '- `results\\review-queue.csv`',
    '- `results\\pattern-bucket-summary.md`',
    '- `results\\topology-rewrite-summary.zh-CN.md`（现已带 `ProofType + FamilyProofTarget + DefinitionClause`）',
    '- `results\\priority-review-pack.inline.csv`',
    "",
    "## 2. 直接重跑当前收敛版",
    "",
    '```powershell',
    "powershell -ExecutionPolicy Bypass -File .\\tools\\Run-PackagedRecognition.ps1 -InputPath I:\\xingcaisuanfa\\cache\\run_body_bracket_real_01\\members -OutputPath .\\results\\run_body_bracket_real_01_retest -GenerateReviewQueue",
    '```',
    "",
    "## 3. 重新跑周一验收",
    "",
    '```powershell',
    "powershell -ExecutionPolicy Bypass -File .\\tools\\Run-MondayAcceptance.ps1 -InputPath I:\\xingcaisuanfa\\cache\\run_body_bracket_real_01\\members",
    '```',
    "",
    "## 4. 对一个结果目录一键生成复核产物",
    "",
    '如果你想同时看包内基线和新结果的差异，推荐把 `-BaselineResultDirectory` 指向包里的 `results\\monday-acceptance-check`。',
    '如果你只是想给单个结果目录补齐 review queue / priority pack / answer sheets / P1 bundle，也可以省略这个参数，它会默认回退到 `-ResultDirectory` 本身。',
    "",
    '```powershell',
    "powershell -ExecutionPolicy Bypass -File .\\tools\\Build-ReviewArtifacts.ps1 -ResultDirectory .\\results\\run_body_bracket_real_01_retest -BaselineResultDirectory .\\results\\monday-acceptance-check -MemberCacheDirectory I:\\xingcaisuanfa\\cache\\run_body_bracket_real_01\\members",
    '```',
    "",
    "这条命令现在还会一起生成：",
    "",
    '- `results\\run_body_bracket_real_01_retest\\pattern-bucket-summary.md`',
    '- `results\\run_body_bracket_real_01_retest\\topology-rewrite-summary.zh-CN.md`（现已带 `ProofType + FamilyProofTarget + DefinitionClause`）',
    '- `results\\run_body_bracket_real_01_retest\\priority-review-pack.inline.csv`',
    "",
    "## 5. 如果后续有新增残留，再用这些填写值",
    "",
    '- `HumanHasBracket`: `yes/no`、`是/否`、`有/无`、`有牛腿/无牛腿`',
    '- `HumanBodyFamily`: `BuiltUpH`、`BuiltUpBox`、`BuiltUpT`、`BuiltUpCross`、`Irregular`，或中文别名 `焊接H`、`箱型`、`T型`、`十字型`、`异形`',
    '- `HumanBracketPartGroups`: 支持 `,`、`，`、`、`、`；`、`;`、`|` 分隔'
)
Set-Content -Path $packageLocalQuickstartPath -Value $packageLocalQuickstartLines -Encoding UTF8

Write-Host "Testable package written to: $packageRoot"
