namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureExportService
{
    public static BodyCandidatePartitionConservativeFixtureCommandResult Export(
        string outputRootDirectory)
    {
        var result = BodyCandidatePartitionConservativeFixturePackageWorkflow.RunDefault(outputRootDirectory);
        var exitCode = result.FailedCount == 0 ? 0 : 1;

        return new BodyCandidatePartitionConservativeFixtureCommandResult(
            exitCode,
            result.OutputDirectory,
            result.JsonPath,
            result.MarkdownPath,
            result.ManifestPath,
            result.ReadmePath,
            result.TotalCount,
            result.PassedCount,
            result.FailedCount);
    }
}
