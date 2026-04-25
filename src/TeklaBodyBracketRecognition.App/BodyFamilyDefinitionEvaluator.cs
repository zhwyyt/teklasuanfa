namespace TeklaBodyBracketRecognition.App;

internal static class BodyFamilyDefinitionEvaluator
{
    public static List<BodyFamilyProofRow> Evaluate(
        IReadOnlyList<DefinitionClauseDecisionFullRunAssemblySource> assemblies)
    {
        return assemblies.Select(EvaluateSingle).ToList();
    }

    private static BodyFamilyProofRow EvaluateSingle(DefinitionClauseDecisionFullRunAssemblySource assembly)
    {
        var readinessReady = string.Equals(
            assembly.LeadClausePromotionReadinessCode,
            "ReadyForPromotion",
            StringComparison.Ordinal);
        var decisiveVerdict =
            string.Equals(assembly.LeadClauseVerdictCode, "Satisfied", StringComparison.Ordinal) ||
            string.Equals(assembly.LeadClauseVerdictCode, "Broken", StringComparison.Ordinal);

        if (IsStandardSectionReady(assembly, readinessReady))
        {
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "STANDARD_SECTION",
                familyLabelZh: "标准截面型材主体",
                subtypeCode: NormalizeCode(assembly.SourceSemanticSectionType),
                subtypeLabelZh: ToStandardSectionSubtypeLabelZh(assembly.SourceSemanticSectionType),
                reasonCode: "SOURCE_STANDARD_SECTION_READY",
                reasonLabelZh: "source semantic 已明确标准截面，可直接进入标准型材家族",
                satisfiedConditions:
                [
                    "SourceSemanticBodyFamily = StandardSection",
                    $"SourceSemanticSectionType = {NormalizeCode(assembly.SourceSemanticSectionType)}",
                    "LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE",
                    "LeadClauseVerdict = Satisfied",
                    "LeadClausePromotionReadiness = ReadyForPromotion"
                ]);
        }

        if (IsBoxReady(assembly, readinessReady, decisiveVerdict))
        {
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "BOX",
                familyLabelZh: "闭合箱形主体",
                subtypeCode: "CLOSED_LOOP_BOX",
                subtypeLabelZh: "闭合箱形截面",
                reasonCode: "BOX_CLAUSE_READY",
                reasonLabelZh: "箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族",
                satisfiedConditions:
                [
                    "LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE",
                    $"LeadClauseVerdict = {NormalizeCode(assembly.LeadClauseVerdictCode)}",
                    "LeadClausePromotionReadiness = ReadyForPromotion"
                ]);
        }

        if (IsVariableSectionBoxReviewReady(assembly, readinessReady))
        {
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "BOX",
                familyLabelZh: "闭合箱形主体",
                subtypeCode: "VARIABLE_SECTION_BOX",
                subtypeLabelZh: "变截面闭合箱壳",
                reasonCode: "BOX_VARIABLE_SECTION_REASSIGNMENT_READY",
                 reasonLabelZh: "包络控制重分配已收敛到闭合箱壳变截面小类，可进入 BOX 家族",
                 satisfiedConditions:
                 [
                     "LeadClause = CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE",
                     "LeadClauseVerdict = Broken",
                     "LeadClausePromotionReadiness = ReadyForPromotion",
                     $"SourceMemberMainClass = {NormalizeCode(assembly.SourceMemberMainClassCode)}",
                     $"ImportSynthesisKind = {NormalizeCode(assembly.ImportSynthesisKind)}"
                 ]);
        }

        if (ShouldPromoteSemanticBoxFromBodyPlate(assembly, readinessReady))
        {
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "BOX",
                familyLabelZh: "闭合箱形主体",
                subtypeCode: "CLOSED_LOOP_BOX",
                subtypeLabelZh: "闭合箱形截面",
                reasonCode: "BODY_PLATE_OVERRIDDEN_BY_BOX_SEMANTIC",
                reasonLabelZh: "上游稳定语义已明确为 BOX 主类，主体板持续条款不再阻断 BOX 家族归属",
                satisfiedConditions:
                [
                    "LeadClause = BODY_PLATE_CONTINUITY_CLAUSE",
                    $"LeadClauseVerdict = {NormalizeCode(assembly.LeadClauseVerdictCode)}",
                    "LeadClausePromotionReadiness = ReadyForPromotion",
                    $"LeadClauseEffectDirection = {NormalizeCode(assembly.LeadClauseEffectDirectionCode)}",
                    $"Break/Rewrite = B{assembly.LeadClauseBreakEffectCount}/R{assembly.LeadClauseRewriteEffectCount}",
                    $"SourceMemberMainClass = {NormalizeCode(assembly.SourceMemberMainClassCode)}",
                    $"ImportSynthesisKind = {NormalizeCode(assembly.ImportSynthesisKind)}"
                ]);
        }

        if (ShouldPromoteSemanticBoxWithoutClause(assembly))
        {
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "BOX",
                familyLabelZh: "闭合箱形主体",
                subtypeCode: "CLOSED_LOOP_BOX",
                subtypeLabelZh: "闭合箱形截面",
                reasonCode: "BOX_DESCRIPTOR_SEMANTIC_CONSENSUS_READY",
                reasonLabelZh: "built-up box 描述与上游 BOX 语义一致，即使 proof 暂未产出主条款，也可进入 BOX 家族",
                satisfiedConditions:
                [
                    "LeadClause = NONE (NO_CLAUSE_ROWS)",
                    $"BodyDescriptorFamily = {NormalizeCode(assembly.BodyDescriptorFamily)}",
                    $"BodyDescriptorSectionType = {NormalizeCode(assembly.BodyDescriptorSectionType)}",
                    $"SourceMemberMainClass = {NormalizeCode(assembly.SourceMemberMainClassCode)}",
                    $"ImportSynthesisKind = {NormalizeCode(assembly.ImportSynthesisKind)}",
                    $"SourceSemanticPriorityApplied = {assembly.SourceSemanticPriorityApplied}",
                    $"HasTopologyRewrite = {assembly.HasTopologyRewrite}"
                ]);
        }

        if (ShouldPromoteGenericHFromPrimaryPlate(assembly, readinessReady, decisiveVerdict))
        {
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "H",
                familyLabelZh: "H型主体",
                subtypeCode: "GENERAL_BUILTUP_H",
                subtypeLabelZh: "一般 built-up H",
                reasonCode: "PRIMARY_PLATE_OVERRIDDEN_BY_H_SEMANTIC",
                reasonLabelZh: "上游稳定语义已明确为 H 主类，单主板条款不再覆盖 H 家族归属",
                 satisfiedConditions:
                 [
                     "LeadClause = PRIMARY_PLATE_CONTINUITY_CLAUSE",
                     $"LeadClauseVerdict = {NormalizeCode(assembly.LeadClauseVerdictCode)}",
                     "LeadClausePromotionReadiness = ReadyForPromotion",
                     $"SourceMemberMainClass = {NormalizeCode(assembly.SourceMemberMainClassCode)}",
                     $"ImportSynthesisKind = {NormalizeCode(assembly.ImportSynthesisKind)}"
                 ]);
        }

        if (ShouldPromoteReviewReadyGenericH(assembly))
        {
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "H",
                familyLabelZh: "H型主体",
                subtypeCode: "GENERAL_BUILTUP_H",
                subtypeLabelZh: "一般 built-up H",
                reasonCode: "H_REVIEW_READY_OVERRIDDEN_BY_STABLE_H_SEMANTIC",
                reasonLabelZh: "H 连续性条款已稳定指向 H 主类，BuiltUpT 启发式不再继续压入人工暂缓",
                 satisfiedConditions:
                 [
                     "LeadClause = H_WEB_FLANGE_CONTINUITY_CLAUSE",
                     "LeadClauseVerdict = Broken",
                     "LeadClausePromotionReadiness = ReadyForReview",
                     $"LeadClauseEffectDirection = {NormalizeCode(assembly.LeadClauseEffectDirectionCode)}",
                     $"Break/Rewrite = B{assembly.LeadClauseBreakEffectCount}/R{assembly.LeadClauseRewriteEffectCount}",
                     $"SourceMemberMainClass = {NormalizeCode(assembly.SourceMemberMainClassCode)}"
                 ]);
        }

        if (IsPrimaryPlateReady(assembly, readinessReady, decisiveVerdict))
        {
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "PRIMARY_PLATE_BODY",
                familyLabelZh: "单主板主体",
                subtypeCode: "MAJORITY_CONTINUITY",
                subtypeLabelZh: "多数站位持续主板",
                reasonCode: "PRIMARY_PLATE_CLAUSE_READY",
                reasonLabelZh: "主板多数站位持续条款已稳定，可进入单主板家族",
                satisfiedConditions:
                [
                    "LeadClause = PRIMARY_PLATE_CONTINUITY_CLAUSE",
                    $"LeadClauseVerdict = {NormalizeCode(assembly.LeadClauseVerdictCode)}",
                    "LeadClausePromotionReadiness = ReadyForPromotion"
                ]);
        }

        if (IsFoldedHReady(assembly, readinessReady))
        {
            var isBrokenFoldedH = string.Equals(assembly.LeadClauseVerdictCode, "Broken", StringComparison.Ordinal);
            return BuildAdjudicatedRow(
                assembly,
                familyCode: "H",
                familyLabelZh: "H型主体",
                subtypeCode: "FOLDED_FLANGE_H",
                subtypeLabelZh: "折型翼缘 H",
                reasonCode: isBrokenFoldedH ? "H_CLAUSE_BROKEN_READY" : "H_CLAUSE_READY",
                reasonLabelZh: isBrokenFoldedH
                    ? "腹板/翼缘连续性条款已稳定破坏，可进入折型翼缘 H 家族"
                    : "腹板/翼缘连续性条款已稳定满足，可进入 H 家族",
                satisfiedConditions:
                [
                    "LeadClause = H_WEB_FLANGE_CONTINUITY_CLAUSE",
                    $"LeadClauseVerdict = {NormalizeCode(assembly.LeadClauseVerdictCode)}",
                    "LeadClausePromotionReadiness = ReadyForPromotion",
                    $"LeadClauseEffectDirection = {NormalizeCode(assembly.LeadClauseEffectDirectionCode)}",
                    $"Break/Rewrite = B{assembly.LeadClauseBreakEffectCount}/R{assembly.LeadClauseRewriteEffectCount}"
                ]);
        }

        return BuildDeferredRow(assembly, readinessReady, decisiveVerdict);
    }

    private static bool IsStandardSectionReady(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        bool readinessReady)
    {
        return readinessReady &&
               string.Equals(assembly.SourceSemanticBodyFamily, "StandardSection", StringComparison.Ordinal) &&
               string.Equals(assembly.LeadClauseCode, "STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE", StringComparison.Ordinal) &&
               string.Equals(assembly.LeadClauseVerdictCode, "Satisfied", StringComparison.Ordinal);
    }

    private static bool IsBoxReady(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        bool readinessReady,
        bool decisiveVerdict)
    {
        return readinessReady &&
               decisiveVerdict &&
               string.Equals(assembly.LeadClauseCode, "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE", StringComparison.Ordinal);
    }

    private static bool IsPrimaryPlateReady(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        bool readinessReady,
        bool decisiveVerdict)
    {
        return readinessReady &&
               decisiveVerdict &&
               string.Equals(assembly.LeadClauseCode, "PRIMARY_PLATE_CONTINUITY_CLAUSE", StringComparison.Ordinal);
    }

    private static bool ShouldPromoteGenericHFromPrimaryPlate(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        bool readinessReady,
        bool decisiveVerdict)
    {
        return IsPrimaryPlateReady(assembly, readinessReady, decisiveVerdict) &&
               (
                   string.Equals(assembly.SourceMemberMainClassCode, "H", StringComparison.Ordinal) ||
                   string.Equals(assembly.ImportSynthesisKind, "H", StringComparison.Ordinal)
                );
    }

    private static bool ShouldPromoteSemanticBoxFromBodyPlate(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        bool readinessReady)
    {
        return readinessReady &&
               string.Equals(assembly.LeadClauseCode, "BODY_PLATE_CONTINUITY_CLAUSE", StringComparison.Ordinal) &&
               string.Equals(assembly.LeadClauseEffectDirectionCode, "SatisfiedCandidate", StringComparison.Ordinal) &&
               assembly.LeadClauseBreakEffectCount == 0 &&
               assembly.LeadClauseRewriteEffectCount >= 2 &&
               (
                   string.Equals(assembly.SourceMemberMainClassCode, "BOX", StringComparison.Ordinal) ||
                   string.Equals(assembly.ImportSynthesisKind, "BOX", StringComparison.Ordinal)
               );
    }

    private static bool ShouldPromoteSemanticBoxWithoutClause(
        DefinitionClauseDecisionFullRunAssemblySource assembly)
    {
        return string.Equals(assembly.ClauseMix, "NO_CLAUSE_ROWS", StringComparison.Ordinal) &&
               string.IsNullOrWhiteSpace(assembly.LeadClauseCode) &&
               string.Equals(assembly.BodyDescriptorFamily, "BuiltUpBox", StringComparison.Ordinal) &&
               string.Equals(assembly.BodyDescriptorSectionType, "BUILTUP_BOX_VARIANT", StringComparison.Ordinal) &&
               string.Equals(assembly.SourceMemberMainClassCode, "BOX", StringComparison.Ordinal) &&
               string.Equals(assembly.ImportSynthesisKind, "BOX", StringComparison.Ordinal) &&
               !assembly.SourceSemanticPriorityApplied &&
               !assembly.HasTopologyRewrite;
    }

    private static bool ShouldPromoteReviewReadyGenericH(
        DefinitionClauseDecisionFullRunAssemblySource assembly)
    {
        return string.Equals(assembly.LeadClauseCode, "H_WEB_FLANGE_CONTINUITY_CLAUSE", StringComparison.Ordinal) &&
               string.Equals(assembly.LeadClauseVerdictCode, "Broken", StringComparison.Ordinal) &&
               string.Equals(assembly.LeadClausePromotionReadinessCode, "ReadyForReview", StringComparison.Ordinal) &&
               string.Equals(assembly.LeadClauseEffectDirectionCode, "BrokenCandidate", StringComparison.Ordinal) &&
               assembly.LeadClauseBreakEffectCount == 2 &&
               assembly.LeadClauseRewriteEffectCount == 1 &&
               string.Equals(assembly.SourceMemberMainClassCode, "H", StringComparison.Ordinal);
    }

    private static bool IsVariableSectionBoxReviewReady(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        bool readinessReady)
    {
        return readinessReady &&
               string.Equals(
                    assembly.LeadClauseCode,
                    "CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE",
                    StringComparison.Ordinal) &&
               string.Equals(assembly.LeadClauseVerdictCode, "Broken", StringComparison.Ordinal) &&
               (
                   string.Equals(assembly.SourceMemberMainClassCode, "BOX", StringComparison.Ordinal) ||
                   string.Equals(assembly.ImportSynthesisKind, "BOX", StringComparison.Ordinal)
               );
    }

    private static bool IsFoldedHReady(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        bool readinessReady)
    {
        return readinessReady &&
               string.Equals(assembly.LeadClauseCode, "H_WEB_FLANGE_CONTINUITY_CLAUSE", StringComparison.Ordinal) &&
               (
                   string.Equals(assembly.LeadClauseVerdictCode, "Satisfied", StringComparison.Ordinal) &&
                   string.Equals(assembly.LeadClauseEffectDirectionCode, "SatisfiedCandidate", StringComparison.Ordinal) &&
                   assembly.LeadClauseBreakEffectCount == 1 &&
                   assembly.LeadClauseRewriteEffectCount == 2 ||
                   string.Equals(assembly.LeadClauseVerdictCode, "Broken", StringComparison.Ordinal) &&
                   string.Equals(assembly.LeadClauseEffectDirectionCode, "BrokenCandidate", StringComparison.Ordinal) &&
                   assembly.LeadClauseBreakEffectCount == 2 &&
                   assembly.LeadClauseRewriteEffectCount == 1
               );
    }

    private static BodyFamilyProofRow BuildAdjudicatedRow(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        string familyCode,
        string familyLabelZh,
        string subtypeCode,
        string subtypeLabelZh,
        string reasonCode,
        string reasonLabelZh,
        IReadOnlyList<string> satisfiedConditions)
    {
        return new BodyFamilyProofRow
        {
            SourceFile = assembly.SourceFile,
            MemberId = assembly.MemberId,
            AssemblyId = assembly.AssemblyId,
            BodyDescriptorFamily = assembly.BodyDescriptorFamily,
            BodyDescriptorSectionType = assembly.BodyDescriptorSectionType,
            SourceSemanticBodyFamily = assembly.SourceSemanticBodyFamily,
            SourceSemanticSectionType = assembly.SourceSemanticSectionType,
            LongitudinalTypeCode = assembly.LongitudinalTypeCode,
            LongitudinalTypeLabelZh = assembly.LongitudinalTypeLabelZh,
            LongitudinalSubtypeCode = assembly.LongitudinalSubtypeCode,
            LongitudinalSubtypeLabelZh = assembly.LongitudinalSubtypeLabelZh,
            LeadClauseCode = assembly.LeadClauseCode,
            LeadClauseLabelZh = assembly.LeadClauseLabelZh,
            LeadClauseVerdictCode = assembly.LeadClauseVerdictCode,
            LeadClauseVerdictLabelZh = assembly.LeadClauseVerdictLabelZh,
            LeadClausePromotionReadinessCode = assembly.LeadClausePromotionReadinessCode,
            LeadClausePromotionReadinessLabelZh = assembly.LeadClausePromotionReadinessLabelZh,
            LeadClauseEffectDirectionCode = assembly.LeadClauseEffectDirectionCode,
            LeadClauseEffectDirectionLabelZh = assembly.LeadClauseEffectDirectionLabelZh,
            LeadClauseBreakEffectCount = assembly.LeadClauseBreakEffectCount,
            LeadClauseRewriteEffectCount = assembly.LeadClauseRewriteEffectCount,
            DefinitionDrivenFamilyCode = familyCode,
            DefinitionDrivenFamilyLabelZh = familyLabelZh,
            DefinitionDrivenSubtypeCode = subtypeCode,
            DefinitionDrivenSubtypeLabelZh = subtypeLabelZh,
            DecisionStatusCode = "Adjudicated",
            DecisionStatusLabelZh = "已进入家族判定",
            DecisionReasonCode = reasonCode,
            DecisionReasonLabelZh = reasonLabelZh,
            SatisfiedConditions = satisfiedConditions,
            MissingConditions = Array.Empty<string>()
        };
    }

    private static BodyFamilyProofRow BuildDeferredRow(
        DefinitionClauseDecisionFullRunAssemblySource assembly,
        bool readinessReady,
        bool decisiveVerdict)
    {
        var satisfiedConditions = new List<string>();
        var missingConditions = new List<string>();
        var reasonCode = "LEAD_CLAUSE_NOT_MAPPED";
        var reasonLabelZh = "当前主条款尚未接入阶段 6 家族映射";

        if (!string.IsNullOrWhiteSpace(assembly.LeadClauseCode))
        {
            satisfiedConditions.Add($"LeadClause = {NormalizeCode(assembly.LeadClauseCode)}");
        }
        else
        {
            missingConditions.Add("LeadClause 非空且可解释");
            reasonCode = "LEAD_CLAUSE_MISSING";
            reasonLabelZh = "当前没有稳定主条款，暂不能进入阶段 6";
        }

        if (readinessReady)
        {
            satisfiedConditions.Add("LeadClausePromotionReadiness = ReadyForPromotion");
        }
        else
        {
            missingConditions.Add("LeadClausePromotionReadiness = ReadyForPromotion");
            reasonCode = "READINESS_NOT_PROMOTABLE";
            reasonLabelZh = "当前仍未达到 ReadyForPromotion";
        }

        if (decisiveVerdict)
        {
            satisfiedConditions.Add($"LeadClauseVerdict = {NormalizeCode(assembly.LeadClauseVerdictCode)}");
        }
        else
        {
            missingConditions.Add("LeadClauseVerdict = Satisfied/Broken");
            if (string.Equals(reasonCode, "LEAD_CLAUSE_NOT_MAPPED", StringComparison.Ordinal))
            {
                reasonCode = "LEAD_VERDICT_NOT_DECISIVE";
                reasonLabelZh = "当前 verdict 仍不够收敛，暂不进入阶段 6";
            }
        }

        if (string.Equals(reasonCode, "LEAD_CLAUSE_NOT_MAPPED", StringComparison.Ordinal))
        {
            missingConditions.Add("当前 LeadClause 的阶段 6 家族映射规则");
        }

        var deferredHint = ResolveDeferredFamilyHint(assembly);

        return new BodyFamilyProofRow
        {
            SourceFile = assembly.SourceFile,
            MemberId = assembly.MemberId,
            AssemblyId = assembly.AssemblyId,
            BodyDescriptorFamily = assembly.BodyDescriptorFamily,
            BodyDescriptorSectionType = assembly.BodyDescriptorSectionType,
            SourceSemanticBodyFamily = assembly.SourceSemanticBodyFamily,
            SourceSemanticSectionType = assembly.SourceSemanticSectionType,
            LongitudinalTypeCode = assembly.LongitudinalTypeCode,
            LongitudinalTypeLabelZh = assembly.LongitudinalTypeLabelZh,
            LongitudinalSubtypeCode = assembly.LongitudinalSubtypeCode,
            LongitudinalSubtypeLabelZh = assembly.LongitudinalSubtypeLabelZh,
            LeadClauseCode = assembly.LeadClauseCode,
            LeadClauseLabelZh = assembly.LeadClauseLabelZh,
            LeadClauseVerdictCode = assembly.LeadClauseVerdictCode,
            LeadClauseVerdictLabelZh = assembly.LeadClauseVerdictLabelZh,
            LeadClausePromotionReadinessCode = assembly.LeadClausePromotionReadinessCode,
            LeadClausePromotionReadinessLabelZh = assembly.LeadClausePromotionReadinessLabelZh,
            LeadClauseEffectDirectionCode = assembly.LeadClauseEffectDirectionCode,
            LeadClauseEffectDirectionLabelZh = assembly.LeadClauseEffectDirectionLabelZh,
            LeadClauseBreakEffectCount = assembly.LeadClauseBreakEffectCount,
            LeadClauseRewriteEffectCount = assembly.LeadClauseRewriteEffectCount,
            DefinitionDrivenFamilyCode = deferredHint.FamilyCode,
            DefinitionDrivenFamilyLabelZh = deferredHint.FamilyLabelZh,
            DefinitionDrivenSubtypeCode = deferredHint.SubtypeCode,
            DefinitionDrivenSubtypeLabelZh = deferredHint.SubtypeLabelZh,
            DecisionReasonCode = reasonCode,
            DecisionReasonLabelZh = reasonLabelZh,
            SatisfiedConditions = satisfiedConditions,
            MissingConditions = missingConditions
        };
    }

    private static (string FamilyCode, string FamilyLabelZh, string SubtypeCode, string SubtypeLabelZh) ResolveDeferredFamilyHint(
        DefinitionClauseDecisionFullRunAssemblySource assembly)
    {
        if (!LooksLikeStableHFamily(assembly))
        {
            return ("NONE", "未进入家族判定", "NONE", "无子类");
        }

        if (string.Equals(assembly.LongitudinalSubtypeCode, "AXIAL_KINKED_STRAIGHT", StringComparison.Ordinal))
        {
            return ("H", "H型主体", "H_MAINLINE_BENT", "主线折弯 H");
        }

        if (string.Equals(assembly.LeadClauseCode, "CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE", StringComparison.Ordinal))
        {
            return ("H", "H型主体", "VARIABLE_SECTION_H", "变截面 H");
        }

        return ("H", "H型主体", "GENERAL_BUILTUP_H", "一般 built-up H");
    }

    private static bool LooksLikeStableHFamily(DefinitionClauseDecisionFullRunAssemblySource assembly)
    {
        return string.Equals(assembly.SourceMemberMainClassCode, "H", StringComparison.Ordinal) ||
               string.Equals(assembly.ImportSynthesisKind, "H", StringComparison.Ordinal);
    }

    private static string NormalizeCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "NONE" : value.Trim();
    }

    private static string ToStandardSectionSubtypeLabelZh(string sectionTypeCode)
    {
        return NormalizeCode(sectionTypeCode) switch
        {
            "STANDARD_ANGLE" => "标准角钢",
            "STANDARD_CHANNEL" => "标准槽钢",
            "STANDARD_IH" => "标准工字/H 型材",
            "STANDARD_TEE" => "标准 T 型材",
            "STANDARD_BOX" => "标准箱形型材",
            "STANDARD_PIPE" => "标准圆管型材",
            "STANDARD_ROD" => "标准圆钢",
            _ => NormalizeCode(sectionTypeCode)
        };
    }
}
