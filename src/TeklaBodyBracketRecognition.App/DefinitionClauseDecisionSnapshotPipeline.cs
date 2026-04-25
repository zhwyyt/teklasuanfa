namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSnapshotPipeline
{
    public static DefinitionClauseDecisionSnapshot BuildSnapshot<TInput>(
        TInput input,
        IDefinitionClauseDecisionSnapshotExtractor<TInput> extractor)
    {
        return extractor.Extract(input);
    }

    public static DefinitionClauseDecisionSnapshotExportResult Export<TInput>(
        TInput input,
        IDefinitionClauseDecisionSnapshotExtractor<TInput> extractor,
        string outputDirectory)
    {
        var snapshot = BuildSnapshot(input, extractor);
        return DefinitionClauseDecisionSnapshotExportService.Export(snapshot, outputDirectory);
    }
}
