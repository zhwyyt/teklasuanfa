param(
    [Parameter(Mandatory = $true)]
    [string]$InputJsonPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputMarkdownPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Escape-MarkdownCell {
    param([string]$Text)

    if ($null -eq $Text) {
        return ""
    }

    return ($Text -replace '\|', '\|') -replace "(`r`n|`n|`r)", ' '
}

function Build-ReviewTable {
    param([object[]]$Rows)

    if ($null -eq $Rows -or $Rows.Count -eq 0) {
        return "无 DefinitionClauseDecision review 结果。"
    }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("| MemberId | AssemblyId | LeadClauseVerdict | LeadClausePromotionReadiness | ClauseVerdictMixStatus | ClausePromotionReadinessMixStatus | ReviewHint |")
    $lines.Add("| --- | --- | --- | --- | --- | --- | --- |")

    foreach ($row in $Rows) {
        $lines.Add("| {0} | {1} | {2} | {3} | {4} | {5} | {6} |" -f `
            (Escape-MarkdownCell $row.MemberId), `
            (Escape-MarkdownCell $row.AssemblyId), `
            (Escape-MarkdownCell $row.LeadClauseVerdictLabelZh), `
            (Escape-MarkdownCell $row.LeadClausePromotionReadinessLabelZh), `
            (Escape-MarkdownCell $row.ClauseVerdictMixStatus), `
            (Escape-MarkdownCell $row.ClausePromotionReadinessMixStatus), `
            (Escape-MarkdownCell $row.ReviewHint))
    }

    return ($lines -join [Environment]::NewLine)
}

function Build-RepresentativeTable {
    param([object[]]$Rows)

    if ($null -eq $Rows -or $Rows.Count -eq 0) {
        return "无代表零件条款判定。"
    }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("| MemberId | PartId | DefinitionClause | DefinitionClauseEffect | ClauseVerdict | ClausePromotionReadiness | ReviewHint |")
    $lines.Add("| --- | --- | --- | --- | --- | --- | --- |")

    foreach ($row in $Rows) {
        $lines.Add("| {0} | {1} | {2} | {3} | {4} | {5} | {6} |" -f `
            (Escape-MarkdownCell $row.MemberId), `
            (Escape-MarkdownCell $row.PartId), `
            (Escape-MarkdownCell $row.DefinitionClauseLabelZh), `
            (Escape-MarkdownCell $row.DefinitionClauseEffectLabelZh), `
            (Escape-MarkdownCell $row.ClauseVerdictLabelZh), `
            (Escape-MarkdownCell $row.ClausePromotionReadinessLabelZh), `
            (Escape-MarkdownCell $row.ReviewHint))
    }

    return ($lines -join [Environment]::NewLine)
}

if (-not (Test-Path -LiteralPath $InputJsonPath)) {
    throw "InputJsonPath 不存在: $InputJsonPath"
}

$raw = Get-Content -LiteralPath $InputJsonPath -Raw -Encoding UTF8
$artifacts = $raw | ConvertFrom-Json

$reviewRows = @()
if ($null -ne $artifacts.ReviewRows) {
    $reviewRows = @($artifacts.ReviewRows)
}

$representativeRows = @()
if ($null -ne $artifacts.RepresentativeRows) {
    $representativeRows = @($artifacts.RepresentativeRows)
}

$builder = New-Object System.Text.StringBuilder
[void]$builder.AppendLine("# DefinitionClauseDecision 摘要")
[void]$builder.AppendLine()
[void]$builder.AppendLine("## 构件级 Review")
[void]$builder.AppendLine()
[void]$builder.AppendLine((Build-ReviewTable -Rows $reviewRows))
[void]$builder.AppendLine()
[void]$builder.AppendLine("## 代表零件")
[void]$builder.AppendLine()
[void]$builder.AppendLine((Build-RepresentativeTable -Rows $representativeRows))

$directory = Split-Path -Path $OutputMarkdownPath -Parent
if (-not [string]::IsNullOrWhiteSpace($directory) -and -not (Test-Path -LiteralPath $directory)) {
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
}

$utf8Bom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($OutputMarkdownPath, $builder.ToString().TrimEnd(), $utf8Bom)
