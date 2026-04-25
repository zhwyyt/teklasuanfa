namespace TeklaBodyBracketRecognition.App;

public static class BodyCandidatePartitionConservativeFixturePackageWorkflow
{
    private const string ManifestFileName = "manifest.json";
    private const string ReadmeFileName = "README.md";

    public static BodyCandidatePartitionConservativeFixturePackageWorkflowResult RunDefault(
        string outputRootDirectory)
    {
        var workflowResult = BodyCandidatePartitionConservativeFixtureWorkflow.RunDefault(outputRootDirectory);

        var manifest = new BodyCandidatePartitionConservativeFixtureManifest(
            workflowResult.OutputDirectory,
            workflowResult.JsonPath,
            workflowResult.MarkdownPath,
            workflowResult.TotalCount,
            workflowResult.PassedCount,
            workflowResult.FailedCount);

        var manifestPath = System.IO.Path.Combine(workflowResult.OutputDirectory, ManifestFileName);
        var readmePath = System.IO.Path.Combine(workflowResult.OutputDirectory, ReadmeFileName);

        BodyCandidatePartitionConservativeFixtureManifestSerializer.Write(manifestPath, manifest);

        System.IO.File.WriteAllText(
            readmePath,
            BodyCandidatePartitionConservativeFixtureOutputReadmeBuilder.Build(manifest) + System.Environment.NewLine,
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        return new BodyCandidatePartitionConservativeFixturePackageWorkflowResult(
            workflowResult.OutputDirectory,
            workflowResult.JsonPath,
            workflowResult.MarkdownPath,
            manifestPath,
            readmePath,
            workflowResult.TotalCount,
            workflowResult.PassedCount,
            workflowResult.FailedCount);
    }
}
