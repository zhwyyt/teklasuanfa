namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionDemoOutputValidationResult
{
    public bool SnapshotJsonExists { get; init; }
    public bool SnapshotRoundTripJsonExists { get; init; }
    public bool SnapshotRoundTripMarkdownExists { get; init; }
    public bool SummaryJsonExists { get; init; }
    public bool SummaryMarkdownExists { get; init; }
    public bool FixtureJsonExists { get; init; }
    public bool FixtureMarkdownExists { get; init; }
    public bool BridgeValidationBundleManifestExists { get; init; }
    public bool BridgeValidationBundleReadmeExists { get; init; }
    public bool BridgeValidationBundleSummaryMarkdownExists { get; init; }
    public bool BridgeValidationBundleMapperFixtureJsonExists { get; init; }
    public bool BridgeValidationBundleMapperFixtureMarkdownExists { get; init; }
    public bool BridgeValidationBundleEffectAdapterFixtureJsonExists { get; init; }
    public bool BridgeValidationBundleEffectAdapterFixtureMarkdownExists { get; init; }
    public bool ManifestJsonExists { get; init; }
    public bool ReadmeExists { get; init; }
    public bool AllExpectedFilesExist { get; init; }
}
