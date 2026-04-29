using System.Collections.Generic;
using System.Linq;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionBridgeMapperFixtureResult
{
    public string SampleCode { get; set; } = string.Empty;

    public string LabelZh { get; set; } = string.Empty;

    public bool IsSynthetic { get; set; }

    public DefinitionClauseDecisionBridgeCandidateDirection ExpectedCandidateDirection { get; set; }

    public DefinitionClauseDecisionBridgeCandidateDirection ActualCandidateDirection { get; set; }

    public DefinitionClauseDecisionBridgeVerdict ExpectedVerdict { get; set; }

    public DefinitionClauseDecisionBridgeVerdict ActualVerdict { get; set; }

    public DefinitionClauseDecisionBridgePromotionReadiness ExpectedPromotionReadiness { get; set; }

    public DefinitionClauseDecisionBridgePromotionReadiness ActualPromotionReadiness { get; set; }

    public bool IsMatch =>
        ExpectedCandidateDirection == ActualCandidateDirection &&
        ExpectedVerdict == ActualVerdict &&
        ExpectedPromotionReadiness == ActualPromotionReadiness;
}

public sealed class DefinitionClauseDecisionBridgeMapperFixtureRunResult
{
    public IReadOnlyList<DefinitionClauseDecisionBridgeMapperFixtureResult> Results { get; set; } =
        [];

    public int TotalCount => Results.Count;

    public int MatchedCount => Results.Count(static item => item.IsMatch);

    public int MismatchCount => Results.Count(static item => !item.IsMatch);

    public bool Succeeded => MismatchCount == 0;
}

public static class DefinitionClauseDecisionBridgeMapperFixtureRunner
{
    public static DefinitionClauseDecisionBridgeMapperFixtureRunResult RunDefault()
    {
        return Run(DefinitionClauseDecisionBridgeMapperFixtures.CreateDefault());
    }

    public static DefinitionClauseDecisionBridgeMapperFixtureRunResult Run(
        IReadOnlyList<DefinitionClauseDecisionBridgeMapperFixture> fixtures)
    {
        var results = fixtures
            .Select(static fixture =>
            {
                var actual = DefinitionClauseDecisionBridgeMapper.Evaluate(fixture.Context);
                return new DefinitionClauseDecisionBridgeMapperFixtureResult
                {
                    SampleCode = fixture.SampleCode,
                    LabelZh = fixture.LabelZh,
                    IsSynthetic = fixture.IsSynthetic,
                    ExpectedCandidateDirection = fixture.ExpectedCandidateDirection,
                    ActualCandidateDirection = actual.CandidateDirection,
                    ExpectedVerdict = fixture.ExpectedVerdict,
                    ActualVerdict = actual.Verdict,
                    ExpectedPromotionReadiness = fixture.ExpectedPromotionReadiness,
                    ActualPromotionReadiness = actual.PromotionReadiness,
                };
            })
            .ToArray();

        return new DefinitionClauseDecisionBridgeMapperFixtureRunResult
        {
            Results = results,
        };
    }
}
