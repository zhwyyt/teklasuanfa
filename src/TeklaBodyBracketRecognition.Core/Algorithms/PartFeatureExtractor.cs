using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class PartFeatureExtractor
{
    private readonly RecognitionOptions _options;

    public PartFeatureExtractor(RecognitionOptions options)
    {
        _options = options;
    }

    public IReadOnlyList<PartFeature> Extract(IReadOnlyList<PartInput> parts)
    {
        return parts.Select(BuildFeature).ToArray();
    }

    private PartFeature BuildFeature(PartInput part)
    {
        var obb = part.ObbDims;
        var sortedDims = new[] { obb.X, obb.Y, obb.Z }.OrderByDescending(x => x).ToArray();
        var longDir = part.PlateLongDirection.Normalize();
        var normal = part.PlateNormal.Normalize();
        var widthDir = part.PlateWidthDirection.ProjectToPlane(normal).Normalize();
        if (widthDir.Length <= 1e-9)
        {
            widthDir = normal.Cross(longDir).Normalize();
        }

        longDir = longDir.ProjectToPlane(normal).Normalize();
        if (longDir.Length <= 1e-9)
        {
            longDir = widthDir.Cross(normal).Normalize();
        }

        return new PartFeature
        {
            PartId = part.PartId,
            RuntimeType = part.RuntimeType,
            Name = part.Name,
            Material = part.Material,
            ProfileString = part.ProfileString,
            IsPlateLike = part.IsPlateLike,
            IsSpecialShape = part.IsSpecialShape,
            Centroid = part.Centroid,
            ObbDims = part.ObbDims,
            BoundingBox = part.BoundingBox,
            Volume = part.Volume,
            SurfaceArea = part.SurfaceArea,
            Thickness = part.Thickness,
            PlateNormal = normal,
            PlateLongDirection = longDir,
            PlateWidthDirection = widthDir,
            MidPlane = new Plane(part.Centroid, normal),
            ContourVertexCount = part.ContourVertexCount,
            ConcaveCornerCount = part.ConcaveCornerCount,
            HoleLikeFeatureCount = part.HoleLikeFeatureCount,
            BooleanCutCount = part.BooleanCutCount,
            BooleanAddCount = part.BooleanAddCount,
            ShopWeldDegree = part.ShopWeldDegree,
            SiteWeldDegree = part.SiteWeldDegree,
            IsTinyPart = part.Volume < _options.TinyPartVolumeThreshold,
            LikelyConnectionPart = part.HoleLikeFeatureCount >= 4 && sortedDims[0] < 0.25 * (sortedDims[0] + sortedDims[1] + sortedDims[2]),
            AspectRatio = sortedDims[1] <= 1e-6 ? sortedDims[0] : sortedDims[0] / sortedDims[1]
        };
    }
}
