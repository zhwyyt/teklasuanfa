namespace TeklaBodyBracketRecognition.App;

public sealed class BodyCandidatePartitionConservativeFixtureOutputPaths
{
    public BodyCandidatePartitionConservativeFixtureOutputPaths(
        string outputRootDirectory,
        string outputDirectory,
        string jsonPath,
        string markdownPath,
        string manifestPath,
        string readmePath)
    {
        OutputRootDirectory = outputRootDirectory;
        OutputDirectory = outputDirectory;
        JsonPath = jsonPath;
        MarkdownPath = markdownPath;
        ManifestPath = manifestPath;
        ReadmePath = readmePath;
    }

    public string OutputRootDirectory { get; }

    public string OutputDirectory { get; }

    public string JsonPath { get; }

    public string MarkdownPath { get; }

    public string ManifestPath { get; }

    public string ReadmePath { get; }
}
