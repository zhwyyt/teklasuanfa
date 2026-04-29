using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook
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

        if (BodyCandidatePartitionConservativeFixturePackageAuditCommandHandler.TryHandle(
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

    private static void WriteUserFacingOutput(BodyCandidatePartitionConservativeFixturePackageAuditCommandResult result)
    {
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteHeader(
            "Stage-2 package audit completed.");
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "OutputDirectory",
            result.OutputDirectory);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalValue(
            "JsonPath",
            result.JsonPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalValue(
            "MarkdownPath",
            result.MarkdownPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalValue(
            "ManifestPath",
            result.ManifestPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalValue(
            "ReadmePath",
            result.ReadmePath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalValue(
            "ValidationJsonPath",
            result.ValidationJsonPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalValue(
            "ValidationMarkdownPath",
            result.ValidationMarkdownPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "TotalCount",
            result.TotalCount);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "PassedCount",
            result.PassedCount);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "FailedCount",
            result.FailedCount);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "PackageIsComplete",
            result.PackageIsComplete);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationSucceeded",
            result.ValidationSucceeded);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteOptionalJoinedValues(
            "MissingFiles",
            result.MissingFiles);
    }
}
