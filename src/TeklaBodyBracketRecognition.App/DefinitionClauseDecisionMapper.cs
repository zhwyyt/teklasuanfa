using System.Collections.Generic;
using System.Linq;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionMapper
{
    public static DefinitionClauseDecisionArtifacts BuildArtifacts(
        IEnumerable<DefinitionClauseDecisionSourceRow>? sourceRows)
    {
        var materialized = sourceRows?.ToList() ?? new List<DefinitionClauseDecisionSourceRow>();
        var representativeRows = BuildRepresentativeRows(materialized);
        var aggregateRows = BuildAggregateRows(representativeRows);
        return DefinitionClauseDecisionArtifactBuilder.Build(representativeRows, aggregateRows);
    }

    public static IReadOnlyList<DefinitionClauseDecisionRepresentativeRow> BuildRepresentativeRows(
        IEnumerable<DefinitionClauseDecisionSourceRow>? sourceRows)
    {
        var materialized = sourceRows?.ToList() ?? new List<DefinitionClauseDecisionSourceRow>();
        return materialized
            .Where(static row => row.IsRepresentative)
            .Select(static row => new DefinitionClauseDecisionRepresentativeRow
            {
                MemberId = row.MemberId,
                AssemblyId = row.AssemblyId,
                PartId = row.PartId,
                PartName = row.PartName,
                DefinitionClauseCode = row.DefinitionClauseCode,
                DefinitionClauseLabelZh = row.DefinitionClauseLabelZh,
                DefinitionClauseEffectCode = row.DefinitionClauseEffectCode,
                DefinitionClauseEffectLabelZh = row.DefinitionClauseEffectLabelZh,
                ClauseVerdictCode = row.ClauseVerdictCode,
                ClauseVerdictLabelZh = row.ClauseVerdictLabelZh,
                ClausePromotionReadinessCode = row.ClausePromotionReadinessCode,
                ClausePromotionReadinessLabelZh = row.ClausePromotionReadinessLabelZh
            })
            .ToList();
    }

    public static IReadOnlyList<DefinitionClauseDecisionAggregateRow> BuildAggregateRows(
        IEnumerable<DefinitionClauseDecisionRepresentativeRow>? representativeRows)
    {
        var materialized = representativeRows?.ToList() ?? new List<DefinitionClauseDecisionRepresentativeRow>();

        return materialized
            .GroupBy(static row => new { row.MemberId, row.AssemblyId })
            .Select(group =>
            {
                var aggregate = DefinitionClauseDecisionAggregate.Build(
                    group.Select(static row => new DefinitionClauseDecisionAggregateItem
                    {
                        ClauseVerdictCode = row.ClauseVerdictCode,
                        ClauseVerdictLabelZh = row.ClauseVerdictLabelZh,
                        ClausePromotionReadinessCode = row.ClausePromotionReadinessCode,
                        ClausePromotionReadinessLabelZh = row.ClausePromotionReadinessLabelZh
                    }));

                return new DefinitionClauseDecisionAggregateRow
                {
                    MemberId = group.Key.MemberId,
                    AssemblyId = group.Key.AssemblyId,
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
                };
            })
            .OrderBy(static row => row.MemberId)
            .ToList();
    }
}
