using TeklaBodyBracketRecognition.Core.Algorithms;
using TeklaBodyBracketRecognition.Core.Config;

namespace TeklaBodyBracketRecognition.App;

internal sealed class AnalysisPipelineServices
{
    public required PartFeatureExtractor FeatureExtractor { get; init; }

    public required PartGraphBuilder GraphBuilder { get; init; }

    public required BodyCandidatePartitioner BodyCandidatePartitioner { get; init; }

    public required StableBodyZoneResolver StableBodyZoneResolver { get; init; }

    public required SectionStationPlanner SectionStationPlanner { get; init; }

    public required SectionTraceExtractor SectionTraceExtractor { get; init; }

    public required SectionTraceCleaner SectionTraceCleaner { get; init; }

    public required SectionTopologyAnalyzer SectionTopologyAnalyzer { get; init; }

    public required CoreBodyProofEngine CoreBodyProofEngine { get; init; }

    public static AnalysisPipelineServices CreateDefault(RecognitionOptions options)
    {
        return new AnalysisPipelineServices
        {
            FeatureExtractor = new PartFeatureExtractor(options),
            GraphBuilder = new PartGraphBuilder(options),
            BodyCandidatePartitioner = new BodyCandidatePartitioner(options),
            StableBodyZoneResolver = new StableBodyZoneResolver(options),
            SectionStationPlanner = new SectionStationPlanner(options),
            SectionTraceExtractor = new SectionTraceExtractor(options),
            SectionTraceCleaner = new SectionTraceCleaner(options),
            SectionTopologyAnalyzer = new SectionTopologyAnalyzer(options),
            CoreBodyProofEngine = new CoreBodyProofEngine(options)
        };
    }
}
