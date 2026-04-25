using System.Collections.Generic;
using System.Linq;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionSnapshotSourceProvider
    : IDefinitionClauseDecisionSourceProvider<DefinitionClauseDecisionSnapshot>
{
    public IReadOnlyList<DefinitionClauseDecisionSourceRow> BuildSourceRows(DefinitionClauseDecisionSnapshot input)
    {
        if (input is null)
        {
            return new List<DefinitionClauseDecisionSourceRow>();
        }

        return input.Members
            .SelectMany(member => member.Parts.Select(part => new DefinitionClauseDecisionSourceRow
            {
                MemberId = member.MemberId,
                AssemblyId = member.AssemblyId,
                PartId = part.PartId,
                PartName = part.PartName,
                IsRepresentative = part.IsRepresentative,
                DefinitionClauseCode = part.DefinitionClauseCode,
                DefinitionClauseLabelZh = part.DefinitionClauseLabelZh,
                DefinitionClauseEffectCode = part.DefinitionClauseEffectCode,
                DefinitionClauseEffectLabelZh = part.DefinitionClauseEffectLabelZh,
                ClauseVerdictCode = part.ClauseVerdictCode,
                ClauseVerdictLabelZh = part.ClauseVerdictLabelZh,
                ClausePromotionReadinessCode = part.ClausePromotionReadinessCode,
                ClausePromotionReadinessLabelZh = part.ClausePromotionReadinessLabelZh
            }))
            .ToList();
    }
}
