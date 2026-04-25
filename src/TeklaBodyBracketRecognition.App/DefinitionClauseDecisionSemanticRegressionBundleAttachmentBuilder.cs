namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionSemanticRegressionBundleAttachmentBuilder
{
    public static DefinitionClauseDecisionSemanticRegressionBundleAttachment Run(string outputRootDirectory)
    {
        var workflowResult = DefinitionClauseDecisionSemanticRegressionWorkflow.Run(outputRootDirectory);
        var section = DefinitionClauseDecisionSemanticRegressionBundleSectionBuilder.Build(workflowResult);
        var summaryMarkdownBlock = DefinitionClauseDecisionSemanticRegressionBundleSectionMarkdownBuilder.BuildSummaryBlock(section);

        return new DefinitionClauseDecisionSemanticRegressionBundleAttachment
        {
            WorkflowResult = workflowResult,
            Section = section,
            SummaryMarkdownBlock = summaryMarkdownBlock
        };
    }
}
