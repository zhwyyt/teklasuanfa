using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionBridgeFixture
{
    public string Name { get; init; } = string.Empty;
    public DefinitionClauseDecisionBridgeInput Input { get; init; } = new();
    public DefinitionClauseDecisionBridgeResult Expected { get; init; } = new();
}

public static class DefinitionClauseDecisionBridgeFixtures
{
    public static IReadOnlyList<DefinitionClauseDecisionBridgeFixture> CreateDefault()
    {
        return new[]
        {
            new DefinitionClauseDecisionBridgeFixture
            {
                Name = "GKZ_box_opposite_wall_rewrite",
                Input = new DefinitionClauseDecisionBridgeInput
                {
                    DefinitionClauseCode = "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE",
                    DefinitionClauseEffectCode = "BOX_OPPOSITE_WALL_STABILITY_REWRITE_EFFECT",
                    PriorityBodyCoverageAfterRemovalRatio = 1.0,
                    EnvelopeBodyCoverageAfterRemovalRatio = 1.0,
                    LostClosedLoopStationIds = System.Array.Empty<int>(),
                    LostBodyCoverageStationIds = System.Array.Empty<int>(),
                    LostEnvelopeSupportStationIds = System.Array.Empty<int>()
                },
                Expected = new DefinitionClauseDecisionBridgeResult
                {
                    ClauseVerdictCode = "CLAUSE_SATISFIED_WITH_REWRITE",
                    ClauseVerdictLabelZh = "定义条款仍成立，但控制路径被改写",
                    ClausePromotionReadinessCode = "READY_WITH_REWRITE_NOTE",
                    ClausePromotionReadinessLabelZh = "可进入定义验证，但需附带改写说明"
                }
            },
            new DefinitionClauseDecisionBridgeFixture
            {
                Name = "HXZ_primary_plate_rewrite",
                Input = new DefinitionClauseDecisionBridgeInput
                {
                    DefinitionClauseCode = "PRIMARY_PLATE_CONTINUITY_CLAUSE",
                    DefinitionClauseEffectCode = "PRIMARY_PLATE_CONTINUITY_REWRITE_EFFECT",
                    PriorityBodyCoverageAfterRemovalRatio = 1.0,
                    EnvelopeBodyCoverageAfterRemovalRatio = 1.0,
                    LostClosedLoopStationIds = System.Array.Empty<int>(),
                    LostBodyCoverageStationIds = System.Array.Empty<int>(),
                    LostEnvelopeSupportStationIds = System.Array.Empty<int>()
                },
                Expected = new DefinitionClauseDecisionBridgeResult
                {
                    ClauseVerdictCode = "CLAUSE_SATISFIED_WITH_REWRITE",
                    ClauseVerdictLabelZh = "定义条款仍成立，但多数站位路径被改写",
                    ClausePromotionReadinessCode = "READY_WITH_REWRITE_NOTE",
                    ClausePromotionReadinessLabelZh = "可进入定义验证，但需附带改写说明"
                }
            },
            new DefinitionClauseDecisionBridgeFixture
            {
                Name = "MJ_direct_controller_not_excludable",
                Input = new DefinitionClauseDecisionBridgeInput
                {
                    DefinitionClauseCode = "CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE",
                    DefinitionClauseEffectCode = "CLUSTER_DIRECT_CONTROLLER_NOT_EXCLUDABLE_EFFECT",
                    PriorityBodyCoverageAfterRemovalRatio = 1.0,
                    EnvelopeBodyCoverageAfterRemovalRatio = 1.0,
                    LostClosedLoopStationIds = System.Array.Empty<int>(),
                    LostBodyCoverageStationIds = System.Array.Empty<int>(),
                    LostEnvelopeSupportStationIds = System.Array.Empty<int>()
                },
                Expected = new DefinitionClauseDecisionBridgeResult
                {
                    ClauseVerdictCode = "CLAUSE_BROKEN_DIRECT",
                    ClauseVerdictLabelZh = "定义条款被直接破坏",
                    ClausePromotionReadinessCode = "READY_FOR_DEFINITION_CHECK",
                    ClausePromotionReadinessLabelZh = "已具备进入定义验证的破坏证据"
                }
            },
            new DefinitionClauseDecisionBridgeFixture
            {
                Name = "GL_general_review_required",
                Input = new DefinitionClauseDecisionBridgeInput
                {
                    DefinitionClauseCode = "GENERAL_DEFINITION_REVIEW_CLAUSE",
                    DefinitionClauseEffectCode = "GENERAL_DEFINITION_REVIEW_TRIGGERED_EFFECT",
                    PriorityBodyCoverageAfterRemovalRatio = 1.0,
                    EnvelopeBodyCoverageAfterRemovalRatio = 1.0,
                    LostClosedLoopStationIds = System.Array.Empty<int>(),
                    LostBodyCoverageStationIds = System.Array.Empty<int>(),
                    LostEnvelopeSupportStationIds = System.Array.Empty<int>()
                },
                Expected = new DefinitionClauseDecisionBridgeResult
                {
                    ClauseVerdictCode = "CLAUSE_REVIEW_REQUIRED",
                    ClauseVerdictLabelZh = "定义条款仍需复核",
                    ClausePromotionReadinessCode = "NOT_READY_NEEDS_REVIEW",
                    ClausePromotionReadinessLabelZh = "仍需复核后才能进入定义验证"
                }
            },
            new DefinitionClauseDecisionBridgeFixture
            {
                Name = "YPGL_controller_reassignment_review",
                Input = new DefinitionClauseDecisionBridgeInput
                {
                    DefinitionClauseCode = "CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE",
                    DefinitionClauseEffectCode = "CONTROLLER_REASSIGNMENT_TRIGGERED_EFFECT",
                    PriorityBodyCoverageAfterRemovalRatio = 1.0,
                    EnvelopeBodyCoverageAfterRemovalRatio = 1.0,
                    LostClosedLoopStationIds = System.Array.Empty<int>(),
                    LostBodyCoverageStationIds = System.Array.Empty<int>(),
                    LostEnvelopeSupportStationIds = System.Array.Empty<int>()
                },
                Expected = new DefinitionClauseDecisionBridgeResult
                {
                    ClauseVerdictCode = "CLAUSE_REVIEW_REQUIRED",
                    ClauseVerdictLabelZh = "定义条款仍需复核",
                    ClausePromotionReadinessCode = "NOT_READY_NEEDS_REVIEW",
                    ClausePromotionReadinessLabelZh = "仍需复核后才能进入定义验证"
                }
            }
        };
    }
}
