namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionSnapshotValidationResult
{
    public int MemberCount { get; init; }
    public int PartCount { get; init; }
    public int MissingMemberIdCount { get; init; }
    public int MissingAssemblyIdCount { get; init; }
    public int MissingPartIdCount { get; init; }
    public int MissingPartNameCount { get; init; }
    public int MissingDefinitionClauseCodeCount { get; init; }
    public int MissingClauseVerdictCodeCount { get; init; }
    public int MissingClausePromotionReadinessCodeCount { get; init; }
    public bool IsStructurallyValid { get; init; }
}
