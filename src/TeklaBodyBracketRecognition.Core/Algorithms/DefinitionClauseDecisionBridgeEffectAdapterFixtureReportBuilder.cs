using System.Text;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class DefinitionClauseDecisionBridgeEffectAdapterFixtureReportBuilder
{
    public static string BuildMarkdown(DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult runResult)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClause Bridge Effect Adapter Fixture Report");
        builder.AppendLine();
        builder.AppendLine($"- Total: `{runResult.TotalCount}`");
        builder.AppendLine($"- Matched: `{runResult.MatchedCount}`");
        builder.AppendLine($"- Mismatch: `{runResult.MismatchCount}`");
        builder.AppendLine($"- Succeeded: `{runResult.Succeeded}`");
        builder.AppendLine();
        builder.AppendLine("| Sample | Source | Station | Topology | Verdict | Readiness | Match |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");

        foreach (var result in runResult.Results)
        {
            builder.Append("| ");
            builder.Append(result.SampleCode);
            builder.Append(" | ");
            builder.Append(result.ActualSourceTier);
            builder.Append(" / ");
            builder.Append(result.ExpectedSourceTier);
            builder.Append(" | ");
            builder.Append(result.ActualStationTier);
            builder.Append(" / ");
            builder.Append(result.ExpectedStationTier);
            builder.Append(" | ");
            builder.Append(result.ActualTopologyTier);
            builder.Append(" / ");
            builder.Append(result.ExpectedTopologyTier);
            builder.Append(" | ");
            builder.Append(result.ActualVerdict);
            builder.Append(" / ");
            builder.Append(result.ExpectedVerdict);
            builder.Append(" | ");
            builder.Append(result.ActualPromotionReadiness);
            builder.Append(" / ");
            builder.Append(result.ExpectedPromotionReadiness);
            builder.Append(" | ");
            builder.Append(result.IsMatch ? "OK" : "MISMATCH");
            builder.AppendLine(" |");
        }

        return builder.ToString();
    }
}
