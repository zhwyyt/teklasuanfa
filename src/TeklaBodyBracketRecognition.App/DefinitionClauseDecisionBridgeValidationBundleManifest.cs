using System;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionBridgeValidationBundleManifest
{
    public DateTime GeneratedAtUtc { get; set; }

    public string OutputDirectory { get; set; } = string.Empty;

    public string MapperFixtureJsonPath { get; set; } = string.Empty;

    public string MapperFixtureMarkdownPath { get; set; } = string.Empty;

    public string EffectAdapterFixtureJsonPath { get; set; } = string.Empty;

    public string EffectAdapterFixtureMarkdownPath { get; set; } = string.Empty;

    public string SummaryMarkdownPath { get; set; } = string.Empty;

    public string ReadmePath { get; set; } = string.Empty;
}
