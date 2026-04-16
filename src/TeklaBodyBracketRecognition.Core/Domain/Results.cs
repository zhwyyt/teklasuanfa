namespace TeklaBodyBracketRecognition.Core.Domain;

public sealed record ProfileMatchResult(
    MatchType MatchType,
    string? ProfileName,
    double Similarity);

public sealed record BodyRecognitionResult
{
    public required BodyType BodyType { get; init; }
    public required BodyFamily BodyFamily { get; init; }
    public required ProfileMatchResult ProfileMatch { get; init; }
    public required Vector3 BodyAxis { get; init; }
    public required (Vector3 X, Vector3 Y, Vector3 Z) BodyCoordinateSystem { get; init; }
    public required IReadOnlyList<int> CoreBodyPartIds { get; init; }
    public required IReadOnlyList<int> BodyAccessoryPartIds { get; init; }
    public required double AxisConfidence { get; init; }
    public required double ChainConfidence { get; init; }
    public required double SectionFamilyConfidence { get; init; }
    public required double ProfileMatchConfidence { get; init; }
    public required double BodyConfidence { get; init; }
    public required bool ReviewRequired { get; init; }
    public required IReadOnlyList<string> ReviewReasons { get; init; }
    public required IReadOnlyDictionary<string, double> SectionFamilyVotes { get; init; }
    public required IReadOnlyList<string> Diagnostics { get; init; }
    public required IReadOnlyList<VirtualPlateChain> VirtualPlateChains { get; init; }
}

public sealed record RootZone(
    Vector3 RootCentroid,
    (double Min, double Max) RootXInterval,
    double RootContactArea,
    double RootWeldLength,
    Vector3 RootSurfaceNormal,
    AttachedBodyFaceType AttachedBodyFaceType);

public sealed record BracketInstance
{
    public required string InstanceId { get; init; }
    public required BracketType Type { get; init; }
    public required IReadOnlyList<int> PartIds { get; init; }
    public required RootZone RootZone { get; init; }
    public required int PlateCount { get; init; }
    public required double WeldFeature { get; init; }
    public required double ShapeFeature { get; init; }
    public required double Score { get; init; }
    public required double Confidence { get; init; }
    public required IReadOnlyList<string> ClassificationReasons { get; init; }
}

public sealed record BracketRecognitionResult
{
    public required int BracketCount { get; init; }
    public required IReadOnlyList<BracketInstance> Instances { get; init; }
    public required double BracketTotalScore { get; init; }
    public required double BracketConfidence { get; init; }
    public required bool ReviewRequired { get; init; }
    public required IReadOnlyList<string> ReviewReasons { get; init; }
    public required IReadOnlyList<string> AppendageClusters { get; init; }
}

public sealed record AssemblyRecognitionResult
{
    public required string AssemblyId { get; init; }
    public required BodyRecognitionResult Body { get; init; }
    public required BracketRecognitionResult Brackets { get; init; }
}
