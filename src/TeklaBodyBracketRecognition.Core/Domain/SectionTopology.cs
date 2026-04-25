namespace TeklaBodyBracketRecognition.Core.Domain;

public sealed record SectionTraceCleanSegment
{
    public required string TraceId { get; init; }
    public required string StationId { get; init; }
    public required int PartId { get; init; }
    public required string PartName { get; init; }
    public required string ProfileString { get; init; }
    public required BodyCandidatePartitionClass PartitionClass { get; init; }
    public required double CenterY { get; init; }
    public required double CenterZ { get; init; }
    public required double StartY { get; init; }
    public required double StartZ { get; init; }
    public required double EndY { get; init; }
    public required double EndZ { get; init; }
    public required double WidthLength { get; init; }
    public required bool IsSuppressed { get; init; }
    public required IReadOnlyList<string> CleanReasons { get; init; }
    public required IReadOnlyList<string> SourceReasons { get; init; }
}

public sealed record SectionTraceCleanStationResult
{
    public required string StationId { get; init; }
    public required double AxisPosition { get; init; }
    public required SectionStationKind StationKind { get; init; }
    public required IReadOnlyList<SectionTraceCleanSegment> Segments { get; init; }
}

public sealed record SectionTraceCleaningResult
{
    public required string AssemblyId { get; init; }
    public required int InputMainPartId { get; init; }
    public required IReadOnlyList<SectionTraceCleanStationResult> Stations { get; init; }
}

public sealed record SectionTopologyStationResult
{
    public required string StationId { get; init; }
    public required double AxisPosition { get; init; }
    public required SectionStationKind StationKind { get; init; }
    public required IReadOnlyList<string> RetainedTraceIds { get; init; }
    public required IReadOnlyList<string> SuppressedTraceIds { get; init; }
    public required IReadOnlyList<string> OuterEnvelopeTraceIds { get; init; }
    public required IReadOnlyList<string> InternalTraceIds { get; init; }
    public required int RetainedBodyCandidateTraceCount { get; init; }
    public required int RetainedBodyAccessoryTraceCount { get; init; }
    public required int RetainedEndConnectionTraceCount { get; init; }
    public required int RetainedLocalStiffenerTraceCount { get; init; }
    public required bool ClosedLoopCandidate { get; init; }
    public required double EnvelopeMinY { get; init; }
    public required double EnvelopeMaxY { get; init; }
    public required double EnvelopeMinZ { get; init; }
    public required double EnvelopeMaxZ { get; init; }
    public required IReadOnlyList<string> StationFlags { get; init; }
}

public sealed record SectionTopologyAnalysisResult
{
    public required string AssemblyId { get; init; }
    public required int InputMainPartId { get; init; }
    public required IReadOnlyList<SectionTopologyStationResult> Stations { get; init; }
}
