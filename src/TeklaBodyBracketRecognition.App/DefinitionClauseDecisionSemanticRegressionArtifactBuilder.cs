using System;
using System.Collections.Generic;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionSemanticRegressionArtifactBuilder
{
    public static DefinitionClauseDecisionSemanticRegressionArtifacts BuildDefault()
    {
        var snapshotRows = DefinitionClauseDecisionSemanticRegressionSnapshot.CreateDefault();
        var report = DefinitionClauseDecisionSemanticRegressionSnapshotReportBuilder.BuildMarkdown(snapshotRows);

        return Build(snapshotRows, report);
    }

    public static DefinitionClauseDecisionSemanticRegressionArtifacts Build(
        IReadOnlyList<DefinitionClauseDecisionSemanticRegressionSnapshotRow> snapshotRows,
        string markdownReport)
    {
        var rows = new List<DefinitionClauseDecisionSemanticRegressionArtifactRow>(snapshotRows.Count);

        foreach (var row in snapshotRows)
        {
            rows.Add(new DefinitionClauseDecisionSemanticRegressionArtifactRow
            {
                Name = row.Name,
                Intent = row.Intent,
                SourceTier = row.SourceTier,
                StationTier = row.StationTier,
                TopologyTier = row.TopologyTier,
                ProofCompletenessTier = row.ProofCompletenessTier,
                LeadClauseTier = row.LeadClauseTier,
                ExpectedCandidateDirection = row.ExpectedCandidateDirection,
                ExpectedVerdict = row.ExpectedVerdict,
                ExpectedPromotionReadiness = row.ExpectedPromotionReadiness
            });
        }

        return new DefinitionClauseDecisionSemanticRegressionArtifacts
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Rows = rows,
            MarkdownReport = markdownReport
        };
    }
}
