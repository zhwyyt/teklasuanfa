using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceArtifactBuilder
{
    public static DefinitionClauseDecisionFullRunSourceArtifact Build(
        IReadOnlyList<DefinitionClauseDecisionFullRunAssemblySource> assemblies,
        IReadOnlyList<DefinitionClauseDecisionFullRunRepresentativePartSource> representativeParts)
    {
        return new DefinitionClauseDecisionFullRunSourceArtifact
        {
            Assemblies = assemblies.ToList(),
            RepresentativeParts = representativeParts.ToList(),
            Summary = new DefinitionClauseDecisionFullRunSourceArtifactSummary
            {
                AssemblyCount = assemblies.Count,
                RepresentativePartCount = representativeParts.Count,
                AssembliesWithTopologyRewrite = assemblies.Count(item => item.HasTopologyRewrite),
                CoreRepresentativePartCount = representativeParts.Count(item => item.IsCurrentCorePart),
                ReviewRepresentativePartCount = representativeParts.Count(item => item.IsCurrentReviewPart),
                InputMainPartRepresentativeCount = representativeParts.Count(item => item.IsInputMainPart),
                LeadClauseBreakdown = BuildBreakdown(
                    representativeParts,
                    item => item.LeadClauseCode,
                    item => item.LeadClauseLabelZh),
                DefinitionClauseBreakdown = BuildBreakdown(
                    representativeParts,
                    item => item.TopologyRewriteDefinitionClauseCode,
                    item => item.TopologyRewriteDefinitionClauseLabelZh),
                DefinitionClauseEffectBreakdown = BuildBreakdown(
                    representativeParts,
                    item => item.TopologyRewriteDefinitionClauseEffectCode,
                    item => item.TopologyRewriteDefinitionClauseEffectLabelZh),
                EffectDirectionBreakdown = BuildAssemblyBreakdown(
                    assemblies,
                    item => item.LeadClauseEffectDirectionCode,
                    item => item.LeadClauseEffectDirectionLabelZh),
                PatternBreakdown = BuildBreakdown(
                    representativeParts,
                    item => item.TopologyRewritePatternCode,
                    item => item.TopologyRewritePatternLabelZh),
                VerdictBreakdown = BuildBreakdown(
                    representativeParts,
                    item => item.ClauseVerdictCode,
                    item => item.ClauseVerdictLabelZh),
                PromotionReadinessBreakdown = BuildBreakdown(
                    representativeParts,
                    item => item.ClausePromotionReadinessCode,
                    item => item.ClausePromotionReadinessLabelZh),
                CandidateDirectionBreakdown = BuildBreakdown(
                    representativeParts,
                    item => item.CandidateDirectionCode,
                    item => item.CandidateDirectionLabelZh)
            }
        };
    }

    public static string BuildMarkdown(DefinitionClauseDecisionFullRunSourceArtifact artifact)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DefinitionClauseDecision Full-Run Source Summary");
        builder.AppendLine();
        builder.AppendLine("## 总览");
        builder.AppendLine();
        builder.AppendLine($"- 构件数：`{artifact.Summary.AssemblyCount}`");
        builder.AppendLine($"- 代表零件数：`{artifact.Summary.RepresentativePartCount}`");
        builder.AppendLine($"- 命中 TopologyRewrite 的构件数：`{artifact.Summary.AssembliesWithTopologyRewrite}`");
        builder.AppendLine($"- core 代表零件数：`{artifact.Summary.CoreRepresentativePartCount}`");
        builder.AppendLine($"- review 代表零件数：`{artifact.Summary.ReviewRepresentativePartCount}`");
        builder.AppendLine($"- 输入主件代表零件数：`{artifact.Summary.InputMainPartRepresentativeCount}`");
        builder.AppendLine();

        AppendBreakdown(builder, "LeadClause 分布", artifact.Summary.LeadClauseBreakdown);
        AppendBreakdown(builder, "DefinitionClause 分布", artifact.Summary.DefinitionClauseBreakdown);
        AppendBreakdown(builder, "DefinitionClauseEffect 分布", artifact.Summary.DefinitionClauseEffectBreakdown);
        AppendBreakdown(builder, "EffectDirection 分布", artifact.Summary.EffectDirectionBreakdown);
        AppendBreakdown(builder, "Pattern 分布", artifact.Summary.PatternBreakdown);
        AppendBreakdown(builder, "ClauseVerdict 分布", artifact.Summary.VerdictBreakdown);
        AppendBreakdown(builder, "PromotionReadiness 分布", artifact.Summary.PromotionReadinessBreakdown);
        AppendBreakdown(builder, "CandidateDirection 分布", artifact.Summary.CandidateDirectionBreakdown);

        builder.AppendLine("## 构件级语义聚合");
        builder.AppendLine();
        builder.AppendLine("| MemberId | AssemblyId | LeadClause | LeadClauseEffect | EffectDirection | Break/Rewrite | LeadClauseVerdict | LeadClausePromotionReadiness | ClauseVerdictMixStatus | ClausePromotionReadinessMixStatus |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");

        foreach (var item in artifact.Assemblies.Take(40))
        {
            builder.Append("| ")
                .Append(Escape(item.MemberId)).Append(" | ")
                .Append(Escape(item.AssemblyId)).Append(" | ")
                .Append(Escape(Coalesce(item.LeadClauseLabelZh, item.LeadClauseCode))).Append(" | ")
                .Append(Escape(Coalesce(item.LeadClauseEffectLabelZh, item.LeadClauseEffectCode))).Append(" | ")
                .Append(Escape(Coalesce(item.LeadClauseEffectDirectionLabelZh, item.LeadClauseEffectDirectionCode))).Append(" | ")
                .Append(Escape($"B{item.LeadClauseBreakEffectCount}/R{item.LeadClauseRewriteEffectCount}")).Append(" | ")
                .Append(Escape(Coalesce(item.LeadClauseVerdictLabelZh, item.LeadClauseVerdictCode))).Append(" | ")
                .Append(Escape(Coalesce(item.LeadClausePromotionReadinessLabelZh, item.LeadClausePromotionReadinessCode))).Append(" | ")
                .Append(Escape(item.ClauseVerdictMixStatus)).Append(" | ")
                .Append(Escape(item.ClausePromotionReadinessMixStatus)).AppendLine(" |");
        }

        builder.AppendLine();

        builder.AppendLine("## 代表样本表");
        builder.AppendLine();
        builder.AppendLine("| MemberId | AssemblyId | PartId | PartName | Role | Pattern | DefinitionClause | DefinitionClauseEffect | LeadClause | StationTier | ProofCompleteness | CandidateDirection | ClauseVerdict | PromotionReadiness | ConflictReasons | ReviewPromptZh |");
        builder.AppendLine("| --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");

        foreach (var item in artifact.RepresentativeParts.Take(40))
        {
            builder.Append("| ")
                .Append(Escape(item.MemberId)).Append(" | ")
                .Append(Escape(item.AssemblyId)).Append(" | ")
                .Append(item.RepresentativePartId.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(" | ")
                .Append(Escape(item.RepresentativePartName)).Append(" | ")
                .Append(Escape(BuildRoleLabel(item))).Append(" | ")
                .Append(Escape(Coalesce(item.TopologyRewritePatternLabelZh, item.TopologyRewritePatternCode))).Append(" | ")
                .Append(Escape(Coalesce(item.TopologyRewriteDefinitionClauseLabelZh, item.TopologyRewriteDefinitionClauseCode))).Append(" | ")
                .Append(Escape(Coalesce(item.TopologyRewriteDefinitionClauseEffectLabelZh, item.TopologyRewriteDefinitionClauseEffectCode))).Append(" | ")
                .Append(Escape(Coalesce(item.LeadClauseLabelZh, item.LeadClauseCode))).Append(" | ")
                .Append(Escape(Coalesce(item.StationTierLabelZh, item.StationTierCode))).Append(" | ")
                .Append(Escape(Coalesce(item.ProofCompletenessTierLabelZh, item.ProofCompletenessTierCode))).Append(" | ")
                .Append(Escape(Coalesce(item.CandidateDirectionLabelZh, item.CandidateDirectionCode))).Append(" | ")
                .Append(Escape(Coalesce(item.ClauseVerdictLabelZh, item.ClauseVerdictCode))).Append(" | ")
                .Append(Escape(Coalesce(item.ClausePromotionReadinessLabelZh, item.ClausePromotionReadinessCode))).Append(" | ")
                .Append(Escape(string.Join(", ", item.ConflictReasons))).Append(" | ")
                .Append(Escape(item.ReviewPromptZh)).AppendLine(" |");
        }

        return builder.ToString();
    }

    private static List<DefinitionClauseDecisionBreakdownItem> BuildBreakdown(
        IEnumerable<DefinitionClauseDecisionFullRunRepresentativePartSource> rows,
        Func<DefinitionClauseDecisionFullRunRepresentativePartSource, string> codeSelector,
        Func<DefinitionClauseDecisionFullRunRepresentativePartSource, string> labelSelector)
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

    private static List<DefinitionClauseDecisionBreakdownItem> BuildAssemblyBreakdown(
        IEnumerable<DefinitionClauseDecisionFullRunAssemblySource> rows,
        Func<DefinitionClauseDecisionFullRunAssemblySource, string> codeSelector,
        Func<DefinitionClauseDecisionFullRunAssemblySource, string> labelSelector)
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

    private static string BuildRoleLabel(DefinitionClauseDecisionFullRunRepresentativePartSource item)
    {
        if (item.IsCurrentCorePart && item.IsInputMainPart)
        {
            return "core+main";
        }

        if (item.IsCurrentCorePart)
        {
            return "core";
        }

        if (item.IsCurrentReviewPart && item.IsInputMainPart)
        {
            return "review+main";
        }

        if (item.IsCurrentReviewPart)
        {
            return "review";
        }

        if (item.IsInputMainPart)
        {
            return "main";
        }

        return "other";
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
