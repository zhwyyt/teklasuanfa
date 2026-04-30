using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal sealed class NewInputLayerDraftArtifact
{
    public string SchemaVersion { get; init; } = "new-input-layer-draft/v0";

    public string GeneratedAtUtc { get; init; } = string.Empty;

    public string SourceRunId { get; init; } = string.Empty;

    public string SourceInputDirectory { get; init; } = string.Empty;

    public string SourceOutputDirectory { get; init; } = string.Empty;

    public NewInputLayerDraftNormalizationProfile NormalizationProfile { get; init; } = new();

    public NewInputLayerDraftSummary Summary { get; init; } = new();

    public List<NewInputLayerDraftMemberRow> Members { get; init; } = new();
}

internal sealed class NewInputLayerDraftNormalizationProfile
{
    public string PathModelPolicy { get; init; } = "PREFER_PATH_SECTION_SWEEP_WITH_EXPLICIT_PATH_AND_SECTION";

    public string PlateModelPolicy { get; init; } = "PREFER_BOUNDARY_THICKNESS_WITH_EXPLICIT_BOUNDARY_AND_THICKNESS";

    public string FallbackPolicy { get; init; } = "ONLY_FALLBACK_WHEN_PATH_SECTION_OR_BOUNDARY_THICKNESS_CANNOT_BE_FORMED";

    public string ConfidencePolicy { get; init; } = "REPRESENTATION_LEVEL_AND_DISTORTION_RISK_ARE_FIRST_CLASS_OUTPUTS";
}

internal sealed class NewInputLayerDraftSummary
{
    public int MemberCount { get; init; }

    public int TotalPartCount { get; init; }

    public List<NewInputLayerDraftBreakdownItem> PathModelTypeBreakdown { get; init; } = new();

    public List<NewInputLayerDraftBreakdownItem> PartModelTypeBreakdown { get; init; } = new();

    public List<NewInputLayerDraftBreakdownItem> RepresentationKindBreakdown { get; init; } = new();

    public List<NewInputLayerDraftBreakdownItem> RepresentationLevelBreakdown { get; init; } = new();

    public List<NewInputLayerDraftBreakdownItem> DistortionRiskBreakdown { get; init; } = new();

    public List<NewInputLayerDraftBreakdownItem> WarningBreakdown { get; init; } = new();
}

internal sealed class NewInputLayerDraftBreakdownItem
{
    public string Code { get; init; } = string.Empty;

    public string LabelZh { get; init; } = string.Empty;

    public int Count { get; init; }
}

internal sealed class NewInputLayerDraftMemberRow
{
    public string AssemblyId { get; init; } = string.Empty;

    public string MemberId { get; init; } = string.Empty;

    public string MemberName { get; init; } = string.Empty;

    public string Profile { get; init; } = string.Empty;

    public string Material { get; init; } = string.Empty;

    public string SourceFileName { get; init; } = string.Empty;

    public NewInputLayerDraftRawMember RawMember { get; init; } = new();

    public NewInputLayerDraftLongitudinalPath NormalizedLongitudinalPath { get; init; } = new();

    public List<NewInputLayerDraftPartRow> NormalizedParts { get; init; } = new();

    public List<NewInputLayerDraftStationQueryContext> StationQueryContexts { get; init; } = new();

    public List<string> NormalizationWarnings { get; init; } = new();
}

internal sealed class NewInputLayerDraftRawMember
{
    public string LongitudinalAxisKind { get; init; } = string.Empty;

    public double? LongitudinalAxisConfidence { get; init; }

    public string LongitudinalAxisSource { get; init; } = string.Empty;

    public int GuidePolylinePointCount { get; init; }

    public int AxisSegmentCount { get; init; }

    public double? MainAxisLength { get; init; }

    public BoundingBox BoundingBox { get; init; } = new(Vector3.Zero, Vector3.Zero);
}

internal sealed class NewInputLayerDraftLongitudinalPath
{
    public string PathModelType { get; init; } = string.Empty;

    public List<NewInputLayerDraftPathSegment> PathSegments { get; init; } = new();

    public double PathLength { get; init; }

    public string PathSourceKind { get; init; } = string.Empty;

    public double? PathConfidence { get; init; }

    public bool FallbackUsed { get; init; }

    public List<string> SupportEvidence { get; init; } = new();

    public List<string> WarningCodes { get; init; } = new();
}

internal sealed class NewInputLayerDraftPathSegment
{
    public int SegmentIndex { get; init; }

    public Vector3 StartPoint { get; init; } = Vector3.Zero;

    public Vector3 EndPoint { get; init; } = Vector3.Zero;

    public Vector3 Direction { get; init; } = Vector3.Zero;

    public double PathStart { get; init; }

    public double PathEnd { get; init; }

    public double Length { get; init; }

    public string SourceKind { get; init; } = string.Empty;

    public string SourceReference { get; init; } = string.Empty;
}

internal sealed class NewInputLayerDraftPartRow
{
    public int PartId { get; init; }

    public string PartName { get; init; } = string.Empty;

    public string PartType { get; init; } = string.Empty;

    public string ProfileString { get; init; } = string.Empty;

    public string Material { get; init; } = string.Empty;

    public NewInputLayerDraftRawGeometry RawGeometry { get; init; } = new();

    public string PartModelType { get; init; } = string.Empty;

    public NewInputLayerDraftRepresentation Representation { get; init; } = new();

    public NewInputLayerDraftNormalizedShape NormalizedShape { get; init; } = new();

    public NewInputLayerDraftLongitudinalProjection LongitudinalProjection { get; init; } = new();

    public NewInputLayerDraftSemanticHints SemanticHints { get; init; } = new();

    public List<string> NormalizationWarnings { get; init; } = new();
}

internal sealed class NewInputLayerDraftRepresentation
{
    public string RepresentationKind { get; init; } = string.Empty;

    public string RepresentationKindLabelZh { get; init; } = string.Empty;

    public string RepresentationLevel { get; init; } = string.Empty;

    public string RepresentationLevelLabelZh { get; init; } = string.Empty;

    public string DistortionRiskCode { get; init; } = string.Empty;

    public string DistortionRiskLabelZh { get; init; } = string.Empty;

    public bool ExactGeometryAvailable { get; init; }

    public bool ApproximationUsed { get; init; }

    public List<string> DistortionRiskReasons { get; init; } = new();

    public List<string> DegradationReasonCodes { get; init; } = new();
}

internal sealed class NewInputLayerDraftRawGeometry
{
    public BoundingBox BoundingBox { get; init; } = new(Vector3.Zero, Vector3.Zero);

    public Vector3 Centroid { get; init; } = Vector3.Zero;

    public Vector3 LocalAxisX { get; init; } = Vector3.Zero;

    public Vector3 LocalAxisY { get; init; } = Vector3.Zero;

    public Vector3 LocalAxisZ { get; init; } = Vector3.Zero;

    public double? Thickness { get; init; }

    public string ThicknessSource { get; init; } = string.Empty;

    public string ThicknessSourceDetail { get; init; } = string.Empty;

    public double? AxisProjectionLength { get; init; }

    public int SolidEdgeCount { get; init; }

    public bool NearMemberStart { get; init; }

    public bool NearMemberEnd { get; init; }

    public bool OuterSideCandidate { get; init; }

    public double? Planarity { get; init; }
}

internal sealed class NewInputLayerDraftNormalizedShape
{
    public Vector3 ReferenceOrigin { get; init; } = Vector3.Zero;

    public Vector3 LongDirection { get; init; } = Vector3.Zero;

    public Vector3 NormalDirection { get; init; } = Vector3.Zero;

    public Vector3 WidthDirection { get; init; } = Vector3.Zero;

    public double? Thickness { get; init; }

    public string BoundaryLoopKind { get; init; } = string.Empty;

    public string BoundarySource { get; init; } = string.Empty;

    public int BoundaryPointCount { get; init; }

    public bool BoundaryClosed { get; init; }

    public string SweepProfile { get; init; } = string.Empty;

    public string SweepPathReference { get; init; } = string.Empty;

    public string SectionSource { get; init; } = string.Empty;

    public bool HasExplicitBoundary { get; init; }

    public bool HasExplicitThickness { get; init; }

    public bool HasExplicitSection { get; init; }

    public double ModelConfidence { get; init; }
}

internal sealed class NewInputLayerDraftLongitudinalProjection
{
    public double? ProjectionStart { get; init; }

    public double? ProjectionEnd { get; init; }

    public double? ProjectionLength { get; init; }

    public double? CoverageRatio { get; init; }

    public double ProjectionConfidence { get; init; }
}

internal sealed class NewInputLayerDraftSemanticHints
{
    public string ImportedRoleHint { get; init; } = string.Empty;

    public double ImportedRoleScore { get; init; }

    public bool SourceMainPartHint { get; init; }

    public bool OuterSideHint { get; init; }
}

internal sealed class NewInputLayerDraftStationQueryContext
{
    public int StationIndex { get; init; }

    public double StationRatio { get; init; }

    public double StationDistance { get; init; }

    public Vector3 SectionFrameOrigin { get; init; } = Vector3.Zero;

    public Vector3 SectionAxisX { get; init; } = Vector3.Zero;

    public Vector3 SectionAxisY { get; init; } = Vector3.Zero;

    public Vector3 SectionAxisZ { get; init; } = Vector3.Zero;

    public int SegmentReference { get; init; }

    public double FrameConfidence { get; init; }

    public List<string> Warnings { get; init; } = new();
}

internal sealed class NewInputLayerDraftWorkflowResult
{
    public string JsonPath { get; init; } = string.Empty;

    public string MarkdownPath { get; init; } = string.Empty;
}
