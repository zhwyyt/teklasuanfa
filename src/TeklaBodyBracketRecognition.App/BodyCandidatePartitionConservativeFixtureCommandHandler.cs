namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureCommandHandler
{
    public const string CommandName = "--body-candidate-partition-conservative-fixture";

    public static bool TryHandle(
        string[] args,
        string defaultOutputRootDirectory,
        out BodyCandidatePartitionConservativeFixtureCommandResult? result)
    {
        if (args.Length == 0 || !string.Equals(args[0], CommandName, System.StringComparison.OrdinalIgnoreCase))
        {
            result = null;
            return false;
        }

        var outputRootDirectory = defaultOutputRootDirectory;

        for (var i = 1; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--output-root", System.StringComparison.OrdinalIgnoreCase)
                && i + 1 < args.Length)
            {
                outputRootDirectory = args[i + 1];
                i++;
            }
        }

        result = BodyCandidatePartitionConservativeFixtureExportService.Export(outputRootDirectory);
        return true;
    }
}
