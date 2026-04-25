namespace TeklaBodyBracketRecognition.App;

public sealed record BodyCandidatePartitionConservativeFixturePackageAuditValidationWorkflowResult(
    string OutputDirectory,
    string ValidationJsonPath,
    string ValidationMarkdownPath,
    int ExitCode,
    bool IsSuccess);
