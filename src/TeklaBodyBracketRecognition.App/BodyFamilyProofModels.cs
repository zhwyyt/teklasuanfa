namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyFamilyProofRow
{
    public string SourceFile { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string AssemblyId { get; init; } = string.Empty;

    public string BodyDescriptorFamily { get; init; } = string.Empty;

    public string BodyDescriptorSectionType { get; init; } = string.Empty;

    public string SourceSemanticBodyFamily { get; init; } = string.Empty;

    public string SourceSemanticSectionType { get; init; } = string.Empty;

    public string LongitudinalTypeCode { get; init; } = "STRAIGHT";

    public string LongitudinalTypeLabelZh { get; init; } = "直线主线";

    public string LongitudinalSubtypeCode { get; init; } = "GENERAL_STRAIGHT";

    public string LongitudinalSubtypeLabelZh { get; init; } = "一般直线主线";

    public string LeadClauseCode { get; init; } = string.Empty;

    public string LeadClauseLabelZh { get; init; } = string.Empty;

    public string LeadClauseVerdictCode { get; init; } = string.Empty;

    public string LeadClauseVerdictLabelZh { get; init; } = string.Empty;

    public string LeadClausePromotionReadinessCode { get; init; } = string.Empty;

    public string LeadClausePromotionReadinessLabelZh { get; init; } = string.Empty;

    public string LeadClauseEffectDirectionCode { get; init; } = string.Empty;

    public string LeadClauseEffectDirectionLabelZh { get; init; } = string.Empty;

    public int LeadClauseBreakEffectCount { get; init; }

    public int LeadClauseRewriteEffectCount { get; init; }

    public string DefinitionDrivenFamilyCode { get; init; } = "NONE";

    public string DefinitionDrivenFamilyLabelZh { get; init; } = "未进入家族判定";

    public string DefinitionDrivenSubtypeCode { get; init; } = "NONE";

    public string DefinitionDrivenSubtypeLabelZh { get; init; } = "无子类";

    public string DecisionStatusCode { get; init; } = "Deferred";

    public string DecisionStatusLabelZh { get; init; } = "暂缓进入家族判定";

    public string DecisionReasonCode { get; init; } = "UNSPECIFIED";

    public string DecisionReasonLabelZh { get; init; } = "未说明原因";

    public IReadOnlyList<string> SatisfiedConditions { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> MissingConditions { get; init; } = Array.Empty<string>();
}
