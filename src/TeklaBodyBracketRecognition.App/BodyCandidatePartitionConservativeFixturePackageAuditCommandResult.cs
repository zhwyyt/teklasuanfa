using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public sealed record BodyCandidatePartitionConservativeFixturePackageAuditCommandResult(
    int ExitCode,
    string OutputDirectory,
    string JsonPath,
    string MarkdownPath,
    string ManifestPath,
    string ReadmePath,
    string ValidationJsonPath,
    string ValidationMarkdownPath,
    bool ValidationSucceeded,
    int TotalCount,
    int PassedCount,
    int FailedCount,
    bool PackageIsComplete,
    IReadOnlyList<string> MissingFiles)
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditCommandResult Failure(
        string outputDirectory,
        string missingFile)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory must not be empty.", nameof(outputDirectory));
        }

        if (string.IsNullOrWhiteSpace(missingFile))
        {
            throw new ArgumentException("Missing file must not be empty.", nameof(missingFile));
        }

        return new BodyCandidatePartitionConservativeFixturePackageAuditCommandResult(
            ExitCode: 1,
            OutputDirectory: outputDirectory,
            JsonPath: string.Empty,
            MarkdownPath: string.Empty,
            ManifestPath: string.Empty,
            ReadmePath: string.Empty,
            ValidationJsonPath: string.Empty,
            ValidationMarkdownPath: string.Empty,
            ValidationSucceeded: false,
            TotalCount: 0,
            PassedCount: 0,
            FailedCount: 1,
            PackageIsComplete: false,
            MissingFiles: new[] { missingFile });
    }
}
