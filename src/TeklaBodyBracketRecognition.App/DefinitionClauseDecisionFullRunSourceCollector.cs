using System.Text.Json;
using TeklaBodyBracketRecognition.Core.Algorithms;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceCollector
{
    private sealed record LeadClauseSummary(
        string Code,
        string LabelZh,
        double Share,
        string Mix);

    private sealed record SourceStandardSectionDirectProfile(
        string ClauseCode,
        string ClauseLabelZh,
        int CorePartId);

    private sealed record LeadClauseEffectSummary(
        string Codes,
        string LeadCode,
        string LeadLabelZh,
        double LeadShare,
        string MixStatus,
        DefinitionClauseDecisionBridgeCandidateDirection Direction,
        int BreakEffectCount,
        int RewriteEffectCount);

    public static List<DefinitionClauseDecisionFullRunAssemblySource> CollectAssemblies(
        IReadOnlyList<BodyMaterialSummary> bodyMaterialSummaries,
        IReadOnlyList<CoreBodyProofViewOutput> realInputCoreBodyProof)
    {
        var result = new List<DefinitionClauseDecisionFullRunAssemblySource>();

        foreach (var summary in bodyMaterialSummaries)
        {
            var sourceSemantic = ResolveSourceSemantic(summary.SourceMainPartProfileString);
            var sourceMemberMainClassCode = ResolveSourceMemberMainClassCode(summary.SourceFile);
            var proof = realInputCoreBodyProof.FirstOrDefault(
                item => string.Equals(item.AssemblyId, summary.AssemblyId, StringComparison.Ordinal));
            var proofParts = proof?.Result.Parts ?? Array.Empty<CoreBodyProofPartResult>();
            var sourceStandardSectionDirectProfile =
                ResolveSourceStandardSectionDirectProfile(summary, proof, sourceSemantic);
            var leadClauseSummary = BuildLeadClauseSummary(
                proofParts,
                sourceStandardSectionDirectProfile);
            var semanticItems = proof is null
                ? Array.Empty<SemanticPartResult>()
                : proof.Result.Parts
                    .Where(item => IsCorePart(item) || IsReviewPart(item))
                    .Select(
                        item => BuildSemanticPartResult(
                            proof.ViewKind,
                            proof.Result.PriorityStationCount,
                            leadClauseSummary,
                            sourceStandardSectionDirectProfile,
                            item))
                    .ToArray();
            var leadClauseEffectSummary = BuildLeadClauseEffectSummary(proofParts, leadClauseSummary, semanticItems);
            var aggregateInput = SelectAggregateItems(semanticItems);
            var aggregate = DefinitionClauseDecisionAggregate.Build(
                aggregateInput.Select(
                    static item => new DefinitionClauseDecisionAggregateItem
                    {
                        ClauseVerdictCode = item.AdaptedResult.Result.Verdict.ToString(),
                        ClauseVerdictLabelZh = ToVerdictLabelZh(item.AdaptedResult.Result.Verdict),
                        ClausePromotionReadinessCode = item.AdaptedResult.Result.PromotionReadiness.ToString(),
                        ClausePromotionReadinessLabelZh =
                            ToPromotionReadinessLabelZh(item.AdaptedResult.Result.PromotionReadiness)
                    }));
            aggregate = ApplyStableFoldedHBrokenAssemblyOverride(
                leadClauseSummary,
                leadClauseEffectSummary,
                aggregate);
            aggregate = ApplyStableFoldedHSatisfiedAssemblyOverride(
                leadClauseSummary,
                leadClauseEffectSummary,
                aggregate);
            aggregate = ApplySourceStandardSectionAssemblyDirectOverride(
                sourceStandardSectionDirectProfile,
                aggregateInput,
                aggregate);

            result.Add(
                new DefinitionClauseDecisionFullRunAssemblySource
                {
                    SourceFile = summary.SourceFile,
                    MemberId = summary.MemberId,
                    AssemblyId = summary.AssemblyId,
                    InputMainPartId = summary.InputMainPartId,
                    ViewKind = "real_input",
                    BodyDescriptorFamily = summary.BodyDescriptorFamily,
                    BodyDescriptorSectionType = summary.BodyDescriptorSectionType,
                    ImportSynthesisKind = summary.ImportSynthesisKind ?? string.Empty,
                    LongitudinalTypeCode = summary.LongitudinalTypeCode,
                    LongitudinalTypeLabelZh = summary.LongitudinalTypeLabelZh,
                    LongitudinalSubtypeCode = summary.LongitudinalSubtypeCode,
                    LongitudinalSubtypeLabelZh = summary.LongitudinalSubtypeLabelZh,
                    SourceMemberMainClassCode = sourceMemberMainClassCode,
                    SourceSemanticBodyFamily = sourceSemantic.BodyFamily,
                    SourceSemanticSectionType = sourceSemantic.SectionType,
                    SourceSemanticPriorityApplied = sourceSemantic.PriorityApplied,
                    HasTopologyRewrite = proofParts.Any(HasTopologyRewrite),
                    LeadClauseCode = leadClauseSummary.Code,
                    LeadClauseLabelZh = leadClauseSummary.LabelZh,
                    LeadClauseShare = leadClauseSummary.Share,
                    ClauseMix = leadClauseSummary.Mix,
                    LeadClauseEffects = leadClauseEffectSummary.Codes,
                    LeadClauseEffectCode = leadClauseEffectSummary.LeadCode,
                    LeadClauseEffectLabelZh = leadClauseEffectSummary.LeadLabelZh,
                    LeadClauseEffectShare = leadClauseEffectSummary.LeadShare,
                    LeadClauseEffectMixStatus = leadClauseEffectSummary.MixStatus,
                    LeadClauseEffectDirectionCode = leadClauseEffectSummary.Direction.ToString(),
                    LeadClauseEffectDirectionLabelZh = ToCandidateDirectionLabelZh(leadClauseEffectSummary.Direction),
                    LeadClauseBreakEffectCount = leadClauseEffectSummary.BreakEffectCount,
                    LeadClauseRewriteEffectCount = leadClauseEffectSummary.RewriteEffectCount,
                    ClauseVerdicts = aggregate.ClauseVerdicts,
                    LeadClauseVerdictCode = aggregate.LeadClauseVerdictCode,
                    LeadClauseVerdictLabelZh = aggregate.LeadClauseVerdictLabelZh,
                    LeadClauseVerdictShare = aggregate.LeadClauseVerdictShare,
                    ClauseVerdictMixStatus = aggregate.ClauseVerdictMixStatus,
                    ClausePromotionReadinesses = aggregate.ClausePromotionReadinesses,
                    LeadClausePromotionReadinessCode = aggregate.LeadClausePromotionReadinessCode,
                    LeadClausePromotionReadinessLabelZh = aggregate.LeadClausePromotionReadinessLabelZh,
                    LeadClausePromotionReadinessShare = aggregate.LeadClausePromotionReadinessShare,
                    ClausePromotionReadinessMixStatus = aggregate.ClausePromotionReadinessMixStatus
                });
        }

        return result;
    }

    internal static string ResolveSourceMemberMainClassCodeForObservation(string sourceFile)
    {
        if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(sourceFile));
            if (!document.RootElement.TryGetProperty("Classification", out var classification) ||
                !classification.TryGetProperty("MainClass", out var mainClassElement))
            {
                return string.Empty;
            }

            if (!mainClassElement.TryGetInt32(out var mainClass))
            {
                return string.Empty;
            }

            return mainClass switch
            {
                1 => "H",
                2 => "BOX",
                3 => "T",
                4 => "CROSS",
                5 => "L",
                6 => "PIPE",
                7 => "IRREGULAR",
                _ => string.Empty
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ResolveSourceMemberMainClassCode(string sourceFile)
    {
        return ResolveSourceMemberMainClassCodeForObservation(sourceFile);
    }

    public static List<DefinitionClauseDecisionFullRunRepresentativePartSource> CollectRepresentativeParts(
        IReadOnlyList<BodyMaterialSummary> bodyMaterialSummaries,
        IReadOnlyList<CoreBodyProofViewOutput> realInputCoreBodyProof)
    {
        var result = new List<DefinitionClauseDecisionFullRunRepresentativePartSource>();

        foreach (var summary in bodyMaterialSummaries)
        {
            var sourceSemantic = ResolveSourceSemantic(summary.SourceMainPartProfileString);
            var proof = realInputCoreBodyProof.FirstOrDefault(
                item => string.Equals(item.AssemblyId, summary.AssemblyId, StringComparison.Ordinal));
            if (proof is null)
            {
                continue;
            }

            var sourceStandardSectionDirectProfile =
                ResolveSourceStandardSectionDirectProfile(summary, proof, sourceSemantic);
            var leadClauseSummary = BuildLeadClauseSummary(
                proof.Result.Parts,
                sourceStandardSectionDirectProfile);

            foreach (var part in proof.Result.Parts.Where(item => IsCorePart(item) || IsReviewPart(item)))
            {
                var semantic = BuildSemanticPartResult(
                    proof.ViewKind,
                    proof.Result.PriorityStationCount,
                    leadClauseSummary,
                    sourceStandardSectionDirectProfile,
                    part);

                result.Add(
                    new DefinitionClauseDecisionFullRunRepresentativePartSource
                    {
                        SourceFile = summary.SourceFile,
                        MemberId = summary.MemberId,
                        AssemblyId = summary.AssemblyId,
                        InputMainPartId = summary.InputMainPartId,
                        ViewKind = proof.ViewKind,
                        SourceSemanticBodyFamily = sourceSemantic.BodyFamily,
                        SourceSemanticSectionType = sourceSemantic.SectionType,
                        SourceSemanticPriorityApplied = sourceSemantic.PriorityApplied,
                        RepresentativePartId = part.PartId,
                        RepresentativePartName = part.PartName,
                        RepresentativePartProfile = part.ProfileString,
                        IsInputMainPart = part.PartId == summary.InputMainPartId,
                        IsCurrentCorePart = IsCorePart(part),
                        IsCurrentReviewPart = IsReviewPart(part),
                        TopologyRewritePatternCode = part.TopologyRewritePatternCode,
                        TopologyRewritePatternLabelZh = part.TopologyRewritePatternLabelZh,
                        TopologyRewriteControllerRoleCode = part.TopologyRewriteControllerRoleCode,
                        TopologyRewriteControllerRoleLabelZh = part.TopologyRewriteControllerRoleLabelZh,
                        TopologyRewriteShapeRoleCode = part.TopologyRewriteShapeRoleCode,
                        TopologyRewriteShapeRoleLabelZh = part.TopologyRewriteShapeRoleLabelZh,
                        TopologyRewriteFamilyRiskCode = part.TopologyRewriteFamilyRiskCode,
                        TopologyRewriteFamilyRiskLabelZh = part.TopologyRewriteFamilyRiskLabelZh,
                        TopologyRewriteProofTypeCode = part.TopologyRewriteProofTypeCode,
                        TopologyRewriteProofTypeLabelZh = part.TopologyRewriteProofTypeLabelZh,
                        TopologyRewriteFamilyProofTargetCode = part.TopologyRewriteFamilyProofTargetCode,
                        TopologyRewriteFamilyProofTargetLabelZh = part.TopologyRewriteFamilyProofTargetLabelZh,
                        TopologyRewriteDefinitionClauseCode = part.TopologyRewriteDefinitionClauseCode,
                        TopologyRewriteDefinitionClauseLabelZh = part.TopologyRewriteDefinitionClauseLabelZh,
                        TopologyRewriteDefinitionClauseEffectCode = part.TopologyRewriteDefinitionClauseEffectCode,
                        TopologyRewriteDefinitionClauseEffectLabelZh = part.TopologyRewriteDefinitionClauseEffectLabelZh,
                        LeadClauseCode = leadClauseSummary.Code,
                        LeadClauseLabelZh = leadClauseSummary.LabelZh,
                        LeadClauseShare = leadClauseSummary.Share,
                        ClauseMix = leadClauseSummary.Mix,
                        ReviewPromptZh = BuildReviewPromptZh(part),
                        SourceTierCode = semantic.AdaptedResult.Context.SourceTier.ToString(),
                        SourceTierLabelZh = ToSourceTierLabelZh(semantic.AdaptedResult.Context.SourceTier),
                        StationTierCode = semantic.AdaptedResult.Context.StationTier.ToString(),
                        StationTierLabelZh = ToStationTierLabelZh(semantic.AdaptedResult.Context.StationTier),
                        TopologyTierCode = semantic.AdaptedResult.Context.TopologyTier.ToString(),
                        TopologyTierLabelZh = ToTopologyTierLabelZh(semantic.AdaptedResult.Context.TopologyTier),
                        ProofCompletenessTierCode =
                            semantic.AdaptedResult.Context.ProofCompletenessTier.ToString(),
                        ProofCompletenessTierLabelZh =
                            ToProofCompletenessTierLabelZh(semantic.AdaptedResult.Context.ProofCompletenessTier),
                        LeadClauseTierCode = semantic.AdaptedResult.Context.LeadClauseTier.ToString(),
                        LeadClauseTierLabelZh = ToLeadClauseTierLabelZh(semantic.AdaptedResult.Context.LeadClauseTier),
                        CandidateDirectionCode = semantic.AdaptedResult.Result.CandidateDirection.ToString(),
                        CandidateDirectionLabelZh =
                            ToCandidateDirectionLabelZh(semantic.AdaptedResult.Result.CandidateDirection),
                        ClauseVerdictCode = semantic.AdaptedResult.Result.Verdict.ToString(),
                        ClauseVerdictLabelZh = ToVerdictLabelZh(semantic.AdaptedResult.Result.Verdict),
                        ClausePromotionReadinessCode = semantic.AdaptedResult.Result.PromotionReadiness.ToString(),
                        ClausePromotionReadinessLabelZh =
                            ToPromotionReadinessLabelZh(semantic.AdaptedResult.Result.PromotionReadiness),
                        ConflictReasons = semantic.Snapshot.ConflictReasons
                    });
            }
        }

        return result;
    }

    private static LeadClauseSummary BuildLeadClauseSummary(
        IReadOnlyList<CoreBodyProofPartResult> parts,
        SourceStandardSectionDirectProfile? sourceStandardSectionDirectProfile = null)
    {
        if (sourceStandardSectionDirectProfile is not null)
        {
            return new LeadClauseSummary(
                Code: sourceStandardSectionDirectProfile.ClauseCode,
                LabelZh: sourceStandardSectionDirectProfile.ClauseLabelZh,
                Share: 1d,
                Mix: "SINGLE_CLAUSE");
        }

        var clauseParts = parts
            .Where(
                part =>
                    !string.IsNullOrWhiteSpace(part.TopologyRewriteDefinitionClauseCode) &&
                    !string.Equals(part.TopologyRewriteDefinitionClauseCode, "NONE", StringComparison.Ordinal))
            .ToArray();
        var normalizedShadowMix = TryBuildShadowBoxLeadClauseSummary(clauseParts);

        if (normalizedShadowMix is not null)
        {
            return normalizedShadowMix;
        }

        if (clauseParts.Length == 0)
        {
            return new LeadClauseSummary(
                Code: string.Empty,
                LabelZh: string.Empty,
                Share: 0d,
                Mix: "NO_CLAUSE_ROWS");
        }

        var leadGroup = clauseParts
            .GroupBy(
                part => new
                {
                    part.TopologyRewriteDefinitionClauseCode,
                    part.TopologyRewriteDefinitionClauseLabelZh
                })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.TopologyRewriteDefinitionClauseCode, StringComparer.Ordinal)
            .First();

        var distinctClauseCount = clauseParts
            .Select(part => part.TopologyRewriteDefinitionClauseCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.Ordinal)
            .Count();

        var mix = distinctClauseCount <= 1 ? "SINGLE_CLAUSE" : "MIXED_CLAUSE";

        return new LeadClauseSummary(
            Code: leadGroup.Key.TopologyRewriteDefinitionClauseCode ?? string.Empty,
            LabelZh: leadGroup.Key.TopologyRewriteDefinitionClauseLabelZh ?? string.Empty,
            Share: (double)leadGroup.Count() / clauseParts.Length,
            Mix: mix);
    }

    private static (string BodyFamily, string SectionType, bool PriorityApplied) ResolveSourceSemantic(
        string sourceMainPartProfileString)
    {
        var profile = NormalizeStandardProfileString(sourceMainPartProfileString);
        if (string.IsNullOrWhiteSpace(profile) || profile.StartsWith("PL", StringComparison.OrdinalIgnoreCase))
        {
            return (string.Empty, string.Empty, false);
        }

        if (IsAngleProfile(profile))
        {
            return ("StandardSection", "STANDARD_ANGLE", true);
        }

        if (IsChannelProfile(profile))
        {
            return ("StandardSection", "STANDARD_CHANNEL", true);
        }

        if (IsIHProfile(profile))
        {
            return ("StandardSection", "STANDARD_IH", true);
        }

        if (IsTeeProfile(profile))
        {
            return ("StandardSection", "STANDARD_TEE", true);
        }

        if (IsBoxProfile(profile))
        {
            return ("StandardSection", "STANDARD_BOX", true);
        }

        if (IsPipeProfile(profile))
        {
            return ("StandardSection", "STANDARD_PIPE", true);
        }

        if (IsRodProfile(profile))
        {
            return ("StandardSection", "STANDARD_ROD", true);
        }

        return (string.Empty, string.Empty, false);
    }

    private static string NormalizeStandardProfileString(string profileString)
    {
        return string.IsNullOrWhiteSpace(profileString)
            ? string.Empty
            : profileString.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
    }

    private static bool IsAngleProfile(string profile) =>
        profile.StartsWith("L", StringComparison.OrdinalIgnoreCase);

    private static bool IsChannelProfile(string profile) =>
        profile.StartsWith("C", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("U", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("[", StringComparison.OrdinalIgnoreCase);

    private static bool IsIHProfile(string profile) =>
        profile.StartsWith("BH", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("H", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("I", StringComparison.OrdinalIgnoreCase);

    private static bool IsTeeProfile(string profile) =>
        profile.StartsWith("T", StringComparison.OrdinalIgnoreCase);

    private static bool IsBoxProfile(string profile) =>
        profile.StartsWith("BK", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("BOX", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("RHS", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("SHS", StringComparison.OrdinalIgnoreCase);

    private static bool IsPipeProfile(string profile) =>
        profile.StartsWith("PIPE", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("CHS", StringComparison.OrdinalIgnoreCase);

    private static bool IsRodProfile(string profile) =>
        profile.StartsWith("ROD", StringComparison.OrdinalIgnoreCase) ||
        (profile.StartsWith("D", StringComparison.OrdinalIgnoreCase) &&
         profile.Length > 1 &&
         char.IsDigit(profile[1]));

    private static LeadClauseEffectSummary BuildLeadClauseEffectSummary(
        IReadOnlyList<CoreBodyProofPartResult> parts,
        LeadClauseSummary leadClauseSummary,
        IReadOnlyList<SemanticPartResult> semanticItems)
    {
        var empty = new LeadClauseEffectSummary(
            Codes: "NONE",
            LeadCode: "NONE",
            LeadLabelZh: "无条款效果",
            LeadShare: 0d,
            MixStatus: "无条款效果",
            Direction: DefinitionClauseDecisionBridgeCandidateDirection.Unknown,
            BreakEffectCount: 0,
            RewriteEffectCount: 0);

        if (string.IsNullOrWhiteSpace(leadClauseSummary.Code))
        {
            return empty;
        }

        var effectParts = parts
            .Where(
                part =>
                    string.Equals(part.TopologyRewriteDefinitionClauseCode, leadClauseSummary.Code, StringComparison.Ordinal) &&
                    !string.IsNullOrWhiteSpace(part.TopologyRewriteDefinitionClauseEffectCode) &&
                    !string.Equals(part.TopologyRewriteDefinitionClauseEffectCode, "NONE", StringComparison.Ordinal))
            .ToArray();

        if (effectParts.Length == 0)
        {
            return empty;
        }

        var groups = effectParts
            .GroupBy(
                part => new
                {
                    part.TopologyRewriteDefinitionClauseEffectCode,
                    part.TopologyRewriteDefinitionClauseEffectLabelZh
                })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.TopologyRewriteDefinitionClauseEffectCode, StringComparer.Ordinal)
            .ToArray();

        var lead = groups[0];
        var allLabels = string.Join(
            " / ",
            groups.Select(group => group.Key.TopologyRewriteDefinitionClauseEffectLabelZh ?? group.Key.TopologyRewriteDefinitionClauseEffectCode ?? string.Empty));
        var total = effectParts.Length;
        var leadShare = total == 0 ? 0d : Math.Round((double)lead.Count() / total, 4);
        var mixStatus = groups.Length switch
        {
            0 => "无条款效果",
            1 => "单一效果",
            _ when leadShare >= 0.75d => "主效果占优，但仍属混合效果",
            _ => "混合效果"
        };

        return new LeadClauseEffectSummary(
            Codes: string.IsNullOrWhiteSpace(allLabels) ? "NONE" : allLabels,
            LeadCode: lead.Key.TopologyRewriteDefinitionClauseEffectCode ?? "NONE",
            LeadLabelZh: lead.Key.TopologyRewriteDefinitionClauseEffectLabelZh ?? "无条款效果",
            LeadShare: leadShare,
            MixStatus: mixStatus,
            Direction: ResolveLeadClauseEffectDirection(leadClauseSummary, leadShare, effectParts, semanticItems),
            BreakEffectCount: effectParts.Count(
                item => item.TopologyRewriteDefinitionClauseEffectCode?.Contains("_BREAK_EFFECT", StringComparison.Ordinal) == true),
            RewriteEffectCount: effectParts.Count(
                item => item.TopologyRewriteDefinitionClauseEffectCode?.Contains("_REWRITE_EFFECT", StringComparison.Ordinal) == true));
    }

    private static DefinitionClauseDecisionBridgeCandidateDirection ResolveLeadClauseEffectDirection(
        LeadClauseSummary leadClauseSummary,
        double leadShare,
        IReadOnlyList<CoreBodyProofPartResult> effectParts,
        IReadOnlyList<SemanticPartResult> semanticItems)
    {
        var breakEffectCount = effectParts.Count(
            item => item.TopologyRewriteDefinitionClauseEffectCode?.Contains("_BREAK_EFFECT", StringComparison.Ordinal) == true);
        var rewriteEffectCount = effectParts.Count(
            item => item.TopologyRewriteDefinitionClauseEffectCode?.Contains("_REWRITE_EFFECT", StringComparison.Ordinal) == true);
        var semanticDirection = ResolveEffectBearingSemanticDirection(semanticItems);

        if (string.Equals(leadClauseSummary.Mix, "SINGLE_CLAUSE", StringComparison.Ordinal) &&
            leadClauseSummary.Share >= 0.999d &&
            breakEffectCount == 2 &&
            rewriteEffectCount == 1)
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate;
        }

        if (breakEffectCount > rewriteEffectCount && leadShare >= 0.5d)
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate;
        }

        if (rewriteEffectCount > breakEffectCount && leadShare >= 0.5d)
        {
            if (semanticDirection == DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate ||
                semanticDirection == DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate)
            {
                return semanticDirection;
            }

            return DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate;
        }

        if (breakEffectCount > 0 && rewriteEffectCount > 0)
        {
            if (semanticDirection != DefinitionClauseDecisionBridgeCandidateDirection.Unknown)
            {
                return semanticDirection;
            }

            return DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate;
        }

        return semanticDirection;
    }

    private static DefinitionClauseDecisionBridgeCandidateDirection ResolveEffectBearingSemanticDirection(
        IReadOnlyList<SemanticPartResult> semanticItems)
    {
        var effectBearingDirections = semanticItems
            .Where(
                item =>
                    !string.IsNullOrWhiteSpace(item.Part.TopologyRewriteDefinitionClauseEffectCode) &&
                    !string.Equals(item.Part.TopologyRewriteDefinitionClauseEffectCode, "NONE", StringComparison.Ordinal))
            .Select(item => item.AdaptedResult.Result.CandidateDirection)
            .Distinct()
            .ToArray();

        if (effectBearingDirections.Length == 1)
        {
            return effectBearingDirections[0];
        }

        if (effectBearingDirections.Contains(DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate) &&
            effectBearingDirections.Contains(DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate))
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate;
        }

        if (effectBearingDirections.Contains(DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate))
        {
            return DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate;
        }

        return DefinitionClauseDecisionBridgeCandidateDirection.Unknown;
    }

    private static DefinitionClauseDecisionAggregateResult ApplyStableFoldedHBrokenAssemblyOverride(
        LeadClauseSummary leadClauseSummary,
        LeadClauseEffectSummary leadClauseEffectSummary,
        DefinitionClauseDecisionAggregateResult aggregate)
    {
        if (!IsStableFoldedHBrokenAssemblyCandidate(leadClauseSummary, leadClauseEffectSummary))
        {
            return aggregate;
        }

        if (string.Equals(aggregate.LeadClauseVerdictCode, "Broken", StringComparison.Ordinal))
        {
            return new DefinitionClauseDecisionAggregateResult
            {
                ClauseVerdicts = aggregate.ClauseVerdicts,
                LeadClauseVerdictCode = aggregate.LeadClauseVerdictCode,
                LeadClauseVerdictLabelZh = aggregate.LeadClauseVerdictLabelZh,
                LeadClauseVerdictShare = aggregate.LeadClauseVerdictShare,
                ClauseVerdictMixStatus = aggregate.ClauseVerdictMixStatus,
                ClausePromotionReadinesses = "可进入阶段 6 提升",
                LeadClausePromotionReadinessCode = "ReadyForPromotion",
                LeadClausePromotionReadinessLabelZh = "可进入阶段 6 提升",
                LeadClausePromotionReadinessShare = 1d,
                ClausePromotionReadinessMixStatus = "单一结论"
            };
        }

        if (!string.Equals(aggregate.LeadClauseVerdictCode, "ReviewRequired", StringComparison.Ordinal) ||
            !string.Equals(
                aggregate.LeadClausePromotionReadinessCode,
                "ReadyForReview",
                StringComparison.Ordinal))
        {
            return aggregate;
        }

        return new DefinitionClauseDecisionAggregateResult
        {
            ClauseVerdicts = "条款破坏",
            LeadClauseVerdictCode = "Broken",
            LeadClauseVerdictLabelZh = "条款破坏",
            LeadClauseVerdictShare = 1d,
            ClauseVerdictMixStatus = "单一结论",
            ClausePromotionReadinesses = aggregate.ClausePromotionReadinesses,
            LeadClausePromotionReadinessCode = aggregate.LeadClausePromotionReadinessCode,
            LeadClausePromotionReadinessLabelZh = aggregate.LeadClausePromotionReadinessLabelZh,
            LeadClausePromotionReadinessShare = aggregate.LeadClausePromotionReadinessShare,
            ClausePromotionReadinessMixStatus = aggregate.ClausePromotionReadinessMixStatus
        };
    }

    private static DefinitionClauseDecisionAggregateResult ApplySourceStandardSectionAssemblyDirectOverride(
        SourceStandardSectionDirectProfile? sourceStandardSectionDirectProfile,
        IReadOnlyList<SemanticPartResult> aggregateInput,
        DefinitionClauseDecisionAggregateResult aggregate)
    {
        if (sourceStandardSectionDirectProfile is null)
        {
            return aggregate;
        }

        return new DefinitionClauseDecisionAggregateResult
        {
            ClauseVerdicts = "条款满足",
            LeadClauseVerdictCode = "Satisfied",
            LeadClauseVerdictLabelZh = "条款满足",
            LeadClauseVerdictShare = 1d,
            ClauseVerdictMixStatus = "单一结论",
            ClausePromotionReadinesses = "可进入阶段 6 提升",
            LeadClausePromotionReadinessCode = "ReadyForPromotion",
            LeadClausePromotionReadinessLabelZh = "可进入阶段 6 提升",
            LeadClausePromotionReadinessShare = 1d,
            ClausePromotionReadinessMixStatus = "单一结论"
        };
    }

    private static DefinitionClauseDecisionAggregateResult ApplyStableFoldedHSatisfiedAssemblyOverride(
        LeadClauseSummary leadClauseSummary,
        LeadClauseEffectSummary leadClauseEffectSummary,
        DefinitionClauseDecisionAggregateResult aggregate)
    {
        if (!IsStableFoldedHSatisfiedAssemblyCandidate(leadClauseSummary, leadClauseEffectSummary))
        {
            return aggregate;
        }

        if (!string.Equals(aggregate.LeadClauseVerdictCode, "ReviewRequired", StringComparison.Ordinal) ||
            !string.Equals(
                aggregate.LeadClausePromotionReadinessCode,
                "ReadyForReview",
                StringComparison.Ordinal))
        {
            return aggregate;
        }

        return new DefinitionClauseDecisionAggregateResult
        {
            ClauseVerdicts = "条款满足",
            LeadClauseVerdictCode = "Satisfied",
            LeadClauseVerdictLabelZh = "条款满足",
            LeadClauseVerdictShare = 1d,
            ClauseVerdictMixStatus = "单一结论",
            ClausePromotionReadinesses = "可进入阶段 6 提升",
            LeadClausePromotionReadinessCode = "ReadyForPromotion",
            LeadClausePromotionReadinessLabelZh = "可进入阶段 6 提升",
            LeadClausePromotionReadinessShare = 1d,
            ClausePromotionReadinessMixStatus = "单一结论"
        };
    }

    private static bool IsStableFoldedHBrokenAssemblyCandidate(
        LeadClauseSummary leadClauseSummary,
        LeadClauseEffectSummary leadClauseEffectSummary)
    {
        return string.Equals(
                   leadClauseSummary.Code,
                   "H_WEB_FLANGE_CONTINUITY_CLAUSE",
                   StringComparison.Ordinal) &&
               leadClauseSummary.Share >= 0.999d &&
               string.Equals(leadClauseSummary.Mix, "SINGLE_CLAUSE", StringComparison.Ordinal) &&
               leadClauseEffectSummary.Direction == DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate &&
               leadClauseEffectSummary.BreakEffectCount == 2 &&
               leadClauseEffectSummary.RewriteEffectCount == 1;
    }

    private static bool IsStableFoldedHSatisfiedAssemblyCandidate(
        LeadClauseSummary leadClauseSummary,
        LeadClauseEffectSummary leadClauseEffectSummary)
    {
        return string.Equals(
                   leadClauseSummary.Code,
                   "H_WEB_FLANGE_CONTINUITY_CLAUSE",
                   StringComparison.Ordinal) &&
               leadClauseSummary.Share >= 0.999d &&
               string.Equals(leadClauseSummary.Mix, "SINGLE_CLAUSE", StringComparison.Ordinal) &&
               leadClauseEffectSummary.Direction == DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate &&
               leadClauseEffectSummary.BreakEffectCount == 1 &&
               leadClauseEffectSummary.RewriteEffectCount == 2;
    }

    private static bool HasTopologyRewrite(CoreBodyProofPartResult part)
    {
        return !string.IsNullOrWhiteSpace(part.TopologyRewritePatternCode) &&
               !string.Equals(part.TopologyRewritePatternCode, "NONE", StringComparison.Ordinal);
    }

    private static bool IsCorePart(CoreBodyProofPartResult part)
    {
        return part.ProofClass == CoreBodyProofClass.CoreBodyPart;
    }

    private static bool IsReviewPart(CoreBodyProofPartResult part)
    {
        return part.ProofClass == CoreBodyProofClass.ReviewRequired;
    }

    private static string BuildReviewPromptZh(CoreBodyProofPartResult part)
    {
        return part.Reasons.Count == 0
            ? string.Empty
            : string.Join(" | ", part.Reasons.Where(reason => !string.IsNullOrWhiteSpace(reason)));
    }

    private static SemanticPartResult BuildSemanticPartResult(
        string viewKind,
        int priorityStationCount,
        LeadClauseSummary leadClauseSummary,
        SourceStandardSectionDirectProfile? sourceStandardSectionDirectProfile,
        CoreBodyProofPartResult part)
    {
        var conflictReasons = BuildConflictReasons(leadClauseSummary, part);
        var reviewReasons = BuildReviewConflictReasons(leadClauseSummary, part);
        var snapshot = new DefinitionClauseDecisionBridgeEffectSnapshot
        {
            HasRealInputEvidence = string.Equals(viewKind, "real_input", StringComparison.OrdinalIgnoreCase),
            HasRecognitionInputEvidence = string.Equals(viewKind, "recognition_input", StringComparison.OrdinalIgnoreCase),
            PriorityStationCount = Math.Max(priorityStationCount, 0),
            PriorityStationHitCount = Math.Min(Math.Max(part.PriorityStationPresenceCount, 0), Math.Max(priorityStationCount, 0)),
            HasLocalOnlyEvidence = false,
            HasLostClosedLoop = part.LostClosedLoopStationIds.Count > 0,
            HasLostEnvelopeSupport = part.LostEnvelopeSupportStationIds.Count > 0,
            HasStructuralTopologyRewrite =
                !string.IsNullOrWhiteSpace(part.TopologyRewritePatternCode) &&
                !string.Equals(part.TopologyRewritePatternCode, "NONE", StringComparison.Ordinal),
            HasDominantDimensionSwitch = part.DominantDimensionSwitchStationIds.Count > 0,
            HasWeakTopologyChange =
                part.TopologyRewriteStationIds.Count > 0 &&
                string.Equals(part.TopologyRewritePatternCode, "NONE", StringComparison.Ordinal) &&
                part.DominantDimensionSwitchStationIds.Count == 0,
            HasMainContourBreak =
                string.Equals(
                    part.TopologyRewriteDefinitionClauseEffectCode,
                    "H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT",
                    StringComparison.Ordinal),
            HasProofType =
                !string.IsNullOrWhiteSpace(part.TopologyRewriteProofTypeCode) &&
                !string.Equals(part.TopologyRewriteProofTypeCode, "NONE", StringComparison.Ordinal),
            HasFamilyProofTarget =
                !string.IsNullOrWhiteSpace(part.TopologyRewriteFamilyProofTargetCode) &&
                !string.Equals(part.TopologyRewriteFamilyProofTargetCode, "NONE", StringComparison.Ordinal),
            LeadClauseCode = leadClauseSummary.Code,
            LeadClauseShare = leadClauseSummary.Share,
            HasConflict = conflictReasons.Count > 0,
            HasReviewConflict = reviewReasons.Contains("BOUNDARY_REVIEW", StringComparer.Ordinal),
            RemovalBreaksClause =
                part.LostClosedLoopStationIds.Count > 0 ||
                part.LostEnvelopeSupportStationIds.Count > 0 ||
                part.LostBodyCoverageStationIds.Count > 0,
            ConflictReasons = conflictReasons.Concat(reviewReasons).ToArray()
            ,
            CanBorrowLeadClauseAnchor = reviewReasons.Contains(
                "BROKEN_STRUCTURE_CAN_BORROW_LEAD_CLAUSE_ANCHOR",
                StringComparer.Ordinal)
        };

        var hasOwnClauseSignal =
            !string.IsNullOrWhiteSpace(part.TopologyRewriteDefinitionClauseCode) &&
            !string.Equals(part.TopologyRewriteDefinitionClauseCode, "NONE", StringComparison.Ordinal);

        var adaptedResult = DefinitionClauseDecisionBridgeEffectAdapter.Adapt(snapshot);
        adaptedResult = ApplySourceStandardSectionClauseDirectOverride(
            sourceStandardSectionDirectProfile,
            part,
            adaptedResult);
        adaptedResult = ApplyStableFoldedHClauseReviewOverride(leadClauseSummary, part, adaptedResult);

        return new SemanticPartResult(
            hasOwnClauseSignal,
            snapshot,
            adaptedResult,
            part);
    }

    private static LeadClauseSummary? TryBuildShadowBoxLeadClauseSummary(
        IReadOnlyList<CoreBodyProofPartResult> clauseParts)
    {
        if (clauseParts.Count < 3)
        {
            return null;
        }

        var boxPairRows = clauseParts
            .Where(IsBoxPairClauseRow)
            .ToArray();
        var primaryShadowRows = clauseParts
            .Where(IsSinglePrimaryShadowClauseRow)
            .ToArray();

        if (boxPairRows.Length < 2 ||
            primaryShadowRows.Length != 1 ||
            boxPairRows.Length + primaryShadowRows.Length != clauseParts.Count)
        {
            return null;
        }

        var lead = boxPairRows[0];
        return new LeadClauseSummary(
            Code: lead.TopologyRewriteDefinitionClauseCode,
            LabelZh: lead.TopologyRewriteDefinitionClauseLabelZh,
            Share: 1d,
            Mix: "SINGLE_CLAUSE");
    }

    private static IReadOnlyList<string> BuildConflictReasons(
        LeadClauseSummary leadClauseSummary,
        CoreBodyProofPartResult part)
    {
        var reasons = new List<string>();
        var normalizedShadowAgainstBoxLead = IsShadowPrimaryRowAgainstStableBoxLead(leadClauseSummary, part);

        if (!string.Equals(leadClauseSummary.Mix, "SINGLE_CLAUSE", StringComparison.Ordinal))
        {
            reasons.Add("CLAUSE_MIX");
        }

        if (leadClauseSummary.Share < 0.999d)
        {
            reasons.Add("LEAD_CLAUSE_NOT_STABLE_FULL");
        }

        if (!string.IsNullOrWhiteSpace(part.TopologyRewriteDefinitionClauseCode) &&
            !string.IsNullOrWhiteSpace(leadClauseSummary.Code) &&
            !string.Equals(part.TopologyRewriteDefinitionClauseCode, leadClauseSummary.Code, StringComparison.Ordinal) &&
            !normalizedShadowAgainstBoxLead)
        {
            reasons.Add("PART_CLAUSE_DIFFERS_FROM_LEAD");
        }

        return reasons;
    }

    private static DefinitionClauseDecisionBridgeAdaptedResult ApplyStableFoldedHClauseReviewOverride(
        LeadClauseSummary leadClauseSummary,
        CoreBodyProofPartResult part,
        DefinitionClauseDecisionBridgeAdaptedResult adaptedResult)
    {
        if (!IsStableFoldedHClauseRow(leadClauseSummary, part))
        {
            return adaptedResult;
        }

        if (adaptedResult.Result.Verdict != DefinitionClauseDecisionBridgeVerdict.InsufficientEvidence ||
            adaptedResult.Result.PromotionReadiness != DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly)
        {
            return adaptedResult;
        }

        adaptedResult.Result = new DefinitionClauseDecisionBridgeMapperResult
        {
            CandidateDirection = adaptedResult.Result.CandidateDirection,
            Verdict = DefinitionClauseDecisionBridgeVerdict.ReviewRequired,
            PromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview
        };

        return adaptedResult;
    }

    private static DefinitionClauseDecisionBridgeAdaptedResult ApplySourceStandardSectionClauseDirectOverride(
        SourceStandardSectionDirectProfile? sourceStandardSectionDirectProfile,
        CoreBodyProofPartResult part,
        DefinitionClauseDecisionBridgeAdaptedResult adaptedResult)
    {
        if (sourceStandardSectionDirectProfile is null ||
            part.PartId != sourceStandardSectionDirectProfile.CorePartId)
        {
            return adaptedResult;
        }

        adaptedResult.Result = new DefinitionClauseDecisionBridgeMapperResult
        {
            CandidateDirection = DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate,
            Verdict = DefinitionClauseDecisionBridgeVerdict.Satisfied,
            PromotionReadiness = DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion
        };

        return adaptedResult;
    }

    private static IReadOnlyList<string> BuildReviewConflictReasons(
        LeadClauseSummary leadClauseSummary,
        CoreBodyProofPartResult part)
    {
        var reasons = new List<string>();

        var hasBrokenStructure =
            part.LostClosedLoopStationIds.Count > 0 ||
            part.LostEnvelopeSupportStationIds.Count > 0 ||
            part.LostBodyCoverageStationIds.Count > 0;

        var hasOwnClauseSignal =
            !string.IsNullOrWhiteSpace(part.TopologyRewriteDefinitionClauseCode) &&
            !string.Equals(part.TopologyRewriteDefinitionClauseCode, "NONE", StringComparison.Ordinal);

        var hasCompleteProof =
            !string.IsNullOrWhiteSpace(part.TopologyRewriteProofTypeCode) &&
            !string.Equals(part.TopologyRewriteProofTypeCode, "NONE", StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(part.TopologyRewriteFamilyProofTargetCode) &&
            !string.Equals(part.TopologyRewriteFamilyProofTargetCode, "NONE", StringComparison.Ordinal);
        var hasAnyProofChainSignal =
            !string.IsNullOrWhiteSpace(part.TopologyRewriteProofTypeCode) &&
            !string.Equals(part.TopologyRewriteProofTypeCode, "NONE", StringComparison.Ordinal) ||
            !string.IsNullOrWhiteSpace(part.TopologyRewriteFamilyProofTargetCode) &&
            !string.Equals(part.TopologyRewriteFamilyProofTargetCode, "NONE", StringComparison.Ordinal);

        if (part.ProofClass == CoreBodyProofClass.ReviewRequired)
        {
            reasons.Add(
                hasOwnClauseSignal || hasCompleteProof
                    ? "BOUNDARY_REVIEW"
                    : "SIDE_ROW_PERSISTENCE_REVIEW");
        }

        var hasStableLeadClauseAnchor =
            !string.IsNullOrWhiteSpace(leadClauseSummary.Code) &&
            leadClauseSummary.Share >= 0.999d;

        if (hasBrokenStructure && !hasOwnClauseSignal)
        {
            reasons.Add(
                hasStableLeadClauseAnchor
                    ? "BROKEN_STRUCTURE_CAN_BORROW_LEAD_CLAUSE_ANCHOR"
                    : hasAnyProofChainSignal
                        ? "BROKEN_STRUCTURE_NEEDS_CLAUSE_ANCHOR"
                        : "BROKEN_STRUCTURE_NEEDS_PROOF_CHAIN");
        }

        if (hasBrokenStructure && hasOwnClauseSignal && !hasCompleteProof)
        {
            reasons.Add("BROKEN_STRUCTURE_NEEDS_COMPLETE_PROOF");
        }

        return reasons;
    }

    private static IReadOnlyList<SemanticPartResult> SelectAggregateItems(
        IReadOnlyList<SemanticPartResult> semanticItems)
    {
        if (semanticItems.Count == 0)
        {
            return semanticItems;
        }

        var ownClauseRows = semanticItems
            .Where(item => item.HasOwnClauseSignal)
            .ToArray();

        if (ownClauseRows.Length > 0)
        {
            return ownClauseRows;
        }

        // Assembly-level semantic aggregation should prefer rows that already
        // carry clause-level meaning, otherwise unresolved side parts drown out
        // the lead-clause signal and make stable samples look artificially mixed.
        var clauseBearing = semanticItems
            .Where(
                item =>
                    item.HasOwnClauseSignal ||
                    item.AdaptedResult.Result.Verdict != DefinitionClauseDecisionBridgeVerdict.InsufficientEvidence ||
                    item.AdaptedResult.Result.PromotionReadiness != DefinitionClauseDecisionBridgePromotionReadiness.HoldEffectOnly)
            .ToArray();

        return clauseBearing.Length > 0 ? clauseBearing : semanticItems;
    }

    private static string ToSourceTierLabelZh(DefinitionClauseDecisionBridgeSourceTier tier)
    {
        return tier switch
        {
            DefinitionClauseDecisionBridgeSourceTier.RealPrimary => "real_input 主导",
            DefinitionClauseDecisionBridgeSourceTier.DualConsistent => "双视图一致",
            _ => "仅 recognition_input 证据"
        };
    }

    private static string ToStationTierLabelZh(DefinitionClauseDecisionBridgeStationTier tier)
    {
        return tier switch
        {
            DefinitionClauseDecisionBridgeStationTier.PriorityMajority => "多数优先站位命中",
            DefinitionClauseDecisionBridgeStationTier.PriorityMinor => "少量优先站位命中",
            DefinitionClauseDecisionBridgeStationTier.LocalOnly => "仅局部站位证据",
            _ => "仅偶发站位命中"
        };
    }

    private static string ToTopologyTierLabelZh(DefinitionClauseDecisionBridgeTopologyTier tier)
    {
        return tier switch
        {
            DefinitionClauseDecisionBridgeTopologyTier.LostClosedLoop => "闭环直接丢失",
            DefinitionClauseDecisionBridgeTopologyTier.LostEnvelope => "包络支撑丢失",
            DefinitionClauseDecisionBridgeTopologyTier.MainContourBreak => "主轮廓直接塌缩",
            DefinitionClauseDecisionBridgeTopologyTier.RewriteStructural => "结构级拓扑改写",
            DefinitionClauseDecisionBridgeTopologyTier.DimensionSwitch => "主导维度切换",
            DefinitionClauseDecisionBridgeTopologyTier.WeakChange => "弱拓扑变化",
            _ => "无拓扑信号"
        };
    }

    private static string ToProofCompletenessTierLabelZh(DefinitionClauseDecisionBridgeProofCompletenessTier tier)
    {
        return tier switch
        {
            DefinitionClauseDecisionBridgeProofCompletenessTier.Complete => "证明对象完整",
            DefinitionClauseDecisionBridgeProofCompletenessTier.TypeOnly => "仅证明对象类型",
            DefinitionClauseDecisionBridgeProofCompletenessTier.TargetOnly => "仅家族证明目标",
            _ => "证明对象未收敛"
        };
    }

    private static string ToLeadClauseTierLabelZh(DefinitionClauseDecisionBridgeLeadClauseTier tier)
    {
        return tier switch
        {
            DefinitionClauseDecisionBridgeLeadClauseTier.StableFull => "主条款完全稳定",
            DefinitionClauseDecisionBridgeLeadClauseTier.Dominant => "主条款占优",
            DefinitionClauseDecisionBridgeLeadClauseTier.Weak => "主条款偏弱",
            _ => "无主导条款"
        };
    }

    private static string ToCandidateDirectionLabelZh(DefinitionClauseDecisionBridgeCandidateDirection direction)
    {
        return direction switch
        {
            DefinitionClauseDecisionBridgeCandidateDirection.SatisfiedCandidate => "更像条款满足候选",
            DefinitionClauseDecisionBridgeCandidateDirection.BrokenCandidate => "更像条款破坏候选",
            DefinitionClauseDecisionBridgeCandidateDirection.MixedCandidate => "更像混合候选",
            _ => "候选方向未知"
        };
    }

    private static string ToVerdictLabelZh(DefinitionClauseDecisionBridgeVerdict verdict)
    {
        return verdict switch
        {
            DefinitionClauseDecisionBridgeVerdict.Satisfied => "条款满足",
            DefinitionClauseDecisionBridgeVerdict.Broken => "条款破坏",
            DefinitionClauseDecisionBridgeVerdict.Mixed => "条款混合",
            DefinitionClauseDecisionBridgeVerdict.ReviewRequired => "仍需复核",
            _ => "证据不足"
        };
    }

    private static string ToPromotionReadinessLabelZh(DefinitionClauseDecisionBridgePromotionReadiness readiness)
    {
        return readiness switch
        {
            DefinitionClauseDecisionBridgePromotionReadiness.ReadyForPromotion => "可进入阶段 6 提升",
            DefinitionClauseDecisionBridgePromotionReadiness.ReadyForReview => "可进入定义复核",
            _ => "仅保留 effect 提示"
        };
    }

    private static bool IsShadowPrimaryRowAgainstStableBoxLead(
        LeadClauseSummary leadClauseSummary,
        CoreBodyProofPartResult part)
    {
        return string.Equals(
                   leadClauseSummary.Code,
                   "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE",
                   StringComparison.Ordinal) &&
               leadClauseSummary.Share >= 0.999d &&
               IsSinglePrimaryShadowClauseRow(part);
    }

    private static bool IsBoxPairClauseRow(CoreBodyProofPartResult part)
    {
        return string.Equals(
                   part.TopologyRewriteDefinitionClauseCode,
                   "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE",
                   StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteProofTypeCode,
                   "PAIRED_WALL_MAIN_CONTOUR_PROOF",
                   StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteFamilyProofTargetCode,
                   "BOX_WALL_PAIR_PROOF_TARGET",
                   StringComparison.Ordinal);
    }

    private static bool IsSinglePrimaryShadowClauseRow(CoreBodyProofPartResult part)
    {
        return string.Equals(
                   part.TopologyRewriteDefinitionClauseCode,
                   "PRIMARY_PLATE_CONTINUITY_CLAUSE",
                   StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteProofTypeCode,
                   "SINGLE_PRIMARY_PLATE_PROOF",
                   StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteFamilyProofTargetCode,
                   "SINGLE_PRIMARY_PLATE_SYSTEM_TARGET",
                   StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteControllerRoleCode,
                   "DIRECT_ENVELOPE_CONTROLLER",
                   StringComparison.Ordinal);
    }

    private static bool IsStableFoldedHClauseRow(
        LeadClauseSummary leadClauseSummary,
        CoreBodyProofPartResult part)
    {
        return string.Equals(
                   leadClauseSummary.Code,
                   "H_WEB_FLANGE_CONTINUITY_CLAUSE",
                   StringComparison.Ordinal) &&
               leadClauseSummary.Share >= 0.999d &&
               string.Equals(leadClauseSummary.Mix, "SINGLE_CLAUSE", StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteDefinitionClauseCode,
                   "H_WEB_FLANGE_CONTINUITY_CLAUSE",
                   StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteShapeRoleCode,
                   "H_BENT_FLANGE_SECTION_REWRITE",
                   StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteProofTypeCode,
                   "H_MAIN_CONTOUR_PROOF",
                   StringComparison.Ordinal) &&
               string.Equals(
                   part.TopologyRewriteFamilyProofTargetCode,
                   "H_WEB_FLANGE_PROOF_TARGET",
                   StringComparison.Ordinal);
    }

    private static SourceStandardSectionDirectProfile? ResolveSourceStandardSectionDirectProfile(
        BodyMaterialSummary summary,
        CoreBodyProofViewOutput? proof,
        (string BodyFamily, string SectionType, bool PriorityApplied) sourceSemantic)
    {
        if (proof is null ||
            !sourceSemantic.PriorityApplied ||
            !string.Equals(sourceSemantic.BodyFamily, "StandardSection", StringComparison.Ordinal))
        {
            return null;
        }

        var parts = proof.Result.Parts ?? Array.Empty<CoreBodyProofPartResult>();

        var targetPartId = ResolveSourceStandardSectionDirectTargetPartId(summary, proof, sourceSemantic, parts);
        if (targetPartId is null)
        {
            return null;
        }

        return new SourceStandardSectionDirectProfile(
            ClauseCode: "STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE",
            ClauseLabelZh: "标准截面：源主截面同一性条款",
            CorePartId: targetPartId.Value);
    }

    private static int? ResolveSourceStandardSectionDirectTargetPartId(
        BodyMaterialSummary summary,
        CoreBodyProofViewOutput proof,
        (string BodyFamily, string SectionType, bool PriorityApplied) sourceSemantic,
        IReadOnlyList<CoreBodyProofPartResult> parts)
    {
        if (parts.Count == 0)
        {
            if (string.Equals(sourceSemantic.SectionType, "STANDARD_ROD", StringComparison.Ordinal) &&
                IsBuiltUpBoxDescriptor(summary))
            {
                return null;
            }

            return summary.SourceMainPartId;
        }

        var sourceMainPartId = summary.SourceMainPartId;
        var inputMainPartId = summary.InputMainPartId;

        var directTarget = parts.FirstOrDefault(
            item => item.PartId == sourceMainPartId || item.PartId == inputMainPartId);
        if (directTarget is not null)
        {
            if (string.Equals(sourceSemantic.SectionType, "STANDARD_ROD", StringComparison.Ordinal) &&
                IsBuiltUpBoxDescriptor(summary))
            {
                return null;
            }

            return directTarget.PartId;
        }

        if (string.Equals(sourceSemantic.SectionType, "STANDARD_ROD", StringComparison.Ordinal))
        {
            var rodTarget = parts.FirstOrDefault(
                item => item.PartId == sourceMainPartId || item.PartId == inputMainPartId);
            if (rodTarget is not null &&
                !IsBuiltUpBoxDescriptor(summary))
            {
                return rodTarget.PartId;
            }
        }

        if (proof.Result.CoreBodyPartIds.Count == 1)
        {
            var singleCorePartId = proof.Result.CoreBodyPartIds[0];
            if (singleCorePartId == sourceMainPartId || singleCorePartId == inputMainPartId)
            {
                return singleCorePartId;
            }

            return null;
        }

        if (string.Equals(sourceSemantic.SectionType, "STANDARD_ROD", StringComparison.Ordinal) &&
            IsBuiltUpBoxDescriptor(summary))
        {
            return null;
        }

        return sourceMainPartId > 0
            ? sourceMainPartId
            : inputMainPartId > 0
                ? inputMainPartId
                : null;
    }
    private sealed record SemanticPartResult(
        bool HasOwnClauseSignal,
        DefinitionClauseDecisionBridgeEffectSnapshot Snapshot,
        DefinitionClauseDecisionBridgeAdaptedResult AdaptedResult,
        CoreBodyProofPartResult Part);

    private static bool IsBuiltUpBoxDescriptor(BodyMaterialSummary summary)
    {
        return string.Equals(summary.BodyDescriptorFamily, "BuiltUpBox", StringComparison.Ordinal) ||
               string.Equals(summary.BodyDescriptorSectionType, "BUILTUP_BOX", StringComparison.Ordinal) ||
               string.Equals(summary.BodyDescriptorSectionType, "BUILTUP_BOX_VARIANT", StringComparison.Ordinal);
    }
}
