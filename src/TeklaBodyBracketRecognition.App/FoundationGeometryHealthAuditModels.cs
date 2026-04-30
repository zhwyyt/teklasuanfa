namespace TeklaBodyBracketRecognition.App;

internal sealed class FoundationGeometryHealthAuditArtifact
{
    public string SchemaVersion { get; init; } = "foundation-geometry-health-audit/v1";

    public string GeneratedAtUtc { get; init; } = string.Empty;

    public string SourceRunId { get; init; } = string.Empty;

    public string SourceInputDirectory { get; init; } = string.Empty;

    public string SourceOutputDirectory { get; init; } = string.Empty;

    public List<string> EnabledChecks { get; init; } = new();

    public List<string> DeferredChecks { get; init; } = new();

    public FoundationGeometryHealthAuditSummary Summary { get; init; } = new();

    public List<FoundationGeometryHealthAuditMemberRow> MemberRows { get; init; } = new();

    public List<FoundationGeometryHealthAuditStationRow> StationRows { get; init; } = new();

    public List<FoundationGeometryHealthAuditCandidateRow> CandidateRows { get; init; } = new();
}

internal sealed class FoundationGeometryHealthAuditSummary
{
    public int TotalMemberCount { get; init; }

    public int PassCount { get; init; }

    public int WarningCount { get; init; }

    public int FailCount { get; init; }

    public int NotEvaluatedCount { get; init; }

    public List<FoundationGeometryHealthAuditBreakdownItem> PrimaryReasonBreakdown { get; init; } = new();

    public List<FoundationGeometryHealthAuditBreakdownItem> SuggestedInvestigationLayerBreakdown { get; init; } = new();

    public List<FoundationGeometryHealthAuditCheckStatusBreakdownItem> CheckStatusBreakdown { get; init; } = new();
}

internal class FoundationGeometryHealthAuditBreakdownItem
{
    public string Code { get; init; } = string.Empty;

    public string LabelZh { get; init; } = string.Empty;

    public int Count { get; init; }

    public double Share { get; init; }
}

internal sealed class FoundationGeometryHealthAuditCheckStatusBreakdownItem : FoundationGeometryHealthAuditBreakdownItem
{
    public string CheckCode { get; init; } = string.Empty;

    public string CheckLabelZh { get; init; } = string.Empty;
}

internal sealed class FoundationGeometryHealthAuditMemberRow
{
    public string AssemblyId { get; init; } = string.Empty;

    public string AssemblyNumber { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string MemberName { get; init; } = string.Empty;

    public string Profile { get; init; } = string.Empty;

    public string SourceFileName { get; init; } = string.Empty;

    public string OverallHealthStatusCode { get; init; } = string.Empty;

    public string OverallHealthStatusLabelZh { get; init; } = string.Empty;

    public string PrimaryReasonCode { get; init; } = string.Empty;

    public string PrimaryReasonLabelZh { get; init; } = string.Empty;

    public string SuggestedInvestigationLayerCode { get; init; } = string.Empty;

    public string SuggestedInvestigationLayerLabelZh { get; init; } = string.Empty;

    public List<string> ReasonCodes { get; init; } = new();

    public string EvidenceSummaryZh { get; init; } = string.Empty;

    public string AxisConsistencyStatusCode { get; init; } = string.Empty;

    public string AxisConsistencyStatusLabelZh { get; init; } = string.Empty;

    public string CandidateSetConsistencyStatusCode { get; init; } = string.Empty;

    public string CandidateSetConsistencyStatusLabelZh { get; init; } = string.Empty;

    public string SampleTraceConsistencyStatusCode { get; init; } = string.Empty;

    public string SampleTraceConsistencyStatusLabelZh { get; init; } = string.Empty;

    public string SectionFrameConsistencyStatusCode { get; init; } = "NOT_EVALUATED";

    public string SectionFrameConsistencyStatusLabelZh { get; init; } = "未评估";

    public string TopologyInputConsistencyStatusCode { get; init; } = "NOT_EVALUATED";

    public string TopologyInputConsistencyStatusLabelZh { get; init; } = "未评估";

    public double? AxisSegmentsLength { get; init; }

    public double? MemberMainAxisLength { get; init; }

    public double? LongestMainPlateAxisProjectionLength { get; init; }

    public double? CandidateSetSpanLength { get; init; }

    public double? AxisLengthToMemberMainAxisRatio { get; init; }

    public double? AxisLengthToLongestMainPlateRatio { get; init; }

    public double? AxisLengthToCandidateSetSpanRatio { get; init; }

    public List<string> AxisEvidenceReasonCodes { get; init; } = new();

    public int TotalPartCount { get; init; }

    public int CandidatePartCount { get; init; }

    public int BodyCandidatePartCount { get; init; }

    public List<int> CandidatePartIds { get; init; } = new();

    public List<int> BodyCandidatePartIds { get; init; } = new();

    public List<int> SuspectMissingMainPartIds { get; init; } = new();

    public List<int> SuspectAccessoryDominantPartIds { get; init; } = new();

    public double? CandidateSetCoverageRatio { get; init; }

    public List<string> CandidateEvidenceReasonCodes { get; init; } = new();

    public int EvaluatedStationCount { get; init; }

    public int EvaluatedTraceCount { get; init; }

    public int TraceDirectionDriftCount { get; init; }

    public int TraceCenterDriftCount { get; init; }

    public int TraceDiagonalizedCount { get; init; }

    public int TraceParallelRelationLostCount { get; init; }

    public int TraceConnectorRelationLostCount { get; init; }

    public double? MaxTraceDirectionDeviationDegrees { get; init; }

    public List<string> SampleTraceEvidenceReasonCodes { get; init; } = new();
}

internal sealed class FoundationGeometryHealthAuditStationRow
{
    public string AssemblyId { get; init; } = string.Empty;

    public string AssemblyNumber { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public int StationIndex { get; init; }

    public double? StationRatio { get; init; }

    public double? StationDistance { get; init; }

    public string FrameStatusCode { get; init; } = "NOT_EVALUATED";

    public string FrameStatusLabelZh { get; init; } = "未评估";

    public string TraceStatusCode { get; init; } = "NOT_EVALUATED";

    public string TraceStatusLabelZh { get; init; } = "未评估";

    public string TopologyInputStatusCode { get; init; } = "NOT_EVALUATED";

    public string TopologyInputStatusLabelZh { get; init; } = "未评估";

    public List<string> ReasonCodes { get; init; } = new();

    public string EvidenceSummaryZh { get; init; } = string.Empty;
}

internal sealed class FoundationGeometryHealthAuditCandidateRow
{
    public string AssemblyId { get; init; } = string.Empty;

    public string AssemblyNumber { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public int PartId { get; init; }

    public string PartName { get; init; } = string.Empty;

    public string PartRole { get; init; } = string.Empty;

    public string PartitionClass { get; init; } = string.Empty;

    public double? LongitudinalCoverageEstimate { get; init; }

    public double? AxisProjectionLength { get; init; }

    public string CandidateDecision { get; init; } = "NOT_EVALUATED";

    public string CandidateDecisionLabelZh { get; init; } = "未评估";

    public List<string> ReasonCodes { get; init; } = new();

    public string EvidenceSummaryZh { get; init; } = string.Empty;
}

internal sealed class FoundationGeometryHealthAuditWorkflowResult
{
    public string JsonPath { get; init; } = string.Empty;

    public string MarkdownPath { get; init; } = string.Empty;
}
