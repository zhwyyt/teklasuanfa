using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAppEarlyCommandDispatcherHook
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

        if (BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleCommandHandler.TryHandle(
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
        BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleCommandResult result)
    {
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteHeader(
            "Stage-2 validation bundle completed.");
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "OutputDirectory",
            result.OutputDirectory);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "SummaryMarkdownPath",
            result.SummaryMarkdownPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ReadmePath",
            result.ReadmePath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ManifestPath",
            result.ManifestPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationOutputDirectory",
            result.ValidationOutputDirectory);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationJsonPath",
            result.ValidationJsonPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationMarkdownPath",
            result.ValidationMarkdownPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationSucceeded",
            result.ValidationSucceeded);
    }
}
