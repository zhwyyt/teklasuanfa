using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyProfileResolutionArtifactBuilder
{
    public static BodyProfileResolutionArtifact Build(IReadOnlyList<BodyProfileResolutionRow> rows)
    {
        return new BodyProfileResolutionArtifact
        {
            Rows = rows.ToList(),
            Summary = new BodyProfileResolutionSummary
            {
                AssemblyCount = rows.Count,
                ResolvedCount = rows.Count(item => string.Equals(item.ResolutionStatusCode, "Resolved", StringComparison.Ordinal)),
                ReviewBypassCount = rows.Count(item => string.Equals(item.ResolutionStatusCode, "ReviewBypass", StringComparison.Ordinal)),
                StatusBreakdown = BuildBreakdown(rows, item => item.ResolutionStatusCode, item => item.ResolutionStatusLabelZh),
                LongitudinalTypeBreakdown = BuildBreakdown(rows, item => item.LongitudinalTypeCode, item => item.LongitudinalTypeLabelZh),
                CategoryBreakdown = BuildBreakdown(rows, item => item.ProfileCategoryCode, item => item.ProfileCategoryLabelZh),
                ProfileBreakdown = BuildBreakdown(rows, item => item.ProfileCode, item => item.ProfileLabelZh),
                ProfileSeriesBreakdown = BuildBreakdown(rows, item => item.ProfileSeriesCode, item => item.ProfileSeriesLabelZh),
                DimensionSourceBreakdown = BuildBreakdown(rows, item => item.DimensionSourceCode, item => item.DimensionSourceLabelZh)
            }
        };
    }

    public static string BuildMarkdown(BodyProfileResolutionArtifact artifact)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Body Profile Resolution Summary");
        builder.AppendLine();
        builder.AppendLine("## 总览");
        builder.AppendLine();
        builder.AppendLine($"- 构件数：`{artifact.Summary.AssemblyCount}`");
        builder.AppendLine($"- 已完成自动细分：`{artifact.Summary.ResolvedCount}`");
        builder.AppendLine($"- 人工复核旁路：`{artifact.Summary.ReviewBypassCount}`");
        builder.AppendLine();

        AppendBreakdown(builder, "ResolutionStatus 分布", artifact.Summary.StatusBreakdown);
        AppendBreakdown(builder, "LongitudinalType 分布", artifact.Summary.LongitudinalTypeBreakdown);
        AppendBreakdown(builder, "ProfileCategory 分布", artifact.Summary.CategoryBreakdown);
        AppendBreakdown(builder, "Profile 分布", artifact.Summary.ProfileBreakdown);
        AppendBreakdown(builder, "ProfileSeries 分布", artifact.Summary.ProfileSeriesBreakdown);
        AppendBreakdown(builder, "DimensionSource 分布", artifact.Summary.DimensionSourceBreakdown);

        builder.AppendLine("## 构件级结果");
        builder.AppendLine();
        builder.AppendLine("| MemberId | AssemblyId | Family | LongitudinalType | LongitudinalSubtype | FamilySubtype | ResolutionStatus | ProfileCategory | Profile | ProfileSeries | DimensionSource | SimilarityBasis | SimilarityScore | EvidenceSummary |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | ---: | --- |");

        foreach (var row in artifact.Rows.Take(60))
        {
            builder.Append("| ")
                .Append(Escape(row.MemberId)).Append(" | ")
                .Append(Escape(row.AssemblyId)).Append(" | ")
                .Append(Escape(Coalesce(row.FamilyLabelZh, row.FamilyCode))).Append(" | ")
                .Append(Escape(Coalesce(row.LongitudinalTypeLabelZh, row.LongitudinalTypeCode))).Append(" | ")
                .Append(Escape(Coalesce(row.LongitudinalSubtypeLabelZh, row.LongitudinalSubtypeCode))).Append(" | ")
                .Append(Escape(Coalesce(row.FamilySubtypeLabelZh, row.FamilySubtypeCode))).Append(" | ")
                .Append(Escape(Coalesce(row.ResolutionStatusLabelZh, row.ResolutionStatusCode))).Append(" | ")
                .Append(Escape(Coalesce(row.ProfileCategoryLabelZh, row.ProfileCategoryCode))).Append(" | ")
                .Append(Escape(Coalesce(row.ProfileLabelZh, row.ProfileCode))).Append(" | ")
                .Append(Escape(Coalesce(row.ProfileSeriesLabelZh, row.ProfileSeriesCode))).Append(" | ")
                .Append(Escape(Coalesce(row.DimensionSourceLabelZh, row.DimensionSourceCode))).Append(" | ")
                .Append(Escape(Coalesce(row.SimilarityBasisLabelZh, row.SimilarityBasisCode))).Append(" | ")
                .Append(row.SimilarityScore.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(" | ")
                .Append(Escape(row.EvidenceSummary)).AppendLine(" |");
        }

        return builder.ToString();
    }

    private static List<DefinitionClauseDecisionBreakdownItem> BuildBreakdown(
        IEnumerable<BodyProfileResolutionRow> rows,
        Func<BodyProfileResolutionRow, string> codeSelector,
        Func<BodyProfileResolutionRow, string> labelSelector)
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
