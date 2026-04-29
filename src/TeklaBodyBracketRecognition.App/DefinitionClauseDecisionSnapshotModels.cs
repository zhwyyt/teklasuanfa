using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionSnapshot
{
    public List<DefinitionClauseDecisionSnapshotMember> Members { get; init; } = new();
}

public sealed class DefinitionClauseDecisionSnapshotMember
{
    public string MemberId { get; init; } = string.Empty;
    public string AssemblyId { get; init; } = string.Empty;
    public List<DefinitionClauseDecisionSnapshotPart> Parts { get; init; } = new();
}

public sealed class DefinitionClauseDecisionSnapshotPart
{
    public string PartId { get; init; } = string.Empty;
    public string PartName { get; init; } = string.Empty;
    public bool IsRepresentative { get; init; }

    public string DefinitionClauseCode { get; init; } = "NONE";
    public string DefinitionClauseLabelZh { get; init; } = "无定义条款";

    public string DefinitionClauseEffectCode { get; init; } = "NONE";
    public string DefinitionClauseEffectLabelZh { get; init; } = "无定义条款效果";

    public string ClauseVerdictCode { get; init; } = "NONE";
    public string ClauseVerdictLabelZh { get; init; } = "无条款判定";

    public string ClausePromotionReadinessCode { get; init; } = "NONE";
    public string ClausePromotionReadinessLabelZh { get; init; } = "无提升准备度";
}
