using System;

namespace TeklaBodyBracketRecognition.App;

public sealed record BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandResult(
    int ExitCode,
    string OutputDirectory,
    string ValidationJsonPath,
    string ValidationMarkdownPath,
    bool ValidationSucceeded)
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandResult Failure(
        string outputDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory must not be empty.", nameof(outputDirectory));
        }

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandResult(
            ExitCode: 1,
            OutputDirectory: outputDirectory,
            ValidationJsonPath: string.Empty,
            ValidationMarkdownPath: string.Empty,
            ValidationSucceeded: false);
    }
}
