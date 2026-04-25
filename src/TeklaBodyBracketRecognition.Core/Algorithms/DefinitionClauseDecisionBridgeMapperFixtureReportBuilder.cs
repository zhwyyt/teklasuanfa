using System.Text;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class DefinitionClauseDecisionBridgeMapperFixtureReportBuilder
{
    public static string BuildMarkdown(DefinitionClauseDecisionBridgeMapperFixtureRunResult runResult)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClause Bridge Mapper Fixture Report");
        builder.AppendLine();
        builder.AppendLine($"- Total: `{runResult.TotalCount}`");
        builder.AppendLine($"- Matched: `{runResult.MatchedCount}`");
        builder.AppendLine($"- Mismatch: `{runResult.MismatchCount}`");
        builder.AppendLine($"- Succeeded: `{runResult.Succeeded}`");
        builder.AppendLine();
        builder.AppendLine("| Sample | Label | Candidate | Verdict | Readiness | Match |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- |");

        foreach (var result in runResult.Results)
        {
            builder.Append("| ");
            builder.Append(result.SampleCode);
            builder.Append(" | ");
            builder.Append(result.LabelZh);
            builder.Append(" | ");
            builder.Append(result.ActualCandidateDirection);
            builder.Append(" / ");
            builder.Append(result.ExpectedCandidateDirection);
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
