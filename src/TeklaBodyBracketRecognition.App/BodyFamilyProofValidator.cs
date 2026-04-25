namespace TeklaBodyBracketRecognition.App;

internal static class BodyFamilyProofValidator
{
    public static BodyFamilyProofValidationResult Validate(BodyFamilyProofArtifact artifact)
    {
        var issues = new List<string>();

        if (artifact.Rows.Count != artifact.Summary.AssemblyCount)
        {
            issues.Add("Summary.AssemblyCount 与 Rows.Count 不一致。");
        }

        var adjudicatedCount = artifact.Rows.Count(item => string.Equals(item.DecisionStatusCode, "Adjudicated", StringComparison.Ordinal));
        var deferredCount = artifact.Rows.Count(item => string.Equals(item.DecisionStatusCode, "Deferred", StringComparison.Ordinal));

        if (artifact.Summary.AdjudicatedCount != adjudicatedCount)
        {
            issues.Add("Summary.AdjudicatedCount 与实际已判定行数不一致。");
        }

        if (artifact.Summary.DeferredCount != deferredCount)
        {
            issues.Add("Summary.DeferredCount 与实际暂缓行数不一致。");
        }

        foreach (var row in artifact.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.MemberId))
            {
                issues.Add("存在 MemberId 为空的行。");
                break;
            }

            if (string.IsNullOrWhiteSpace(row.DecisionReasonCode))
            {
                issues.Add($"构件 {row.MemberId} 缺少 DecisionReasonCode。");
            }

            if (string.Equals(row.DecisionStatusCode, "Adjudicated", StringComparison.Ordinal) &&
                string.Equals(row.DefinitionDrivenFamilyCode, "NONE", StringComparison.Ordinal))
            {
                issues.Add($"构件 {row.MemberId} 已判定，但 FamilyCode 仍为 NONE。");
            }
        }

        return new BodyFamilyProofValidationResult
        {
            IsValid = issues.Count == 0,
            AssemblyCount = artifact.Summary.AssemblyCount,
            AdjudicatedCount = artifact.Summary.AdjudicatedCount,
            DeferredCount = artifact.Summary.DeferredCount,
            Issues = issues
        };
    }
}
