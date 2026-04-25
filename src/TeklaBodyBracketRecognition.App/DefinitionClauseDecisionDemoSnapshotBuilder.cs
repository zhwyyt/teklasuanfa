using System.Linq;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoSnapshotBuilder
{
    public static DefinitionClauseDecisionSnapshot Build()
    {
        var sourceRows = DefinitionClauseDecisionDemoSourceBuilder.BuildFromDefaultFixtures();

        return new DefinitionClauseDecisionSnapshot
        {
            Members = sourceRows
                .GroupBy(static row => new { row.MemberId, row.AssemblyId })
                .Select(group => new DefinitionClauseDecisionSnapshotMember
                {
                    MemberId = group.Key.MemberId,
                    AssemblyId = group.Key.AssemblyId,
                    Parts = group.Select(static row => new DefinitionClauseDecisionSnapshotPart
                    {
                        PartId = row.PartId,
                        PartName = row.PartName,
                        IsRepresentative = row.IsRepresentative,
                        DefinitionClauseCode = row.DefinitionClauseCode,
                        DefinitionClauseLabelZh = row.DefinitionClauseLabelZh,
                        DefinitionClauseEffectCode = row.DefinitionClauseEffectCode,
                        DefinitionClauseEffectLabelZh = row.DefinitionClauseEffectLabelZh,
                        ClauseVerdictCode = row.ClauseVerdictCode,
                        ClauseVerdictLabelZh = row.ClauseVerdictLabelZh,
                        ClausePromotionReadinessCode = row.ClausePromotionReadinessCode,
                        ClausePromotionReadinessLabelZh = row.ClausePromotionReadinessLabelZh
                    }).ToList()
                })
                .ToList()
        };
    }
}
