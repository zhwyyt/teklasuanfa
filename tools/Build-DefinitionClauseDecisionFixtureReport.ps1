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

if (-not (Test-Path -LiteralPath $InputJsonPath)) {
    throw "InputJsonPath 不存在: $InputJsonPath"
}

$raw = Get-Content -LiteralPath $InputJsonPath -Raw -Encoding UTF8
$artifacts = $raw | ConvertFrom-Json
$results = @()
if ($null -ne $artifacts.Results) {
    $results = @($artifacts.Results)
}

$builder = New-Object System.Text.StringBuilder
[void]$builder.AppendLine("# DefinitionClauseDecision Fixture 报告")
[void]$builder.AppendLine()
[void]$builder.Append("总计：")
[void]$builder.Append($artifacts.TotalCount)
[void]$builder.Append("；通过：")
[void]$builder.Append($artifacts.PassedCount)
[void]$builder.Append("；失败：")
[void]$builder.AppendLine($artifacts.FailedCount)
[void]$builder.AppendLine()
[void]$builder.AppendLine("| Name | Passed | ActualClauseVerdict | ExpectedClauseVerdict | ActualClausePromotionReadiness | ExpectedClausePromotionReadiness |")
[void]$builder.AppendLine("| --- | --- | --- | --- | --- | --- |")

foreach ($result in $results) {
    [void]$builder.Append("| ")
    [void]$builder.Append((Escape-MarkdownCell $result.Name))
    [void]$builder.Append(" | ")
    [void]$builder.Append((if ($result.Passed) { "Yes" } else { "No" }))
    [void]$builder.Append(" | ")
    [void]$builder.Append((Escape-MarkdownCell $result.ActualClauseVerdictCode))
    [void]$builder.Append(" | ")
    [void]$builder.Append((Escape-MarkdownCell $result.ExpectedClauseVerdictCode))
    [void]$builder.Append(" | ")
    [void]$builder.Append((Escape-MarkdownCell $result.ActualClausePromotionReadinessCode))
    [void]$builder.Append(" | ")
    [void]$builder.Append((Escape-MarkdownCell $result.ExpectedClausePromotionReadinessCode))
    [void]$builder.AppendLine(" |")
}

$directory = Split-Path -Path $OutputMarkdownPath -Parent
if (-not [string]::IsNullOrWhiteSpace($directory) -and -not (Test-Path -LiteralPath $directory)) {
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
}

$utf8Bom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($OutputMarkdownPath, $builder.ToString().TrimEnd(), $utf8Bom)
