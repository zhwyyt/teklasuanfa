using System;
using System.Collections.Generic;
using System.Linq;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionAggregateItem
{
    public string ClauseVerdictCode { get; init; } = "NONE";
    public string ClauseVerdictLabelZh { get; init; } = "无条款判定";
    public string ClausePromotionReadinessCode { get; init; } = "NONE";
    public string ClausePromotionReadinessLabelZh { get; init; } = "无提升准备度";
}

public sealed class DefinitionClauseDecisionAggregateResult
{
    public string ClauseVerdicts { get; init; } = "NONE";
    public string LeadClauseVerdictCode { get; init; } = "NONE";
    public string LeadClauseVerdictLabelZh { get; init; } = "无条款判定";
    public double LeadClauseVerdictShare { get; init; }
    public string ClauseVerdictMixStatus { get; init; } = "无条款判定";

    public string ClausePromotionReadinesses { get; init; } = "NONE";
    public string LeadClausePromotionReadinessCode { get; init; } = "NONE";
    public string LeadClausePromotionReadinessLabelZh { get; init; } = "无提升准备度";
    public double LeadClausePromotionReadinessShare { get; init; }
    public string ClausePromotionReadinessMixStatus { get; init; } = "无提升准备度";
}

public static class DefinitionClauseDecisionAggregate
{
    public static DefinitionClauseDecisionAggregateResult Build(IEnumerable<DefinitionClauseDecisionAggregateItem>? items)
    {
        var materialized = items?
            .Where(static x => x is not null)
            .ToList() ?? new List<DefinitionClauseDecisionAggregateItem>();

        if (materialized.Count == 0)
        {
            return new DefinitionClauseDecisionAggregateResult();
        }

        var verdictSummary = BuildSummary(
            materialized,
            static x => x.ClauseVerdictCode,
            static x => x.ClauseVerdictLabelZh,
            "无条款判定");

        var readinessSummary = BuildSummary(
            materialized,
            static x => x.ClausePromotionReadinessCode,
            static x => x.ClausePromotionReadinessLabelZh,
            "无提升准备度");

        return new DefinitionClauseDecisionAggregateResult
        {
            ClauseVerdicts = verdictSummary.AllLabels,
            LeadClauseVerdictCode = verdictSummary.LeadCode,
            LeadClauseVerdictLabelZh = verdictSummary.LeadLabel,
            LeadClauseVerdictShare = verdictSummary.LeadShare,
            ClauseVerdictMixStatus = verdictSummary.MixStatus,
            ClausePromotionReadinesses = readinessSummary.AllLabels,
            LeadClausePromotionReadinessCode = readinessSummary.LeadCode,
            LeadClausePromotionReadinessLabelZh = readinessSummary.LeadLabel,
            LeadClausePromotionReadinessShare = readinessSummary.LeadShare,
            ClausePromotionReadinessMixStatus = readinessSummary.MixStatus
        };
    }

    private static AggregateSummary BuildSummary(
        IEnumerable<DefinitionClauseDecisionAggregateItem> items,
        Func<DefinitionClauseDecisionAggregateItem, string> codeSelector,
        Func<DefinitionClauseDecisionAggregateItem, string> labelSelector,
        string emptyLabel)
    {
        var groups = items
            .GroupBy(codeSelector)
            .Select(group => new
            {
                Code = group.Key,
                Label = group.Select(labelSelector).FirstOrDefault() ?? emptyLabel,
                Count = group.Count()
            })
            .OrderByDescending(static x => x.Count)
            .ThenBy(static x => x.Code, StringComparer.Ordinal)
            .ToList();

        if (groups.Count == 0)
        {
            return new AggregateSummary
            {
                AllLabels = "NONE",
                LeadCode = "NONE",
                LeadLabel = emptyLabel,
                LeadShare = 0,
                MixStatus = emptyLabel
            };
        }

        var total = groups.Sum(static x => x.Count);
        var lead = groups[0];
        var allLabels = string.Join(" / ", groups.Select(static x => x.Label));
        var leadShare = total == 0 ? 0 : Math.Round((double)lead.Count / total, 4);

        var mixStatus = groups.Count switch
        {
            0 => emptyLabel,
            1 => "单一结论",
            _ when leadShare >= 0.75 => "主结论占优，但仍属混合结论",
            _ => "混合结论"
        };

        return new AggregateSummary
        {
            AllLabels = allLabels,
            LeadCode = lead.Code,
            LeadLabel = lead.Label,
            LeadShare = leadShare,
            MixStatus = mixStatus
        };
    }

    private sealed class AggregateSummary
    {
        public string AllLabels { get; init; } = "NONE";
        public string LeadCode { get; init; } = "NONE";
        public string LeadLabel { get; init; } = string.Empty;
        public double LeadShare { get; init; }
        public string MixStatus { get; init; } = string.Empty;
    }
}
