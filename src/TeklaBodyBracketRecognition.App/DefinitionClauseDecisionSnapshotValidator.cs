using System.Linq;

namespace TeklaBodyBracketRecognition.App;

public static class DefinitionClauseDecisionSnapshotValidator
{
    public static DefinitionClauseDecisionSnapshotValidationResult Validate(
        DefinitionClauseDecisionSnapshot snapshot)
    {
        snapshot ??= new DefinitionClauseDecisionSnapshot();

        var members = snapshot.Members;
        var parts = members.SelectMany(static member => member.Parts).ToList();

        var missingMemberIdCount = members.Count(static member => string.IsNullOrWhiteSpace(member.MemberId));
        var missingAssemblyIdCount = members.Count(static member => string.IsNullOrWhiteSpace(member.AssemblyId));
        var missingPartIdCount = parts.Count(static part => string.IsNullOrWhiteSpace(part.PartId));
        var missingPartNameCount = parts.Count(static part => string.IsNullOrWhiteSpace(part.PartName));
        var missingDefinitionClauseCodeCount = parts.Count(static part => string.IsNullOrWhiteSpace(part.DefinitionClauseCode) || part.DefinitionClauseCode == "NONE");
        var missingClauseVerdictCodeCount = parts.Count(static part => string.IsNullOrWhiteSpace(part.ClauseVerdictCode) || part.ClauseVerdictCode == "NONE");
        var missingClausePromotionReadinessCodeCount = parts.Count(static part => string.IsNullOrWhiteSpace(part.ClausePromotionReadinessCode) || part.ClausePromotionReadinessCode == "NONE");

        return new DefinitionClauseDecisionSnapshotValidationResult
        {
            MemberCount = members.Count,
            PartCount = parts.Count,
            MissingMemberIdCount = missingMemberIdCount,
            MissingAssemblyIdCount = missingAssemblyIdCount,
            MissingPartIdCount = missingPartIdCount,
            MissingPartNameCount = missingPartNameCount,
            MissingDefinitionClauseCodeCount = missingDefinitionClauseCodeCount,
            MissingClauseVerdictCodeCount = missingClauseVerdictCodeCount,
            MissingClausePromotionReadinessCodeCount = missingClausePromotionReadinessCodeCount,
            IsStructurallyValid =
                missingMemberIdCount == 0 &&
                missingAssemblyIdCount == 0 &&
                missingPartIdCount == 0 &&
                missingPartNameCount == 0
        };
    }
}
