namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionDemoExportResult
{
    public DefinitionClauseDecisionDemoWorkflowResult WorkflowResult { get; init; } = new();
    public string SnapshotJsonPath => WorkflowResult.Manifest.SnapshotJsonPath;
    public string SnapshotValidationJsonPath => WorkflowResult.Manifest.SnapshotValidationJsonPath;
    public string SnapshotValidationMarkdownPath => WorkflowResult.Manifest.SnapshotValidationMarkdownPath;
    public string SnapshotRoundTripJsonPath => WorkflowResult.Manifest.SnapshotRoundTripJsonPath;
    public string SnapshotRoundTripMarkdownPath => WorkflowResult.Manifest.SnapshotRoundTripMarkdownPath;
    public string SummaryJsonPath => WorkflowResult.Manifest.SummaryJsonPath;
    public string SummaryMarkdownPath => WorkflowResult.Manifest.SummaryMarkdownPath;
    public string FixtureJsonPath => WorkflowResult.Manifest.FixtureJsonPath;
    public string FixtureMarkdownPath => WorkflowResult.Manifest.FixtureMarkdownPath;
    public string BridgeValidationBundleOutputDirectory => WorkflowResult.Manifest.BridgeValidationBundleOutputDirectory;
    public string BridgeValidationBundleManifestPath => WorkflowResult.Manifest.BridgeValidationBundleManifestPath;
    public string BridgeValidationBundleReadmePath => WorkflowResult.Manifest.BridgeValidationBundleReadmePath;
    public string BridgeValidationBundleSummaryMarkdownPath => WorkflowResult.Manifest.BridgeValidationBundleSummaryMarkdownPath;
    public string BridgeValidationBundleMapperFixtureJsonPath => WorkflowResult.Manifest.BridgeValidationBundleMapperFixtureJsonPath;
    public string BridgeValidationBundleMapperFixtureMarkdownPath => WorkflowResult.Manifest.BridgeValidationBundleMapperFixtureMarkdownPath;
    public string BridgeValidationBundleEffectAdapterFixtureJsonPath => WorkflowResult.Manifest.BridgeValidationBundleEffectAdapterFixtureJsonPath;
    public string BridgeValidationBundleEffectAdapterFixtureMarkdownPath => WorkflowResult.Manifest.BridgeValidationBundleEffectAdapterFixtureMarkdownPath;
    public string ManifestJsonPath { get; init; } = string.Empty;
    public string ReadmePath { get; init; } = string.Empty;
    public string ValidationJsonPath => WorkflowResult.Manifest.ValidationJsonPath;
    public string ValidationMarkdownPath { get; init; } = string.Empty;
    public DefinitionClauseDecisionDemoOutputValidationResult ValidationResult { get; init; } = new();
}

public static class DefinitionClauseDecisionDemoExportService
{
    public static DefinitionClauseDecisionDemoExportResult Export(string outputDirectory)
    {
        var workflowResult = DefinitionClauseDecisionDemoWorkflowRunner.Run(outputDirectory);
        var manifestJsonPath = DefinitionClauseDecisionDemoManifestSerializer.Write(outputDirectory, workflowResult.Manifest);
        var readmePath = DefinitionClauseDecisionDemoOutputReadmeBuilder.Write(outputDirectory, workflowResult.Manifest);
        var provisionalResult = new DefinitionClauseDecisionDemoExportResult
        {
            WorkflowResult = workflowResult,
            ManifestJsonPath = manifestJsonPath,
            ReadmePath = readmePath
        };
        var validationResult = DefinitionClauseDecisionDemoOutputValidator.Validate(provisionalResult);
        DefinitionClauseDecisionDemoOutputValidationSerializer.WriteJson(outputDirectory, validationResult);
        var validationMarkdownPath = DefinitionClauseDecisionDemoOutputValidationSerializer.WriteMarkdown(outputDirectory, validationResult);

        return new DefinitionClauseDecisionDemoExportResult
        {
            WorkflowResult = workflowResult,
            ManifestJsonPath = manifestJsonPath,
            ReadmePath = readmePath,
            ValidationMarkdownPath = validationMarkdownPath,
            ValidationResult = validationResult
        };
    }
}
