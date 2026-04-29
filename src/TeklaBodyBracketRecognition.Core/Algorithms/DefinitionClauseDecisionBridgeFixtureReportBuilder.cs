using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public static class DefinitionClauseDecisionBridgeFixtureReportBuilder
{
    public static string BuildMarkdown(IEnumerable<DefinitionClauseDecisionBridgeFixtureResult>? results)
    {
        var materialized = results?.ToList() ?? new List<DefinitionClauseDecisionBridgeFixtureResult>();
        if (materialized.Count == 0)
        {
            return "无 DefinitionClauseDecision fixture 结果。";
        }

        var passedCount = materialized.Count(static x => x.Passed);
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision Fixture 报告");
        builder.AppendLine();
        builder.Append("总计：");
        builder.Append(materialized.Count);
        builder.Append("；通过：");
        builder.Append(passedCount);
        builder.Append("；失败：");
        builder.Append(materialized.Count - passedCount);
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("| Name | Passed | ActualClauseVerdict | ExpectedClauseVerdict | ActualClausePromotionReadiness | ExpectedClausePromotionReadiness |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- |");

        foreach (var result in materialized)
        {
            builder.Append("| ");
            builder.Append(Escape(result.Name));
            builder.Append(" | ");
            builder.Append(result.Passed ? "Yes" : "No");
            builder.Append(" | ");
            builder.Append(Escape(result.ActualClauseVerdictCode));
            builder.Append(" | ");
            builder.Append(Escape(result.ExpectedClauseVerdictCode));
            builder.Append(" | ");
            builder.Append(Escape(result.ActualClausePromotionReadinessCode));
            builder.Append(" | ");
            builder.Append(Escape(result.ExpectedClausePromotionReadinessCode));
            builder.AppendLine(" |");
        }

        return builder.ToString().TrimEnd();
    }

    private static string Escape(string? text)
    {
        return (text ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
    }
}
