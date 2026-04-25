namespace TeklaBodyBracketRecognition.App;

public sealed class BodyCandidatePartitionConservativeFixtureCommandResult
{
    public BodyCandidatePartitionConservativeFixtureCommandResult(
        int exitCode,
        string outputDirectory,
        string jsonPath,
        string markdownPath,
        string manifestPath,
        string readmePath,
        int totalCount,
        int passedCount,
        int failedCount)
    {
        ExitCode = exitCode;
        OutputDirectory = outputDirectory;
        JsonPath = jsonPath;
        MarkdownPath = markdownPath;
        ManifestPath = manifestPath;
        ReadmePath = readmePath;
        TotalCount = totalCount;
        PassedCount = passedCount;
        FailedCount = failedCount;
    }

    public int ExitCode { get; }

    public string OutputDirectory { get; }

    public string JsonPath { get; }

    public string MarkdownPath { get; }

    public string ManifestPath { get; }

    public string ReadmePath { get; }

    public int TotalCount { get; }

    public int PassedCount { get; }

    public int FailedCount { get; }
}
