namespace TeklaBodyBracketRecognition.App;

public sealed class BodyCandidatePartitionConservativeFixtureOutputAudit
{
    public BodyCandidatePartitionConservativeFixtureOutputAudit(
        BodyCandidatePartitionConservativeFixtureOutputPaths outputPaths,
        bool jsonExists,
        bool markdownExists,
        bool manifestExists,
        bool readmeExists,
        string[] missingFiles)
    {
        OutputPaths = outputPaths;
        JsonExists = jsonExists;
        MarkdownExists = markdownExists;
        ManifestExists = manifestExists;
        ReadmeExists = readmeExists;
        MissingFiles = missingFiles;
    }

    public BodyCandidatePartitionConservativeFixtureOutputPaths OutputPaths { get; }

    public bool JsonExists { get; }

    public bool MarkdownExists { get; }

    public bool ManifestExists { get; }

    public bool ReadmeExists { get; }

    public string[] MissingFiles { get; }

    public bool IsComplete => MissingFiles.Length == 0;
}
