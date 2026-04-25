namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionBridgeValidationBundleWorkflowSemanticRegressionPostMergeResult
{
    public required string OutputDirectory { get; init; }

    public required string SummaryPath { get; init; }

    public required string ReadmePath { get; init; }

    public required string ManifestPath { get; init; }

    public required DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeResult SemanticRegressionMerge { get; init; }
}
