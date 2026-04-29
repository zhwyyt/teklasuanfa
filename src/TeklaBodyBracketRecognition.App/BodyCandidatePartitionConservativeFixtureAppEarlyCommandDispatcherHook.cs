namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureAppEarlyCommandDispatcherHook
{
    public static bool TryRun(
        string[] args,
        string defaultOutputRootDirectory,
        out int exitCode)
    {
        if (!BodyCandidatePartitionConservativeFixtureDispatcherAdapter.TryRun(
                args,
                defaultOutputRootDirectory,
                out exitCode,
                out var message,
                out var outputDirectory))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            System.Console.WriteLine(message);
        }

        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            System.Console.WriteLine($"Output directory: {outputDirectory}");
        }

        return true;
    }
}
