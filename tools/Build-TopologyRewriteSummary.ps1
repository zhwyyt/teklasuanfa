param(
    [Parameter(Mandatory = $true)]
    [string]$CoreBodyProofRealInputJson,

    [string]$OutputMarkdown
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Escape-MarkdownCell {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    return $Value.Replace("`r", "").Replace("`n", "<br>").Replace("|", "\|")
}

function Join-UniqueLabels {
    param([object[]]$Values)

    $filtered = @(
        $Values |
            Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
            Select-Object -Unique
    )

    return ($filtered -join " / ")
}

function Get-UniqueNonNoneCodeCount {
    param(
        [object[]]$Rows,
        [string]$PropertyName
    )

    $values = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($row in $Rows) {
        $property = $row.PSObject.Properties[$PropertyName]
        if ($null -eq $property) {
            continue
        }

        $value = [string]($property.Value)
        if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "NONE") {
            continue
        }

        [void]$values.Add($value)
    }

    return $values.Count
}

function Get-ClauseMixSummary {
    param([object[]]$GroupRows)

    $totalTopologyCount = 0
    foreach ($row in $GroupRows) {
        $totalTopologyCount += @($row.TopologyRewrite).Count
    }

    $nonNoneRows = @()
    foreach ($row in $GroupRows) {
        if ([string]($row.DefinitionClauseCode) -ne "NONE") {
            $nonNoneRows += $row
        }
    }

    if ($nonNoneRows.Count -eq 0) {
        return [pscustomobject]@{
            LeadDefinitionClause = "无主条款"
            LeadDefinitionClauseShare = ("0/{0} (0%)" -f $totalTopologyCount)
            ClauseMixStatus = "无条款提示"
        }
    }

    $leadClause = $null
    foreach ($group in @($nonNoneRows | Group-Object DefinitionClauseCode)) {
        $weight = 0
        foreach ($item in @($group.Group)) {
            $weight += @($item.TopologyRewrite).Count
        }

        $first = $group.Group[0]
        $candidate = [pscustomobject]@{
            DefinitionClauseCode = [string]($first.DefinitionClauseCode)
            DefinitionClauseLabel = [string]($first.DefinitionClauseLabel)
            Weight = $weight
        }

        $isBetterWeight = $false
        $isSameWeightButEarlierCode = $false
        if ($null -ne $leadClause) {
            $isBetterWeight = $candidate.Weight -gt $leadClause.Weight
            $isSameWeightButEarlierCode =
                $candidate.Weight -eq $leadClause.Weight -and
                ([string]($candidate.DefinitionClauseCode)) -lt ([string]($leadClause.DefinitionClauseCode))
        }

        if ($null -eq $leadClause -or $isBetterWeight -or $isSameWeightButEarlierCode) {
            $leadClause = $candidate
        }
    }

    if ($totalTopologyCount -le 0) {
        $leadShare = 0.0
    }
    else {
        $leadShare = $leadClause.Weight / [double]$totalTopologyCount
    }

    $hasNoClauseRows = $nonNoneRows.Count -ne $GroupRows.Count
    $nonNoneClauseCount = Get-UniqueNonNoneCodeCount $nonNoneRows "DefinitionClauseCode"
    $nonNoneTargetCount = Get-UniqueNonNoneCodeCount $nonNoneRows "FamilyProofTargetCode"
    $nonNoneShapeRoleCount = Get-UniqueNonNoneCodeCount $nonNoneRows "ShapeRoleCode"
    $nonNonePatternCount = Get-UniqueNonNoneCodeCount $nonNoneRows "PatternCode"
    $nonNoneControllerRoleCount = Get-UniqueNonNoneCodeCount $nonNoneRows "ControllerRoleCode"

    if ($hasNoClauseRows) {
        $clauseMixStatus = "主条款占优，但仍有未定零件"
    }
    elseif ($nonNoneClauseCount -eq 1 -and
            $nonNoneTargetCount -le 1 -and
            $nonNoneShapeRoleCount -le 1 -and
            $nonNonePatternCount -le 1 -and
            $nonNoneControllerRoleCount -le 1) {
        $clauseMixStatus = "单条款稳定"
    }
    elseif ($nonNoneClauseCount -eq 1) {
        $clauseMixStatus = "同条款多形态混合"
    }
    elseif ($leadShare -ge 0.6) {
        $clauseMixStatus = "主条款占优，但仍属混合改写"
    }
    else {
        $clauseMixStatus = "多条款并列，暂不直判"
    }

    return [pscustomobject]@{
        LeadDefinitionClause = [string]($leadClause.DefinitionClauseLabel)
        LeadDefinitionClauseShare = ("{0}/{1} ({2})" -f $leadClause.Weight, $totalTopologyCount, $leadShare.ToString("0%", [System.Globalization.CultureInfo]::InvariantCulture))
        ClauseMixStatus = $clauseMixStatus
    }
}

function Get-ClauseEffectMixSummary {
    param([object[]]$GroupRows)

    $totalTopologyCount = 0
    foreach ($row in $GroupRows) {
        $totalTopologyCount += @($row.TopologyRewrite).Count
    }

    $nonNoneRows = @()
    foreach ($row in $GroupRows) {
        if ([string]($row.DefinitionClauseEffectCode) -ne "NONE") {
            $nonNoneRows += $row
        }
    }

    if ($nonNoneRows.Count -eq 0) {
        return [pscustomobject]@{
            LeadDefinitionClauseEffect = "无主条款效果"
            LeadDefinitionClauseEffectShare = ("0/{0} (0%)" -f $totalTopologyCount)
            ClauseEffectMixStatus = "无条款效果提示"
        }
    }

    $leadEffect = $null
    foreach ($group in @($nonNoneRows | Group-Object DefinitionClauseEffectCode)) {
        $weight = 0
        foreach ($item in @($group.Group)) {
            $weight += @($item.TopologyRewrite).Count
        }

        $first = $group.Group[0]
        $candidate = [pscustomobject]@{
            DefinitionClauseEffectCode = [string]($first.DefinitionClauseEffectCode)
            DefinitionClauseEffectLabel = [string]($first.DefinitionClauseEffectLabel)
            Weight = $weight
        }

        $isBetterWeight = $false
        $isSameWeightButEarlierCode = $false
        if ($null -ne $leadEffect) {
            $isBetterWeight = $candidate.Weight -gt $leadEffect.Weight
            $isSameWeightButEarlierCode =
                $candidate.Weight -eq $leadEffect.Weight -and
                ([string]($candidate.DefinitionClauseEffectCode)) -lt ([string]($leadEffect.DefinitionClauseEffectCode))
        }

        if ($null -eq $leadEffect -or $isBetterWeight -or $isSameWeightButEarlierCode) {
            $leadEffect = $candidate
        }
    }

    if ($totalTopologyCount -le 0) {
        $leadShare = 0.0
    }
    else {
        $leadShare = $leadEffect.Weight / [double]$totalTopologyCount
    }

    $hasNoEffectRows = $nonNoneRows.Count -ne $GroupRows.Count
    $nonNoneEffectCount = Get-UniqueNonNoneCodeCount $nonNoneRows "DefinitionClauseEffectCode"
    $nonNoneClauseCount = Get-UniqueNonNoneCodeCount $nonNoneRows "DefinitionClauseCode"

    if ($hasNoEffectRows) {
        $clauseEffectMixStatus = "主条款效果占优，但仍有未定零件"
    }
    elseif ($nonNoneEffectCount -eq 1 -and $nonNoneClauseCount -le 1) {
        $clauseEffectMixStatus = "单条款效果稳定"
    }
    elseif ($nonNoneEffectCount -eq 1) {
        $clauseEffectMixStatus = "同条款效果多形态混合"
    }
    elseif ($leadShare -ge 0.6) {
        $clauseEffectMixStatus = "主条款效果占优，但仍属混合改写"
    }
    else {
        $clauseEffectMixStatus = "多条款效果并列，暂不直判"
    }

    return [pscustomobject]@{
        LeadDefinitionClauseEffect = [string]($leadEffect.DefinitionClauseEffectLabel)
        LeadDefinitionClauseEffectShare = ("{0}/{1} ({2})" -f $leadEffect.Weight, $totalTopologyCount, $leadShare.ToString("0%", [System.Globalization.CultureInfo]::InvariantCulture))
        ClauseEffectMixStatus = $clauseEffectMixStatus
    }
}

if (-not (Test-Path $CoreBodyProofRealInputJson)) {
    throw "Missing core-body-proof-real-input.json: $CoreBodyProofRealInputJson"
}

if ([string]::IsNullOrWhiteSpace($OutputMarkdown)) {
    $OutputMarkdown = Join-Path (Split-Path $CoreBodyProofRealInputJson -Parent) "topology-rewrite-summary.zh-CN.md"
}

$parsed = Get-Content $CoreBodyProofRealInputJson -Raw | ConvertFrom-Json
if ($parsed -is [System.Array]) {
    $views = [object[]]$parsed
}
else {
    $views = @($parsed)
}
$rows = @()

foreach ($view in $views) {
    foreach ($part in @($view.Result.Parts)) {
        if (@($part.TopologyRewriteStationIds).Count -le 0) {
            continue
        }

        $rows += [pscustomobject]@{
            MemberId = $view.MemberId
            AssemblyId = $view.AssemblyId
            PartId = $part.PartId
            PatternCode = $part.TopologyRewritePatternCode
            PatternLabel = $part.TopologyRewritePatternLabelZh
            ControllerRoleCode = $part.TopologyRewriteControllerRoleCode
            ControllerRoleLabel = $part.TopologyRewriteControllerRoleLabelZh
            ShapeRoleCode = $part.TopologyRewriteShapeRoleCode
            ShapeRoleLabel = $part.TopologyRewriteShapeRoleLabelZh
            FamilyRiskCode = $part.TopologyRewriteFamilyRiskCode
            FamilyRiskLabel = $part.TopologyRewriteFamilyRiskLabelZh
            ProofTypeCode = $part.TopologyRewriteProofTypeCode
            ProofTypeLabel = $part.TopologyRewriteProofTypeLabelZh
            FamilyProofTargetCode = $part.TopologyRewriteFamilyProofTargetCode
            FamilyProofTargetLabel = $part.TopologyRewriteFamilyProofTargetLabelZh
            DefinitionClauseCode = $part.TopologyRewriteDefinitionClauseCode
            DefinitionClauseLabel = $part.TopologyRewriteDefinitionClauseLabelZh
            DefinitionClauseEffectCode = $part.TopologyRewriteDefinitionClauseEffectCode
            DefinitionClauseEffectLabel = $part.TopologyRewriteDefinitionClauseEffectLabelZh
            CohortPartIds = @($part.TopologyRewriteCohortPartIds)
            TopologyRewrite = @($part.TopologyRewriteStationIds)
            SpanYRewrite = @($part.TopologyRewriteSpanYStationIds)
            SpanZRewrite = @($part.TopologyRewriteSpanZStationIds)
            ControllerSwitch = @($part.TopologyRewriteEnvelopeControllerSwitchStationIds)
            LostClosedLoop = @($part.LostClosedLoopStationIds)
        }
    }
}

$memberRows = @(
    $rows |
        Group-Object MemberId, AssemblyId |
        ForEach-Object {
            $groupRows = @($_.Group)
            $clauseSummary = Get-ClauseMixSummary $groupRows
            $clauseEffectSummary = Get-ClauseEffectMixSummary $groupRows
            [pscustomobject]@{
                MemberId = $groupRows[0].MemberId
                AssemblyId = $groupRows[0].AssemblyId
                PartCount = $groupRows.Count
                TopologyCount = (@($groupRows | ForEach-Object { @($_.TopologyRewrite).Count } | Measure-Object -Sum).Sum)
                SpanYCount = (@($groupRows | ForEach-Object { @($_.SpanYRewrite).Count } | Measure-Object -Sum).Sum)
                SpanZCount = (@($groupRows | ForEach-Object { @($_.SpanZRewrite).Count } | Measure-Object -Sum).Sum)
                ControllerSwitchCount = (@($groupRows | ForEach-Object { @($_.ControllerSwitch).Count } | Measure-Object -Sum).Sum)
                ControllerRoles = (Join-UniqueLabels ($groupRows | Select-Object -ExpandProperty ControllerRoleLabel))
                ShapeRoles = (Join-UniqueLabels ($groupRows | Select-Object -ExpandProperty ShapeRoleLabel))
                FamilyRisks = (Join-UniqueLabels ($groupRows | Select-Object -ExpandProperty FamilyRiskLabel))
                ProofTypes = (Join-UniqueLabels ($groupRows | Select-Object -ExpandProperty ProofTypeLabel))
                FamilyProofTargets = (Join-UniqueLabels ($groupRows | Select-Object -ExpandProperty FamilyProofTargetLabel))
                DefinitionClauses = (Join-UniqueLabels ($groupRows | Select-Object -ExpandProperty DefinitionClauseLabel))
                DefinitionClauseEffects = (Join-UniqueLabels ($groupRows | Select-Object -ExpandProperty DefinitionClauseEffectLabel))
                Patterns = (Join-UniqueLabels ($groupRows | Select-Object -ExpandProperty PatternLabel))
                LeadDefinitionClause = $clauseSummary.LeadDefinitionClause
                LeadDefinitionClauseShare = $clauseSummary.LeadDefinitionClauseShare
                ClauseMixStatus = $clauseSummary.ClauseMixStatus
                LeadDefinitionClauseEffect = $clauseEffectSummary.LeadDefinitionClauseEffect
                LeadDefinitionClauseEffectShare = $clauseEffectSummary.LeadDefinitionClauseEffectShare
                ClauseEffectMixStatus = $clauseEffectSummary.ClauseEffectMixStatus
            }
        } |
        Sort-Object `
            @{ Expression = "TopologyCount"; Descending = $true }, `
            @{ Expression = "MemberId"; Descending = $false } |
        Select-Object -First 20
)

$partRows = @(
    $rows |
        Sort-Object `
            @{ Expression = { @($_.TopologyRewrite).Count }; Descending = $true }, `
            @{ Expression = { @($_.SpanYRewrite).Count }; Descending = $true }, `
            @{ Expression = { @($_.ControllerSwitch).Count }; Descending = $true }, `
            @{ Expression = "MemberId"; Descending = $false }, `
            @{ Expression = "PartId"; Descending = $false } |
        Select-Object -First 20
)

$lines = @()
$lines += "# Topology Rewrite Summary"
$lines += ""
$lines += ("- TopologyRewrite part hits: " + $rows.Count)
$lines += ("- Assembly hits: " + (@($rows | Select-Object -ExpandProperty AssemblyId -Unique)).Count)
$lines += ""
$lines += "## Member Aggregate"
$lines += ""

if ($memberRows.Count -eq 0) {
    $lines += "No TopologyRewrite hit in the current result set."
}
else {
    $lines += "| MemberId | AssemblyId | PartCount | TopologyCount | SpanYCount | SpanZCount | ControllerSwitchCount | ControllerRoles | ShapeRoles | FamilyRisks | ProofTypes | FamilyProofTargets | DefinitionClauses | DefinitionClauseEffects | Patterns | LeadClause | LeadClauseShare | ClauseMix | LeadClauseEffect | LeadClauseEffectShare | ClauseEffectMix |"
    $lines += "| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |"
    foreach ($row in $memberRows) {
        $rowCells = @(
            (Escape-MarkdownCell $row.MemberId),
            (Escape-MarkdownCell $row.AssemblyId),
            [string]$row.PartCount,
            [string]$row.TopologyCount,
            [string]$row.SpanYCount,
            [string]$row.SpanZCount,
            [string]$row.ControllerSwitchCount,
            (Escape-MarkdownCell $row.ControllerRoles),
            (Escape-MarkdownCell $row.ShapeRoles),
            (Escape-MarkdownCell $row.FamilyRisks),
            (Escape-MarkdownCell $row.ProofTypes),
            (Escape-MarkdownCell $row.FamilyProofTargets),
            (Escape-MarkdownCell $row.DefinitionClauses),
            (Escape-MarkdownCell $row.DefinitionClauseEffects),
            (Escape-MarkdownCell $row.Patterns),
            (Escape-MarkdownCell $row.LeadDefinitionClause),
            (Escape-MarkdownCell $row.LeadDefinitionClauseShare),
            (Escape-MarkdownCell $row.ClauseMixStatus),
            (Escape-MarkdownCell $row.LeadDefinitionClauseEffect),
            (Escape-MarkdownCell $row.LeadDefinitionClauseEffectShare),
            (Escape-MarkdownCell $row.ClauseEffectMixStatus)
        )
        $lines += ('| ' + ($rowCells -join ' | ') + ' |')
    }
}

$lines += ""
$lines += "## Representative Parts"
$lines += ""

if ($partRows.Count -eq 0) {
    $lines += "No representative part row is available."
}
else {
    $lines += "| MemberId | AssemblyId | PartId | Pattern | ControllerRole | ShapeRole | FamilyRisk | ProofType | FamilyProofTarget | DefinitionClause | DefinitionClauseEffect | CohortParts | TopologyRewrite | SpanYRewrite | SpanZRewrite | ControllerSwitch | LostClosedLoop |"
    $lines += "| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |"
    foreach ($row in $partRows) {
        $rowCells = @(
            (Escape-MarkdownCell $row.MemberId),
            (Escape-MarkdownCell $row.AssemblyId),
            [string]$row.PartId,
            (Escape-MarkdownCell $row.PatternLabel),
            (Escape-MarkdownCell $row.ControllerRoleLabel),
            (Escape-MarkdownCell $row.ShapeRoleLabel),
            (Escape-MarkdownCell $row.FamilyRiskLabel),
            (Escape-MarkdownCell $row.ProofTypeLabel),
            (Escape-MarkdownCell $row.FamilyProofTargetLabel),
            (Escape-MarkdownCell $row.DefinitionClauseLabel),
            (Escape-MarkdownCell $row.DefinitionClauseEffectLabel),
            (Escape-MarkdownCell ($row.CohortPartIds -join '<br>')),
            (Escape-MarkdownCell ($row.TopologyRewrite -join '<br>')),
            (Escape-MarkdownCell ($row.SpanYRewrite -join '<br>')),
            (Escape-MarkdownCell ($row.SpanZRewrite -join '<br>')),
            (Escape-MarkdownCell ($row.ControllerSwitch -join '<br>')),
            (Escape-MarkdownCell ($row.LostClosedLoop -join '<br>'))
        )
        $lines += ('| ' + ($rowCells -join ' | ') + ' |')
    }
}

$lines += ""
$lines += "## Notes"
$lines += ""
$lines += "- This summary only covers current TopologyRewrite hits."
$lines += "- Pure LostClosedLoop / LostBodyCoverage / LostEnvelopeSupport cases are excluded."
$lines += "- SpanYRewrite / SpanZRewrite / ControllerSwitch are stage-5 structured reasons, not final family labels."
$lines += "- ControllerRole tells whether the removed part itself was a dominant envelope controller on rewrite stations."
$lines += "- ShapeRole tells whether the current rewrite pattern behaves like a single body plate, paired body plates, or a multi-plate cluster."
$lines += "- FamilyRisk compresses Pattern + ControllerRole + ShapeRole into a family-proof risk hint, but still does not equal the final family label."
$lines += "- ProofType further answers what kind of main-contour proof object is being rewritten, which is closer to engineering semantics than FamilyRisk."
$lines += "- FamilyProofTarget further answers which family-definition track should be validated next, but it still does not equal the final family label."
$lines += "- DefinitionClause further answers which concrete definition clause is most likely being triggered or broken, but it still does not equal the final family label."
$lines += "- DefinitionClauseEffect further answers whether removing the part is more like breaking that clause, rewriting its control, or only keeping the case in review."
$lines += "- LeadClause / LeadClauseShare / ClauseMix further separate stable single-clause members from mixed-clause or still-undetermined rewrite members."
$lines += "- LeadClauseEffect / LeadClauseEffectShare / ClauseEffectMix further separate stable single-effect members from mixed break/review effect members."

Set-Content -Path $OutputMarkdown -Value $lines -Encoding UTF8
Write-Host "Topology rewrite summary written to: $OutputMarkdown"
