using System;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditValidationExportService
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandResult Export(
        string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
        {
            throw new ArgumentException("Output root directory must not be empty.", nameof(outputRootDirectory));
        }

        var workflowResult = BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflow.Run(outputRootDirectory);
        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandResult(
            ExitCode: workflowResult.ExitCode,
            OutputDirectory: workflowResult.OutputDirectory,
            ValidationJsonPath: workflowResult.ValidationJsonPath,
            ValidationMarkdownPath: workflowResult.ValidationMarkdownPath,
            ValidationSucceeded: workflowResult.IsSuccess);
    }
}
