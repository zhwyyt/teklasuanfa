namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixtureStage2AggregateManifestSerializer
{
    public static string Write(
        BodyCandidatePartitionConservativeFixtureStage2AggregateManifest manifest,
        string outputDirectory)
    {
        return BodyCandidatePartitionConservativeFixtureStage2OutputArtifactWriter.WriteManifest(
            manifest,
            outputDirectory);
    }
}
