namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSectionBuilder
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSection Build(
        BodyCandidatePartitionConservativeFixturePackageAuditCommandResult commandResult)
    {
        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSection
        {
            Key = BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleSection.SectionKey,
            OutputDirectory = commandResult.OutputDirectory,
            ValidationJsonPath = commandResult.ValidationJsonPath,
            ValidationMarkdownPath = commandResult.ValidationMarkdownPath,
            ValidationSucceeded = commandResult.ValidationSucceeded,
            PackageIsComplete = commandResult.PackageIsComplete,
            TotalCount = commandResult.TotalCount,
            PassedCount = commandResult.PassedCount,
            FailedCount = commandResult.FailedCount
        };
    }
}
