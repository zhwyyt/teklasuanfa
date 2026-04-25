using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeApplicator
{
    public static DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMergeResult Apply(
        string outputRootDirectory,
        ref string summaryMarkdown,
        ref string readmeMarkdown,
        ICollection<DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionManifestEntry> manifestEntries)
    {
        var result = DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionMerger.Merge(
            outputRootDirectory,
            summaryMarkdown,
            readmeMarkdown);

        summaryMarkdown = result.SummaryMarkdown;
        readmeMarkdown = result.ReadmeMarkdown;
        manifestEntries.Add(result.ManifestEntry);

        return result;
    }
}
