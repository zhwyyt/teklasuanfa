using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionArtifacts
{
    public List<DefinitionClauseDecisionRepresentativeRow> RepresentativeRows { get; init; } = new();
    public List<DefinitionClauseDecisionAggregateRow> AggregateRows { get; init; } = new();
}

public sealed class DefinitionClauseDecisionReviewRow
{
    public string MemberId { get; init; } = string.Empty;
    public string AssemblyId { get; init; } = string.Empty;
    public string LeadClauseVerdictLabelZh { get; init; } = "无条款判定";
    public string LeadClausePromotionReadinessLabelZh { get; init; } = "无提升准备度";
    public string ClauseVerdictMixStatus { get; init; } = "无条款判定";
    public string ClausePromotionReadinessMixStatus { get; init; } = "无提升准备度";
    public string ReviewHint { get; init; } = "定义条款仍需复核";
}
