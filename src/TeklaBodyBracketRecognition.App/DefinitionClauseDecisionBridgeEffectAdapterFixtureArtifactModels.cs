using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifact
{
    public DateTime GeneratedAtUtc { get; set; }

    public int TotalCount { get; set; }

    public int MatchedCount { get; set; }

    public int MismatchCount { get; set; }

    public bool Succeeded { get; set; }

    public IReadOnlyList<DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactRow> Rows { get; set; } =
        [];
}

public sealed class DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactRow
{
    public string SampleCode { get; set; } = string.Empty;

    public string LabelZh { get; set; } = string.Empty;

    public bool IsSynthetic { get; set; }

    public string ExpectedSourceTier { get; set; } = string.Empty;

    public string ActualSourceTier { get; set; } = string.Empty;

    public string ExpectedStationTier { get; set; } = string.Empty;

    public string ActualStationTier { get; set; } = string.Empty;

    public string ExpectedTopologyTier { get; set; } = string.Empty;

    public string ActualTopologyTier { get; set; } = string.Empty;

    public string ExpectedProofCompletenessTier { get; set; } = string.Empty;

    public string ActualProofCompletenessTier { get; set; } = string.Empty;

    public string ExpectedLeadClauseTier { get; set; } = string.Empty;

    public string ActualLeadClauseTier { get; set; } = string.Empty;

    public string ExpectedCandidateDirection { get; set; } = string.Empty;

    public string ActualCandidateDirection { get; set; } = string.Empty;

    public string ExpectedVerdict { get; set; } = string.Empty;

    public string ActualVerdict { get; set; } = string.Empty;

    public string ExpectedPromotionReadiness { get; set; } = string.Empty;

    public string ActualPromotionReadiness { get; set; } = string.Empty;

    public bool IsMatch { get; set; }
}

public sealed class DefinitionClauseDecisionBridgeEffectAdapterFixtureExportResult
{
    public string JsonPath { get; set; } = string.Empty;

    public string MarkdownPath { get; set; } = string.Empty;
}
