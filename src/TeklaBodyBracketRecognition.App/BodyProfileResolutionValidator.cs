namespace TeklaBodyBracketRecognition.App;

internal static class BodyProfileResolutionValidator
{
    public static BodyProfileResolutionValidationResult Validate(BodyProfileResolutionArtifact artifact)
    {
        var issues = new List<string>();

        if (artifact.Rows.Count != artifact.Summary.AssemblyCount)
        {
            issues.Add("Summary.AssemblyCount 与 Rows.Count 不一致。");
        }

        var resolvedCount = artifact.Rows.Count(item => string.Equals(item.ResolutionStatusCode, "Resolved", StringComparison.Ordinal));
        var reviewBypassCount = artifact.Rows.Count(item => string.Equals(item.ResolutionStatusCode, "ReviewBypass", StringComparison.Ordinal));

        if (artifact.Summary.ResolvedCount != resolvedCount)
        {
            issues.Add("Summary.ResolvedCount 与实际已细分行数不一致。");
        }

        if (artifact.Summary.ReviewBypassCount != reviewBypassCount)
        {
            issues.Add("Summary.ReviewBypassCount 与实际旁路行数不一致。");
        }

        foreach (var row in artifact.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.MemberId))
            {
                issues.Add("存在 MemberId 为空的行。");
                break;
            }

            if (string.Equals(row.ResolutionStatusCode, "Resolved", StringComparison.Ordinal) &&
                string.Equals(row.ProfileCode, "NONE", StringComparison.Ordinal))
            {
                issues.Add($"构件 {row.MemberId} 已完成自动细分，但 ProfileCode 仍为 NONE。");
            }
        }

        return new BodyProfileResolutionValidationResult
        {
            IsValid = issues.Count == 0,
            AssemblyCount = artifact.Summary.AssemblyCount,
            ResolvedCount = artifact.Summary.ResolvedCount,
            ReviewBypassCount = artifact.Summary.ReviewBypassCount,
            Issues = issues
        };
    }
}
