namespace TeklaBodyBracketRecognition.Core.Domain;

public sealed record PartFeature
{
    public required int PartId { get; init; }
    public required RuntimePartType RuntimeType { get; init; }
    public required string Name { get; init; }
    public required string Material { get; init; }
    public required string ProfileString { get; init; }
    public required bool IsPlateLike { get; init; }
    public required bool IsSpecialShape { get; init; }
    public required Vector3 Centroid { get; init; }
    public required Vector3 ObbDims { get; init; }
    public required BoundingBox BoundingBox { get; init; }
    public required double Volume { get; init; }
    public required double SurfaceArea { get; init; }
    public required double? Thickness { get; init; }
    public required Vector3 PlateNormal { get; init; }
    public required Vector3 PlateLongDirection { get; init; }
    public required Vector3 PlateWidthDirection { get; init; }
    public required Plane MidPlane { get; init; }
    public required int ContourVertexCount { get; init; }
    public required int ConcaveCornerCount { get; init; }
    public required int HoleLikeFeatureCount { get; init; }
    public required int BooleanCutCount { get; init; }
    public required int BooleanAddCount { get; init; }
    public required double ShopWeldDegree { get; init; }
    public required double SiteWeldDegree { get; init; }
    public required PartSemanticRole SemanticRole { get; init; }
    public required double SemanticRoleScore { get; init; }
    public required bool NearMemberStart { get; init; }
    public required bool NearMemberEnd { get; init; }
    public required bool OuterSideCandidate { get; init; }
    public required double Planarity { get; init; }
    public required IReadOnlyList<LineSegment3> SolidEdges { get; init; }
    public required bool IsTinyPart { get; init; }
    public required bool LikelyConnectionPart { get; init; }
    public required double AspectRatio { get; init; }
}

public sealed record PartEdge(
    int PartIdA,
    int PartIdB,
    EdgeType EdgeType,
    double Strength,
    double GeometricSupport,
    string? Meta);

public sealed record PartGraph(
    IReadOnlyDictionary<int, PartFeature> Parts,
    IReadOnlyList<PartEdge> Edges)
{
    public IEnumerable<PartEdge> GetEdgesFor(int partId) =>
        Edges.Where(edge => edge.PartIdA == partId || edge.PartIdB == partId);

    public IEnumerable<int> GetNeighbors(int partId, params EdgeType[] types)
    {
        var allowed = types.Length == 0 ? null : types.ToHashSet();
        return GetEdgesFor(partId)
            .Where(edge => allowed is null || allowed.Contains(edge.EdgeType))
            .Select(edge => edge.PartIdA == partId ? edge.PartIdB : edge.PartIdA)
            .Distinct();
    }
}

public sealed record VirtualPlateChain
{
    public required string ChainId { get; init; }
    public required IReadOnlyList<int> PartIds { get; init; }
    public required Vector3 MeanNormal { get; init; }
    public required Vector3 MeanLongDirection { get; init; }
    public required double MeanTransverseLocation { get; init; }
    public required (double Min, double Max) ThicknessStats { get; init; }
    public required (double Min, double Max) UnionIntervalX { get; init; }
    public required double ContinuityScore { get; init; }
    public required double Persistence { get; init; }
    public required bool IsLongitudinalChain { get; init; }
}
