namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyCandidatePartitionConservativeFixturePackageAuditValidationBundleWorkflowResult
    : BodyCandidatePartitionConservativeFixtureStage2TopLevelWorkflowResult
{
    public required string ValidationOutputDirectory { get; init; }

    public required string ValidationJsonPath { get; init; }

    public required string ValidationMarkdownPath { get; init; }

    public required bool ValidationSucceeded { get; init; }
}
