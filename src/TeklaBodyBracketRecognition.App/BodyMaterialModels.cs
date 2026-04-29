using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal sealed class BodyMaterialSummary
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string MemberProfileString { get; set; } = string.Empty;

    public int SourceMainPartId { get; set; }

    public string SourceMainPartName { get; set; } = string.Empty;

    public string SourceMainPartProfileString { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public int InputMainPartId { get; set; }

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public IReadOnlyList<SourceBodySeedPart> SourceBodySeedParts { get; set; } = Array.Empty<SourceBodySeedPart>();

    public string BodyDescriptorFamily { get; set; } = string.Empty;

    public string BodyDescriptorSectionType { get; set; } = string.Empty;

    public string BodyDescriptorDerivationType { get; set; } = string.Empty;

    public double BodyDescriptorConfidence { get; set; }

    public string LongitudinalTypeCode { get; set; } = "STRAIGHT";

    public string LongitudinalTypeLabelZh { get; set; } = "直线主线";

    public string LongitudinalSubtypeCode { get; set; } = "GENERAL_STRAIGHT";

    public string LongitudinalSubtypeLabelZh { get; set; } = "一般直线主线";

    public bool BodyDescriptorReviewRequired { get; set; }

    public IReadOnlyList<string> BodyDescriptorReviewReasons { get; set; } = Array.Empty<string>();

    public IReadOnlyList<int> BodyDescriptorCorePartIds { get; set; } = Array.Empty<int>();

    public IReadOnlyList<CoreBodyPartSummary> BodyDescriptorCoreParts { get; set; } = Array.Empty<CoreBodyPartSummary>();

    public string? DefinitionBodyFamily { get; set; }

    public bool? DefinitionSatisfied { get; set; }

    public IReadOnlyList<string>? DefinitionFailReasons { get; set; }

    public IReadOnlyList<int>? DefinitionCoreBodyPartIds { get; set; }

    public IReadOnlyList<int>? DefinitionAccessoryPartIds { get; set; }

    public IReadOnlyList<string>? DefinitionStableStations { get; set; }

    public bool? DefinitionReviewRequired { get; set; }

    public IReadOnlyList<string>? DefinitionReviewReasons { get; set; }
}

internal sealed class BodyMaterialExplanation
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string MemberProfileString { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public int InputMainPartId { get; set; }

    public int SourceMainPartId { get; set; }

    public string SourceMainPartName { get; set; } = string.Empty;

    public string SourceMainPartProfileString { get; set; } = string.Empty;

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public bool DependsOnImportSynthesis { get; set; }

    public IReadOnlyList<int> SourceBodySeedPartIds { get; set; } = Array.Empty<int>();

    public IReadOnlyList<SourceBodySeedPart> SourceBodySeedParts { get; set; } = Array.Empty<SourceBodySeedPart>();

    public string BodyDescriptorFamily { get; set; } = string.Empty;

    public string BodyDescriptorSectionType { get; set; } = string.Empty;

    public string BodyDescriptorDerivationType { get; set; } = string.Empty;

    public double BodyDescriptorConfidence { get; set; }

    public string LongitudinalTypeCode { get; set; } = "STRAIGHT";

    public string LongitudinalTypeLabelZh { get; set; } = "直线主线";

    public string LongitudinalSubtypeCode { get; set; } = "GENERAL_STRAIGHT";

    public string LongitudinalSubtypeLabelZh { get; set; } = "一般直线主线";

    public bool BodyDescriptorReviewRequired { get; set; }

    public IReadOnlyList<string> BodyDescriptorReviewReasons { get; set; } = Array.Empty<string>();

    public IReadOnlyList<int> BodyDescriptorCorePartIds { get; set; } = Array.Empty<int>();

    public IReadOnlyList<CoreBodyPartSummary> BodyDescriptorCoreParts { get; set; } = Array.Empty<CoreBodyPartSummary>();

    public IReadOnlyList<int> BodyDescriptorAccessoryPartIds { get; set; } = Array.Empty<int>();

    public IReadOnlyDictionary<string, double> BodyDescriptorSectionFamilyVotes { get; set; } = new Dictionary<string, double>();

    public bool ImportDescriptorMismatch { get; set; }

    public string? DefinitionBodyFamily { get; set; }

    public bool? DefinitionSatisfied { get; set; }

    public IReadOnlyList<string>? DefinitionFailReasons { get; set; }

    public IReadOnlyList<string> ExplanationNotes { get; set; } = Array.Empty<string>();
}

internal sealed class CoreBodyPartSummary
{
    public int CorePartOrder { get; set; }

    public int PartId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string ProfileString { get; set; } = string.Empty;

    public string Material { get; set; } = string.Empty;

    public string RuntimeType { get; set; } = string.Empty;

    public double? Thickness { get; set; }

    public string SemanticRole { get; set; } = string.Empty;

    public bool IsMainPart { get; set; }

    public bool IsSyntheticPart { get; set; }
}

internal sealed class BodyMaterialPartRow
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string MemberProfileString { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public int InputMainPartId { get; set; }

    public int SourceMainPartId { get; set; }

    public string BodyDescriptorFamily { get; set; } = string.Empty;

    public string BodyDescriptorSectionType { get; set; } = string.Empty;

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public int CorePartOrder { get; set; }

    public int PartId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string ProfileString { get; set; } = string.Empty;

    public string Material { get; set; } = string.Empty;

    public string RuntimeType { get; set; } = string.Empty;

    public double? Thickness { get; set; }

    public string SemanticRole { get; set; } = string.Empty;

    public bool IsMainPart { get; set; }

    public bool IsSyntheticPart { get; set; }
}

internal sealed class BodyMaterialSourceSeedRow
{
    public string SourceFile { get; set; } = string.Empty;

    public string MemberId { get; set; } = string.Empty;

    public string MemberProfileString { get; set; } = string.Empty;

    public string AssemblyId { get; set; } = string.Empty;

    public string BodyDescriptorFamily { get; set; } = string.Empty;

    public string BodyDescriptorSectionType { get; set; } = string.Empty;

    public bool SynthesizedBody { get; set; }

    public string? ImportSynthesisKind { get; set; }

    public int SourceMainPartId { get; set; }

    public string SourceMainPartName { get; set; } = string.Empty;

    public string SourceMainPartProfileString { get; set; } = string.Empty;

    public int SeedOrder { get; set; }

    public int PartId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string ProfileString { get; set; } = string.Empty;

    public string Material { get; set; } = string.Empty;

    public double? Thickness { get; set; }

    public string SemanticRole { get; set; } = string.Empty;

    public double SemanticRoleScore { get; set; }

    public bool IsSourceMainPart { get; set; }
}
