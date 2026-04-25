namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionSnapshotExportResult
{
    public DefinitionClauseDecisionSnapshot Snapshot { get; init; } = new();
    public DefinitionClauseDecisionArtifacts Artifacts { get; init; } = new();
    public DefinitionClauseDecisionSidecarManifest Manifest { get; init; } = new();

    public string SnapshotJsonPath => Manifest.SnapshotJsonPath;
    public string SnapshotValidationJsonPath => Manifest.SnapshotValidationJsonPath;
    public string SnapshotValidationMarkdownPath => Manifest.SnapshotValidationMarkdownPath;
    public string SnapshotRoundTripJsonPath => Manifest.SnapshotRoundTripJsonPath;
    public string SnapshotRoundTripMarkdownPath => Manifest.SnapshotRoundTripMarkdownPath;
    public string SummaryJsonPath => Manifest.SummaryJsonPath;
    public string SummaryMarkdownPath => Manifest.SummaryMarkdownPath;
    public string FixtureJsonPath => Manifest.FixtureJsonPath;
    public string FixtureMarkdownPath => Manifest.FixtureMarkdownPath;
}

public static class DefinitionClauseDecisionSnapshotExportService
{
    public static DefinitionClauseDecisionSnapshotExportResult Export(
        DefinitionClauseDecisionSnapshot snapshot,
        string outputDirectory)
    {
        var provider = new DefinitionClauseDecisionSnapshotSourceProvider();
        var artifacts = DefinitionClauseDecisionSourcePipeline.BuildArtifacts(snapshot, provider);
        var validation = DefinitionClauseDecisionSnapshotValidator.Validate(snapshot);
        var roundTrip = DefinitionClauseDecisionSnapshotRoundTripService.Analyze(snapshot);
        var manifest = DefinitionClauseDecisionSidecarManifestBuilder.Build(outputDirectory);

        DefinitionClauseDecisionSnapshotSerializer.Write(outputDirectory, snapshot);
        DefinitionClauseDecisionSnapshotValidationSerializer.WriteJson(outputDirectory, validation);
        DefinitionClauseDecisionSnapshotValidationSerializer.WriteMarkdown(outputDirectory, validation);
        DefinitionClauseDecisionSnapshotRoundTripSerializer.WriteJson(outputDirectory, roundTrip);
        DefinitionClauseDecisionSnapshotRoundTripSerializer.WriteMarkdown(outputDirectory, roundTrip);
        DefinitionClauseDecisionSidecarWorkflow.WriteSummaryArtifacts(outputDirectory, artifacts);

        return new DefinitionClauseDecisionSnapshotExportResult
        {
            Snapshot = snapshot,
            Artifacts = artifacts,
            Manifest = manifest
        };
    }
}
