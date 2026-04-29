using System;

namespace TeklaBodyBracketRecognition.App;

internal sealed record BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleCommandResult(
    int ExitCode,
    string OutputDirectory,
    string SummaryMarkdownPath,
    string ReadmePath,
    string ManifestPath,
    string ValidationOutputDirectory,
    string ValidationJsonPath,
    string ValidationMarkdownPath,
    bool ValidationSucceeded)
    : BodyCandidatePartitionConservativeFixtureStage2TopLevelCommandResult(
        ExitCode,
        OutputDirectory,
        SummaryMarkdownPath,
        ReadmePath,
        ManifestPath)
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleCommandResult Failure(
        string outputDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory must not be empty.", nameof(outputDirectory));
        }

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleCommandResult(
            ExitCode: 1,
            OutputDirectory: outputDirectory,
            SummaryMarkdownPath: string.Empty,
            ReadmePath: string.Empty,
            ManifestPath: string.Empty,
            ValidationOutputDirectory: string.Empty,
            ValidationJsonPath: string.Empty,
            ValidationMarkdownPath: string.Empty,
            ValidationSucceeded: false);
    }
}
