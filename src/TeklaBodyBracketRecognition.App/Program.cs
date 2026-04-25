using System.Text.Json;
namespace TeklaBodyBracketRecognition.App;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    private static int Main(string[] args)
    {
        if (AppEarlyCommandDispatcher.TryRun(args, Console.Out, Console.Error, out var earlyExitCode))
        {
            return earlyExitCode;
        }

        return OfflineRecognitionApp.Run(args, JsonOptions, Console.Out, Console.Error);
    }

}
