using System.IO;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionSidecarManifest
{
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
    public string ValidationJsonPath { get; init; } = string.Empty;
    public string ValidationMarkdownPath { get; init; } = string.Empty;
}

public static class DefinitionClauseDecisionSidecarManifestBuilder
{
    public static DefinitionClauseDecisionSidecarManifest Build(string outputDirectory)
    {
        return new DefinitionClauseDecisionSidecarManifest
        {
            OutputDirectory = outputDirectory,
            SnapshotJsonPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSnapshotSerializer.FileName),
            SnapshotValidationJsonPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSnapshotValidationSerializer.JsonFileName),
            SnapshotValidationMarkdownPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSnapshotValidationSerializer.MarkdownFileName),
            SnapshotRoundTripJsonPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSnapshotRoundTripSerializer.JsonFileName),
            SnapshotRoundTripMarkdownPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSnapshotRoundTripSerializer.MarkdownFileName),
            SummaryJsonPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSidecarWorkflow.SummaryJsonFileName),
            SummaryMarkdownPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSidecarWorkflow.SummaryMarkdownFileName),
            FixtureJsonPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSidecarWorkflow.FixtureJsonFileName),
            FixtureMarkdownPath = Path.Combine(outputDirectory, DefinitionClauseDecisionSidecarWorkflow.FixtureMarkdownFileName),
            BridgeValidationBundleOutputDirectory = Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionSidecarWorkflow.ValidationBundleDirectoryName),
            BridgeValidationBundleManifestPath = Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionSidecarWorkflow.ValidationBundleDirectoryName,
                "definition-clause-decision-bridge-validation-bundle-manifest.json"),
            BridgeValidationBundleReadmePath = Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionSidecarWorkflow.ValidationBundleDirectoryName,
                "README.md"),
            BridgeValidationBundleSummaryMarkdownPath = Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionSidecarWorkflow.ValidationBundleDirectoryName,
                "definition-clause-decision-bridge-validation-summary.md"),
            BridgeValidationBundleMapperFixtureJsonPath = Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionSidecarWorkflow.ValidationBundleDirectoryName,
                "mapper-fixtures",
                "definition-clause-decision-bridge-mapper-fixtures.json"),
            BridgeValidationBundleMapperFixtureMarkdownPath = Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionSidecarWorkflow.ValidationBundleDirectoryName,
                "mapper-fixtures",
                "definition-clause-decision-bridge-mapper-fixtures.md"),
            BridgeValidationBundleEffectAdapterFixtureJsonPath = Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionSidecarWorkflow.ValidationBundleDirectoryName,
                "effect-adapter-fixtures",
                "definition-clause-decision-bridge-effect-adapter-fixtures.json"),
            BridgeValidationBundleEffectAdapterFixtureMarkdownPath = Path.Combine(
                outputDirectory,
                DefinitionClauseDecisionSidecarWorkflow.ValidationBundleDirectoryName,
                "effect-adapter-fixtures",
                "definition-clause-decision-bridge-effect-adapter-fixtures.md"),
            ValidationJsonPath = Path.Combine(outputDirectory, DefinitionClauseDecisionDemoOutputValidationSerializer.JsonFileName),
            ValidationMarkdownPath = Path.Combine(outputDirectory, DefinitionClauseDecisionDemoOutputValidationSerializer.MarkdownFileName)
        };
    }
}
