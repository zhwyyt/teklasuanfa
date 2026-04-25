namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionSemanticRegressionBundleSectionBuilder
{
    public static DefinitionClauseDecisionSemanticRegressionBundleSection Build(
        DefinitionClauseDecisionSemanticRegressionWorkflowResult workflowResult)
    {
        return new DefinitionClauseDecisionSemanticRegressionBundleSection
        {
            Key = DefinitionClauseDecisionSemanticRegressionBundleSection.SectionKey,
            OutputDirectory = workflowResult.OutputDirectory,
            JsonPath = workflowResult.JsonPath,
            MarkdownPath = workflowResult.MarkdownPath,
            ManifestPath = workflowResult.ManifestPath,
            ReadmePath = workflowResult.ReadmePath,
            RowCount = workflowResult.RowCount
        };
    }
}
