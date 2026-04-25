using System;
using System.Linq;
using TeklaBodyBracketRecognition.Core.Algorithms;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactBuilder
{
    public static DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifact Build(
        DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult runResult)
    {
        return new DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifact
        {
            GeneratedAtUtc = DateTime.UtcNow,
            TotalCount = runResult.TotalCount,
            MatchedCount = runResult.MatchedCount,
            MismatchCount = runResult.MismatchCount,
            Succeeded = runResult.Succeeded,
            Rows = runResult.Results
                .Select(static result => new DefinitionClauseDecisionBridgeEffectAdapterFixtureArtifactRow
                {
                    SampleCode = result.SampleCode,
                    LabelZh = result.LabelZh,
                    IsSynthetic = result.IsSynthetic,
                    ExpectedSourceTier = result.ExpectedSourceTier.ToString(),
                    ActualSourceTier = result.ActualSourceTier.ToString(),
                    ExpectedStationTier = result.ExpectedStationTier.ToString(),
                    ActualStationTier = result.ActualStationTier.ToString(),
                    ExpectedTopologyTier = result.ExpectedTopologyTier.ToString(),
                    ActualTopologyTier = result.ActualTopologyTier.ToString(),
                    ExpectedProofCompletenessTier = result.ExpectedProofCompletenessTier.ToString(),
                    ActualProofCompletenessTier = result.ActualProofCompletenessTier.ToString(),
                    ExpectedLeadClauseTier = result.ExpectedLeadClauseTier.ToString(),
                    ActualLeadClauseTier = result.ActualLeadClauseTier.ToString(),
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
