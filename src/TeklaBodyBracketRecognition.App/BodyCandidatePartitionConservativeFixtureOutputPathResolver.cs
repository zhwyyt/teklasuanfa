namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureOutputPathResolver
{
    public static BodyCandidatePartitionConservativeFixtureOutputPaths ResolveFromOutputRootDirectory(
        string outputRootDirectory)
    {
        var outputDirectory = System.IO.Path.Combine(
            outputRootDirectory,
            BodyCandidatePartitionConservativeFixtureOutputContract.OutputDirectoryName);

        return ResolveFromOutputDirectory(outputRootDirectory, outputDirectory);
    }

    public static BodyCandidatePartitionConservativeFixtureOutputPaths ResolveFromOutputDirectory(
        string outputDirectory)
    {
        var outputRootDirectory =
            System.IO.Directory.GetParent(outputDirectory)?.FullName
            ?? outputDirectory;

        return ResolveFromOutputDirectory(outputRootDirectory, outputDirectory);
    }

    private static BodyCandidatePartitionConservativeFixtureOutputPaths ResolveFromOutputDirectory(
        string outputRootDirectory,
        string outputDirectory)
    {
        return new BodyCandidatePartitionConservativeFixtureOutputPaths(
            outputRootDirectory: outputRootDirectory,
            outputDirectory: outputDirectory,
            jsonPath: System.IO.Path.Combine(
                outputDirectory,
                BodyCandidatePartitionConservativeFixtureOutputContract.JsonFileName),
            markdownPath: System.IO.Path.Combine(
                outputDirectory,
                BodyCandidatePartitionConservativeFixtureOutputContract.MarkdownFileName),
            manifestPath: System.IO.Path.Combine(
                outputDirectory,
                BodyCandidatePartitionConservativeFixtureOutputContract.ManifestFileName),
            readmePath: System.IO.Path.Combine(
                outputDirectory,
                BodyCandidatePartitionConservativeFixtureOutputContract.ReadmeFileName));
    }
}
