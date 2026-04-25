using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeMapperFixtureArtifact
{
    public DateTime GeneratedAtUtc { get; set; }

    public int TotalCount { get; set; }

    public int MatchedCount { get; set; }

    public int MismatchCount { get; set; }

    public bool Succeeded { get; set; }

    public IReadOnlyList<DefinitionClauseDecisionBridgeMapperFixtureArtifactRow> Rows { get; set; } =
        [];
}

public sealed class DefinitionClauseDecisionBridgeMapperFixtureArtifactRow
{
    public string SampleCode { get; set; } = string.Empty;

    public string LabelZh { get; set; } = string.Empty;

    public bool IsSynthetic { get; set; }

    public string ExpectedCandidateDirection { get; set; } = string.Empty;

    public string ActualCandidateDirection { get; set; } = string.Empty;

    public string ExpectedVerdict { get; set; } = string.Empty;

    public string ActualVerdict { get; set; } = string.Empty;

    public string ExpectedPromotionReadiness { get; set; } = string.Empty;

    public string ActualPromotionReadiness { get; set; } = string.Empty;

    public bool IsMatch { get; set; }
}

public sealed class DefinitionClauseDecisionBridgeMapperFixtureExportResult
{
    public string JsonPath { get; set; } = string.Empty;

    public string MarkdownPath { get; set; } = string.Empty;
}
