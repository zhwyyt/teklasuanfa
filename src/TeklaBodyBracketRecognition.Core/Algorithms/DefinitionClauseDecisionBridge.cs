using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionBridgeInput
{
    public string DefinitionClauseCode { get; init; } = "NONE";
    public string DefinitionClauseEffectCode { get; init; } = "NONE";
    public double PriorityBodyCoverageAfterRemovalRatio { get; init; }
    public double EnvelopeBodyCoverageAfterRemovalRatio { get; init; }
    public IReadOnlyCollection<int>? LostClosedLoopStationIds { get; init; }
    public IReadOnlyCollection<int>? LostBodyCoverageStationIds { get; init; }
    public IReadOnlyCollection<int>? LostEnvelopeSupportStationIds { get; init; }
}

public sealed class DefinitionClauseDecisionBridgeResult
{
    public string ClauseVerdictCode { get; init; } = "NONE";
    public string ClauseVerdictLabelZh { get; init; } = "无条款判定";
    public string ClausePromotionReadinessCode { get; init; } = "NONE";
    public string ClausePromotionReadinessLabelZh { get; init; } = "无提升准备度";
}

public static class DefinitionClauseDecisionBridge
{
    public static DefinitionClauseDecisionBridgeResult Evaluate(DefinitionClauseDecisionBridgeInput input)
    {
        if (input is null)
        {
            return None();
        }

        return input.DefinitionClauseCode switch
        {
            "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE" => EvaluateBoxClosedLoop(input),
            "PRIMARY_PLATE_CONTINUITY_CLAUSE" => EvaluatePrimaryPlateContinuity(input),
            "BODY_PLATE_CONTINUITY_CLAUSE" => EvaluateBodyPlateContinuity(input),
            "H_WEB_FLANGE_CONTINUITY_CLAUSE" => EvaluateHWebFlangeContinuity(input),
            "CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE" => EvaluateClusterDirectControl(input),
            "CLUSTER_ACCESSORY_EXCLUSION_CLAUSE" => EvaluateClusterAccessoryExclusion(input),
            "CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE" => ReviewRequired(
                "CLAUSE_REVIEW_REQUIRED",
                "定义条款仍需复核",
                "NOT_READY_NEEDS_REVIEW",
                "仍需复核后才能进入定义验证"),
            "GENERAL_DEFINITION_REVIEW_CLAUSE" => ReviewRequired(
                "CLAUSE_REVIEW_REQUIRED",
                "定义条款仍需复核",
                "NOT_READY_NEEDS_REVIEW",
                "仍需复核后才能进入定义验证"),
            _ => None()
        };
    }

    private static DefinitionClauseDecisionBridgeResult EvaluateBoxClosedLoop(DefinitionClauseDecisionBridgeInput input)
    {
        if (HasAny(input.LostClosedLoopStationIds))
        {
            return Ready(
                "CLAUSE_BROKEN_DIRECT",
                "定义条款被直接破坏",
                "READY_FOR_DEFINITION_CHECK",
                "已具备进入定义验证的破坏证据");
        }

        if (input.DefinitionClauseEffectCode == "BOX_OPPOSITE_WALL_STABILITY_REWRITE_EFFECT")
        {
            return Ready(
                "CLAUSE_SATISFIED_WITH_REWRITE",
                "定义条款仍成立，但控制路径被改写",
                "READY_WITH_REWRITE_NOTE",
                "可进入定义验证，但需附带改写说明");
        }

        if (input.DefinitionClauseEffectCode == "BOX_CLOSED_LOOP_DIRECT_BREAK_EFFECT")
        {
            return Ready(
                "CLAUSE_BROKEN_DIRECT",
                "定义条款被直接破坏",
                "READY_FOR_DEFINITION_CHECK",
                "已具备进入定义验证的破坏证据");
        }

        return ReviewRequired(
            "CLAUSE_REVIEW_REQUIRED",
            "定义条款仍需复核",
            "NOT_READY_NEEDS_REVIEW",
            "当前证据不足以升级为定义验证");
    }

    private static DefinitionClauseDecisionBridgeResult EvaluatePrimaryPlateContinuity(DefinitionClauseDecisionBridgeInput input)
    {
        if (HasAny(input.LostBodyCoverageStationIds) || input.PriorityBodyCoverageAfterRemovalRatio < 0.999)
        {
            return Ready(
                "CLAUSE_BROKEN_MAJORITY",
                "定义条款在多数站位被破坏",
                "READY_FOR_DEFINITION_CHECK",
                "已具备多数站位持续性破坏证据");
        }

        if (input.DefinitionClauseEffectCode == "PRIMARY_PLATE_CONTINUITY_REWRITE_EFFECT")
        {
            return Ready(
                "CLAUSE_SATISFIED_WITH_REWRITE",
                "定义条款仍成立，但多数站位路径被改写",
                "READY_WITH_REWRITE_NOTE",
                "可进入定义验证，但需附带改写说明");
        }

        return ReviewRequired(
            "CLAUSE_REVIEW_REQUIRED",
            "定义条款仍需复核",
            "NOT_READY_NEEDS_REVIEW",
            "当前证据不足以升级为定义验证");
    }

    private static DefinitionClauseDecisionBridgeResult EvaluateBodyPlateContinuity(DefinitionClauseDecisionBridgeInput input)
    {
        if (HasAny(input.LostBodyCoverageStationIds) || input.PriorityBodyCoverageAfterRemovalRatio < 0.999)
        {
            return Ready(
                "CLAUSE_BROKEN_MAJORITY",
                "定义条款在多数站位被破坏",
                "READY_FOR_DEFINITION_CHECK",
                "已具备多数站位持续性破坏证据");
        }

        if (input.DefinitionClauseEffectCode == "BODY_PLATE_CONTINUITY_REWRITE_EFFECT")
        {
            return Ready(
                "CLAUSE_SATISFIED_WITH_REWRITE",
                "定义条款仍成立，但多数站位路径被改写",
                "READY_WITH_REWRITE_NOTE",
                "可进入定义验证，但需附带改写说明");
        }

        return ReviewRequired(
            "CLAUSE_REVIEW_REQUIRED",
            "定义条款仍需复核",
            "NOT_READY_NEEDS_REVIEW",
            "当前证据不足以升级为定义验证");
    }

    private static DefinitionClauseDecisionBridgeResult EvaluateHWebFlangeContinuity(DefinitionClauseDecisionBridgeInput input)
    {
        if (input.DefinitionClauseEffectCode == "H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT" ||
            HasAny(input.LostBodyCoverageStationIds) ||
            input.PriorityBodyCoverageAfterRemovalRatio < 0.999)
        {
            return Ready(
                "CLAUSE_BROKEN_MAJORITY",
                "定义条款在多数站位被破坏",
                "READY_FOR_DEFINITION_CHECK",
                "已具备进入定义验证的 H 腹板/翼缘连续性破坏证据");
        }

        if (input.DefinitionClauseEffectCode == "H_WEB_FLANGE_CONTINUITY_REWRITE_EFFECT")
        {
            return Ready(
                "CLAUSE_SATISFIED_WITH_REWRITE",
                "定义条款仍成立，但腹板/翼缘连续性路径被改写",
                "READY_WITH_REWRITE_NOTE",
                "可进入定义验证，但需附带 H 腹板/翼缘改写说明");
        }

        return ReviewRequired(
            "CLAUSE_REVIEW_REQUIRED",
            "定义条款仍需复核",
            "NOT_READY_NEEDS_REVIEW",
            "当前证据不足以升级为定义验证");
    }

    private static DefinitionClauseDecisionBridgeResult EvaluateClusterDirectControl(DefinitionClauseDecisionBridgeInput input)
    {
        if (input.DefinitionClauseEffectCode == "CLUSTER_DIRECT_CONTROLLER_NOT_EXCLUDABLE_EFFECT")
        {
            return Ready(
                "CLAUSE_BROKEN_DIRECT",
                "定义条款被直接破坏",
                "READY_FOR_DEFINITION_CHECK",
                "已具备直接控制件不可排除证据");
        }

        return ReviewRequired(
            "CLAUSE_REVIEW_REQUIRED",
            "定义条款仍需复核",
            "NOT_READY_NEEDS_REVIEW",
            "当前证据不足以升级为定义验证");
    }

    private static DefinitionClauseDecisionBridgeResult EvaluateClusterAccessoryExclusion(DefinitionClauseDecisionBridgeInput input)
    {
        if (input.DefinitionClauseEffectCode == "CLUSTER_ACCESSORY_EXCLUSION_SUPPORTED_EFFECT" &&
            !HasAny(input.LostClosedLoopStationIds) &&
            !HasAny(input.LostEnvelopeSupportStationIds) &&
            input.EnvelopeBodyCoverageAfterRemovalRatio >= 0.999)
        {
            return Ready(
                "CLAUSE_SATISFIED_STABLE",
                "定义条款稳定成立",
                "READY_FOR_DEFINITION_CHECK",
                "已具备进入定义验证的稳定证据");
        }

        return ReviewRequired(
            "CLAUSE_REVIEW_REQUIRED",
            "定义条款仍需复核",
            "NOT_READY_NEEDS_REVIEW",
            "当前证据不足以升级为定义验证");
    }

    private static DefinitionClauseDecisionBridgeResult Ready(
        string verdictCode,
        string verdictLabelZh,
        string readinessCode,
        string readinessLabelZh)
    {
        return new DefinitionClauseDecisionBridgeResult
        {
            ClauseVerdictCode = verdictCode,
            ClauseVerdictLabelZh = verdictLabelZh,
            ClausePromotionReadinessCode = readinessCode,
            ClausePromotionReadinessLabelZh = readinessLabelZh
        };
    }

    private static DefinitionClauseDecisionBridgeResult ReviewRequired(
        string verdictCode,
        string verdictLabelZh,
        string readinessCode,
        string readinessLabelZh)
    {
        return new DefinitionClauseDecisionBridgeResult
        {
            ClauseVerdictCode = verdictCode,
            ClauseVerdictLabelZh = verdictLabelZh,
            ClausePromotionReadinessCode = readinessCode,
            ClausePromotionReadinessLabelZh = readinessLabelZh
        };
    }

    private static DefinitionClauseDecisionBridgeResult None()
    {
        return new DefinitionClauseDecisionBridgeResult
        {
            ClauseVerdictCode = "NONE",
            ClauseVerdictLabelZh = "无条款判定",
            ClausePromotionReadinessCode = "NONE",
            ClausePromotionReadinessLabelZh = "无提升准备度"
        };
    }

    private static bool HasAny(IReadOnlyCollection<int>? ids)
    {
        return ids is { Count: > 0 };
    }
}
