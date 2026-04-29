namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyFamilyProofValidationResult
{
    public bool IsValid { get; init; }

    public int AssemblyCount { get; init; }

    public int AdjudicatedCount { get; init; }

    public int DeferredCount { get; init; }

    public List<string> Issues { get; init; } = new();
}
