namespace TeklaBodyBracketRecognition.App;

internal static class BodyProfileResolver
{
    public static List<BodyProfileResolutionRow> Resolve(
        IReadOnlyList<BodyMaterialSummary> bodyMaterialSummaries,
        IReadOnlyList<BodyFamilyProofRow> familyProofRows)
    {
        var familyByAssembly = familyProofRows.ToDictionary(item => item.AssemblyId, StringComparer.Ordinal);
        var result = new List<BodyProfileResolutionRow>(bodyMaterialSummaries.Count);

        foreach (var summary in bodyMaterialSummaries)
        {
            if (!familyByAssembly.TryGetValue(summary.AssemblyId, out var familyProof))
            {
                result.Add(BuildReviewBypass(summary, null, "缺少阶段 6 家族判定结果"));
                continue;
            }

            if (!string.Equals(familyProof.DecisionStatusCode, "Adjudicated", StringComparison.Ordinal))
            {
                result.Add(BuildReviewBypass(summary, familyProof, "阶段 6 已明确旁路到人工复核"));
                continue;
            }

            result.Add(ResolveSingle(summary, familyProof));
        }

        return result;
    }

    private static BodyProfileResolutionRow ResolveSingle(
        BodyMaterialSummary summary,
        BodyFamilyProofRow familyProof)
    {
        return familyProof.DefinitionDrivenFamilyCode switch
        {
            "STANDARD_SECTION" => BuildResolved(
                summary,
                familyProof,
                categoryCode: "STANDARD_SECTION",
                categoryLabelZh: "标准型材",
                profileCode: Normalize(familyProof.DefinitionDrivenSubtypeCode),
                profileLabelZh: ToProfileLabelZh(familyProof.DefinitionDrivenSubtypeCode),
                profileSeriesCode: ResolveStandardSectionSeries(summary.SourceMainPartProfileString).Code,
                profileSeriesLabelZh: ResolveStandardSectionSeries(summary.SourceMainPartProfileString).LabelZh,
                dimensionSourceCode: "SOURCE_MAIN_PART_PROFILE",
                dimensionSourceLabelZh: "源主件截面字符串",
                similarityBasisCode: "SOURCE_SEMANTIC_EXACT",
                similarityBasisLabelZh: "source semantic 精确命中",
                similarityScore: 1.0,
                evidenceSummary: $"SourceMainPartProfile={Normalize(summary.SourceMainPartProfileString)}"),
            "BOX" when string.Equals(familyProof.DefinitionDrivenSubtypeCode, "VARIABLE_SECTION_BOX", StringComparison.Ordinal) => BuildResolved(
                summary,
                familyProof,
                categoryCode: "VARIABLE_SECTION_BUILT_UP",
                categoryLabelZh: "变截面 built-up",
                profileCode: "VARIABLE_SECTION_BOX",
                profileLabelZh: "变截面闭合箱壳",
                profileSeriesCode: "VARIABLE_SECTION_BOX_SERIES",
                profileSeriesLabelZh: "变截面箱壳系列",
                dimensionSourceCode: "FAMILY_SUBTYPE_PROOF",
                dimensionSourceLabelZh: "阶段 6 家族子类证明",
                similarityBasisCode: "FAMILY_SUBTYPE_STRICT",
                similarityBasisLabelZh: "阶段 6 子类严格命中",
                similarityScore: 0.95,
                evidenceSummary: $"LeadClause={Normalize(familyProof.LeadClauseCode)}; FamilySubtype={Normalize(familyProof.DefinitionDrivenSubtypeCode)}"),
            "BOX" => BuildResolved(
                summary,
                familyProof,
                categoryCode: "BUILTUP_BOX",
                categoryLabelZh: "规则箱形 built-up",
                profileCode: "BUILTUP_BOX",
                profileLabelZh: "闭合箱形 built-up",
                profileSeriesCode: "BOX_CLOSED_LOOP_SERIES",
                profileSeriesLabelZh: "闭合箱形系列",
                dimensionSourceCode: "FAMILY_SUBTYPE_PROOF",
                dimensionSourceLabelZh: "阶段 6 家族子类证明",
                similarityBasisCode: "FAMILY_SUBTYPE_STRICT",
                similarityBasisLabelZh: "阶段 6 子类严格命中",
                similarityScore: 0.93,
                evidenceSummary: $"LeadClause={Normalize(familyProof.LeadClauseCode)}; FamilySubtype={Normalize(familyProof.DefinitionDrivenSubtypeCode)}"),
            "PRIMARY_PLATE_BODY" => BuildResolved(
                summary,
                familyProof,
                categoryCode: "BUILTUP_PRIMARY_PLATE",
                categoryLabelZh: "单主板 built-up",
                profileCode: "PRIMARY_PLATE_BODY",
                profileLabelZh: "单主板 built-up",
                profileSeriesCode: "PRIMARY_PLATE_SERIES",
                profileSeriesLabelZh: "单主板系列",
                dimensionSourceCode: "FAMILY_PROOF_CHAIN",
                dimensionSourceLabelZh: "阶段 5/6 主板证明链",
                similarityBasisCode: "CLAUSE_AND_FAMILY_MATCH",
                similarityBasisLabelZh: "条款与家族一致",
                similarityScore: 0.9,
                evidenceSummary: $"LeadClause={Normalize(familyProof.LeadClauseCode)}; Verdict={Normalize(familyProof.LeadClauseVerdictCode)}"),
            "H" => BuildResolved(
                summary,
                familyProof,
                categoryCode: "BUILTUP_H",
                categoryLabelZh: "H 型 built-up",
                profileCode: Normalize(familyProof.DefinitionDrivenSubtypeCode),
                profileLabelZh: Coalesce(familyProof.DefinitionDrivenSubtypeLabelZh, familyProof.DefinitionDrivenSubtypeCode),
                profileSeriesCode: string.Equals(familyProof.DefinitionDrivenSubtypeCode, "FOLDED_FLANGE_H", StringComparison.Ordinal)
                    ? "H_FOLDED_FLANGE_SERIES"
                    : "H_BUILTUP_SERIES",
                profileSeriesLabelZh: string.Equals(familyProof.DefinitionDrivenSubtypeCode, "FOLDED_FLANGE_H", StringComparison.Ordinal)
                    ? "折型翼缘 H 系列"
                    : "一般 H built-up 系列",
                dimensionSourceCode: "FAMILY_SUBTYPE_PROOF",
                dimensionSourceLabelZh: "阶段 6 家族子类证明",
                similarityBasisCode: string.Equals(familyProof.DefinitionDrivenSubtypeCode, "FOLDED_FLANGE_H", StringComparison.Ordinal)
                    ? "CLAUSE_AND_EFFECT_MATCH"
                    : "CLAUSE_AND_UPSTREAM_H_SEMANTIC",
                similarityBasisLabelZh: string.Equals(familyProof.DefinitionDrivenSubtypeCode, "FOLDED_FLANGE_H", StringComparison.Ordinal)
                    ? "条款与 effect 聚合一致"
                    : "条款与上游 H 语义一致",
                similarityScore: 0.9,
                evidenceSummary: $"LeadClause={Normalize(familyProof.LeadClauseCode)}; FamilySubtype={Normalize(familyProof.DefinitionDrivenSubtypeCode)}; Break/Rewrite={familyProof.LeadClauseBreakEffectCount}/{familyProof.LeadClauseRewriteEffectCount}"),
            _ => BuildReviewBypass(summary, familyProof, "阶段 7 尚未接入该家族的自动细分规则")
        };
    }

    private static BodyProfileResolutionRow BuildResolved(
        BodyMaterialSummary summary,
        BodyFamilyProofRow familyProof,
        string categoryCode,
        string categoryLabelZh,
        string profileCode,
        string profileLabelZh,
        string profileSeriesCode,
        string profileSeriesLabelZh,
        string dimensionSourceCode,
        string dimensionSourceLabelZh,
        string similarityBasisCode,
        string similarityBasisLabelZh,
        double similarityScore,
        string evidenceSummary)
    {
        return new BodyProfileResolutionRow
        {
            SourceFile = summary.SourceFile,
            MemberId = summary.MemberId,
            AssemblyId = summary.AssemblyId,
            FamilyCode = familyProof.DefinitionDrivenFamilyCode,
            FamilyLabelZh = familyProof.DefinitionDrivenFamilyLabelZh,
            FamilySubtypeCode = familyProof.DefinitionDrivenSubtypeCode,
            FamilySubtypeLabelZh = familyProof.DefinitionDrivenSubtypeLabelZh,
            LongitudinalTypeCode = familyProof.LongitudinalTypeCode,
            LongitudinalTypeLabelZh = familyProof.LongitudinalTypeLabelZh,
            LongitudinalSubtypeCode = familyProof.LongitudinalSubtypeCode,
            LongitudinalSubtypeLabelZh = familyProof.LongitudinalSubtypeLabelZh,
            ResolutionStatusCode = "Resolved",
            ResolutionStatusLabelZh = "已完成自动细分",
            ProfileCategoryCode = categoryCode,
            ProfileCategoryLabelZh = categoryLabelZh,
            ProfileCode = profileCode,
            ProfileLabelZh = profileLabelZh,
            ProfileSeriesCode = profileSeriesCode,
            ProfileSeriesLabelZh = profileSeriesLabelZh,
            DimensionSourceCode = dimensionSourceCode,
            DimensionSourceLabelZh = dimensionSourceLabelZh,
            SimilarityBasisCode = similarityBasisCode,
            SimilarityBasisLabelZh = similarityBasisLabelZh,
            SimilarityScore = similarityScore,
            EvidenceSummary = evidenceSummary,
            SatisfiedConditions =
            [
                $"Family = {Normalize(familyProof.DefinitionDrivenFamilyCode)}",
                $"LongitudinalType = {Normalize(familyProof.LongitudinalTypeCode)}",
                $"LongitudinalSubtype = {Normalize(familyProof.LongitudinalSubtypeCode)}",
                $"FamilySubtype = {Normalize(familyProof.DefinitionDrivenSubtypeCode)}"
            ],
            MissingConditions = Array.Empty<string>()
        };
    }

    private static BodyProfileResolutionRow BuildReviewBypass(
        BodyMaterialSummary summary,
        BodyFamilyProofRow? familyProof,
        string reason)
    {
        return new BodyProfileResolutionRow
        {
            SourceFile = summary.SourceFile,
            MemberId = summary.MemberId,
            AssemblyId = summary.AssemblyId,
            FamilyCode = familyProof?.DefinitionDrivenFamilyCode ?? "NONE",
            FamilyLabelZh = familyProof?.DefinitionDrivenFamilyLabelZh ?? "未进入家族判定",
            FamilySubtypeCode = familyProof?.DefinitionDrivenSubtypeCode ?? "NONE",
            FamilySubtypeLabelZh = familyProof?.DefinitionDrivenSubtypeLabelZh ?? "无子类",
            LongitudinalTypeCode = familyProof?.LongitudinalTypeCode ?? summary.LongitudinalTypeCode,
            LongitudinalTypeLabelZh = familyProof?.LongitudinalTypeLabelZh ?? summary.LongitudinalTypeLabelZh,
            LongitudinalSubtypeCode = familyProof?.LongitudinalSubtypeCode ?? summary.LongitudinalSubtypeCode,
            LongitudinalSubtypeLabelZh = familyProof?.LongitudinalSubtypeLabelZh ?? summary.LongitudinalSubtypeLabelZh,
            ResolutionStatusCode = "ReviewBypass",
            ResolutionStatusLabelZh = "人工复核旁路",
            ProfileCategoryCode = "MANUAL_REVIEW",
            ProfileCategoryLabelZh = "人工复核",
            ProfileCode = "MANUAL_REVIEW",
            ProfileLabelZh = "人工复核",
            ProfileSeriesCode = "MANUAL_REVIEW",
            ProfileSeriesLabelZh = "人工复核",
            DimensionSourceCode = "MANUAL_REVIEW",
            DimensionSourceLabelZh = "人工复核",
            SimilarityBasisCode = "MANUAL_REVIEW",
            SimilarityBasisLabelZh = "人工复核旁路",
            SimilarityScore = 0,
            EvidenceSummary = reason,
            SatisfiedConditions = familyProof is null
                ? Array.Empty<string>()
                :
                [
                    $"FamilyDecisionStatus = {Normalize(familyProof.DecisionStatusCode)}",
                    $"DecisionReason = {Normalize(familyProof.DecisionReasonCode)}"
                ],
            MissingConditions =
            [
                "阶段 7 自动细分规则"
            ]
        };
    }

    private static string ToProfileLabelZh(string profileCode)
    {
        return Normalize(profileCode) switch
        {
            "STANDARD_ANGLE" => "标准角钢",
            "STANDARD_CHANNEL" => "标准槽钢",
            "STANDARD_IH" => "标准工字/H 型材",
            "STANDARD_TEE" => "标准 T 型材",
            "STANDARD_BOX" => "标准箱型材",
            "STANDARD_PIPE" => "标准圆管",
            "STANDARD_ROD" => "标准圆钢",
            _ => Normalize(profileCode)
        };
    }

    private static (string Code, string LabelZh) ResolveStandardSectionSeries(string sourceMainPartProfileString)
    {
        var profile = NormalizeProfile(sourceMainPartProfileString);

        if (profile.StartsWith("BH", StringComparison.OrdinalIgnoreCase))
        {
            return ("BH_SERIES", "BH 焊接 H/I 系列");
        }

        if (profile.StartsWith("H", StringComparison.OrdinalIgnoreCase))
        {
            return ("H_SERIES", "热轧 H 系列");
        }

        if (profile.StartsWith("I", StringComparison.OrdinalIgnoreCase))
        {
            return ("I_SERIES", "工字钢系列");
        }

        if (profile.StartsWith("L", StringComparison.OrdinalIgnoreCase))
        {
            return ("L_SERIES", "角钢系列");
        }

        if (profile.StartsWith("C", StringComparison.OrdinalIgnoreCase) ||
            profile.StartsWith("U", StringComparison.OrdinalIgnoreCase) ||
            profile.StartsWith("[", StringComparison.OrdinalIgnoreCase))
        {
            return ("CHANNEL_SERIES", "槽钢系列");
        }

        if (profile.StartsWith("BOX", StringComparison.OrdinalIgnoreCase) ||
            profile.StartsWith("BK", StringComparison.OrdinalIgnoreCase) ||
            profile.StartsWith("RHS", StringComparison.OrdinalIgnoreCase) ||
            profile.StartsWith("SHS", StringComparison.OrdinalIgnoreCase))
        {
            return ("BOX_SERIES", "箱型材系列");
        }

        if (profile.StartsWith("PIPE", StringComparison.OrdinalIgnoreCase) ||
            profile.StartsWith("CHS", StringComparison.OrdinalIgnoreCase))
        {
            return ("PIPE_SERIES", "圆管系列");
        }

        if (profile.StartsWith("ROD", StringComparison.OrdinalIgnoreCase))
        {
            return ("ROD_SERIES", "圆钢系列");
        }

        if (profile.StartsWith("D", StringComparison.OrdinalIgnoreCase) &&
            profile.Length > 1 &&
            char.IsDigit(profile[1]))
        {
            return ("D_SERIES", "直径圆钢系列");
        }

        if (profile.StartsWith("T", StringComparison.OrdinalIgnoreCase))
        {
            return ("T_SERIES", "T 型钢系列");
        }

        return ("UNRESOLVED_SERIES", "未细分系列");
    }

    private static string NormalizeProfile(string profileString)
    {
        return string.IsNullOrWhiteSpace(profileString)
            ? string.Empty
            : profileString.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "NONE" : value.Trim();
    }

    private static string Coalesce(string? preferred, string? fallback)
    {
        return string.IsNullOrWhiteSpace(preferred) ? Normalize(fallback) : preferred.Trim();
    }
}
