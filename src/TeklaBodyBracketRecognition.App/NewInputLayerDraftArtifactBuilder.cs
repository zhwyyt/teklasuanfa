using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class NewInputLayerDraftArtifactBuilder
{
    public static NewInputLayerDraftArtifact Build(
        string outputDirectory,
        IReadOnlyList<NewInputLayerDraftMemberRow> members)
    {
        var sourceInputDirectory = ResolveSourceInputDirectory(members);
        var allWarnings = members
            .SelectMany(item => item.NormalizationWarnings)
            .Concat(members.SelectMany(item => item.NormalizedLongitudinalPath.WarningCodes))
            .Concat(members.SelectMany(item => item.NormalizedParts.SelectMany(part => part.NormalizationWarnings)))
            .ToArray();
        return new NewInputLayerDraftArtifact
        {
            GeneratedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            SourceRunId = string.IsNullOrWhiteSpace(sourceInputDirectory) ? string.Empty : Path.GetFileName(sourceInputDirectory),
            SourceInputDirectory = sourceInputDirectory,
            SourceOutputDirectory = outputDirectory,
            Summary = new NewInputLayerDraftSummary
            {
                MemberCount = members.Count,
                TotalPartCount = members.Sum(item => item.NormalizedParts.Count),
                PathModelTypeBreakdown = BuildBreakdown(
                    members.Select(item => (item.NormalizedLongitudinalPath.PathModelType, ToPathModelLabelZh(item.NormalizedLongitudinalPath.PathModelType)))),
                PartModelTypeBreakdown = BuildBreakdown(
                    members.SelectMany(item => item.NormalizedParts.Select(part => (part.PartModelType, ToPartModelLabelZh(part.PartModelType))))),
                RepresentationKindBreakdown = BuildBreakdown(
                    members.SelectMany(item => item.NormalizedParts.Select(part => (part.Representation.RepresentationKind, part.Representation.RepresentationKindLabelZh)))),
                RepresentationLevelBreakdown = BuildBreakdown(
                    members.SelectMany(item => item.NormalizedParts.Select(part => (part.Representation.RepresentationLevel, part.Representation.RepresentationLevelLabelZh)))),
                DistortionRiskBreakdown = BuildBreakdown(
                    members.SelectMany(item => item.NormalizedParts.Select(part => (part.Representation.DistortionRiskCode, part.Representation.DistortionRiskLabelZh)))),
                WarningBreakdown = BuildBreakdown(
                    allWarnings.Select(item => (item, ToWarningLabelZh(item))))
            },
            Members = members.ToList()
        };
    }

    public static string BuildMarkdown(NewInputLayerDraftArtifact artifact)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# New Input Layer Draft");
        builder.AppendLine();
        builder.AppendLine($"- 生成时间：`{artifact.GeneratedAtUtc}`");
        builder.AppendLine($"- SourceRunId：`{Normalize(artifact.SourceRunId)}`");
        builder.AppendLine($"- SourceInputDirectory：`{Normalize(artifact.SourceInputDirectory)}`");
        builder.AppendLine($"- SourceOutputDirectory：`{Normalize(artifact.SourceOutputDirectory)}`");
        builder.AppendLine();
        builder.AppendLine("## 归一化策略");
        builder.AppendLine();
        builder.AppendLine($"- PathModelPolicy：`{artifact.NormalizationProfile.PathModelPolicy}`");
        builder.AppendLine($"- PlateModelPolicy：`{artifact.NormalizationProfile.PlateModelPolicy}`");
        builder.AppendLine($"- FallbackPolicy：`{artifact.NormalizationProfile.FallbackPolicy}`");
        builder.AppendLine($"- ConfidencePolicy：`{artifact.NormalizationProfile.ConfidencePolicy}`");
        builder.AppendLine();

        AppendBreakdown(builder, "路径模型分布", artifact.Summary.PathModelTypeBreakdown);
        AppendBreakdown(builder, "零件模型分布", artifact.Summary.PartModelTypeBreakdown);
        AppendBreakdown(builder, "表达类型分布", artifact.Summary.RepresentationKindBreakdown);
        AppendBreakdown(builder, "表达等级分布", artifact.Summary.RepresentationLevelBreakdown);
        AppendBreakdown(builder, "失真风险分布", artifact.Summary.DistortionRiskBreakdown);
        AppendBreakdown(builder, "归一化警告分布", artifact.Summary.WarningBreakdown);

        builder.AppendLine("## Member Summary");
        builder.AppendLine();
        builder.AppendLine("| MemberId | AssemblyId | PathModelType | PathLength | PartCount | 精确/近精确 | 近似/退化 | 高风险 | Warnings |");
        builder.AppendLine("| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | --- |");
        foreach (var member in artifact.Members)
        {
            var warnings = member.NormalizationWarnings
                .Concat(member.NormalizedLongitudinalPath.WarningCodes)
                .Concat(member.NormalizedParts.SelectMany(item => item.NormalizationWarnings))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var exactLikeCount = member.NormalizedParts.Count(item =>
                string.Equals(item.Representation.RepresentationLevel, "EXACT", StringComparison.Ordinal) ||
                string.Equals(item.Representation.RepresentationLevel, "NEAR_EXACT", StringComparison.Ordinal));
            var approximateCount = member.NormalizedParts.Count(item =>
                string.Equals(item.Representation.RepresentationLevel, "APPROXIMATE", StringComparison.Ordinal) ||
                string.Equals(item.Representation.RepresentationLevel, "FALLBACK", StringComparison.Ordinal));
            var highRiskCount = member.NormalizedParts.Count(item =>
                string.Equals(item.Representation.DistortionRiskCode, "HIGH", StringComparison.Ordinal));
            builder.Append("| ").Append(Escape(member.MemberId))
                .Append(" | ").Append(Escape(member.AssemblyId))
                .Append(" | ").Append(Escape(ToPathModelLabelZh(member.NormalizedLongitudinalPath.PathModelType)))
                .Append(" | ").Append(member.NormalizedLongitudinalPath.PathLength.ToString("0.###"))
                .Append(" | ").Append(member.NormalizedParts.Count)
                .Append(" | ").Append(exactLikeCount)
                .Append(" | ").Append(approximateCount)
                .Append(" | ").Append(highRiskCount)
                .Append(" | ").Append(Escape(string.Join(",", warnings.DefaultIfEmpty("NONE"))))
                .AppendLine(" |");
        }

        return builder.ToString();
    }

    private static List<NewInputLayerDraftBreakdownItem> BuildBreakdown(
        IEnumerable<(string Code, string LabelZh)> items)
    {
        return items
            .GroupBy(item => Normalize(item.Code))
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new NewInputLayerDraftBreakdownItem
            {
                Code = group.Key,
                LabelZh = group.First().LabelZh,
                Count = group.Count()
            })
            .ToList();
    }

    private static void AppendBreakdown(
        StringBuilder builder,
        string title,
        IReadOnlyList<NewInputLayerDraftBreakdownItem> items)
    {
        builder.AppendLine($"## {title}");
        builder.AppendLine();
        builder.AppendLine("| Code | LabelZh | Count |");
        builder.AppendLine("| --- | --- | ---: |");
        foreach (var item in items)
        {
            builder.Append("| ").Append(Escape(item.Code))
                .Append(" | ").Append(Escape(item.LabelZh))
                .Append(" | ").Append(item.Count)
                .AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static string ResolveSourceInputDirectory(IReadOnlyList<NewInputLayerDraftMemberRow> members)
    {
        var directories = members
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
            return string.Equals(Path.GetFileName(first), "members", StringComparison.OrdinalIgnoreCase)
                ? Path.GetDirectoryName(first) ?? first
                : first;
        }

        return Path.GetDirectoryName(first) ?? first;
    }

    private static string ToPathModelLabelZh(string code) => code switch
    {
        "RAW_AXIS_SEGMENTS" => "原始轴段路径",
        "GUIDE_POLYLINE_RECONSTRUCTED" => "guide polyline 重建路径",
        "MAIN_AXIS_FALLBACK" => "主轴回退路径",
        "SYNTHETIC_PATH" => "合成路径",
        _ => Normalize(code)
    };

    private static string ToPartModelLabelZh(string code) => code switch
    {
        "PATH_SECTION_SWEEP" => "路径+截面扫掠",
        "PLATE_BOUNDARY_THICKNESS" => "边界+厚度板件",
        "UNCLASSIFIED_SOLID_SUMMARY" => "未分类实体摘要",
        _ => Normalize(code)
    };

    private static string ToWarningLabelZh(string code) => code switch
    {
        "NONE" => "无",
        "RAW_INPUT_MISSING" => "原始输入缺失",
        "FALLBACK_USED" => "发生回退",
        "MULTIPLE_SOURCE_CONFLICT" => "多来源冲突",
        "LOW_CONFIDENCE_NORMALIZATION" => "归一化置信度偏低",
        "PATH_TOO_SHORT_FOR_MEMBER" => "路径相对构件跨度偏短",
        "PATH_SOURCE_CONFLICT" => "路径来源存在冲突",
        "PATH_FALLBACK_TO_MAIN_AXIS" => "路径回退到主轴",
        "PATH_SEGMENTS_GAPPED" => "路径片段存在断裂",
        "THICKNESS_INFERRED_FROM_SIZE" => "厚度由尺寸推断",
        "BOUNDARY_UNRESOLVED" => "边界未解析",
        "PART_MODEL_TYPE_UNCLASSIFIED" => "零件模型类型未分类",
        "LOCAL_FRAME_REPAIRED" => "局部坐标系已修复",
        _ => Normalize(code)
    };

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "NONE" : value.Trim();
    }

    private static string Escape(string? value)
    {
        return Normalize(value).Replace("|", "\\|");
    }
}
