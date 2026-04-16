using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class PartGraphBuilder
{
    private readonly RecognitionOptions _options;

    public PartGraphBuilder(RecognitionOptions options)
    {
        _options = options;
    }

    public PartGraph Build(IReadOnlyList<PartFeature> features, IReadOnlyList<RelationshipInput> relationships)
    {
        var parts = features.ToDictionary(feature => feature.PartId);
        var edges = new List<PartEdge>(relationships.Count);

        edges.AddRange(
            relationships
                .Where(rel => parts.ContainsKey(rel.PartIdA) && parts.ContainsKey(rel.PartIdB))
                .Select(rel => new PartEdge(rel.PartIdA, rel.PartIdB, rel.EdgeType, rel.Strength, rel.GeometricSupport, rel.Meta)));

        foreach (var pair in Pairwise(features))
        {
            if (edges.Any(edge => Connects(edge, pair.Left.PartId, pair.Right.PartId)))
            {
                continue;
            }

            if (pair.Left.BoundingBox.Expand(_options.ContactDistanceToleranceMm)
                .Intersects(pair.Right.BoundingBox.Expand(_options.ContactDistanceToleranceMm)))
            {
                edges.Add(new PartEdge(pair.Left.PartId, pair.Right.PartId, EdgeType.Contact, 0.35, 0.75, "bbox-contact-fallback"));
            }
        }

        return new PartGraph(parts, edges);
    }

    private static bool Connects(PartEdge edge, int a, int b) =>
        (edge.PartIdA == a && edge.PartIdB == b) || (edge.PartIdA == b && edge.PartIdB == a);

    private static IEnumerable<(PartFeature Left, PartFeature Right)> Pairwise(IReadOnlyList<PartFeature> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                yield return (items[i], items[j]);
            }
        }
    }
}
