namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureAppEntry
{
    public static bool TryRun(
        string[] args,
        string defaultOutputRootDirectory,
        out int exitCode)
    {
        if (!BodyCandidatePartitionConservativeFixtureCommandHandler.TryHandle(
                args,
                defaultOutputRootDirectory,
                out var result)
            || result is null)
        {
            exitCode = 0;
            return false;
        }

        System.Console.WriteLine($"Output directory: {result.OutputDirectory}");
        System.Console.WriteLine($"JSON: {result.JsonPath}");
        System.Console.WriteLine($"Markdown: {result.MarkdownPath}");
        System.Console.WriteLine($"Manifest: {result.ManifestPath}");
        System.Console.WriteLine($"README: {result.ReadmePath}");
        System.Console.WriteLine($"Passed: {result.PassedCount}/{result.TotalCount}");

        exitCode = result.ExitCode;
        return true;
    }
}
