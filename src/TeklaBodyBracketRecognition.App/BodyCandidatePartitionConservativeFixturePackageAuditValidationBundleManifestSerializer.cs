namespace TeklaBodyBracketRecognition.App;

internal static class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleManifestSerializer
{
    public static string Write(
        BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleManifest manifest,
        string outputDirectory)
    {
        return BodyCandidatePartitionConservativeFixtureStage2OutputArtifactWriter.WriteManifest(
            manifest,
            outputDirectory);
    }
}
