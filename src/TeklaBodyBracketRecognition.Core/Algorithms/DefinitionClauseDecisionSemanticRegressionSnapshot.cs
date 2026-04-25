using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionSemanticRegressionSnapshotRow
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

public static class DefinitionClauseDecisionSemanticRegressionSnapshot
{
    public static IReadOnlyList<DefinitionClauseDecisionSemanticRegressionSnapshotRow> CreateDefault()
    {
        var cases = DefinitionClauseDecisionSemanticRegressionCatalog.CreateDefault();
        var rows = new List<DefinitionClauseDecisionSemanticRegressionSnapshotRow>(cases.Count);

        foreach (var @case in cases)
        {
            rows.Add(new DefinitionClauseDecisionSemanticRegressionSnapshotRow
            {
                Name = @case.Name,
                Intent = @case.Intent,
                SourceTier = @case.SourceTier.ToString(),
                StationTier = @case.StationTier.ToString(),
                TopologyTier = @case.TopologyTier.ToString(),
                ProofCompletenessTier = @case.ProofCompletenessTier.ToString(),
                LeadClauseTier = @case.LeadClauseTier.ToString(),
                ExpectedCandidateDirection = @case.ExpectedCandidateDirection.ToString(),
                ExpectedVerdict = @case.ExpectedVerdict.ToString(),
                ExpectedPromotionReadiness = @case.ExpectedPromotionReadiness.ToString()
            });
        }

        return rows;
    }
}
