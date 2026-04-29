namespace TeklaBodyBracketRecognition.App;

internal abstract record BodyCandidatePartitionConservativeFixtureStage2TopLevelCommandResult(
    int ExitCode,
    string OutputDirectory,
    string SummaryMarkdownPath,
    string ReadmePath,
    string ManifestPath);
