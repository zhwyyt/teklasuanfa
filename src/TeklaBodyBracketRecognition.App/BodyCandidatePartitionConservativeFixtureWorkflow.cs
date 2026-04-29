using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixtureWorkflow
{
    private const string OutputDirectoryName = "body-candidate-partition-conservative-fixture";
    private const string JsonFileName = "body-candidate-partition-conservative-fixture.json";
    private const string MarkdownFileName = "body-candidate-partition-conservative-fixture.md";

    public static BodyCandidatePartitionConservativeFixtureWorkflowResult RunDefault(
        string outputRootDirectory)
    {
        var results = BodyCandidatePartitionConservativeFixtureRunner.RunDefault();
        var artifact = BodyCandidatePartitionConservativeFixtureArtifactBuilder.Build(results);
        var outputDirectory = System.IO.Path.Combine(outputRootDirectory, OutputDirectoryName);
        var jsonPath = System.IO.Path.Combine(outputDirectory, JsonFileName);
        var markdownPath = System.IO.Path.Combine(outputDirectory, MarkdownFileName);

        BodyCandidatePartitionConservativeFixtureSerializer.WriteJson(jsonPath, artifact);
        BodyCandidatePartitionConservativeFixtureSerializer.WriteMarkdown(markdownPath, artifact);

        return new BodyCandidatePartitionConservativeFixtureWorkflowResult(
            outputDirectory,
            jsonPath,
            markdownPath,
            artifact.TotalCount,
            artifact.PassedCount,
            artifact.FailedCount);
    }
}
