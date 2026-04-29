using System.IO;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionDemoAppEntry
{
    public static bool TryRun(
        string[]? args,
        TextWriter standardOutput,
        TextWriter standardError,
        out int exitCode)
    {
        var result = DefinitionClauseDecisionDemoCommandHandler.TryHandle(args);
        if (!result.Handled)
        {
            exitCode = 0;
            return false;
        }

        if (!result.Succeeded)
        {
            standardError.WriteLine(result.Message);
            exitCode = 2;
            return true;
        }

        standardOutput.WriteLine(result.Message);
        standardOutput.WriteLine($"OutputDirectory: {result.OutputDirectory}");
        standardOutput.WriteLine($"SnapshotJsonPath: {result.SnapshotJsonPath}");
        standardOutput.WriteLine($"SnapshotValidationJsonPath: {result.SnapshotValidationJsonPath}");
        standardOutput.WriteLine($"SnapshotValidationMarkdownPath: {result.SnapshotValidationMarkdownPath}");
        standardOutput.WriteLine($"SnapshotRoundTripJsonPath: {result.SnapshotRoundTripJsonPath}");
        standardOutput.WriteLine($"SnapshotRoundTripMarkdownPath: {result.SnapshotRoundTripMarkdownPath}");
        standardOutput.WriteLine($"SummaryJsonPath: {result.SummaryJsonPath}");
        standardOutput.WriteLine($"SummaryMarkdownPath: {result.SummaryMarkdownPath}");
        standardOutput.WriteLine($"FixtureJsonPath: {result.FixtureJsonPath}");
        standardOutput.WriteLine($"FixtureMarkdownPath: {result.FixtureMarkdownPath}");
        standardOutput.WriteLine($"BridgeValidationBundleOutputDirectory: {result.BridgeValidationBundleOutputDirectory}");
        standardOutput.WriteLine($"BridgeValidationBundleManifestPath: {result.BridgeValidationBundleManifestPath}");
        standardOutput.WriteLine($"BridgeValidationBundleReadmePath: {result.BridgeValidationBundleReadmePath}");
        standardOutput.WriteLine($"BridgeValidationBundleSummaryMarkdownPath: {result.BridgeValidationBundleSummaryMarkdownPath}");
        standardOutput.WriteLine($"BridgeValidationBundleMapperFixtureJsonPath: {result.BridgeValidationBundleMapperFixtureJsonPath}");
        standardOutput.WriteLine($"BridgeValidationBundleMapperFixtureMarkdownPath: {result.BridgeValidationBundleMapperFixtureMarkdownPath}");
        standardOutput.WriteLine($"BridgeValidationBundleEffectAdapterFixtureJsonPath: {result.BridgeValidationBundleEffectAdapterFixtureJsonPath}");
        standardOutput.WriteLine($"BridgeValidationBundleEffectAdapterFixtureMarkdownPath: {result.BridgeValidationBundleEffectAdapterFixtureMarkdownPath}");
        standardOutput.WriteLine($"ManifestJsonPath: {result.ManifestJsonPath}");
        standardOutput.WriteLine($"ReadmePath: {result.ReadmePath}");
        standardOutput.WriteLine($"ValidationJsonPath: {result.ValidationJsonPath}");
        standardOutput.WriteLine($"ValidationMarkdownPath: {result.ValidationMarkdownPath}");

        exitCode = 0;
        return true;
    }
}
