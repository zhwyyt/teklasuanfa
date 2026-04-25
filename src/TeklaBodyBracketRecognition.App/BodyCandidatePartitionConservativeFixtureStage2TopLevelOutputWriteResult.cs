namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyCandidatePartitionConservativeFixtureStage2TopLevelOutputWriteResult<TManifest>
{
    public required string OutputDirectory { get; init; }

    public required string SummaryMarkdownPath { get; init; }

    public required string ReadmePath { get; init; }

    public required string ManifestPath { get; init; }

    public required TManifest Manifest { get; init; }
}
