namespace TeklaBodyBracketRecognition.App;

internal static class DefinitionClauseDecisionFullRunSourceValidator
{
    public static DefinitionClauseDecisionFullRunSourceValidationResult Validate(DefinitionClauseDecisionFullRunSourceArtifact artifact)
    {
        var issues = new List<string>();

        if (artifact.Assemblies.Count == 0)
        {
            issues.Add("Assemblies 为空。");
        }

        if (artifact.RepresentativeParts.Count == 0)
        {
            issues.Add("RepresentativeParts 为空。");
        }

        if (artifact.Summary.AssemblyCount != artifact.Assemblies.Count)
        {
            issues.Add("Summary.AssemblyCount 与 Assemblies 实际数量不一致。");
        }

        if (artifact.Summary.RepresentativePartCount != artifact.RepresentativeParts.Count)
        {
            issues.Add("Summary.RepresentativePartCount 与 RepresentativeParts 实际数量不一致。");
        }

        if (artifact.RepresentativeParts.Any(item => string.IsNullOrWhiteSpace(item.MemberId)))
        {
            issues.Add("存在缺少 MemberId 的代表零件。");
        }

        if (artifact.RepresentativeParts.Any(item => string.IsNullOrWhiteSpace(item.AssemblyId)))
        {
            issues.Add("存在缺少 AssemblyId 的代表零件。");
        }

        if (artifact.RepresentativeParts.Any(item => item.RepresentativePartId == 0))
        {
            issues.Add("存在 RepresentativePartId 为 0 的代表零件。");
        }

        return new DefinitionClauseDecisionFullRunSourceValidationResult
        {
            IsValid = issues.Count == 0,
            AssemblyCount = artifact.Assemblies.Count,
            RepresentativePartCount = artifact.RepresentativeParts.Count,
            AssembliesWithTopologyRewrite = artifact.Summary.AssembliesWithTopologyRewrite,
            CoreRepresentativePartCount = artifact.Summary.CoreRepresentativePartCount,
            ReviewRepresentativePartCount = artifact.Summary.ReviewRepresentativePartCount,
            Issues = issues
        };
    }
}
