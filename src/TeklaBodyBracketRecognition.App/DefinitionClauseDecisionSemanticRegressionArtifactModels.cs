using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionSemanticRegressionArtifactRow
{
    public required string Name { get; init; }

    public required string Intent { get; init; }

    public required string SourceTier { get; init; }

    public required string StationTier { get; init; }

    public required string TopologyTier { get; init; }

    public required string ProofCompletenessTier { get; init; }

    public required string LeadClauseTier { get; init; }

    public required string ExpectedCandidateDirection { get; init; }

    public required string ExpectedVerdict { get; init; }

    public required string ExpectedPromotionReadiness { get; init; }
}

internal sealed class DefinitionClauseDecisionSemanticRegressionArtifacts
{
    public required DateTime GeneratedAtUtc { get; init; }

    public required IReadOnlyList<DefinitionClauseDecisionSemanticRegressionArtifactRow> Rows { get; init; }

    public required string MarkdownReport { get; init; }
}

internal sealed class DefinitionClauseDecisionSemanticRegressionWorkflowResult
{
    public required string OutputDirectory { get; init; }

    public required string JsonPath { get; init; }

    public required string MarkdownPath { get; init; }

    public required string ManifestPath { get; init; }

    public required string ReadmePath { get; init; }

    public required int RowCount { get; init; }

    public required string BundleSectionJsonPath { get; init; }

    public required string BundleSectionMarkdownPath { get; init; }
}
