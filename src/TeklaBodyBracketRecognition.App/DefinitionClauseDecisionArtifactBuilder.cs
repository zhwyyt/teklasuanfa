using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionArtifactBuilder
{
    public static DefinitionClauseDecisionArtifacts Build(
        IEnumerable<DefinitionClauseDecisionRepresentativeRow>? representativeRows,
        IEnumerable<DefinitionClauseDecisionAggregateRow>? aggregateRows)
    {
        return new DefinitionClauseDecisionArtifacts
        {
            RepresentativeRows = representativeRows?.ToList() ?? new List<DefinitionClauseDecisionRepresentativeRow>(),
            AggregateRows = aggregateRows?.ToList() ?? new List<DefinitionClauseDecisionAggregateRow>()
        };
    }

    public static IReadOnlyList<DefinitionClauseDecisionReviewRow> BuildReviewRows(
        IEnumerable<DefinitionClauseDecisionAggregateRow>? aggregateRows)
    {
        var rows = aggregateRows?.ToList() ?? new List<DefinitionClauseDecisionAggregateRow>();
        return rows
            .Select(row => new DefinitionClauseDecisionReviewRow
            {
                MemberId = row.MemberId,
                AssemblyId = row.AssemblyId,
                LeadClauseVerdictLabelZh = row.LeadClauseVerdictLabelZh,
                LeadClausePromotionReadinessLabelZh = row.LeadClausePromotionReadinessLabelZh,
                ClauseVerdictMixStatus = row.ClauseVerdictMixStatus,
                ClausePromotionReadinessMixStatus = row.ClausePromotionReadinessMixStatus,
                ReviewHint = DefinitionClauseDecisionPresentation.BuildAggregateReviewHint(row)
            })
            .ToList();
    }

    public static string BuildReviewMarkdown(IEnumerable<DefinitionClauseDecisionReviewRow>? reviewRows)
    {
        var rows = reviewRows?.ToList() ?? new List<DefinitionClauseDecisionReviewRow>();
        if (rows.Count == 0)
        {
            return "无 DefinitionClauseDecision review 结果。";
        }

        var lines = new List<string>
        {
            "| MemberId | AssemblyId | LeadClauseVerdict | LeadClausePromotionReadiness | ClauseVerdictMixStatus | ClausePromotionReadinessMixStatus | ReviewHint |",
            "| --- | --- | --- | --- | --- | --- | --- |"
        };

        foreach (var row in rows)
        {
            lines.Add(string.Join(" | ", new[]
            {
                "| " + Escape(row.MemberId),
                Escape(row.AssemblyId),
                Escape(row.LeadClauseVerdictLabelZh),
                Escape(row.LeadClausePromotionReadinessLabelZh),
                Escape(row.ClauseVerdictMixStatus),
                Escape(row.ClausePromotionReadinessMixStatus),
                Escape(row.ReviewHint) + " |"
            }));
        }

        return string.Join("\n", lines);
    }

    public static string BuildFullMarkdown(DefinitionClauseDecisionArtifacts artifacts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision 摘要");
        builder.AppendLine();
        builder.AppendLine("## 构件级 Review");
        builder.AppendLine();
        builder.AppendLine(BuildReviewMarkdown(BuildReviewRows(artifacts.AggregateRows)));
        builder.AppendLine();
        builder.AppendLine("## 代表零件");
        builder.AppendLine();
        builder.AppendLine(DefinitionClauseDecisionPresentation.BuildRepresentativeMarkdownTable(artifacts.RepresentativeRows));
        return builder.ToString().TrimEnd();
    }

    private static string Escape(string? text)
    {
        return (text ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
    }
}
