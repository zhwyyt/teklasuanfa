using System.IO;

namespace TeklaBodyBracketRecognition.App;

public static class AppEarlyCommandDispatcher
{
    private const string DefaultEarlyCommandOutputDirectoryName = ".tmpresults";

    public static bool TryRun(
        string[]? args,
        TextWriter standardOutput,
        TextWriter standardError,
        out int exitCode)
    {
        if (DefinitionClauseDecisionDemoAppEntry.TryRun(args, standardOutput, standardError, out exitCode))
        {
            return true;
        }

        if (BodyCandidatePartitionConservativeFixtureStage2AggregateAppEarlyCommandDispatcherHook.TryRun(
            args ?? [],
            ResolveDefaultOutputRootDirectory(),
            out exitCode))
        {
            return true;
        }

        if (BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleAppEarlyCommandDispatcherHook.TryRun(
            args ?? [],
            ResolveDefaultOutputRootDirectory(),
            out exitCode))
        {
            return true;
        }

        if (BodyCandidatePartitionConservativeFixturePackageAuditValidationAppEarlyCommandDispatcherHook.TryRun(
            args ?? [],
            ResolveDefaultOutputRootDirectory(),
            out exitCode))
        {
            return true;
        }

        if (BodyCandidatePartitionConservativeFixturePackageAuditAppEarlyCommandDispatcherHook.TryRun(
            args ?? [],
            ResolveDefaultOutputRootDirectory(),
            out exitCode))
        {
            return true;
        }

        exitCode = 0;
        return false;
    }

    private static string ResolveDefaultOutputRootDirectory()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), DefaultEarlyCommandOutputDirectoryName);
    }
}
