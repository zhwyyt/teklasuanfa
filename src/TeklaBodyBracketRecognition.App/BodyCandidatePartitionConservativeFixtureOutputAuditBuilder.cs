namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureOutputAuditBuilder
{
    public static BodyCandidatePartitionConservativeFixtureOutputAudit BuildFromOutputRootDirectory(
        string outputRootDirectory)
    {
        var outputPaths =
            BodyCandidatePartitionConservativeFixtureOutputPathResolver
                .ResolveFromOutputRootDirectory(outputRootDirectory);

        return Build(outputPaths);
    }

    public static BodyCandidatePartitionConservativeFixtureOutputAudit BuildFromOutputDirectory(
        string outputDirectory)
    {
        var outputPaths =
            BodyCandidatePartitionConservativeFixtureOutputPathResolver
                .ResolveFromOutputDirectory(outputDirectory);

        return Build(outputPaths);
    }

    public static BodyCandidatePartitionConservativeFixtureOutputAudit Build(
        BodyCandidatePartitionConservativeFixtureOutputPaths outputPaths)
    {
        var missingFiles = new System.Collections.Generic.List<string>();

        var jsonExists = System.IO.File.Exists(outputPaths.JsonPath);
        if (!jsonExists)
        {
            missingFiles.Add(BodyCandidatePartitionConservativeFixtureOutputContract.JsonFileName);
        }

        var markdownExists = System.IO.File.Exists(outputPaths.MarkdownPath);
        if (!markdownExists)
        {
            missingFiles.Add(BodyCandidatePartitionConservativeFixtureOutputContract.MarkdownFileName);
        }

        var manifestExists = System.IO.File.Exists(outputPaths.ManifestPath);
        if (!manifestExists)
        {
            missingFiles.Add(BodyCandidatePartitionConservativeFixtureOutputContract.ManifestFileName);
        }

        var readmeExists = System.IO.File.Exists(outputPaths.ReadmePath);
        if (!readmeExists)
        {
            missingFiles.Add(BodyCandidatePartitionConservativeFixtureOutputContract.ReadmeFileName);
        }

        return new BodyCandidatePartitionConservativeFixtureOutputAudit(
            outputPaths,
            jsonExists,
            markdownExists,
            manifestExists,
            readmeExists,
            missingFiles.ToArray());
    }
}
