namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditWorkflow
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditResult RunDefault(
        string outputRootDirectory)
    {
        var packageResult = BodyCandidatePartitionConservativeFixturePackageWorkflow.RunDefault(outputRootDirectory);
        var outputAudit = BodyCandidatePartitionConservativeFixtureOutputAuditBuilder
            .BuildFromOutputDirectory(packageResult.OutputDirectory);

        var exitCode = packageResult.FailedCount == 0 && outputAudit.IsComplete ? 0 : 1;

        return new BodyCandidatePartitionConservativeFixturePackageAuditResult(
            outputDirectory: packageResult.OutputDirectory,
            jsonPath: packageResult.JsonPath,
            markdownPath: packageResult.MarkdownPath,
            manifestPath: packageResult.ManifestPath,
            readmePath: packageResult.ReadmePath,
            totalCount: packageResult.TotalCount,
            passedCount: packageResult.PassedCount,
            failedCount: packageResult.FailedCount,
            packageIsComplete: outputAudit.IsComplete,
            missingFiles: outputAudit.MissingFiles,
            exitCode: exitCode);
    }
}
