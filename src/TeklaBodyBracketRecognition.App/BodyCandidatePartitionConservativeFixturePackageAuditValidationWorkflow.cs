using System;
using System.IO;
using System.Text;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflow
{
    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflowResult Run(
        string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
        {
            throw new ArgumentException("Output root directory must not be empty.", nameof(outputRootDirectory));
        }

        var commandResult = BodyCandidatePartitionConservativeFixturePackageAuditExportService.ExportPackageAuditOnly(outputRootDirectory);
        return Run(commandResult);
    }

    public static BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflowResult Run(
        BodyCandidatePartitionConservativeFixturePackageAuditCommandResult commandResult)
    {
        if (commandResult is null)
        {
            throw new ArgumentNullException(nameof(commandResult));
        }

        var validationResult = BodyCandidatePartitionConservativeFixturePackageAuditValidator.Validate(commandResult);
        var artifact = BodyCandidatePartitionConservativeFixturePackageAuditValidationArtifactBuilder.Build(validationResult);
        var outputPaths = BodyCandidatePartitionConservativeFixturePackageAuditValidationOutputPathResolver.Resolve(
            validationResult.OutputDirectory);

        Directory.CreateDirectory(validationResult.OutputDirectory);
        File.WriteAllText(
            outputPaths.ValidationJsonPath,
            BodyCandidatePartitionConservativeFixturePackageAuditValidationSerializer.SerializeJson(artifact),
            Encoding.UTF8);
        File.WriteAllText(
            outputPaths.ValidationMarkdownPath,
            BodyCandidatePartitionConservativeFixturePackageAuditValidationSerializer.SerializeMarkdown(artifact),
            Encoding.UTF8);

        return new BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflowResult(
            OutputDirectory: validationResult.OutputDirectory,
            ValidationJsonPath: outputPaths.ValidationJsonPath,
            ValidationMarkdownPath: outputPaths.ValidationMarkdownPath,
            ExitCode: validationResult.ExitCode,
            IsSuccess: validationResult.IsSuccess);
    }
}
