using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class CoarseMainClassObservationArtifactBuilder
{
    public static CoarseMainClassObservationArtifact Build(IReadOnlyList<CoarseMainClassObservationRow> rows)
    {
        return new CoarseMainClassObservationArtifact
        {
            Rows = rows.ToList(),
            Summary = new CoarseMainClassObservationArtifactSummary
            {
                AssemblyCount = rows.Count,
                AttributeDirectBreakdown = BuildBreakdown(rows, item => item.SourceMemberMainClassCode, item => item.SourceMemberMainClassLabelZh),
                CoarseMainClassBreakdown = BuildBreakdown(rows, item => item.CoarseMainClassCode, item => item.CoarseMainClassLabelZh),
                CoarseSubtypeBreakdown = BuildBreakdown(rows, item => item.CoarseMainClassSubtypeCode, item => item.CoarseMainClassSubtypeLabelZh),
                CoarseReasonBreakdown = BuildBreakdown(rows, item => item.CoarseMainClassReasonCode, item => item.CoarseMainClassReasonLabelZh),
                LongitudinalTypeBreakdown = BuildBreakdown(rows, item => item.LongitudinalTypeCode, item => item.LongitudinalTypeLabelZh)
            }
        };
    }

    public static string BuildMarkdown(CoarseMainClassObservationArtifact artifact)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Coarse Main Class Observation");
        builder.AppendLine();
        builder.AppendLine("- 这份工件只观察“纯粗拓扑层”，不参与最终家族输出。");
        builder.AppendLine($"- 构件数：`{artifact.Summary.AssemblyCount}`");
        builder.AppendLine();

        AppendBreakdown(builder, "属性直达主类分布", artifact.Summary.AttributeDirectBreakdown);
        AppendBreakdown(builder, "粗主类分布", artifact.Summary.CoarseMainClassBreakdown);
        AppendBreakdown(builder, "粗子类分布", artifact.Summary.CoarseSubtypeBreakdown);
        AppendBreakdown(builder, "粗分类原因分布", artifact.Summary.CoarseReasonBreakdown);
        AppendBreakdown(builder, "主线类型分布", artifact.Summary.LongitudinalTypeBreakdown);

        builder.AppendLine("## 构件级结果");
        builder.AppendLine();
        builder.AppendLine("| MemberId | AssemblyId | AttributeDirect | CoarseMainClass | CoarseSubtype | Confidence | CandidateParts | EligibleStations | Box/H/Primary | ClosedLoopRatio | Reason |");
        builder.AppendLine("| --- | --- | --- | --- | --- | ---: | --- | ---: | --- | ---: | --- |");

        foreach (var row in artifact.Rows.Take(80))
        {
            builder.Append("| ").Append(Escape(row.MemberId))
                .Append(" | ").Append(Escape(row.AssemblyId))
                .Append(" | ").Append(Escape(Coalesce(row.SourceMemberMainClassLabelZh, row.SourceMemberMainClassCode)))
                .Append(" | ").Append(Escape(Coalesce(row.CoarseMainClassLabelZh, row.CoarseMainClassCode)))
                .Append(" | ").Append(Escape(Coalesce(row.CoarseMainClassSubtypeLabelZh, row.CoarseMainClassSubtypeCode)))
                .Append(" | ").Append(row.CoarseMainClassConfidence.ToString("0.00"))
                .Append(" | ").Append(Escape(row.CandidatePartIds))
                .Append(" | ").Append(row.EligibleStationCount.ToString())
                .Append(" | ").Append(Escape($"{row.BoxStationCount}/{row.HStationCount}/{row.PrimaryPlateStationCount}"))
                .Append(" | ").Append(row.ClosedLoopStationRatio.ToString("0.00"))
                .Append(" | ").Append(Escape(Coalesce(row.CoarseMainClassReasonLabelZh, row.CoarseMainClassReasonCode)))
                .AppendLine(" |");
        }

        return builder.ToString();
    }

    private static List<DefinitionClauseDecisionBreakdownItem> BuildBreakdown(
        IEnumerable<CoarseMainClassObservationRow> rows,
        Func<CoarseMainClassObservationRow, string> codeSelector,
        Func<CoarseMainClassObservationRow, string> labelSelector)
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
            builder.Append("| ").Append(Escape(item.Code))
                .Append(" | ").Append(Escape(item.LabelZh))
                .Append(" | ").Append(item.Count.ToString())
                .AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "NONE" : value.Trim();
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
