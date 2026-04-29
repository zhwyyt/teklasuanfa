using System.Linq;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSnapshotRoundTripService
{
    public static DefinitionClauseDecisionSnapshotRoundTripResult Analyze(
        DefinitionClauseDecisionSnapshot snapshot)
    {
        snapshot ??= new DefinitionClauseDecisionSnapshot();

        var provider = new DefinitionClauseDecisionSnapshotSourceProvider();
        var sourceRows = DefinitionClauseDecisionSourcePipeline.BuildSourceRows(snapshot, provider);
        var representativeRows = DefinitionClauseDecisionMapper.BuildRepresentativeRows(sourceRows);
        var aggregateRows = DefinitionClauseDecisionMapper.BuildAggregateRows(representativeRows);
        var reviewRows = DefinitionClauseDecisionArtifactBuilder.BuildReviewRows(aggregateRows);

        return new DefinitionClauseDecisionSnapshotRoundTripResult
        {
            MemberCount = snapshot.Members.Count,
            PartCount = snapshot.Members.Sum(static member => member.Parts.Count),
            SourceRowCount = sourceRows.Count,
            RepresentativeRowCount = representativeRows.Count,
            AggregateRowCount = aggregateRows.Count,
            ReviewRowCount = reviewRows.Count
        };
    }
}
