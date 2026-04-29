namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyProfileResolutionRow
{
    public string SourceFile { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string AssemblyId { get; init; } = string.Empty;

    public string FamilyCode { get; init; } = "NONE";

    public string FamilyLabelZh { get; init; } = "未进入家族判定";

    public string FamilySubtypeCode { get; init; } = "NONE";

    public string FamilySubtypeLabelZh { get; init; } = "无子类";

    public string LongitudinalTypeCode { get; init; } = "STRAIGHT";

    public string LongitudinalTypeLabelZh { get; init; } = "直线主线";

    public string LongitudinalSubtypeCode { get; init; } = "GENERAL_STRAIGHT";

    public string LongitudinalSubtypeLabelZh { get; init; } = "一般直线主线";

    public string ResolutionStatusCode { get; init; } = "ReviewBypass";

    public string ResolutionStatusLabelZh { get; init; } = "人工复核旁路";

    public string ProfileCategoryCode { get; init; } = "NONE";

    public string ProfileCategoryLabelZh { get; init; } = "未自动细分";

    public string ProfileCode { get; init; } = "NONE";

    public string ProfileLabelZh { get; init; } = "未自动细分";

    public string ProfileSeriesCode { get; init; } = "NONE";

    public string ProfileSeriesLabelZh { get; init; } = "无系列";

    public string DimensionSourceCode { get; init; } = "NONE";

    public string DimensionSourceLabelZh { get; init; } = "无尺寸来源";

    public string SimilarityBasisCode { get; init; } = "NONE";

    public string SimilarityBasisLabelZh { get; init; } = "无相似度依据";

    public double SimilarityScore { get; init; }

    public string EvidenceSummary { get; init; } = string.Empty;

    public IReadOnlyList<string> SatisfiedConditions { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> MissingConditions { get; init; } = Array.Empty<string>();
}

internal sealed class BodyProfileResolutionArtifact
{
    public List<BodyProfileResolutionRow> Rows { get; init; } = new();

    public BodyProfileResolutionSummary Summary { get; init; } = new();
}

internal sealed class BodyProfileResolutionSummary
{
    public int AssemblyCount { get; init; }

    public int ResolvedCount { get; init; }

    public int ReviewBypassCount { get; init; }

    public List<DefinitionClauseDecisionBreakdownItem> StatusBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> LongitudinalTypeBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> CategoryBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> ProfileBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> ProfileSeriesBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> DimensionSourceBreakdown { get; init; } = new();
}

internal sealed class BodyProfileResolutionValidationResult
{
    public bool IsValid { get; init; }

    public int AssemblyCount { get; init; }

    public int ResolvedCount { get; init; }

    public int ReviewBypassCount { get; init; }

    public List<string> Issues { get; init; } = new();
}

internal sealed class BodyProfileResolutionManifest
{
    public string JsonPath { get; init; } = string.Empty;

    public string MarkdownPath { get; init; } = string.Empty;

    public string ValidationJsonPath { get; init; } = string.Empty;

    public string ValidationMarkdownPath { get; init; } = string.Empty;
}

internal sealed class BodyProfileResolutionExportResult
{
    public string JsonPath { get; init; } = string.Empty;

    public string MarkdownPath { get; init; } = string.Empty;

    public string ValidationJsonPath { get; init; } = string.Empty;

    public string ValidationMarkdownPath { get; init; } = string.Empty;

    public string ManifestJsonPath { get; init; } = string.Empty;

    public string ReadmePath { get; init; } = string.Empty;
}

internal sealed class BodyProfileResolutionWorkflowResult
{
    public string JsonPath { get; init; } = string.Empty;

    public string MarkdownPath { get; init; } = string.Empty;

    public string ValidationJsonPath { get; init; } = string.Empty;

    public string ValidationMarkdownPath { get; init; } = string.Empty;

    public string ManifestJsonPath { get; init; } = string.Empty;

    public string ReadmePath { get; init; } = string.Empty;
}
