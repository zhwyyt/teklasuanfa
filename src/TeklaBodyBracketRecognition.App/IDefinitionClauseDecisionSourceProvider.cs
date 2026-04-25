using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.App;

public interface IDefinitionClauseDecisionSourceProvider<in TInput>
{
    IReadOnlyList<DefinitionClauseDecisionSourceRow> BuildSourceRows(TInput input);
}
