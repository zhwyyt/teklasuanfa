namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationContributionBuilder
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationContribution Run(
        string outputRootDirectory)
    {
        var attachment = BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachmentBuilder.Run(outputRootDirectory);
        return Build(attachment);
    }

    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationContribution Build(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachment attachment)
    {
        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationContribution
        {
            Attachment = attachment,
            SummaryMarkdownBlock = attachment.SummaryMarkdownBlock,
            ReadmeMarkdownBlock = attachment.SummaryMarkdownBlock
        };
    }

    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationContribution Build(
        BodyCandidatePartitionConservativeFixturePackageAuditCommandResult commandResult)
    {
        var attachment =
            BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachmentBuilder.Build(commandResult);
        return Build(attachment);
    }
}
