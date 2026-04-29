using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

internal static class SectionClosedLoopEvidence
{
    [Flags]
    private enum EnvelopeSide
    {
        None = 0,
        MinY = 1 << 0,
        MaxY = 1 << 1,
        MinZ = 1 << 2,
        MaxZ = 1 << 3
    }

    public static bool HasTrueClosedLoop(
        IReadOnlyList<SectionTraceCleanSegment> retained,
        double minY,
        double maxY,
        double minZ,
        double maxZ,
        double tolerance)
    {
        var bodyCandidates = retained
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .ToArray();
        if (bodyCandidates.Length < 4)
        {
            return false;
        }

        var envelopeSegments = bodyCandidates
            .Where(item => TouchesEnvelope(item, minY, maxY, minZ, maxZ, tolerance))
            .ToArray();
        if (envelopeSegments.Length < 4)
        {
            return false;
        }

        if (HasEndpointCycle(envelopeSegments, minY, maxY, minZ, maxZ, tolerance))
        {
            return true;
        }

        if (HasExtendedLineLoop(envelopeSegments, minY, maxY, minZ, maxZ, tolerance))
        {
            return true;
        }

        return HasEnvelopeBoundaryLoop(envelopeSegments, minY, maxY, minZ, maxZ, tolerance);
    }

    public static bool TouchesEnvelope(
        SectionTraceCleanSegment segment,
        double minY,
        double maxY,
        double minZ,
        double maxZ,
        double tolerance)
    {
        return GetTouchedSides(segment, minY, maxY, minZ, maxZ, tolerance) != EnvelopeSide.None;
    }

    private static bool HasEndpointCycle(
        IReadOnlyList<SectionTraceCleanSegment> envelopeSegments,
        double minY,
        double maxY,
        double minZ,
        double maxZ,
        double tolerance)
    {
        
        var nodes = new List<(double Y, double Z)>();
        var segmentNodes = new (int StartNode, int EndNode)[envelopeSegments.Count];
        var nodeDegrees = new Dictionary<int, int>();
        var nodeToSegments = new Dictionary<int, List<int>>();
        var segmentSides = new EnvelopeSide[envelopeSegments.Count];

        for (var i = 0; i < envelopeSegments.Count; i++)
        {
            var segment = envelopeSegments[i];
            var startNode = FindOrAddNode(nodes, segment.StartY, segment.StartZ, tolerance);
            var endNode = FindOrAddNode(nodes, segment.EndY, segment.EndZ, tolerance);
            if (startNode == endNode)
            {
                continue;
            }

            segmentNodes[i] = (startNode, endNode);
            Increment(nodeDegrees, startNode);
            Increment(nodeDegrees, endNode);
            AddSegment(nodeToSegments, startNode, i);
            AddSegment(nodeToSegments, endNode, i);
            segmentSides[i] = GetTouchedSides(segment, minY, maxY, minZ, maxZ, tolerance);
        }

        var visitedSegments = new HashSet<int>();
        for (var i = 0; i < envelopeSegments.Count; i++)
        {
            if (visitedSegments.Contains(i) || segmentSides[i] == EnvelopeSide.None)
            {
                continue;
            }

            var componentSegments = CollectComponentSegments(i, segmentNodes, nodeToSegments, visitedSegments);
            if (componentSegments.Count < 4)
            {
                continue;
            }

            var componentNodes = new HashSet<int>();
            var componentSides = EnvelopeSide.None;
            foreach (var segmentIndex in componentSegments)
            {
                var nodesOfSegment = segmentNodes[segmentIndex];
                componentNodes.Add(nodesOfSegment.StartNode);
                componentNodes.Add(nodesOfSegment.EndNode);
                componentSides |= segmentSides[segmentIndex];
            }

            if (componentNodes.Count < 4 ||
                componentSides != (EnvelopeSide.MinY | EnvelopeSide.MaxY | EnvelopeSide.MinZ | EnvelopeSide.MaxZ))
            {
                continue;
            }

            if (componentNodes.All(node => nodeDegrees.TryGetValue(node, out var degree) && degree == 2))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasEnvelopeBoundaryLoop(
        IReadOnlyList<SectionTraceCleanSegment> envelopeSegments,
        double minY,
        double maxY,
        double minZ,
        double maxZ,
        double tolerance)
    {
        var touchedSides = envelopeSegments
            .Select(item => GetTouchedSides(item, minY, maxY, minZ, maxZ, tolerance))
            .Aggregate(EnvelopeSide.None, (current, side) => current | side);
        if (touchedSides != (EnvelopeSide.MinY | EnvelopeSide.MaxY | EnvelopeSide.MinZ | EnvelopeSide.MaxZ))
        {
            return false;
        }

        if (!HasMultipleDirectionFamilies(envelopeSegments, tolerance))
        {
            return false;
        }

        var hull = BuildSimplifiedHull(envelopeSegments, tolerance);
        if (hull.Count < 4)
        {
            return false;
        }

        var lineTolerance = Math.Max(tolerance * 1.5, 10.0);
        var usedSegmentIndexes = new HashSet<int>();
        double coveredPerimeter = 0.0;
        double perimeter = 0.0;

        for (var i = 0; i < hull.Count; i++)
        {
            var start = hull[i];
            var end = hull[(i + 1) % hull.Count];
            var edgeLength = Distance(start, end);
            if (edgeLength <= 1e-6)
            {
                continue;
            }

            perimeter += edgeLength;
            var supportingIndex = FindSupportingSegmentIndex(envelopeSegments, start, end, lineTolerance);
            if (supportingIndex < 0)
            {
                continue;
            }

            usedSegmentIndexes.Add(supportingIndex);
            coveredPerimeter += edgeLength;
        }

        return usedSegmentIndexes.Count >= 4 &&
               perimeter > 1e-6 &&
               coveredPerimeter / perimeter >= 0.80;
    }

    private static bool HasExtendedLineLoop(
        IReadOnlyList<SectionTraceCleanSegment> envelopeSegments,
        double minY,
        double maxY,
        double minZ,
        double maxZ,
        double tolerance)
    {
        if (envelopeSegments.Count < 4)
        {
            return false;
        }

        var envelopeCenter = (
            Y: (minY + maxY) * 0.5d,
            Z: (minZ + maxZ) * 0.5d);
        var orderedSegments = envelopeSegments
            .Select(
                segment => new
                {
                    Segment = segment,
                    Angle = Math.Atan2(segment.CenterZ - envelopeCenter.Z, segment.CenterY - envelopeCenter.Y)
                })
            .OrderBy(item => item.Angle)
            .Select(item => item.Segment)
            .ToArray();
        if (orderedSegments.Length < 4)
        {
            return false;
        }

        // Wall-midline closures on beveled box corners routinely miss the exact hull vertex
        // by roughly one plate thickness on each adjacent wall. Keep this allowance local to
        // the extended-line closure path instead of inflating the global envelope tolerance.
        var vertexTolerance = Math.Max(tolerance * 4.0d, 60.0d);
        var envelopeSlack = Math.Max(tolerance * 5.0d, 80.0d);
        var polygon = new List<(double Y, double Z)>(orderedSegments.Length);

        for (var i = 0; i < orderedSegments.Length; i++)
        {
            var current = orderedSegments[i];
            var next = orderedSegments[(i + 1) % orderedSegments.Length];
            if (!TryIntersectSupportingLines(current, next, out var vertex))
            {
                return false;
            }

            if (vertex.Y < minY - envelopeSlack ||
                vertex.Y > maxY + envelopeSlack ||
                vertex.Z < minZ - envelopeSlack ||
                vertex.Z > maxZ + envelopeSlack)
            {
                return false;
            }

            if (DistanceBeyondSegment(vertex, current) > vertexTolerance ||
                DistanceBeyondSegment(vertex, next) > vertexTolerance)
            {
                return false;
            }

            polygon.Add(vertex);
        }

        var orderedPolygon = OrderPolygonVertices(polygon);
        var simplifiedPolygon = SimplifyPolygonVertices(orderedPolygon, tolerance);
        if (simplifiedPolygon.Count < 4)
        {
            return false;
        }

        if (!IsConsistentlyOrientedPolygon(simplifiedPolygon, tolerance))
        {
            return false;
        }

        var polygonArea = Math.Abs(ComputeSignedArea(simplifiedPolygon));
        var envelopeArea = Math.Max((maxY - minY) * (maxZ - minZ), 1.0d);
        if (polygonArea < envelopeArea * 0.35d)
        {
            return false;
        }

        return IsPointInsidePolygon(envelopeCenter, simplifiedPolygon, tolerance);
    }

    private static EnvelopeSide GetTouchedSides(
        SectionTraceCleanSegment segment,
        double minY,
        double maxY,
        double minZ,
        double maxZ,
        double tolerance)
    {
        var segmentMinY = Math.Min(segment.StartY, segment.EndY);
        var segmentMaxY = Math.Max(segment.StartY, segment.EndY);
        var segmentMinZ = Math.Min(segment.StartZ, segment.EndZ);
        var segmentMaxZ = Math.Max(segment.StartZ, segment.EndZ);

        var side = EnvelopeSide.None;
        if (segmentMinY <= minY + tolerance)
        {
            side |= EnvelopeSide.MinY;
        }

        if (segmentMaxY >= maxY - tolerance)
        {
            side |= EnvelopeSide.MaxY;
        }

        if (segmentMinZ <= minZ + tolerance)
        {
            side |= EnvelopeSide.MinZ;
        }

        if (segmentMaxZ >= maxZ - tolerance)
        {
            side |= EnvelopeSide.MaxZ;
        }

        return side;
    }

    private static int FindOrAddNode(List<(double Y, double Z)> nodes, double y, double z, double tolerance)
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (AxisAlignedDistance(node.Y, node.Z, y, z) <= tolerance)
            {
                return i;
            }
        }

        nodes.Add((y, z));
        return nodes.Count - 1;
    }

    private static bool HasMultipleDirectionFamilies(
        IReadOnlyList<SectionTraceCleanSegment> segments,
        double tolerance)
    {
        var angles = new List<double>();
        var angleTolerance = ResolveAngleTolerance(tolerance);

        foreach (var segment in segments)
        {
            var angle = NormalizeAngleDegrees(
                Math.Atan2(segment.EndZ - segment.StartZ, segment.EndY - segment.StartY) * 180.0 / Math.PI);
            if (angles.Any(existing => AngularDistanceDegrees(existing, angle) <= angleTolerance))
            {
                continue;
            }

            angles.Add(angle);
        }

        return angles.Count >= 2;
    }

    private static double ResolveAngleTolerance(double tolerance)
    {
        if (tolerance >= 10.0)
        {
            return 18.0;
        }

        return 12.0;
    }

    private static double NormalizeAngleDegrees(double angle)
    {
        var normalized = angle % 180.0;
        if (normalized < 0.0)
        {
            normalized += 180.0;
        }

        return normalized;
    }

    private static double AngularDistanceDegrees(double left, double right)
    {
        var distance = Math.Abs(left - right);
        return Math.Min(distance, 180.0 - distance);
    }

    private static List<(double Y, double Z)> BuildSimplifiedHull(
        IReadOnlyList<SectionTraceCleanSegment> segments,
        double tolerance)
    {
        var points = segments
            .SelectMany(
                item => new[]
                {
                    (item.StartY, item.StartZ),
                    (item.EndY, item.EndZ)
                })
            .Distinct()
            .ToArray();
        if (points.Length < 4)
        {
            return points.ToList();
        }

        var sorted = points
            .OrderBy(item => item.Item1)
            .ThenBy(item => item.Item2)
            .ToArray();
        var lower = new List<(double Y, double Z)>();
        foreach (var point in sorted)
        {
            while (lower.Count >= 2 &&
                   Cross(lower[^2], lower[^1], point) <= 0.0)
            {
                lower.RemoveAt(lower.Count - 1);
            }

            lower.Add(point);
        }

        var upper = new List<(double Y, double Z)>();
        for (var i = sorted.Length - 1; i >= 0; i--)
        {
            var point = sorted[i];
            while (upper.Count >= 2 &&
                   Cross(upper[^2], upper[^1], point) <= 0.0)
            {
                upper.RemoveAt(upper.Count - 1);
            }

            upper.Add(point);
        }

        var hull = lower
            .Take(Math.Max(lower.Count - 1, 0))
            .Concat(upper.Take(Math.Max(upper.Count - 1, 0)))
            .ToList();

        return SimplifyHull(hull, tolerance);
    }

    private static List<(double Y, double Z)> SimplifyHull(
        IReadOnlyList<(double Y, double Z)> hull,
        double tolerance)
    {
        if (hull.Count < 4)
        {
            return hull.ToList();
        }

        var simplified = hull.ToList();
        var changed = true;
        var lineTolerance = Math.Max(tolerance, 8.0);

        while (changed && simplified.Count >= 4)
        {
            changed = false;
            for (var i = 0; i < simplified.Count; i++)
            {
                var previous = simplified[(i - 1 + simplified.Count) % simplified.Count];
                var current = simplified[i];
                var next = simplified[(i + 1) % simplified.Count];
                if (DistancePointToLine(current, previous, next) <= lineTolerance)
                {
                    simplified.RemoveAt(i);
                    changed = true;
                    break;
                }
            }
        }

        return simplified;
    }

    private static int FindSupportingSegmentIndex(
        IReadOnlyList<SectionTraceCleanSegment> segments,
        (double Y, double Z) edgeStart,
        (double Y, double Z) edgeEnd,
        double tolerance)
    {
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (DistancePointToSegment(edgeStart, segment) <= tolerance &&
                DistancePointToSegment(edgeEnd, segment) <= tolerance)
            {
                return i;
            }

            if (SupportsHullEdgeWithEndpointBridge(segment, edgeStart, edgeEnd, tolerance))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool SupportsHullEdgeWithEndpointBridge(
        SectionTraceCleanSegment segment,
        (double Y, double Z) edgeStart,
        (double Y, double Z) edgeEnd,
        double tolerance)
    {
        var relaxedTolerance = Math.Max(tolerance * 2.0d, 50.0d);
        var directMatch =
            Distance(edgeStart, (segment.StartY, segment.StartZ)) <= relaxedTolerance &&
            Distance(edgeEnd, (segment.EndY, segment.EndZ)) <= relaxedTolerance;
        var reversedMatch =
            Distance(edgeStart, (segment.EndY, segment.EndZ)) <= relaxedTolerance &&
            Distance(edgeEnd, (segment.StartY, segment.StartZ)) <= relaxedTolerance;
        if (!directMatch && !reversedMatch)
        {
            return false;
        }

        var edgeLength = Distance(edgeStart, edgeEnd);
        var segmentLength = Distance((segment.StartY, segment.StartZ), (segment.EndY, segment.EndZ));
        return segmentLength >= edgeLength * 0.45d;
    }

    private static bool TryIntersectSupportingLines(
        SectionTraceCleanSegment left,
        SectionTraceCleanSegment right,
        out (double Y, double Z) intersection)
    {
        var leftDeltaY = left.EndY - left.StartY;
        var leftDeltaZ = left.EndZ - left.StartZ;
        var rightDeltaY = right.EndY - right.StartY;
        var rightDeltaZ = right.EndZ - right.StartZ;
        var denominator = (leftDeltaY * rightDeltaZ) - (leftDeltaZ * rightDeltaY);
        if (Math.Abs(denominator) <= 1e-6)
        {
            intersection = default;
            return false;
        }

        var startOffsetY = right.StartY - left.StartY;
        var startOffsetZ = right.StartZ - left.StartZ;
        var ratio = ((startOffsetY * rightDeltaZ) - (startOffsetZ * rightDeltaY)) / denominator;
        intersection = (
            left.StartY + (leftDeltaY * ratio),
            left.StartZ + (leftDeltaZ * ratio));
        return true;
    }

    private static double DistanceBeyondSegment(
        (double Y, double Z) point,
        SectionTraceCleanSegment segment)
    {
        var deltaY = segment.EndY - segment.StartY;
        var deltaZ = segment.EndZ - segment.StartZ;
        var lengthSquared = (deltaY * deltaY) + (deltaZ * deltaZ);
        if (lengthSquared <= 1e-6)
        {
            return Distance(point, (segment.StartY, segment.StartZ));
        }

        var projection = ((point.Y - segment.StartY) * deltaY) +
                         ((point.Z - segment.StartZ) * deltaZ);
        var ratio = projection / lengthSquared;
        if (ratio >= 0.0d && ratio <= 1.0d)
        {
            return 0.0d;
        }

        var endpoint = ratio < 0.0d
            ? (segment.StartY, segment.StartZ)
            : (segment.EndY, segment.EndZ);
        return Distance(point, endpoint);
    }

    private static List<(double Y, double Z)> SimplifyPolygonVertices(
        IReadOnlyList<(double Y, double Z)> polygon,
        double tolerance)
    {
        var simplified = new List<(double Y, double Z)>();
        foreach (var point in polygon)
        {
            if (simplified.Count > 0 && Distance(simplified[^1], point) <= tolerance)
            {
                continue;
            }

            simplified.Add(point);
        }

        if (simplified.Count >= 2 && Distance(simplified[0], simplified[^1]) <= tolerance)
        {
            simplified.RemoveAt(simplified.Count - 1);
        }

        return simplified;
    }

    private static List<(double Y, double Z)> OrderPolygonVertices(
        IReadOnlyList<(double Y, double Z)> polygon)
    {
        var centroid = (
            Y: polygon.Average(point => point.Y),
            Z: polygon.Average(point => point.Z));
        return polygon
            .OrderBy(point => Math.Atan2(point.Z - centroid.Z, point.Y - centroid.Y))
            .ToList();
    }

    private static bool IsConsistentlyOrientedPolygon(
        IReadOnlyList<(double Y, double Z)> polygon,
        double tolerance)
    {
        if (polygon.Count < 4)
        {
            return false;
        }

        double orientation = 0.0d;
        var edgeTolerance = Math.Max(tolerance, 5.0d);

        for (var i = 0; i < polygon.Count; i++)
        {
            var previous = polygon[(i - 1 + polygon.Count) % polygon.Count];
            var current = polygon[i];
            var next = polygon[(i + 1) % polygon.Count];
            if (Distance(previous, current) <= edgeTolerance || Distance(current, next) <= edgeTolerance)
            {
                return false;
            }

            var cross = Cross(previous, current, next);
            if (Math.Abs(cross) <= edgeTolerance)
            {
                continue;
            }

            if (orientation == 0.0d)
            {
                orientation = Math.Sign(cross);
                continue;
            }

            if (Math.Sign(cross) != Math.Sign(orientation))
            {
                return false;
            }
        }

        return orientation != 0.0d;
    }

    private static double ComputeSignedArea(IReadOnlyList<(double Y, double Z)> polygon)
    {
        var area = 0.0d;
        for (var i = 0; i < polygon.Count; i++)
        {
            var current = polygon[i];
            var next = polygon[(i + 1) % polygon.Count];
            area += (current.Y * next.Z) - (next.Y * current.Z);
        }

        return area * 0.5d;
    }

    private static bool IsPointInsidePolygon(
        (double Y, double Z) point,
        IReadOnlyList<(double Y, double Z)> polygon,
        double tolerance)
    {
        var inside = false;
        for (var i = 0; i < polygon.Count; i++)
        {
            var current = polygon[i];
            var next = polygon[(i + 1) % polygon.Count];
            if (DistancePointToLine(point, current, next) <= tolerance &&
                DistancePointToSegment(point, current, next) <= tolerance)
            {
                return true;
            }

            var crosses = ((current.Z > point.Z) != (next.Z > point.Z)) &&
                          (point.Y < ((next.Y - current.Y) * (point.Z - current.Z) / ((next.Z - current.Z) + 1e-12)) + current.Y);
            if (crosses)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static double AxisAlignedDistance(double leftY, double leftZ, double rightY, double rightZ)
    {
        return Math.Max(Math.Abs(leftY - rightY), Math.Abs(leftZ - rightZ));
    }

    private static double Cross((double Y, double Z) origin, (double Y, double Z) left, (double Y, double Z) right)
    {
        return ((left.Y - origin.Y) * (right.Z - origin.Z)) -
               ((left.Z - origin.Z) * (right.Y - origin.Y));
    }

    private static double Distance((double Y, double Z) left, (double Y, double Z) right)
    {
        var deltaY = left.Y - right.Y;
        var deltaZ = left.Z - right.Z;
        return Math.Sqrt((deltaY * deltaY) + (deltaZ * deltaZ));
    }

    private static double DistancePointToLine(
        (double Y, double Z) point,
        (double Y, double Z) lineStart,
        (double Y, double Z) lineEnd)
    {
        var deltaY = lineEnd.Y - lineStart.Y;
        var deltaZ = lineEnd.Z - lineStart.Z;
        var denominator = Math.Sqrt((deltaY * deltaY) + (deltaZ * deltaZ));
        if (denominator <= 1e-6)
        {
            return Distance(point, lineStart);
        }

        var numerator = Math.Abs(
            ((point.Y - lineStart.Y) * deltaZ) -
            ((point.Z - lineStart.Z) * deltaY));
        return numerator / denominator;
    }

    private static double DistancePointToSegment(
        (double Y, double Z) point,
        SectionTraceCleanSegment segment)
    {
        return DistancePointToSegment(point, (segment.StartY, segment.StartZ), (segment.EndY, segment.EndZ));
    }

    private static double DistancePointToSegment(
        (double Y, double Z) point,
        (double Y, double Z) segmentStart,
        (double Y, double Z) segmentEnd)
    {
        var deltaY = segmentEnd.Y - segmentStart.Y;
        var deltaZ = segmentEnd.Z - segmentStart.Z;
        var lengthSquared = (deltaY * deltaY) + (deltaZ * deltaZ);
        if (lengthSquared <= 1e-6)
        {
            return Distance(point, segmentStart);
        }

        var projection = ((point.Y - segmentStart.Y) * deltaY) +
                         ((point.Z - segmentStart.Z) * deltaZ);
        var ratio = Math.Clamp(projection / lengthSquared, 0.0, 1.0);
        var nearest = (
            segmentStart.Y + (ratio * deltaY),
            segmentStart.Z + (ratio * deltaZ));
        return Distance(point, nearest);
    }

    private static void Increment(Dictionary<int, int> degrees, int nodeId)
    {
        degrees[nodeId] = degrees.TryGetValue(nodeId, out var degree) ? degree + 1 : 1;
    }

    private static void AddSegment(Dictionary<int, List<int>> lookup, int nodeId, int segmentIndex)
    {
        if (!lookup.TryGetValue(nodeId, out var segments))
        {
            segments = new List<int>();
            lookup[nodeId] = segments;
        }

        segments.Add(segmentIndex);
    }

    private static HashSet<int> CollectComponentSegments(
        int seedSegment,
        IReadOnlyList<(int StartNode, int EndNode)> segmentNodes,
        IReadOnlyDictionary<int, List<int>> nodeToSegments,
        ISet<int> visitedSegments)
    {
        var componentSegments = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(seedSegment);

        while (queue.Count > 0)
        {
            var segmentIndex = queue.Dequeue();
            if (!componentSegments.Add(segmentIndex))
            {
                continue;
            }

            visitedSegments.Add(segmentIndex);
            var nodes = segmentNodes[segmentIndex];
            foreach (var nodeId in new[] { nodes.StartNode, nodes.EndNode })
            {
                if (!nodeToSegments.TryGetValue(nodeId, out var connectedSegments))
                {
                    continue;
                }

                foreach (var connected in connectedSegments)
                {
                    if (!componentSegments.Contains(connected))
                    {
                        queue.Enqueue(connected);
                    }
                }
            }
        }

        return componentSegments;
    }
}
