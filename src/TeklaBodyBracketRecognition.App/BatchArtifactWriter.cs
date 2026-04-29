using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal sealed class BatchArtifacts
{
    public List<BodyMaterialSummary> BodyMaterialSummaries { get; } = new();

    public List<BodyMaterialExplanation> BodyMaterialExplanations { get; } = new();

    public List<BodyCandidatePartitionViewOutput> RecognitionInputPartitions { get; } = new();

    public List<BodyCandidatePartitionViewOutput> RealInputPartitions { get; } = new();

    public List<StableBodyZoneViewOutput> RecognitionInputStableZones { get; } = new();

    public List<StableBodyZoneViewOutput> RealInputStableZones { get; } = new();

    public List<SectionStationViewOutput> RecognitionInputSectionStations { get; } = new();

    public List<SectionStationViewOutput> RealInputSectionStations { get; } = new();

    public List<SectionTraceViewOutput> RecognitionInputSectionTraces { get; } = new();

    public List<SectionTraceViewOutput> RealInputSectionTraces { get; } = new();

    public List<SectionTraceCleaningViewOutput> RecognitionInputTraceCleaning { get; } = new();

    public List<SectionTraceCleaningViewOutput> RealInputTraceCleaning { get; } = new();

    public List<SectionTopologyViewOutput> RecognitionInputTopology { get; } = new();

    public List<SectionTopologyViewOutput> RealInputTopology { get; } = new();

    public List<CoreBodyProofViewOutput> RecognitionInputCoreBodyProof { get; } = new();

    public List<CoreBodyProofViewOutput> RealInputCoreBodyProof { get; } = new();

    public List<BodyMaterialPartRow> BodyMaterialPartRows { get; } = new();

    public List<BodyMaterialSourceSeedRow> BodyMaterialSourceSeedRows { get; } = new();
}

internal static class BatchArtifactWriter
{
    public static void Write(string outputDirectory, BatchArtifacts artifacts, JsonSerializerOptions jsonOptions)
    {
        WriteJsonArtifact(Path.Combine(outputDirectory, "body-main-material-summary.json"), artifacts.BodyMaterialSummaries, jsonOptions);
        File.WriteAllText(
            Path.Combine(outputDirectory, "body-main-material-summary.csv"),
            BodyMaterialSupport.BuildBodyMaterialSummaryCsv(artifacts.BodyMaterialSummaries));

        WriteJsonArtifact(Path.Combine(outputDirectory, "body-main-material-explanation.json"), artifacts.BodyMaterialExplanations, jsonOptions);
        File.WriteAllText(
            Path.Combine(outputDirectory, "body-main-material-parts.csv"),
            BodyMaterialSupport.BuildBodyMaterialPartCsv(artifacts.BodyMaterialPartRows));
        File.WriteAllText(
            Path.Combine(outputDirectory, "body-main-material-source-seeds.csv"),
            BodyMaterialSupport.BuildBodyMaterialSourceSeedCsv(artifacts.BodyMaterialSourceSeedRows));

        WriteJsonArtifact(Path.Combine(outputDirectory, "body-candidate-partition-recognition-input.json"), artifacts.RecognitionInputPartitions, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "body-candidate-partition-real-input.json"), artifacts.RealInputPartitions, jsonOptions);
        File.WriteAllText(
            Path.Combine(outputDirectory, "body-main-material-review-summary.zh-CN.md"),
            BodyMaterialSupport.BuildBodyMaterialReviewSummaryMarkdown(artifacts.BodyMaterialExplanations));
        File.WriteAllText(
            Path.Combine(outputDirectory, "body-candidate-partition-summary.zh-CN.md"),
            PipelineSummarySupport.BuildBodyCandidatePartitionSummaryMarkdown(artifacts.RecognitionInputPartitions, artifacts.RealInputPartitions));

        WriteJsonArtifact(Path.Combine(outputDirectory, "stable-body-zone-recognition-input.json"), artifacts.RecognitionInputStableZones, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "stable-body-zone-real-input.json"), artifacts.RealInputStableZones, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-stations-recognition-input.json"), artifacts.RecognitionInputSectionStations, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-stations-real-input.json"), artifacts.RealInputSectionStations, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-traces-recognition-input.json"), artifacts.RecognitionInputSectionTraces, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-traces-real-input.json"), artifacts.RealInputSectionTraces, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-trace-cleaning-recognition-input.json"), artifacts.RecognitionInputTraceCleaning, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-trace-cleaning-real-input.json"), artifacts.RealInputTraceCleaning, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-topology-recognition-input.json"), artifacts.RecognitionInputTopology, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-topology-real-input.json"), artifacts.RealInputTopology, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "core-body-proof-recognition-input.json"), artifacts.RecognitionInputCoreBodyProof, jsonOptions);
        WriteJsonArtifact(Path.Combine(outputDirectory, "core-body-proof-real-input.json"), artifacts.RealInputCoreBodyProof, jsonOptions);

        var sectionTopologySummary = PipelineArtifactSupport.BuildSectionTraceTopologySummaryRows(
            artifacts.RecognitionInputSectionTraces,
            artifacts.RealInputSectionTraces);
        WriteJsonArtifact(Path.Combine(outputDirectory, "section-topology-summary.json"), sectionTopologySummary, jsonOptions);

        File.WriteAllText(
            Path.Combine(outputDirectory, "stable-body-zone-summary.zh-CN.md"),
            PipelineSummarySupport.BuildStableBodyZoneSummaryMarkdown(
                artifacts.RecognitionInputStableZones,
                artifacts.RealInputStableZones,
                artifacts.RealInputSectionStations));
        File.WriteAllText(
            Path.Combine(outputDirectory, "section-trace-summary.zh-CN.md"),
            PipelineSummarySupport.BuildSectionTraceSummaryMarkdown(
                artifacts.RecognitionInputSectionTraces,
                artifacts.RealInputSectionTraces,
                sectionTopologySummary));
        File.WriteAllText(
            Path.Combine(outputDirectory, "section-topology-analysis-summary.zh-CN.md"),
            PipelineSummarySupport.BuildSectionTopologyAnalysisSummaryMarkdown(
                artifacts.RecognitionInputTopology,
                artifacts.RealInputTopology));
        File.WriteAllText(
            Path.Combine(outputDirectory, "core-body-proof-summary.zh-CN.md"),
            CoreBodyProofSummarySupport.BuildCoreBodyProofSummaryMarkdown(
                artifacts.RecognitionInputCoreBodyProof,
                artifacts.RealInputCoreBodyProof));
        File.WriteAllText(
            Path.Combine(outputDirectory, "topology-rewrite-summary.zh-CN.md"),
            CoreBodyProofSummarySupport.BuildTopologyRewriteSummaryMarkdown(artifacts.RealInputCoreBodyProof));
    }

    private static void WriteJsonArtifact<T>(string path, T value, JsonSerializerOptions jsonOptions)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, jsonOptions));
    }
}
