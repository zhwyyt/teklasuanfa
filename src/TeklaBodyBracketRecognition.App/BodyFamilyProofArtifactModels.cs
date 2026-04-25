namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyFamilyProofArtifact
{
    public List<BodyFamilyProofRow> Rows { get; init; } = new();

    public BodyFamilyProofArtifactSummary Summary { get; init; } = new();
}

internal sealed class BodyFamilyProofArtifactSummary
{
    public int AssemblyCount { get; init; }

    public int AdjudicatedCount { get; init; }

    public int DeferredCount { get; init; }

    public List<DefinitionClauseDecisionBreakdownItem> FamilyBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> LongitudinalTypeBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> SubtypeBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> StatusBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> ReasonBreakdown { get; init; } = new();
}
