namespace TeklaBodyBracketRecognition.Core.Domain;

public sealed record AssemblyInput(
    string AssemblyId,
    int MainPartId,
    IReadOnlyList<PartInput> Parts,
    IReadOnlyList<RelationshipInput> Relationships);

public sealed record PartInput(
    int PartId,
    RuntimePartType RuntimeType,
    string Name,
    string Material,
    string ProfileString,
    bool IsPlateLike,
    bool IsSpecialShape,
    Vector3 Centroid,
    Vector3 ObbDims,
    BoundingBox BoundingBox,
    double Volume,
    double SurfaceArea,
    double? Thickness,
    Vector3 PlateNormal,
    Vector3 PlateLongDirection,
    Vector3 PlateWidthDirection,
    int ContourVertexCount,
    int ConcaveCornerCount,
    int HoleLikeFeatureCount,
    int BooleanCutCount,
    int BooleanAddCount,
    double ShopWeldDegree,
    double SiteWeldDegree);

public sealed record RelationshipInput(
    int PartIdA,
    int PartIdB,
    EdgeType EdgeType,
    double Strength,
    double GeometricSupport,
    string? Meta = null);
