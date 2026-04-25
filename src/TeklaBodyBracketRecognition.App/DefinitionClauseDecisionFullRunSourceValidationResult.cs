namespace TeklaBodyBracketRecognition.App;

internal sealed class DefinitionClauseDecisionFullRunSourceValidationResult
{
    public bool IsValid { get; init; }

    public int AssemblyCount { get; init; }

    public int RepresentativePartCount { get; init; }

    public int AssembliesWithTopologyRewrite { get; init; }

    public int CoreRepresentativePartCount { get; init; }

    public int ReviewRepresentativePartCount { get; init; }

    public List<string> Issues { get; init; } = new();
}
