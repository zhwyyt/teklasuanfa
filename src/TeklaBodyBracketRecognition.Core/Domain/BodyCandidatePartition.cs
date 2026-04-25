namespace TeklaBodyBracketRecognition.Core.Domain;

public sealed record BodyCandidatePartitionItem
{
    public required int PartId { get; init; }
    public required string PartName { get; init; }
    public required string ProfileString { get; init; }
    public required bool IsInputMainPart { get; init; }
    public required BodyCandidatePartitionClass PartitionClass { get; init; }
    public required double PartitionConfidence { get; init; }
    public required IReadOnlyList<string> PartitionReasons { get; init; }
    public required string UpstreamRoleHint { get; init; }
    public required bool NearStableZone { get; init; }
    public required double LongitudinalCoverageEstimate { get; init; }
    public required double AxisIntervalMin { get; init; }
    public required double AxisIntervalMax { get; init; }
}

public sealed record BodyCandidatePartitionResult
{
    public required string AssemblyId { get; init; }
    public required int InputMainPartId { get; init; }
    public required Vector3 ProvisionalBodyAxis { get; init; }
    public required IReadOnlyList<LongitudinalAxisSegment> ProvisionalBodyAxisSegments { get; init; }
    public required IReadOnlyList<BodyCandidatePartitionItem> Items { get; init; }
}
