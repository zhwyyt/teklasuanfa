using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal sealed class ProofPipelineResult
{
    public required BodyCandidatePartitionResult Partition { get; init; }

    public required StableBodyZoneResult StableZones { get; init; }

    public required SectionStationPlanResult StationPlan { get; init; }

    public required SectionTraceExtractionResult TraceResult { get; init; }

    public required SectionTraceCleaningResult TraceCleaningResult { get; init; }

    public required SectionTopologyAnalysisResult TopologyResult { get; init; }

    public required CoreBodyProofResult ProofResult { get; init; }
}

internal sealed class PipelineRunResult
{
    public required BodyCandidatePartitionViewOutput PartitionView { get; init; }

    public required StableBodyZoneViewOutput StableZoneView { get; init; }

    public required SectionStationViewOutput SectionStationView { get; init; }

    public required SectionTraceViewOutput SectionTraceView { get; init; }

    public required SectionTraceCleaningViewOutput TraceCleaningView { get; init; }

    public required SectionTopologyViewOutput TopologyView { get; init; }

    public required CoreBodyProofViewOutput ProofView { get; init; }
}
