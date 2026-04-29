using System.Collections.Generic;
using System.Text;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class DefinitionClauseDecisionSemanticRegressionSnapshotReportBuilder
{
    public static string BuildMarkdown(IReadOnlyList<DefinitionClauseDecisionSemanticRegressionSnapshotRow> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClause Semantic Regression Snapshot");
        builder.AppendLine();
        builder.AppendLine("| Name | Source | Station | Topology | Proof | LeadClause | Candidate | Verdict | Readiness |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");

        foreach (var row in rows)
        {
            builder.Append("| ")
                .Append(row.Name).Append(" | ")
                .Append(row.SourceTier).Append(" | ")
                .Append(row.StationTier).Append(" | ")
                .Append(row.TopologyTier).Append(" | ")
                .Append(row.ProofCompletenessTier).Append(" | ")
                .Append(row.LeadClauseTier).Append(" | ")
                .Append(row.ExpectedCandidateDirection).Append(" | ")
                .Append(row.ExpectedVerdict).Append(" | ")
                .Append(row.ExpectedPromotionReadiness).AppendLine(" |");
        }

        builder.AppendLine();

        foreach (var row in rows)
        {
            builder.Append("## ").AppendLine(row.Name);
            builder.AppendLine();
            builder.Append("- 意图：").AppendLine(row.Intent);
            builder.Append("- 预期 verdict：").AppendLine(row.ExpectedVerdict);
            builder.Append("- 预期 readiness：").AppendLine(row.ExpectedPromotionReadiness);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
