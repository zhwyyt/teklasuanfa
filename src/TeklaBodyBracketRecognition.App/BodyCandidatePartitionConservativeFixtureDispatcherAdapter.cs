namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureDispatcherAdapter
{
    public static bool TryRun(
        string[] args,
        string defaultOutputRootDirectory,
        out int exitCode,
        out string? message,
        out string? outputDirectory)
    {
        if (!BodyCandidatePartitionConservativeFixtureCommandHandler.TryHandle(
                args,
                defaultOutputRootDirectory,
                out var result)
            || result is null)
        {
            exitCode = 0;
            message = null;
            outputDirectory = null;
            return false;
        }

        exitCode = result.ExitCode;
        outputDirectory = result.OutputDirectory;
        message = $"Body candidate partition conservative fixture export finished: {result.PassedCount}/{result.TotalCount} passed.";
        return true;
    }
}
