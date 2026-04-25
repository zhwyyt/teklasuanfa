using System;
using System.Linq;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeMapperFixtureArtifactBuilder
{
    public static DefinitionClauseDecisionBridgeMapperFixtureArtifact Build(
        DefinitionClauseDecisionBridgeMapperFixtureRunResult runResult)
    {
        return new DefinitionClauseDecisionBridgeMapperFixtureArtifact
        {
            GeneratedAtUtc = DateTime.UtcNow,
            TotalCount = runResult.TotalCount,
            MatchedCount = runResult.MatchedCount,
            MismatchCount = runResult.MismatchCount,
            Succeeded = runResult.Succeeded,
            Rows = runResult.Results
                .Select(static result => new DefinitionClauseDecisionBridgeMapperFixtureArtifactRow
                {
                    SampleCode = result.SampleCode,
                    LabelZh = result.LabelZh,
                    IsSynthetic = result.IsSynthetic,
                    ExpectedCandidateDirection = result.ExpectedCandidateDirection.ToString(),
                    ActualCandidateDirection = result.ActualCandidateDirection.ToString(),
                    ExpectedVerdict = result.ExpectedVerdict.ToString(),
                    ActualVerdict = result.ActualVerdict.ToString(),
                    ExpectedPromotionReadiness = result.ExpectedPromotionReadiness.ToString(),
                    ActualPromotionReadiness = result.ActualPromotionReadiness.ToString(),
                    IsMatch = result.IsMatch,
                })
                .ToArray(),
        };
    }
}
