using System.IO;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoOutputValidator
{
    public static DefinitionClauseDecisionDemoOutputValidationResult Validate(
        DefinitionClauseDecisionDemoExportResult result)
    {
        var snapshotJsonExists = File.Exists(result.SnapshotJsonPath);
        var snapshotRoundTripJsonExists = File.Exists(result.SnapshotRoundTripJsonPath);
        var snapshotRoundTripMarkdownExists = File.Exists(result.SnapshotRoundTripMarkdownPath);
        var summaryJsonExists = File.Exists(result.SummaryJsonPath);
        var summaryMarkdownExists = File.Exists(result.SummaryMarkdownPath);
        var fixtureJsonExists = File.Exists(result.FixtureJsonPath);
        var fixtureMarkdownExists = File.Exists(result.FixtureMarkdownPath);
        var bridgeValidationBundleManifestExists = File.Exists(result.BridgeValidationBundleManifestPath);
        var bridgeValidationBundleReadmeExists = File.Exists(result.BridgeValidationBundleReadmePath);
        var bridgeValidationBundleSummaryMarkdownExists = File.Exists(result.BridgeValidationBundleSummaryMarkdownPath);
        var bridgeValidationBundleMapperFixtureJsonExists = File.Exists(result.BridgeValidationBundleMapperFixtureJsonPath);
        var bridgeValidationBundleMapperFixtureMarkdownExists = File.Exists(result.BridgeValidationBundleMapperFixtureMarkdownPath);
        var bridgeValidationBundleEffectAdapterFixtureJsonExists = File.Exists(result.BridgeValidationBundleEffectAdapterFixtureJsonPath);
        var bridgeValidationBundleEffectAdapterFixtureMarkdownExists = File.Exists(result.BridgeValidationBundleEffectAdapterFixtureMarkdownPath);
        var manifestJsonExists = File.Exists(result.ManifestJsonPath);
        var readmeExists = File.Exists(result.ReadmePath);

        return new DefinitionClauseDecisionDemoOutputValidationResult
        {
            SnapshotJsonExists = snapshotJsonExists,
            SnapshotRoundTripJsonExists = snapshotRoundTripJsonExists,
            SnapshotRoundTripMarkdownExists = snapshotRoundTripMarkdownExists,
            SummaryJsonExists = summaryJsonExists,
            SummaryMarkdownExists = summaryMarkdownExists,
            FixtureJsonExists = fixtureJsonExists,
            FixtureMarkdownExists = fixtureMarkdownExists,
            BridgeValidationBundleManifestExists = bridgeValidationBundleManifestExists,
            BridgeValidationBundleReadmeExists = bridgeValidationBundleReadmeExists,
            BridgeValidationBundleSummaryMarkdownExists = bridgeValidationBundleSummaryMarkdownExists,
            BridgeValidationBundleMapperFixtureJsonExists = bridgeValidationBundleMapperFixtureJsonExists,
            BridgeValidationBundleMapperFixtureMarkdownExists = bridgeValidationBundleMapperFixtureMarkdownExists,
            BridgeValidationBundleEffectAdapterFixtureJsonExists = bridgeValidationBundleEffectAdapterFixtureJsonExists,
            BridgeValidationBundleEffectAdapterFixtureMarkdownExists = bridgeValidationBundleEffectAdapterFixtureMarkdownExists,
            ManifestJsonExists = manifestJsonExists,
            ReadmeExists = readmeExists,
            AllExpectedFilesExist =
                snapshotJsonExists &&
                snapshotRoundTripJsonExists &&
                snapshotRoundTripMarkdownExists &&
                summaryJsonExists &&
                summaryMarkdownExists &&
                fixtureJsonExists &&
                fixtureMarkdownExists &&
                bridgeValidationBundleManifestExists &&
                bridgeValidationBundleReadmeExists &&
                bridgeValidationBundleSummaryMarkdownExists &&
                bridgeValidationBundleMapperFixtureJsonExists &&
                bridgeValidationBundleMapperFixtureMarkdownExists &&
                bridgeValidationBundleEffectAdapterFixtureJsonExists &&
                bridgeValidationBundleEffectAdapterFixtureMarkdownExists &&
                manifestJsonExists &&
                readmeExists
        };
    }
}
