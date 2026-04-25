using TeklaBodyBracketRecognition.Core.Algorithms;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyCandidatePartitionViewOutput
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string ViewKind { get; set; } = string.Empty;

    public int InputMainPartId { get; set; }

    public int SourceMainPartId { get; set; }

    public string SourceMainPartName { get; set; } = string.Empty;

    public string SourceMainPartProfileString { get; set; } = string.Empty;

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public IReadOnlyList<int> SourceBodySeedPartIds { get; set; } = Array.Empty<int>();

    public BodyCandidatePartitionResult Partition { get; set; } = new()
    {
        AssemblyId = string.Empty,
        InputMainPartId = 0,
        ProvisionalBodyAxis = Vector3.Zero,
        ProvisionalBodyAxisSegments = Array.Empty<LongitudinalAxisSegment>(),
        Items = Array.Empty<BodyCandidatePartitionItem>()
    };
}

internal sealed class StableBodyZoneViewOutput
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string ViewKind { get; set; } = string.Empty;

    public int SourceMainPartId { get; set; }

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public StableBodyZoneResult Result { get; set; } = new()
    {
        AssemblyId = string.Empty,
        InputMainPartId = 0,
        ProvisionalBodyAxis = Vector3.Zero,
        ProvisionalBodyAxisSegments = Array.Empty<LongitudinalAxisSegment>(),
        AssemblyAxisIntervalMin = 0,
        AssemblyAxisIntervalMax = 0,
        Zones = Array.Empty<StableBodyZone>()
    };
}

internal sealed class SectionStationViewOutput
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string ViewKind { get; set; } = string.Empty;

    public int SourceMainPartId { get; set; }

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public SectionStationPlanResult Result { get; set; } = new()
    {
        AssemblyId = string.Empty,
        InputMainPartId = 0,
        Stations = Array.Empty<SectionStation>()
    };
}

internal sealed class SectionTraceViewOutput
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string ViewKind { get; set; } = string.Empty;

    public int SourceMainPartId { get; set; }

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public SectionTraceExtractionResult Result { get; set; } = new()
    {
        AssemblyId = string.Empty,
        InputMainPartId = 0,
        SectionAxisX = Vector3.Zero,
        SectionAxisY = Vector3.Zero,
        SectionAxisZ = Vector3.Zero,
        Stations = Array.Empty<SectionTraceStationResult>()
    };
}

internal sealed class SectionTraceCleaningViewOutput
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string ViewKind { get; set; } = string.Empty;

    public int SourceMainPartId { get; set; }

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public SectionTraceCleaningResult Result { get; set; } = new()
    {
        AssemblyId = string.Empty,
        InputMainPartId = 0,
        Stations = Array.Empty<SectionTraceCleanStationResult>()
    };
}

internal sealed class SectionTopologyViewOutput
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string ViewKind { get; set; } = string.Empty;

    public int SourceMainPartId { get; set; }

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public SectionTopologyAnalysisResult Result { get; set; } = new()
    {
        AssemblyId = string.Empty,
        InputMainPartId = 0,
        Stations = Array.Empty<SectionTopologyStationResult>()
    };
}

internal sealed class CoreBodyProofViewOutput
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string ViewKind { get; set; } = string.Empty;

    public int SourceMainPartId { get; set; }

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public CoreBodyProofResult Result { get; set; } = new()
    {
        AssemblyId = string.Empty,
        InputMainPartId = 0,
        PriorityStationCount = 0,
        BootstrapOnly = true,
        CoreBodyPartIds = Array.Empty<int>(),
        BodyAccessoryPartIds = Array.Empty<int>(),
        ReviewPartIds = Array.Empty<int>(),
        Parts = Array.Empty<CoreBodyProofPartResult>()
    };
}

internal sealed class SectionTraceTopologySummaryRow
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string ViewKind { get; set; } = string.Empty;

    public int SourceMainPartId { get; set; }

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public int StationCount { get; set; }

    public int NonEmptyStationCount { get; set; }

    public int PriorityStationCount { get; set; }

    public int PriorityStationsWithBodyCandidate { get; set; }

    public double PriorityStationBodyCoverage { get; set; }

    public int DistinctTracedPartCount { get; set; }

    public IReadOnlyList<int> TracedPartIds { get; set; } = Array.Empty<int>();

    public IReadOnlyList<int> DominantPartIds { get; set; } = Array.Empty<int>();

    public int BodyCandidateSegmentCount { get; set; }

    public int BodyAccessorySegmentCount { get; set; }

    public int EndConnectionSegmentCount { get; set; }

    public int LocalStiffenerSegmentCount { get; set; }

    public int TinyPartSegmentCount { get; set; }

    public int SpecialShapeSegmentCount { get; set; }

    public double AverageSegmentsPerNonEmptyStation { get; set; }

    public double AverageBodyCandidateSegmentsPerStation { get; set; }

    public IReadOnlyList<string> Flags { get; set; } = Array.Empty<string>();
}
