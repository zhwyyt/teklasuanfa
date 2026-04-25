namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeSmokeCommandResult
{
    public required bool Handled { get; init; }

    public required int ExitCode { get; init; }

    public string? OutputDirectory { get; init; }

    public string? ManifestPath { get; init; }

    public string? ReadmePath { get; init; }

    public string? Message { get; init; }

    public bool? ValidationSucceeded { get; init; }

    public string? ValidationJsonPath { get; init; }

    public string? ValidationMarkdownPath { get; init; }
}
