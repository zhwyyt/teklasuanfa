namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionDemoSnapshotExtractor
    : IDefinitionClauseDecisionSnapshotExtractor<object?>
{
    public DefinitionClauseDecisionSnapshot Extract(object? input)
    {
        return DefinitionClauseDecisionDemoSnapshotBuilder.Build();
    }
}
