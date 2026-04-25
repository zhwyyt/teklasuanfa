namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowResult
{
    public string OutputDirectory { get; set; } = string.Empty;

    public string ManifestPath { get; set; } = string.Empty;

    public string ReadmePath { get; set; } = string.Empty;

    public string SummaryMarkdownPath { get; set; } = string.Empty;

    public DefinitionClauseDecisionBridgeMapperFixtureExportResult MapperFixtureExport { get; set; } = new();

    public DefinitionClauseDecisionBridgeEffectAdapterFixtureExportResult EffectAdapterFixtureExport { get; set; } = new();
}
