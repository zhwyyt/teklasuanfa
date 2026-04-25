using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public sealed record BodyCandidatePartitionConservativeFixturePackageAuditValidationResult(
    string OutputDirectory,
    int ExitCode,
    bool IsSuccess,
    int TotalCount,
    int PassedCount,
    int FailedCount,
    bool PackageIsComplete,
    IReadOnlyList<string> MissingFiles,
    IReadOnlyList<string> Issues)
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationResult Create(
        string outputDirectory,
        int exitCode,
        bool isSuccess,
        int totalCount,
        int passedCount,
        int failedCount,
        bool packageIsComplete,
        IReadOnlyList<string> missingFiles,
        IReadOnlyList<string> issues)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory must not be empty.", nameof(outputDirectory));
        }

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationResult(
            OutputDirectory: outputDirectory,
            ExitCode: exitCode,
            IsSuccess: isSuccess,
            TotalCount: totalCount,
            PassedCount: passedCount,
            FailedCount: failedCount,
            PackageIsComplete: packageIsComplete,
            MissingFiles: missingFiles ?? Array.Empty<string>(),
            Issues: issues ?? Array.Empty<string>());
    }
}
