namespace TeklaBodyBracketRecognition.Core.Domain;

public sealed record StableBodyZone
{
    public required string ZoneId { get; init; }
    public required StableBodyZoneKind ZoneKind { get; init; }
    public required double AxisIntervalMin { get; init; }
    public required double AxisIntervalMax { get; init; }
    public required double Confidence { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
}

public sealed record StableBodyZoneResult
{
    public required string AssemblyId { get; init; }
    public required int InputMainPartId { get; init; }
    public required Vector3 ProvisionalBodyAxis { get; init; }
    public required IReadOnlyList<LongitudinalAxisSegment> ProvisionalBodyAxisSegments { get; init; }
    public required double AssemblyAxisIntervalMin { get; init; }
    public required double AssemblyAxisIntervalMax { get; init; }
    public required IReadOnlyList<StableBodyZone> Zones { get; init; }
}

public sealed record SectionStation
{
    public required string StationId { get; init; }
    public required string ZoneId { get; init; }
    public required double AxisPosition { get; init; }
    public required SectionStationKind StationKind { get; init; }
    public required IReadOnlyList<string> StationReasons { get; init; }
}

public sealed record SectionStationPlanResult
{
    public required string AssemblyId { get; init; }
    public required int InputMainPartId { get; init; }
    public required IReadOnlyList<SectionStation> Stations { get; init; }
}
