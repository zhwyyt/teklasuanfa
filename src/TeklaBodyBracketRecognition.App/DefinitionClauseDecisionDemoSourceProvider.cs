using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionDemoSourceProvider
    : IDefinitionClauseDecisionSourceProvider<object?>
{
    public IReadOnlyList<DefinitionClauseDecisionSourceRow> BuildSourceRows(object? input)
    {
        return DefinitionClauseDecisionDemoSourceBuilder.BuildFromDefaultFixtures();
    }
}
