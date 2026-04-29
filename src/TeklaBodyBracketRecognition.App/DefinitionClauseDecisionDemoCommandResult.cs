namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionDemoCommandResult
{
    public bool Handled { get; init; }
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public string OutputDirectory { get; init; } = string.Empty;
    public string SnapshotJsonPath { get; init; } = string.Empty;
    public string SnapshotValidationJsonPath { get; init; } = string.Empty;
    public string SnapshotValidationMarkdownPath { get; init; } = string.Empty;
    public string SnapshotRoundTripJsonPath { get; init; } = string.Empty;
    public string SnapshotRoundTripMarkdownPath { get; init; } = string.Empty;
    public string SummaryJsonPath { get; init; } = string.Empty;
    public string SummaryMarkdownPath { get; init; } = string.Empty;
    public string FixtureJsonPath { get; init; } = string.Empty;
    public string FixtureMarkdownPath { get; init; } = string.Empty;
    public string BridgeValidationBundleOutputDirectory { get; init; } = string.Empty;
    public string BridgeValidationBundleManifestPath { get; init; } = string.Empty;
    public string BridgeValidationBundleReadmePath { get; init; } = string.Empty;
    public string BridgeValidationBundleSummaryMarkdownPath { get; init; } = string.Empty;
    public string BridgeValidationBundleMapperFixtureJsonPath { get; init; } = string.Empty;
    public string BridgeValidationBundleMapperFixtureMarkdownPath { get; init; } = string.Empty;
    public string BridgeValidationBundleEffectAdapterFixtureJsonPath { get; init; } = string.Empty;
    public string BridgeValidationBundleEffectAdapterFixtureMarkdownPath { get; init; } = string.Empty;
    public string ManifestJsonPath { get; init; } = string.Empty;
    public string ReadmePath { get; init; } = string.Empty;
    public string ValidationJsonPath { get; init; } = string.Empty;
    public string ValidationMarkdownPath { get; init; } = string.Empty;
}
