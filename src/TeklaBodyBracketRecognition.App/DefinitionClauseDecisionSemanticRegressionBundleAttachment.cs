namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionSemanticRegressionBundleAttachment
{
    public required DefinitionClauseDecisionSemanticRegressionWorkflowResult WorkflowResult { get; init; }

    public required DefinitionClauseDecisionSemanticRegressionBundleSection Section { get; init; }

    public required string SummaryMarkdownBlock { get; init; }
}
