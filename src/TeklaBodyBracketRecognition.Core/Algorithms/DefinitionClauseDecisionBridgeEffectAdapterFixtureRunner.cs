using System.Collections.Generic;
using System.Linq;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionBridgeEffectAdapterFixtureResult
{
    public string SampleCode { get; set; } = string.Empty;

    public string LabelZh { get; set; } = string.Empty;

    public bool IsSynthetic { get; set; }

    public DefinitionClauseDecisionBridgeSourceTier ExpectedSourceTier { get; set; }

    public DefinitionClauseDecisionBridgeSourceTier ActualSourceTier { get; set; }

    public DefinitionClauseDecisionBridgeStationTier ExpectedStationTier { get; set; }

    public DefinitionClauseDecisionBridgeStationTier ActualStationTier { get; set; }

    public DefinitionClauseDecisionBridgeTopologyTier ExpectedTopologyTier { get; set; }

    public DefinitionClauseDecisionBridgeTopologyTier ActualTopologyTier { get; set; }

    public DefinitionClauseDecisionBridgeProofCompletenessTier ExpectedProofCompletenessTier { get; set; }

    public DefinitionClauseDecisionBridgeProofCompletenessTier ActualProofCompletenessTier { get; set; }

    public DefinitionClauseDecisionBridgeLeadClauseTier ExpectedLeadClauseTier { get; set; }

    public DefinitionClauseDecisionBridgeLeadClauseTier ActualLeadClauseTier { get; set; }

    public DefinitionClauseDecisionBridgeCandidateDirection ExpectedCandidateDirection { get; set; }

    public DefinitionClauseDecisionBridgeCandidateDirection ActualCandidateDirection { get; set; }

    public DefinitionClauseDecisionBridgeVerdict ExpectedVerdict { get; set; }

    public DefinitionClauseDecisionBridgeVerdict ActualVerdict { get; set; }

    public DefinitionClauseDecisionBridgePromotionReadiness ExpectedPromotionReadiness { get; set; }

    public DefinitionClauseDecisionBridgePromotionReadiness ActualPromotionReadiness { get; set; }

    public bool IsMatch =>
        ExpectedSourceTier == ActualSourceTier &&
        ExpectedStationTier == ActualStationTier &&
        ExpectedTopologyTier == ActualTopologyTier &&
        ExpectedProofCompletenessTier == ActualProofCompletenessTier &&
        ExpectedLeadClauseTier == ActualLeadClauseTier &&
        ExpectedCandidateDirection == ActualCandidateDirection &&
        ExpectedVerdict == ActualVerdict &&
        ExpectedPromotionReadiness == ActualPromotionReadiness;
}

public sealed class DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult
{
    public IReadOnlyList<DefinitionClauseDecisionBridgeEffectAdapterFixtureResult> Results { get; set; } = [];

    public int TotalCount => Results.Count;

    public int MatchedCount => Results.Count(static item => item.IsMatch);

    public int MismatchCount => Results.Count(static item => !item.IsMatch);

    public bool Succeeded => MismatchCount == 0;
}

public static class DefinitionClauseDecisionBridgeEffectAdapterFixtureRunner
{
    public static DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult RunDefault()
    {
        return Run(DefinitionClauseDecisionBridgeEffectAdapterFixtures.CreateDefault());
    }

    public static DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult Run(
        IReadOnlyList<DefinitionClauseDecisionBridgeEffectAdapterFixture> fixtures)
    {
        var results = fixtures
            .Select(static fixture =>
            {
                var adapted = DefinitionClauseDecisionBridgeEffectAdapter.Adapt(fixture.Snapshot);
                return new DefinitionClauseDecisionBridgeEffectAdapterFixtureResult
                {
                    SampleCode = fixture.SampleCode,
                    LabelZh = fixture.LabelZh,
                    IsSynthetic = fixture.IsSynthetic,
                    ExpectedSourceTier = fixture.ExpectedSourceTier,
                    ActualSourceTier = adapted.Context.SourceTier,
                    ExpectedStationTier = fixture.ExpectedStationTier,
                    ActualStationTier = adapted.Context.StationTier,
                    ExpectedTopologyTier = fixture.ExpectedTopologyTier,
                    ActualTopologyTier = adapted.Context.TopologyTier,
                    ExpectedProofCompletenessTier = fixture.ExpectedProofCompletenessTier,
                    ActualProofCompletenessTier = adapted.Context.ProofCompletenessTier,
                    ExpectedLeadClauseTier = fixture.ExpectedLeadClauseTier,
                    ActualLeadClauseTier = adapted.Context.LeadClauseTier,
                    ExpectedCandidateDirection = fixture.ExpectedCandidateDirection,
                    ActualCandidateDirection = adapted.Result.CandidateDirection,
                    ExpectedVerdict = fixture.ExpectedVerdict,
                    ActualVerdict = adapted.Result.Verdict,
                    ExpectedPromotionReadiness = fixture.ExpectedPromotionReadiness,
                    ActualPromotionReadiness = adapted.Result.PromotionReadiness,
                };
            })
            .ToArray();

        return new DefinitionClauseDecisionBridgeEffectAdapterFixtureRunResult
        {
            Results = results,
        };
    }
}
