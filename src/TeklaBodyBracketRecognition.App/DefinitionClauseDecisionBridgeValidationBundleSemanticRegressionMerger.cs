namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger
{
    public static DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeResult Merge(
        string outputRootDirectory,
        string existingSummaryMarkdown,
        string existingReadmeMarkdown)
    {
        var contribution =
            DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionBuilder.Build(outputRootDirectory);
        var manifestEntry =
            DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionComposer.BuildManifestEntry(
                contribution);
        var summaryMarkdown =
            DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionComposer.AppendSummaryMarkdown(
                existingSummaryMarkdown,
                contribution);
        var readmeMarkdown =
            DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionComposer.AppendReadmeMarkdown(
                existingReadmeMarkdown,
                contribution);

        return new DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeResult
        {
            Contribution = contribution,
            ManifestEntry = manifestEntry,
            SummaryMarkdown = summaryMarkdown,
            ReadmeMarkdown = readmeMarkdown
        };
    }
}
