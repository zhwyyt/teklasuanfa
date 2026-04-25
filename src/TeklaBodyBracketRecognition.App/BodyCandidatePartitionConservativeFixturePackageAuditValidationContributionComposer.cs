using System.Text;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationContributionComposer
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleManifestEntry BuildManifestEntry(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationContribution contribution)
    {
        var section = contribution.Attachment.Section;

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleManifestEntry
        {
            SectionKey = section.Key,
            OutputDirectory = section.OutputDirectory,
            ValidationJsonPath = section.ValidationJsonPath,
            ValidationMarkdownPath = section.ValidationMarkdownPath,
            ValidationSucceeded = section.ValidationSucceeded,
            PackageIsComplete = section.PackageIsComplete,
            TotalCount = section.TotalCount,
            PassedCount = section.PassedCount,
            FailedCount = section.FailedCount
        };
    }

    public static string AppendSummaryMarkdown(
        string existingMarkdown,
        BodyCandidatePartitionConservativeFixturePackageAuditValidationContribution contribution)
    {
        return AppendMarkdownBlock(existingMarkdown, contribution.SummaryMarkdownBlock);
    }

    public static string AppendReadmeMarkdown(
        string existingMarkdown,
        BodyCandidatePartitionConservativeFixturePackageAuditValidationContribution contribution)
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
