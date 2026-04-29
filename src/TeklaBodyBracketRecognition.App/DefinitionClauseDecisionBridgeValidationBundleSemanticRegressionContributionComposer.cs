using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContributionComposer
{
    public static DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionManifestEntry BuildManifestEntry(
        DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContribution contribution)
    {
        var section = contribution.Attachment.Section;

        return new DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionManifestEntry
        {
            SectionKey = section.Key,
            OutputDirectory = section.OutputDirectory,
            JsonPath = section.JsonPath,
            MarkdownPath = section.MarkdownPath,
            ManifestPath = section.ManifestPath,
            ReadmePath = section.ReadmePath,
            RowCount = section.RowCount
        };
    }

    public static string AppendSummaryMarkdown(
        string existingMarkdown,
        DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContribution contribution)
    {
        return AppendMarkdownBlock(existingMarkdown, contribution.SummaryMarkdownBlock);
    }

    public static string AppendReadmeMarkdown(
        string existingMarkdown,
        DefinitionClauseDecisionBridgeValidationBundleSemanticRegressionContribution contribution)
    {
        return AppendMarkdownBlock(existingMarkdown, contribution.ReadmeMarkdownBlock);
    }

    private static string AppendMarkdownBlock(string existingMarkdown, string block)
    {
        if (string.IsNullOrWhiteSpace(existingMarkdown))
        {
            return block.TrimEnd() + "\n";
        }

        var builder = new StringBuilder();
        builder.Append(existingMarkdown.TrimEnd());
        builder.AppendLine();
        builder.AppendLine();
        builder.Append(block.TrimEnd());
        builder.AppendLine();
        return builder.ToString();
    }
}
