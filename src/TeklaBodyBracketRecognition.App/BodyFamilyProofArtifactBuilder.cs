using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyFamilyProofArtifactBuilder
{
    public static BodyFamilyProofArtifact Build(IReadOnlyList<BodyFamilyProofRow> rows)
    {
        return new BodyFamilyProofArtifact
        {
            Rows = rows.ToList(),
            Summary = new BodyFamilyProofArtifactSummary
            {
                AssemblyCount = rows.Count,
                AdjudicatedCount = rows.Count(item => string.Equals(item.DecisionStatusCode, "Adjudicated", StringComparison.Ordinal)),
                DeferredCount = rows.Count(item => string.Equals(item.DecisionStatusCode, "Deferred", StringComparison.Ordinal)),
                FamilyBreakdown = BuildBreakdown(rows, item => item.DefinitionDrivenFamilyCode, item => item.DefinitionDrivenFamilyLabelZh),
                LongitudinalTypeBreakdown = BuildBreakdown(rows, item => item.LongitudinalTypeCode, item => item.LongitudinalTypeLabelZh),
                SubtypeBreakdown = BuildBreakdown(rows, item => item.DefinitionDrivenSubtypeCode, item => item.DefinitionDrivenSubtypeLabelZh),
                StatusBreakdown = BuildBreakdown(rows, item => item.DecisionStatusCode, item => item.DecisionStatusLabelZh),
                ReasonBreakdown = BuildBreakdown(rows, item => item.DecisionReasonCode, item => item.DecisionReasonLabelZh)
            }
        };
    }

    public static string BuildMarkdown(BodyFamilyProofArtifact artifact)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Body Family Proof Summary");
        builder.AppendLine();
        builder.AppendLine("## 总览");
        builder.AppendLine();
        builder.AppendLine($"- 构件数：`{artifact.Summary.AssemblyCount}`");
        builder.AppendLine($"- 已进入家族判定：`{artifact.Summary.AdjudicatedCount}`");
        builder.AppendLine($"- 暂缓进入家族判定：`{artifact.Summary.DeferredCount}`");
        builder.AppendLine();

        AppendBreakdown(builder, "Family 分布", artifact.Summary.FamilyBreakdown);
        AppendBreakdown(builder, "LongitudinalType 分布", artifact.Summary.LongitudinalTypeBreakdown);
        AppendBreakdown(builder, "Subtype 分布", artifact.Summary.SubtypeBreakdown);
        AppendBreakdown(builder, "DecisionStatus 分布", artifact.Summary.StatusBreakdown);
        AppendBreakdown(builder, "DecisionReason 分布", artifact.Summary.ReasonBreakdown);

        builder.AppendLine("## 构件级结果");
        builder.AppendLine();
        builder.AppendLine("| MemberId | AssemblyId | Family | LongitudinalType | LongitudinalSubtype | Subtype | LeadClause | LeadClauseVerdict | PromotionReadiness | DecisionStatus | DecisionReason | SatisfiedConditions | MissingConditions |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");

        foreach (var row in artifact.Rows.Take(60))
        {
            builder.Append("| ")
                .Append(Escape(row.MemberId)).Append(" | ")
                .Append(Escape(row.AssemblyId)).Append(" | ")
                .Append(Escape(Coalesce(row.DefinitionDrivenFamilyLabelZh, row.DefinitionDrivenFamilyCode))).Append(" | ")
                .Append(Escape(Coalesce(row.LongitudinalTypeLabelZh, row.LongitudinalTypeCode))).Append(" | ")
                .Append(Escape(Coalesce(row.LongitudinalSubtypeLabelZh, row.LongitudinalSubtypeCode))).Append(" | ")
                .Append(Escape(Coalesce(row.DefinitionDrivenSubtypeLabelZh, row.DefinitionDrivenSubtypeCode))).Append(" | ")
                .Append(Escape(Coalesce(row.LeadClauseLabelZh, row.LeadClauseCode))).Append(" | ")
                .Append(Escape(Coalesce(row.LeadClauseVerdictLabelZh, row.LeadClauseVerdictCode))).Append(" | ")
                .Append(Escape(Coalesce(row.LeadClausePromotionReadinessLabelZh, row.LeadClausePromotionReadinessCode))).Append(" | ")
                .Append(Escape(Coalesce(row.DecisionStatusLabelZh, row.DecisionStatusCode))).Append(" | ")
                .Append(Escape(Coalesce(row.DecisionReasonLabelZh, row.DecisionReasonCode))).Append(" | ")
                .Append(Escape(string.Join("; ", row.SatisfiedConditions))).Append(" | ")
                .Append(Escape(string.Join("; ", row.MissingConditions))).AppendLine(" |");
        }

        return builder.ToString();
    }

    private static List<DefinitionClauseDecisionBreakdownItem> BuildBreakdown(
        IEnumerable<BodyFamilyProofRow> rows,
        Func<BodyFamilyProofRow, string> codeSelector,
        Func<BodyFamilyProofRow, string> labelSelector)
    {
        return rows
            .GroupBy(row => Normalize(codeSelector(row)))
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(
                group =>
                {
                    var sample = group.First();
                    return new DefinitionClauseDecisionBreakdownItem
                    {
                        Code = Normalize(codeSelector(sample)),
                        LabelZh = Normalize(Coalesce(labelSelector(sample), codeSelector(sample))),
                        Count = group.Count()
                    };
                })
            .ToList();
    }

    private static void AppendBreakdown(
        StringBuilder builder,
        string title,
        IReadOnlyList<DefinitionClauseDecisionBreakdownItem> items)
    {
        builder.AppendLine($"## {title}");
        builder.AppendLine();
        builder.AppendLine("| Code | LabelZh | Count |");
        builder.AppendLine("| --- | --- | ---: |");

        foreach (var item in items)
        {
            builder.Append("| ")
                .Append(Escape(item.Code)).Append(" | ")
                .Append(Escape(item.LabelZh)).Append(" | ")
                .Append(item.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)).AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "(empty)" : value.Trim();
    }

    private static string Coalesce(string? preferred, string? fallback)
    {
        return string.IsNullOrWhiteSpace(preferred) ? Normalize(fallback) : preferred.Trim();
    }

    private static string Escape(string? value)
    {
        return Normalize(value).Replace("|", "\\|");
    }
}
