using System;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditValidationArtifactBuilder
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationArtifact Build(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationResult validationResult)
    {
        if (validationResult is null)
        {
            throw new ArgumentNullException(nameof(validationResult));
        }

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationArtifact(
            OutputDirectory: validationResult.OutputDirectory,
            ExitCode: validationResult.ExitCode,
            IsSuccess: validationResult.IsSuccess,
            TotalCount: validationResult.TotalCount,
            PassedCount: validationResult.PassedCount,
            FailedCount: validationResult.FailedCount,
            PackageIsComplete: validationResult.PackageIsComplete,
            MissingFiles: validationResult.MissingFiles,
            Issues: validationResult.Issues,
            GeneratedAtUtc: DateTimeOffset.UtcNow);
    }
}
