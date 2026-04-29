using System.Collections.Generic;
using System.Linq;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public sealed class DefinitionClauseDecisionFixtureArtifacts
{
    public List<DefinitionClauseDecisionBridgeFixtureResult> Results { get; init; } = new();
    public int TotalCount { get; init; }
    public int PassedCount { get; init; }
    public int FailedCount { get; init; }
}

public static class DefinitionClauseDecisionFixtureArtifactBuilder
{
    public static DefinitionClauseDecisionFixtureArtifacts Build(
        IEnumerable<DefinitionClauseDecisionBridgeFixtureResult>? results)
    {
        var materialized = results?.ToList() ?? new List<DefinitionClauseDecisionBridgeFixtureResult>();
        var passed = materialized.Count(static x => x.Passed);

        return new DefinitionClauseDecisionFixtureArtifacts
        {
            Results = materialized,
            TotalCount = materialized.Count,
            PassedCount = passed,
            FailedCount = materialized.Count - passed
        };
    }

    public static DefinitionClauseDecisionFixtureArtifacts BuildDefault()
    {
        return Build(DefinitionClauseDecisionBridgeFixtureRunner.RunDefault());
    }
}
