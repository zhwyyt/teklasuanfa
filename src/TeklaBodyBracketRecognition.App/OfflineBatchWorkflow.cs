using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class OfflineBatchWorkflow
{
    public static void ProcessJob(
        ImportedAssemblyJob job,
        AnalysisPipelineServices services,
        BatchArtifacts artifacts,
        TextWriter log)
    {
        var recognitionProofView = RunProofPipeline(job, "recognition_input", job.Input, services);
        artifacts.RecognitionInputPartitions.Add(recognitionProofView.PartitionView);
        artifacts.RecognitionInputStableZones.Add(recognitionProofView.StableZoneView);
        artifacts.RecognitionInputSectionStations.Add(recognitionProofView.SectionStationView);
        artifacts.RecognitionInputSectionTraces.Add(recognitionProofView.SectionTraceView);
        artifacts.RecognitionInputTraceCleaning.Add(recognitionProofView.TraceCleaningView);
        artifacts.RecognitionInputTopology.Add(recognitionProofView.TopologyView);
        artifacts.RecognitionInputCoreBodyProof.Add(recognitionProofView.ProofView);

        var realProofView = RunProofPipeline(job, "real_input", job.RealInput, services);
        artifacts.RealInputPartitions.Add(realProofView.PartitionView);
        artifacts.RealInputStableZones.Add(realProofView.StableZoneView);
        artifacts.RealInputSectionStations.Add(realProofView.SectionStationView);
        artifacts.RealInputSectionTraces.Add(realProofView.SectionTraceView);
        artifacts.RealInputTraceCleaning.Add(realProofView.TraceCleaningView);
        artifacts.RealInputTopology.Add(realProofView.TopologyView);
        artifacts.RealInputCoreBodyProof.Add(realProofView.ProofView);

        var bodyMaterialSummary = BodyMaterialSupport.BuildBodyMaterialSummary(job, realProofView.ProofView);
        artifacts.BodyMaterialSummaries.Add(bodyMaterialSummary);
        artifacts.BodyMaterialExplanations.Add(BodyMaterialSupport.BuildBodyMaterialExplanation(job, bodyMaterialSummary, realProofView.ProofView));
        artifacts.BodyMaterialPartRows.AddRange(BodyMaterialSupport.BuildBodyMaterialPartRows(bodyMaterialSummary));
        artifacts.BodyMaterialSourceSeedRows.AddRange(BodyMaterialSupport.BuildBodyMaterialSourceSeedRows(bodyMaterialSummary));

        log.WriteLine(
            $"[{job.Input.AssemblyId}] core={realProofView.ProofView.Result.CoreBodyPartIds.Count} " +
            $"accessory={realProofView.ProofView.Result.BodyAccessoryPartIds.Count} " +
            $"review={realProofView.ProofView.Result.ReviewPartIds.Count} " +
            $"synthetic={job.SynthesizedBody}");
    }

    public static PipelineRunResult RunProofPipeline(
        ImportedAssemblyJob job,
        string viewKind,
        AssemblyInput input,
        AnalysisPipelineServices services)
    {
        var pipelineResult = RunProofPipeline(
            input.AssemblyId,
            input.MainPartId,
            input.Parts,
            input.Relationships,
            input.ImportedLongitudinalAxisSegments,
            services);

        return new PipelineRunResult
        {
            PartitionView = PipelineArtifactSupport.BuildBodyCandidatePartitionViewOutput(job, viewKind, pipelineResult.Partition),
            StableZoneView = PipelineArtifactSupport.BuildStableBodyZoneViewOutput(job, viewKind, pipelineResult.StableZones),
            SectionStationView = PipelineArtifactSupport.BuildSectionStationViewOutput(job, viewKind, pipelineResult.StationPlan),
            SectionTraceView = PipelineArtifactSupport.BuildSectionTraceViewOutput(job, viewKind, pipelineResult.TraceResult),
            TraceCleaningView = PipelineArtifactSupport.BuildSectionTraceCleaningViewOutput(job, viewKind, pipelineResult.TraceCleaningResult),
            TopologyView = PipelineArtifactSupport.BuildSectionTopologyViewOutput(job, viewKind, pipelineResult.TopologyResult),
            ProofView = PipelineArtifactSupport.BuildCoreBodyProofViewOutput(job, viewKind, pipelineResult.ProofResult)
        };
    }

    public static ProofPipelineResult RunProofPipeline(
        string assemblyId,
        int inputMainPartId,
        IReadOnlyList<PartInput> parts,
        IReadOnlyList<RelationshipInput> relationships,
        IReadOnlyList<LongitudinalAxisSegment>? importedLongitudinalAxisSegments,
        AnalysisPipelineServices services)
    {
        var features = services.FeatureExtractor.Extract(parts);
        var graph = services.GraphBuilder.Build(features, relationships);
        var partition = services.BodyCandidatePartitioner.Partition(assemblyId, graph, inputMainPartId, importedLongitudinalAxisSegments);
        var stableZones = services.StableBodyZoneResolver.Resolve(partition);
        var stationPlan = services.SectionStationPlanner.Plan(stableZones);
        var traceResult = services.SectionTraceExtractor.Extract(partition, stationPlan, graph);
        var traceCleaningResult = services.SectionTraceCleaner.Clean(traceResult);
        var topologyResult = services.SectionTopologyAnalyzer.Analyze(traceCleaningResult);
        var proofResult = services.CoreBodyProofEngine.Prove(partition, traceCleaningResult, topologyResult);

        return new ProofPipelineResult
        {
            Partition = partition,
            StableZones = stableZones,
            StationPlan = stationPlan,
            TraceResult = traceResult,
            TraceCleaningResult = traceCleaningResult,
            TopologyResult = topologyResult,
            ProofResult = proofResult
        };
    }
}
