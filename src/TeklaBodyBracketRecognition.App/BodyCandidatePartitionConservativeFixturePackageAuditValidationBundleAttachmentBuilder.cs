namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachmentBuilder
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachment Run(
        string outputRootDirectory)
    {
        var commandResult = BodyCandidatePartitionConservativeFixturePackageAuditExportService.Export(outputRootDirectory);
        return Build(commandResult);
    }

    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachment Build(
        BodyCandidatePartitionConservativeFixturePackageAuditCommandResult commandResult)
    {
        var section = BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSectionBuilder.Build(commandResult);
        var summaryMarkdownBlock =
            BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSectionMarkdownBuilder.BuildSummaryBlock(section);

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAttachment
        {
            CommandResult = commandResult,
            Section = section,
            SummaryMarkdownBlock = summaryMarkdownBlock
        };
    }
}
