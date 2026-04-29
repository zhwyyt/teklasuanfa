namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionBuilder
{
    public static DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContribution Build(string outputRootDirectory)
    {
        var attachment = DefinitionClauseDecisionSemanticRegressionBundleAttachmentBuilder.Run(outputRootDirectory);

        return new DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContribution
        {
            Attachment = attachment,
            SummaryMarkdownBlock = attachment.SummaryMarkdownBlock,
            ReadmeMarkdownBlock = attachment.SummaryMarkdownBlock
        };
    }
}
