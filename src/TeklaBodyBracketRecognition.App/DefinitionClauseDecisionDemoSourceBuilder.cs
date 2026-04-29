using System.Collections.Generic;
using System.Linq;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoSourceBuilder
{
    public static IReadOnlyList<DefinitionClauseDecisionSourceRow> BuildFromDefaultFixtures()
    {
        var fixtures = DefinitionClauseDecisionBridgeFixtures.CreateDefault();

        return fixtures
            .Select(fixture =>
            {
                var actual = DefinitionClauseDecisionBridge.Evaluate(fixture.Input);
                return new DefinitionClauseDecisionSourceRow
                {
                    MemberId = fixture.Name,
                    AssemblyId = fixture.Name,
                    PartId = fixture.Name,
                    PartName = fixture.Name,
                    IsRepresentative = true,
                    DefinitionClauseCode = fixture.Input.DefinitionClauseCode,
                    DefinitionClauseLabelZh = ResolveDefinitionClauseLabel(fixture.Input.DefinitionClauseCode),
                    DefinitionClauseEffectCode = fixture.Input.DefinitionClauseEffectCode,
                    DefinitionClauseEffectLabelZh = ResolveDefinitionClauseEffectLabel(fixture.Input.DefinitionClauseEffectCode),
                    ClauseVerdictCode = actual.ClauseVerdictCode,
                    ClauseVerdictLabelZh = actual.ClauseVerdictLabelZh,
                    ClausePromotionReadinessCode = actual.ClausePromotionReadinessCode,
                    ClausePromotionReadinessLabelZh = actual.ClausePromotionReadinessLabelZh
                };
            })
            .ToList();
    }

    private static string ResolveDefinitionClauseLabel(string code)
    {
        return code switch
        {
            "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE" => "箱型：闭合/对边稳定条款",
            "PRIMARY_PLATE_CONTINUITY_CLAUSE" => "主板：多数站位持续性条款",
            "BODY_PLATE_CONTINUITY_CLAUSE" => "主体板：多数站位持续性条款",
            "CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE" => "多板簇：直接控制排除条款",
            "CLUSTER_ACCESSORY_EXCLUSION_CLAUSE" => "多板簇：附属件排除条款",
            "CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE" => "包络控制：重分配复核条款",
            "GENERAL_DEFINITION_REVIEW_CLAUSE" => "一般定义：复核条款",
            _ => "无定义条款"
        };
    }

    private static string ResolveDefinitionClauseEffectLabel(string code)
    {
        return code switch
        {
            "BOX_CLOSED_LOOP_DIRECT_BREAK_EFFECT" => "箱型：移除此件会直接破坏闭合",
            "BOX_OPPOSITE_WALL_STABILITY_REWRITE_EFFECT" => "箱型：移除此件会改写对边稳定控制",
            "PRIMARY_PLATE_CONTINUITY_BREAK_EFFECT" => "主板：移除此件会破坏多数站位持续性",
            "PRIMARY_PLATE_CONTINUITY_REWRITE_EFFECT" => "主板：移除此件会改写多数站位持续性",
            "BODY_PLATE_CONTINUITY_BREAK_EFFECT" => "主体板：移除此件会破坏多数站位持续性",
            "BODY_PLATE_CONTINUITY_REWRITE_EFFECT" => "主体板：移除此件会改写多数站位持续性",
            "CLUSTER_DIRECT_CONTROLLER_NOT_EXCLUDABLE_EFFECT" => "多板簇：此件更像直接控制件，不能直接排除",
            "CLUSTER_DIRECT_CONTROL_REVIEW_EFFECT" => "多板簇：此件仍需按直接控制路径复核",
            "CLUSTER_ACCESSORY_EXCLUSION_SUPPORTED_EFFECT" => "多板簇：此件更像附属/边界件，可优先排除",
            "CLUSTER_ACCESSORY_EXCLUSION_REVIEW_EFFECT" => "多板簇：此件排除后仍会改写主截面，需复核",
            "CONTROLLER_REASSIGNMENT_TRIGGERED_EFFECT" => "包络控制：移除此件会触发控制重分配",
            "GENERAL_DEFINITION_REVIEW_TRIGGERED_EFFECT" => "一般定义：移除此件会触发定义级复核",
            _ => "无定义条款效果"
        };
    }
}
