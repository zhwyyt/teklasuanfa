using System;
using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureStage2AggregateAppEarlyCommandDispatcherHook
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

        if (BodyCandidatePartitionConservativeFixtureStage2AggregateCommandHandler.TryHandle(
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
        BodyCandidatePartitionConservativeFixtureStage2AggregateCommandResult result)
    {
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteHeader(
            "Stage-2 aggregate completed.");
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
            "PackageOutputDirectory",
            result.PackageOutputDirectory);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationJsonPath",
            result.ValidationJsonPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationMarkdownPath",
            result.ValidationMarkdownPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "BundleOutputDirectory",
            result.BundleOutputDirectory);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "BundleSummaryMarkdownPath",
            result.BundleSummaryMarkdownPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "BundleReadmePath",
            result.BundleReadmePath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "BundleManifestPath",
            result.BundleManifestPath);
        BodyCandidatePartitionConservativeFixtureDispatcherOutputFormatter.WriteValue(
            "ValidationSucceeded",
            result.ValidationSucceeded);
    }
}
