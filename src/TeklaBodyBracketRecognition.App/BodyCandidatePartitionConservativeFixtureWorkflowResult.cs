namespace TeklaBodyBracketRecognition.App;

public sealed class BodyCandidatePartitionConservativeFixtureWorkflowResult
{
    public BodyCandidatePartitionConservativeFixtureWorkflowResult(
        string outputDirectory,
        string jsonPath,
        string markdownPath,
        int totalCount,
        int passedCount,
        int failedCount)
    {
        OutputDirectory = outputDirectory;
        JsonPath = jsonPath;
        MarkdownPath = markdownPath;
        TotalCount = totalCount;
        PassedCount = passedCount;
        FailedCount = failedCount;
    }

    public string OutputDirectory { get; }

    public string JsonPath { get; }

    public string MarkdownPath { get; }

    public int TotalCount { get; }

    public int PassedCount { get; }

    public int FailedCount { get; }
}
