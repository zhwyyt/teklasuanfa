namespace TeklaBodyBracketRecognition.App;

public interface IDefinitionClauseDecisionSnapshotExtractor<in TInput>
{
    DefinitionClauseDecisionSnapshot Extract(TInput input);
}
