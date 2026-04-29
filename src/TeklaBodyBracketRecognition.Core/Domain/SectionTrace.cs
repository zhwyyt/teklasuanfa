namespace TeklaBodyBracketRecognition.Core.Domain;

public sealed record SectionTraceSegment
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
    public required IReadOnlyList<string> Reasons { get; init; }
}

public sealed record SectionTraceStationResult
{
    public required string StationId { get; init; }
    public required double AxisPosition { get; init; }
    public required SectionStationKind StationKind { get; init; }
    public required IReadOnlyList<SectionTraceSegment> Segments { get; init; }
}

public sealed record SectionTraceExtractionResult
{
    public required string AssemblyId { get; init; }
    public required int InputMainPartId { get; init; }
    public required Vector3 SectionAxisX { get; init; }
    public required Vector3 SectionAxisY { get; init; }
    public required Vector3 SectionAxisZ { get; init; }
    public required IReadOnlyList<SectionTraceStationResult> Stations { get; init; }
}
