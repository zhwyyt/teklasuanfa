using System.Collections.Generic;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class DefinitionClauseDecisionBridgeFixtureResult
{
    public string Name { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string ActualClauseVerdictCode { get; init; } = "NONE";
    public string ExpectedClauseVerdictCode { get; init; } = "NONE";
    public string ActualClausePromotionReadinessCode { get; init; } = "NONE";
    public string ExpectedClausePromotionReadinessCode { get; init; } = "NONE";
}

public static class DefinitionClauseDecisionBridgeFixtureRunner
{
    public static IReadOnlyList<DefinitionClauseDecisionBridgeFixtureResult> RunDefault()
    {
        var fixtures = DefinitionClauseDecisionBridgeFixtures.CreateDefault();
        var results = new List<DefinitionClauseDecisionBridgeFixtureResult>(fixtures.Count);

        foreach (var fixture in fixtures)
        {
            var actual = DefinitionClauseDecisionBridge.Evaluate(fixture.Input);
            var passed =
                actual.ClauseVerdictCode == fixture.Expected.ClauseVerdictCode &&
                actual.ClausePromotionReadinessCode == fixture.Expected.ClausePromotionReadinessCode;

            results.Add(new DefinitionClauseDecisionBridgeFixtureResult
            {
                Name = fixture.Name,
                Passed = passed,
                ActualClauseVerdictCode = actual.ClauseVerdictCode,
                ExpectedClauseVerdictCode = fixture.Expected.ClauseVerdictCode,
                ActualClausePromotionReadinessCode = actual.ClausePromotionReadinessCode,
                ExpectedClausePromotionReadinessCode = fixture.Expected.ClausePromotionReadinessCode
            });
        }

        return results;
    }
}
