using System;
using System.IO;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditValidationOutputPathResolver
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationOutputPaths Resolve(
        string outputDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory must not be empty.", nameof(outputDirectory));
        }

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationOutputPaths(
            ValidationJsonPath: Path.Combine(
                outputDirectory,
                BodyCandidatePartitionConservativeFixturePackageAuditValidationOutputContract.ValidationJsonFileName),
            ValidationMarkdownPath: Path.Combine(
                outputDirectory,
                BodyCandidatePartitionConservativeFixturePackageAuditValidationOutputContract.ValidationMarkdownFileName));
    }
}
