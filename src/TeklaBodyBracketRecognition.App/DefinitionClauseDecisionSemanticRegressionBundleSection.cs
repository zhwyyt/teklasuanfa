namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionSemanticRegressionBundleSection
{
    public const string SectionKey = "semantic-regression";

    public required string Key { get; init; }

    public required string OutputDirectory { get; init; }

    public required string JsonPath { get; init; }

    public required string MarkdownPath { get; init; }

    public required string ManifestPath { get; init; }

    public required string ReadmePath { get; init; }

    public required int RowCount { get; init; }
}
