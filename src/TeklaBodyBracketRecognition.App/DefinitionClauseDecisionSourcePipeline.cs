using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSourcePipeline
{
    public static DefinitionClauseDecisionArtifacts BuildArtifacts<TInput>(
        TInput input,
        IDefinitionClauseDecisionSourceProvider<TInput> provider)
    {
        var sourceRows = provider.BuildSourceRows(input);
        return DefinitionClauseDecisionMapper.BuildArtifacts(sourceRows);
    }

    public static IReadOnlyList<DefinitionClauseDecisionSourceRow> BuildSourceRows<TInput>(
        TInput input,
        IDefinitionClauseDecisionSourceProvider<TInput> provider)
    {
        return provider.BuildSourceRows(input);
    }
}
