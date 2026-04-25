namespace TeklaBodyBracketRecognition.App;

public sealed class BodyCandidatePartitionConservativeFixturePackageAuditResult
{
    public BodyCandidatePartitionConservativeFixturePackageAuditResult(
        string outputDirectory,
        string jsonPath,
        string markdownPath,
        string manifestPath,
        string readmePath,
        int totalCount,
        int passedCount,
        int failedCount,
        bool packageIsComplete,
        string[] missingFiles,
        int exitCode)
    {
        OutputDirectory = outputDirectory;
        JsonPath = jsonPath;
        MarkdownPath = markdownPath;
        ManifestPath = manifestPath;
        ReadmePath = readmePath;
        TotalCount = totalCount;
        PassedCount = passedCount;
        FailedCount = failedCount;
        PackageIsComplete = packageIsComplete;
        MissingFiles = missingFiles;
        ExitCode = exitCode;
    }

    public string OutputDirectory { get; }

    public string JsonPath { get; }

    public string MarkdownPath { get; }

    public string ManifestPath { get; }

    public string ReadmePath { get; }

    public int TotalCount { get; }

    public int PassedCount { get; }

    public int FailedCount { get; }

    public bool PackageIsComplete { get; }

    public string[] MissingFiles { get; }

    public int ExitCode { get; }
}
