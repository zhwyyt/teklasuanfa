using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;
using TeklaBodyBracketRecognition.Core.Utils;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class BracketRecognizer
{
    private readonly RecognitionOptions _options;

    public BracketRecognizer(RecognitionOptions options)
    {
        _options = options;
    }

    public BracketRecognitionResult Recognize(PartGraph graph, BodyRecognitionResult body)
    {
        var appendageCandidates = graph.Parts.Values
            .Where(part => !body.CoreBodyPartIds.Contains(part.PartId))
            .Where(part => !body.BodyAccessoryPartIds.Contains(part.PartId))
            .Where(part => !part.IsTinyPart)
            .Where(part => !part.LikelyConnectionPart)
            .ToArray();

        var clusters = BuildAppendageClusters(graph, appendageCandidates);
        var instances = new List<BracketInstance>();
        var clusterTags = new List<string>();
        var reviewReasons = new List<string>();

        foreach (var cluster in clusters)
        {
            clusterTags.Add($"{cluster.ClusterId}:{string.Join(",", cluster.PartIds.OrderBy(id => id))}");
            var instance = TryClassifyCluster(cluster, graph, body);
            if (instance is null)
            {
                continue;
            }

            instances.Add(instance);
            if (instance.Confidence < _options.ManualReviewConfidenceMin)
            {
                reviewReasons.Add("BRACKET_VS_STIFFENER_AMBIGUOUS");
            }
        }

        if (instances.Count == 0 && appendageCandidates.Length > 0)
        {
            reviewReasons.Add("BRACKET_GEOMETRY_INCOMPLETE");
        }

        return new BracketRecognitionResult
        {
            BracketCount = instances.Count,
            Instances = instances.OrderBy(instance => instance.InstanceId).ToArray(),
            BracketTotalScore = instances.Sum(instance => instance.Score),
            BracketConfidence = instances.Count == 0 ? 0 : instances.Average(instance => instance.Confidence),
            ReviewRequired = reviewReasons.Count > 0 || instances.Any(instance => instance.Confidence < _options.ManualReviewConfidenceMin),
            ReviewReasons = reviewReasons.Distinct().ToArray(),
            AppendageClusters = clusterTags
        };
    }

    private IReadOnlyList<AppendageCluster> BuildAppendageClusters(PartGraph graph, IReadOnlyList<PartFeature> candidates)
    {
        var remaining = candidates.ToDictionary(part => part.PartId);
        var clusters = new List<AppendageCluster>();
        var index = 1;

        while (remaining.Count > 0)
        {
            var seed = remaining.Keys.First();
            var queue = new Queue<int>();
            var partIds = new List<int>();
            queue.Enqueue(seed);
            remaining.Remove(seed);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                partIds.Add(current);
                foreach (var neighbor in graph.GetNeighbors(current, EdgeType.Weld, EdgeType.Contact, EdgeType.Boolean))
                {
                    if (remaining.Remove(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            var clusterParts = partIds.Select(id => graph.Parts[id]).ToArray();
            clusters.Add(new AppendageCluster(
                $"CL-{index++}",
                partIds.OrderBy(id => id).ToArray(),
                clusterParts));
        }

        return clusters;
    }

    private BracketInstance? TryClassifyCluster(AppendageCluster cluster, PartGraph graph, BodyRecognitionResult body)
    {
        var attachedCoreIds = cluster.PartIds
            .SelectMany(id => graph.GetNeighbors(id, EdgeType.Weld, EdgeType.Contact, EdgeType.Boolean))
            .Where(body.CoreBodyPartIds.Contains)
            .Distinct()
            .ToArray();

        if (attachedCoreIds.Length == 0)
        {
            return null;
        }

        var axis = body.BodyAxis.Normalize();
        var xValues = cluster.Parts.Select(part => GeometryUtil.ProjectScalar(part.Centroid, axis)).ToArray();
        var spanX = xValues.Max() - xValues.Min();
        var clusterBox = BoundingBox.FromPoints(cluster.Parts.SelectMany(part => new[] { part.BoundingBox.Min, part.BoundingBox.Max }));
        var span = clusterBox.Size;
        var spanPerp = Math.Sqrt((span.Y * span.Y) + (span.Z * span.Z));
        var cantileverRatio = spanPerp / Math.Max(1.0, spanX);
        var totalWeld = cluster.Parts.Sum(part => part.ShopWeldDegree);
        var totalHoles = cluster.Parts.Sum(part => part.HoleLikeFeatureCount);

        if (totalHoles >= 6 && xValues.Max() >= body.VirtualPlateChains.Select(chain => chain.UnionIntervalX.Max).DefaultIfEmpty(0).Max())
        {
            return null;
        }

        var isBracket = cantileverRatio >= _options.BracketOverhangRatioMin &&
                        spanX <= Math.Max(1.0, BodySpan(body) * _options.BracketAxisSpanRatioMax);

        if (!isBracket)
        {
            return null;
        }

        var type = cluster.Parts.Count switch
        {
            <= 2 => BracketType.SimplePlate,
            <= 4 when cluster.Parts.Any(part => part.ConcaveCornerCount > 0) => BracketType.IrregularBracket,
            <= 5 => BracketType.RibbedBracket,
            _ => BracketType.BoxBracket
        };

        var rootX = attachedCoreIds
            .Select(id => graph.Parts[id].Centroid.Dot(axis))
            .ToArray();
        var rootCentroid = Average(cluster.Parts.Select(part => part.Centroid));
        var rootZone = new RootZone(
            rootCentroid,
            (rootX.Min(), rootX.Max()),
            cluster.Parts.Sum(part => part.SurfaceArea) * 0.1,
            totalWeld * 10.0,
            body.BodyCoordinateSystem.Z,
            InferFaceType(body, rootCentroid));

        var confidence = GeometryUtil.Clamp01(
            0.35 +
            (0.25 * Math.Min(1.0, cantileverRatio / 2.0)) +
            (0.20 * Math.Min(1.0, totalWeld / 4.0)) +
            (0.20 * Math.Min(1.0, cluster.Parts.Count / 5.0)));

        var score = BaseScore(type) +
                    (0.2 * cluster.Parts.Count) +
                    (0.1 * cluster.Parts.Sum(part => part.BooleanCutCount + part.BooleanAddCount)) +
                    (0.05 * totalWeld);

        return new BracketInstance
        {
            InstanceId = $"BKT-{cluster.ClusterId}",
            Type = type,
            PartIds = cluster.PartIds,
            RootZone = rootZone,
            PlateCount = cluster.Parts.Count,
            WeldFeature = totalWeld,
            ShapeFeature = cluster.Parts.Sum(part => part.ContourVertexCount + (2 * part.ConcaveCornerCount)),
            Score = Math.Round(score, 2),
            Confidence = confidence,
            ClassificationReasons = new[]
            {
                $"cantilever_ratio={cantileverRatio:0.##}",
                $"span_x={spanX:0.##}",
                $"attached_core_count={attachedCoreIds.Length}"
            }
        };
    }

    private static double BodySpan(BodyRecognitionResult body)
    {
        if (body.VirtualPlateChains.Count == 0)
        {
            return 1.0;
        }

        return body.VirtualPlateChains.Max(chain => chain.UnionIntervalX.Max) -
               body.VirtualPlateChains.Min(chain => chain.UnionIntervalX.Min);
    }

    private static AttachedBodyFaceType InferFaceType(BodyRecognitionResult body, Vector3 rootCentroid)
    {
        var offset = rootCentroid - body.BodyCoordinateSystem.X;
        var y = Math.Abs(offset.Dot(body.BodyCoordinateSystem.Y));
        var z = Math.Abs(offset.Dot(body.BodyCoordinateSystem.Z));
        if (z > y)
        {
            return offset.Dot(body.BodyCoordinateSystem.Z) >= 0 ? AttachedBodyFaceType.FlangeTop : AttachedBodyFaceType.FlangeBottom;
        }

        return AttachedBodyFaceType.Web;
    }

    private static double BaseScore(BracketType type) =>
        type switch
        {
            BracketType.SimplePlate => 2.0,
            BracketType.RibbedBracket => 4.0,
            BracketType.BoxBracket => 6.0,
            BracketType.IrregularBracket => 5.5,
            _ => 1.0
        };

    private static Vector3 Average(IEnumerable<Vector3> values)
    {
        var materialized = values.ToArray();
        if (materialized.Length == 0)
        {
            return Vector3.Zero;
        }

        return new Vector3(
            materialized.Average(value => value.X),
            materialized.Average(value => value.Y),
            materialized.Average(value => value.Z));
    }

    private sealed record AppendageCluster(string ClusterId, IReadOnlyList<int> PartIds, IReadOnlyList<PartFeature> Parts);
}
