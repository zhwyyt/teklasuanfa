using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionPresentation
{
    public static string BuildRepresentativeReviewHint(DefinitionClauseDecisionRepresentativeRow row)
    {
        if (row is null)
        {
            return "定义条款仍需复核";
        }

        if (row.ClauseVerdictCode == "CLAUSE_BROKEN_DIRECT")
        {
            return "这件更像直接破坏定义条款的关键件，优先看它是否必须保留在主截面证明中。";
        }

        if (row.ClauseVerdictCode == "CLAUSE_BROKEN_MAJORITY")
        {
            return "这件更像会破坏多数站位持续性的关键件，优先看它被移除后主截面是否还能沿主轴连续成立。";
        }

        if (row.ClauseVerdictCode == "CLAUSE_SATISFIED_WITH_REWRITE")
        {
            return "这件移除后未必直接打断条款，但会改写主轮廓/控制路径，后续定义验证要带上改写说明。";
        }

        if (row.ClauseVerdictCode == "CLAUSE_SATISFIED_STABLE")
        {
            return "这件当前更像可稳定排除或不影响定义条款的附属/边界件，可优先降级复核。";
        }

        return "当前证据仍不足以直接升级成定义判定，先按复核件保留。";
    }

    public static string BuildAggregateReviewHint(DefinitionClauseDecisionAggregateRow row)
    {
        if (row is null)
        {
            return "当前没有定义条款判定。";
        }

        var builder = new StringBuilder();
        builder.Append("主条款判定：");
        builder.Append(row.LeadClauseVerdictLabelZh);
        builder.Append("；主提升准备度：");
        builder.Append(row.LeadClausePromotionReadinessLabelZh);

        if (!string.IsNullOrWhiteSpace(row.ClauseVerdictMixStatus) && row.ClauseVerdictMixStatus != "无条款判定")
        {
            builder.Append("；判定混合度：");
            builder.Append(row.ClauseVerdictMixStatus);
        }

        if (row.LeadClauseVerdictCode == "CLAUSE_SATISFIED_WITH_REWRITE")
        {
            builder.Append("；说明当前更像“条款仍成立，但验证路径被改写”。");
        }
        else if (row.LeadClauseVerdictCode == "CLAUSE_BROKEN_DIRECT")
        {
            builder.Append("；说明当前已存在直接破坏定义条款的关键件。");
        }
        else if (row.LeadClauseVerdictCode == "CLAUSE_BROKEN_MAJORITY")
        {
            builder.Append("；说明当前已存在多数站位持续性被破坏的关键件。");
        }
        else if (row.LeadClauseVerdictCode == "CLAUSE_REVIEW_REQUIRED")
        {
            builder.Append("；说明当前仍应保守维持定义复核，不宜过早升级。");
        }

        return builder.ToString();
    }

    public static string BuildRepresentativeMarkdownTable(IEnumerable<DefinitionClauseDecisionRepresentativeRow>? rows)
    {
        var materialized = rows?.ToList() ?? new List<DefinitionClauseDecisionRepresentativeRow>();
        if (materialized.Count == 0)
        {
            return "无代表零件条款判定。";
        }

        var lines = new List<string>
        {
            "| MemberId | PartId | DefinitionClause | DefinitionClauseEffect | ClauseVerdict | ClausePromotionReadiness | ReviewHint |",
            "| --- | --- | --- | --- | --- | --- | --- |"
        };

        foreach (var row in materialized)
        {
            lines.Add(string.Join(" | ", new[]
            {
                "| " + Escape(row.MemberId),
                Escape(row.PartId),
                Escape(row.DefinitionClauseLabelZh),
                Escape(row.DefinitionClauseEffectLabelZh),
                Escape(row.ClauseVerdictLabelZh),
                Escape(row.ClausePromotionReadinessLabelZh),
                Escape(BuildRepresentativeReviewHint(row)) + " |"
            }));
        }

        return string.Join("\n", lines);
    }

    private static string Escape(string? text)
    {
        return (text ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
    }
}
