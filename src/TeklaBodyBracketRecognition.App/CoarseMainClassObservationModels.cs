namespace TeklaBodyBracketRecognition.App;

internal sealed class CoarseMainClassObservationRow
{
    public string SourceFile { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string AssemblyId { get; init; } = string.Empty;

    public string SourceMemberMainClassCode { get; init; } = string.Empty;

    public string SourceMemberMainClassLabelZh { get; init; } = string.Empty;

    public string BodyDescriptorFamily { get; init; } = string.Empty;

    public string BodyDescriptorSectionType { get; init; } = string.Empty;

    public string ImportSynthesisKind { get; init; } = string.Empty;

    public string LongitudinalTypeCode { get; init; } = "STRAIGHT";

    public string LongitudinalTypeLabelZh { get; init; } = "直线主线";

    public int PriorityStationCount { get; init; }

    public int EligibleStationCount { get; init; }

    public int CandidatePartCount { get; init; }

    public string CandidatePartIds { get; init; } = string.Empty;

    public int ExcludedPartCount { get; init; }

    public int BoxStationCount { get; init; }

    public int HStationCount { get; init; }

    public int PrimaryPlateStationCount { get; init; }

    public int ClosedLoopStationCount { get; init; }

    public double BoxStationRatio { get; init; }

    public double HStationRatio { get; init; }

    public double PrimaryPlateStationRatio { get; init; }

    public double ClosedLoopStationRatio { get; init; }

    public string CoarseMainClassCode { get; init; } = string.Empty;

    public string CoarseMainClassLabelZh { get; init; } = string.Empty;

    public double CoarseMainClassConfidence { get; init; }

    public string CoarseMainClassSubtypeCode { get; init; } = string.Empty;

    public string CoarseMainClassSubtypeLabelZh { get; init; } = string.Empty;

    public string CoarseMainClassReasonCode { get; init; } = string.Empty;

    public string CoarseMainClassReasonLabelZh { get; init; } = string.Empty;
}

internal sealed class CoarseMainClassObservationArtifact
{
    public List<CoarseMainClassObservationRow> Rows { get; init; } = new();

    public CoarseMainClassObservationArtifactSummary Summary { get; init; } = new();
}

internal sealed class CoarseMainClassObservationArtifactSummary
{
    public int AssemblyCount { get; init; }

    public List<DefinitionClauseDecisionBreakdownItem> AttributeDirectBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> CoarseMainClassBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> CoarseSubtypeBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> CoarseReasonBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> LongitudinalTypeBreakdown { get; init; } = new();
}
