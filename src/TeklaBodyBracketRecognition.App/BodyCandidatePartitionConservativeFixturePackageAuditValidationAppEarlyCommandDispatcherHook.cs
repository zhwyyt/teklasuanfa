using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditValidationAppEarlyCommandDispatcherHook
{
    public static bool TryRun(
        IReadOnlyList<string> args,
        string defaultOutputRootDirectory,
        out int exitCode)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (string.IsNullOrWhiteSpace(defaultOutputRootDirectory))
        {
            throw new ArgumentException("Default output root directory must not be empty.", nameof(defaultOutputRootDirectory));
        }

        if (BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandHandler.TryHandle(
            args,
            defaultOutputRootDirectory,
            out var result))
        {
            WriteUserFacingOutput(result);
            exitCode = result.ExitCode;
            return true;
        }

        exitCode = default;
        return false;
    }

    private static void WriteUserFacingOutput(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationCommandResult result)
    {
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteHeader(
            "Stage-2 package-audit validation completed.");
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "OutputDirectory",
            result.OutputDirectory);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalValue(
            "ValidationJsonPath",
            result.ValidationJsonPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalValue(
            "ValidationMarkdownPath",
            result.ValidationMarkdownPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationSucceeded",
            result.ValidationSucceeded);
    }
}
