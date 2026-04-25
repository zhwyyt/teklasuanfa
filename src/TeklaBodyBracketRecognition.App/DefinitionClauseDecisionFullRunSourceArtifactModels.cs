namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionFullRunSourceArtifact
{
    public List<DefinitionClauseDecisionFullRunAssemblySource> Assemblies { get; init; } = new();

    public List<DefinitionClauseDecisionFullRunRepresentativePartSource> RepresentativeParts { get; init; } = new();

    public DefinitionClauseDecisionFullRunSourceArtifactSummary Summary { get; init; } = new();
}

internal sealed class DefinitionClauseDecisionFullRunSourceArtifactSummary
{
    public int AssemblyCount { get; init; }

    public int RepresentativePartCount { get; init; }

    public int AssembliesWithTopologyRewrite { get; init; }

    public int CoreRepresentativePartCount { get; init; }

    public int ReviewRepresentativePartCount { get; init; }

    public int InputMainPartRepresentativeCount { get; init; }

    public List<DefinitionClauseDecisionBreakdownItem> LeadClauseBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> DefinitionClauseBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> DefinitionClauseEffectBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> EffectDirectionBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> PatternBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> VerdictBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> PromotionReadinessBreakdown { get; init; } = new();

    public List<DefinitionClauseDecisionBreakdownItem> CandidateDirectionBreakdown { get; init; } = new();
}

internal sealed class DefinitionClauseDecisionBreakdownItem
{
    public string Code { get; init; } = string.Empty;

    public string LabelZh { get; init; } = string.Empty;

    public int Count { get; init; }
}
