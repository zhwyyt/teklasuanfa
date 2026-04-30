using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class FoundationGeometryHealthAuditArtifactBuilder
{
    public static FoundationGeometryHealthAuditArtifact Build(
        string outputDirectory,
        IReadOnlyList<FoundationGeometryHealthAuditMemberRow> memberRows)
    {
        var sourceInputDirectory = ResolveSourceInputDirectory(memberRows);
        return new FoundationGeometryHealthAuditArtifact
        {
            GeneratedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            SourceRunId = string.IsNullOrWhiteSpace(sourceInputDirectory) ? string.Empty : Path.GetFileName(sourceInputDirectory),
            SourceInputDirectory = sourceInputDirectory,
            SourceOutputDirectory = outputDirectory,
            EnabledChecks = new List<string>
            {
                "AxisConsistency",
                "CandidateSetConsistency",
                "SampleTraceConsistency"
            },
            DeferredChecks = new List<string>
            {
                "SectionFrameConsistency",
                "TopologyInputConsistency"
            },
            Summary = BuildSummary(memberRows),
            MemberRows = memberRows.ToList(),
            StationRows = new List<FoundationGeometryHealthAuditStationRow>(),
            CandidateRows = new List<FoundationGeometryHealthAuditCandidateRow>()
        };
    }

    public static string BuildMarkdown(FoundationGeometryHealthAuditArtifact artifact)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Foundation Geometry Health Audit");
        builder.AppendLine();
        builder.AppendLine($"- 生成时间：`{artifact.GeneratedAtUtc}`");
        builder.AppendLine($"- SourceRunId：`{Normalize(artifact.SourceRunId)}`");
        builder.AppendLine($"- SourceInputDirectory：`{Normalize(artifact.SourceInputDirectory)}`");
        builder.AppendLine($"- SourceOutputDirectory：`{Normalize(artifact.SourceOutputDirectory)}`");
        builder.AppendLine($"- 启用检查：`{string.Join(" / ", artifact.EnabledChecks)}`");
        builder.AppendLine($"- 暂缓检查：`{string.Join(" / ", artifact.DeferredChecks)}`");
        builder.AppendLine();

        builder.AppendLine("## 总体状态统计");
        builder.AppendLine();
        builder.AppendLine("| Total | PASS | WARNING | FAIL | NOT_EVALUATED |");
        builder.AppendLine("| ---: | ---: | ---: | ---: | ---: |");
        builder.Append("| ").Append(artifact.Summary.TotalMemberCount)
            .Append(" | ").Append(artifact.Summary.PassCount)
            .Append(" | ").Append(artifact.Summary.WarningCount)
            .Append(" | ").Append(artifact.Summary.FailCount)
            .Append(" | ").Append(artifact.Summary.NotEvaluatedCount)
            .AppendLine(" |");
        builder.AppendLine();

        AppendBreakdown(builder, "主要原因分布", artifact.Summary.PrimaryReasonBreakdown);
        AppendBreakdown(builder, "建议排查层级分布", artifact.Summary.SuggestedInvestigationLayerBreakdown);

        builder.AppendLine("## 检查状态分布");
        builder.AppendLine();
        builder.AppendLine("| Check | Status | Count | Share |");
        builder.AppendLine("| --- | --- | ---: | ---: |");
        foreach (var item in artifact.Summary.CheckStatusBreakdown)
        {
            builder.Append("| ").Append(Escape(item.CheckLabelZh))
                .Append(" | ").Append(Escape(item.LabelZh))
                .Append(" | ").Append(item.Count)
                .Append(" | ").Append(item.Share.ToString("0.00"))
                .AppendLine(" |");
        }

        builder.AppendLine();
        builder.AppendLine("## FAIL / WARNING 构件");
        builder.AppendLine();
        AppendMemberTable(
            builder,
            artifact.MemberRows.Where(item => item.OverallHealthStatusCode is "FAIL" or "WARNING"));

        builder.AppendLine();
        builder.AppendLine("## 全量 Member Summary");
        builder.AppendLine();
        AppendMemberTable(builder, artifact.MemberRows);

        builder.AppendLine();
        builder.AppendLine("## 明细说明");
        builder.AppendLine();
        builder.AppendLine("- 首版只落 member-level summary。");
        builder.AppendLine("- `StationRows` 与 `CandidateRows` 当前按契约输出为空数组，后续按需要再启用。");

        return builder.ToString();
    }

    private static FoundationGeometryHealthAuditSummary BuildSummary(
        IReadOnlyList<FoundationGeometryHealthAuditMemberRow> rows)
    {
        return new FoundationGeometryHealthAuditSummary
        {
            TotalMemberCount = rows.Count,
            PassCount = rows.Count(item => item.OverallHealthStatusCode == "PASS"),
            WarningCount = rows.Count(item => item.OverallHealthStatusCode == "WARNING"),
            FailCount = rows.Count(item => item.OverallHealthStatusCode == "FAIL"),
            NotEvaluatedCount = rows.Count(item => item.OverallHealthStatusCode == "NOT_EVALUATED"),
            PrimaryReasonBreakdown = BuildBreakdown(rows, item => item.PrimaryReasonCode, item => item.PrimaryReasonLabelZh),
            SuggestedInvestigationLayerBreakdown = BuildBreakdown(rows, item => item.SuggestedInvestigationLayerCode, item => item.SuggestedInvestigationLayerLabelZh),
            CheckStatusBreakdown = BuildCheckBreakdown(rows)
        };
    }

    private static List<FoundationGeometryHealthAuditBreakdownItem> BuildBreakdown(
        IReadOnlyList<FoundationGeometryHealthAuditMemberRow> rows,
        Func<FoundationGeometryHealthAuditMemberRow, string> codeSelector,
        Func<FoundationGeometryHealthAuditMemberRow, string> labelSelector)
    {
        var total = Math.Max(1, rows.Count);
        return rows
            .GroupBy(item => Normalize(codeSelector(item)))
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(
                group =>
                {
                    var sample = group.First();
                    return new FoundationGeometryHealthAuditBreakdownItem
                    {
                        Code = Normalize(codeSelector(sample)),
                        LabelZh = Normalize(labelSelector(sample)),
                        Count = group.Count(),
                        Share = group.Count() / (double)total
                    };
                })
            .ToList();
    }

    private static List<FoundationGeometryHealthAuditCheckStatusBreakdownItem> BuildCheckBreakdown(
        IReadOnlyList<FoundationGeometryHealthAuditMemberRow> rows)
    {
        var total = Math.Max(1, rows.Count);
        var checks = new[]
        {
            ("AxisConsistency", "轴线一致性", new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.AxisConsistencyStatusCode), new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.AxisConsistencyStatusLabelZh)),
            ("CandidateSetConsistency", "主体候选集合一致性", new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.CandidateSetConsistencyStatusCode), new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.CandidateSetConsistencyStatusLabelZh)),
            ("SampleTraceConsistency", "sample/trace 一致性", new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.SampleTraceConsistencyStatusCode), new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.SampleTraceConsistencyStatusLabelZh)),
            ("SectionFrameConsistency", "截面坐标系一致性", new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.SectionFrameConsistencyStatusCode), new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.SectionFrameConsistencyStatusLabelZh)),
            ("TopologyInputConsistency", "拓扑输入一致性", new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.TopologyInputConsistencyStatusCode), new Func<FoundationGeometryHealthAuditMemberRow, string>(item => item.TopologyInputConsistencyStatusLabelZh))
        };

        return checks
            .SelectMany(
                check => rows
                    .GroupBy(item => Normalize(check.Item3(item)))
                    .Select(
                        group =>
                        {
                            var sample = group.First();
                            return new FoundationGeometryHealthAuditCheckStatusBreakdownItem
                            {
                                CheckCode = check.Item1,
                                CheckLabelZh = check.Item2,
                                Code = Normalize(check.Item3(sample)),
                                LabelZh = Normalize(check.Item4(sample)),
                                Count = group.Count(),
                                Share = group.Count() / (double)total
                            };
                        }))
            .OrderBy(item => item.CheckCode, StringComparer.Ordinal)
            .ThenByDescending(item => item.Count)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ToList();
    }

    private static void AppendBreakdown(
        StringBuilder builder,
        string title,
        IReadOnlyList<FoundationGeometryHealthAuditBreakdownItem> items)
    {
        builder.AppendLine($"## {title}");
        builder.AppendLine();
        builder.AppendLine("| Code | LabelZh | Count | Share |");
        builder.AppendLine("| --- | --- | ---: | ---: |");
        foreach (var item in items)
        {
            builder.Append("| ").Append(Escape(item.Code))
                .Append(" | ").Append(Escape(item.LabelZh))
                .Append(" | ").Append(item.Count)
                .Append(" | ").Append(item.Share.ToString("0.00"))
                .AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendMemberTable(
        StringBuilder builder,
        IEnumerable<FoundationGeometryHealthAuditMemberRow> rows)
    {
        builder.AppendLine("| AssemblyNumber | OverallHealthStatus | PrimaryReason | SuggestedInvestigationLayer | AxisConsistency | CandidateSetConsistency | SampleTraceConsistency | EvidenceSummary |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");

        foreach (var row in rows)
        {
            builder.Append("| ").Append(Escape(row.AssemblyNumber))
                .Append(" | ").Append(Escape(row.OverallHealthStatusLabelZh))
                .Append(" | ").Append(Escape(row.PrimaryReasonLabelZh))
                .Append(" | ").Append(Escape(row.SuggestedInvestigationLayerLabelZh))
                .Append(" | ").Append(Escape(row.AxisConsistencyStatusLabelZh))
                .Append(" | ").Append(Escape(row.CandidateSetConsistencyStatusLabelZh))
                .Append(" | ").Append(Escape(row.SampleTraceConsistencyStatusLabelZh))
                .Append(" | ").Append(Escape(row.EvidenceSummaryZh))
                .AppendLine(" |");
        }
    }

    private static string ResolveSourceInputDirectory(IReadOnlyList<FoundationGeometryHealthAuditMemberRow> rows)
    {
        var directories = rows
            .Select(item => item.SourceFileName)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(Path.GetDirectoryName)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray();
        if (directories.Length == 0)
        {
            return string.Empty;
        }

        var first = directories[0];
        if (directories.All(item => string.Equals(item, first, StringComparison.OrdinalIgnoreCase)))
        {
            if (string.Equals(Path.GetFileName(first), "members", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetDirectoryName(first) ?? first;
            }

            return first;
        }

        return Path.GetDirectoryName(first) ?? first;
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "NONE" : value.Trim();
    }

    private static string Escape(string? value)
    {
        return Normalize(value).Replace("|", "\\|");
    }
}
