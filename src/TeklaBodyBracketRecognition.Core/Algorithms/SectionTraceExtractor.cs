using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class SectionTraceExtractor
{
    private readonly RecognitionOptions _options;

    public SectionTraceExtractor(RecognitionOptions options)
    {
        _options = options;
    }

    public SectionTraceExtractionResult Extract(
        BodyCandidatePartitionResult partition,
        SectionStationPlanResult stationPlan,
        PartGraph graph)
    {
        var defaultSectionAxisX = partition.ProvisionalBodyAxis.Normalize();
        var defaultSectionAxisY = ResolveSectionAxisY(partition.Items, graph, defaultSectionAxisX);
        var defaultSectionAxisZ = defaultSectionAxisX.Cross(defaultSectionAxisY).Normalize();

        var partitionLookup = partition.Items.ToDictionary(item => item.PartId);
        var stations = new List<SectionTraceStationResult>(stationPlan.Stations.Count);

        foreach (var station in stationPlan.Stations)
        {
            var stationItems = partition.Items
                .Where(
                    item =>
                        station.AxisPosition >= item.AxisIntervalMin - _options.ContactDistanceToleranceMm &&
                        station.AxisPosition <= item.AxisIntervalMax + _options.ContactDistanceToleranceMm)
                .ToArray();
            var sectionAxisX = ResolveSectionAxisX(partition, station.AxisPosition, defaultSectionAxisX);
            var sectionPlanePoint = ResolveSectionAxisPoint(partition, station.AxisPosition, sectionAxisX);
            var sectionAxisY = ResolveSectionAxisY(stationItems, graph, sectionAxisX);
            if (sectionAxisY.Length <= 1e-6)
            {
                sectionAxisY = defaultSectionAxisY;
            }

            var sectionAxisZ = sectionAxisX.Cross(sectionAxisY).Normalize();
            if (sectionAxisZ.Length <= 1e-6)
            {
                sectionAxisZ = defaultSectionAxisZ;
            }

            var segments = new List<SectionTraceSegment>();
            foreach (var item in stationItems)
            {
                if (!graph.Parts.TryGetValue(item.PartId, out var part) || !part.IsPlateLike)
                {
                    continue;
                }

                if (!TryResolveTraceGeometries(
                        part,
                        station.AxisPosition,
                        sectionPlanePoint,
                        sectionAxisX,
                        sectionAxisY,
                        sectionAxisZ,
                        out var traceGeometries))
                {
                    continue;
                }

                for (var geometryIndex = 0; geometryIndex < traceGeometries.Count; geometryIndex++)
                {
                    var geometry = traceGeometries[geometryIndex];
                    segments.Add(
                        new SectionTraceSegment
                        {
                            TraceId = $"{station.StationId}-part-{part.PartId}-edge-{geometryIndex + 1:00}",
                            StationId = station.StationId,
                            PartId = part.PartId,
                            PartName = string.IsNullOrWhiteSpace(part.Name) ? "<unnamed>" : part.Name,
                            ProfileString = part.ProfileString,
                            PartitionClass = partitionLookup[part.PartId].PartitionClass,
                            CenterY = Math.Round(geometry.CenterY, 3),
                            CenterZ = Math.Round(geometry.CenterZ, 3),
                            StartY = Math.Round(geometry.StartY, 3),
                            StartZ = Math.Round(geometry.StartZ, 3),
                            EndY = Math.Round(geometry.EndY, 3),
                            EndZ = Math.Round(geometry.EndZ, 3),
                            WidthLength = Math.Round(Math.Sqrt(Math.Pow(geometry.EndY - geometry.StartY, 2) + Math.Pow(geometry.EndZ - geometry.StartZ, 2)), 3),
                            Reasons = new[]
                            {
                                "SECTION_TRACE_FROM_PART_GEOMETRY",
                                geometry.Reason,
                                $"PARTITION_{partitionLookup[part.PartId].PartitionClass}"
                            }
                    });
                }
            }

            stations.Add(
                new SectionTraceStationResult
                {
                    StationId = station.StationId,
                    AxisPosition = station.AxisPosition,
                    StationKind = station.StationKind,
                    Segments = segments
                        .OrderBy(segment => segment.CenterY)
                        .ThenBy(segment => segment.CenterZ)
                        .ToArray()
                });
        }

        return new SectionTraceExtractionResult
        {
            AssemblyId = partition.AssemblyId,
            InputMainPartId = partition.InputMainPartId,
            SectionAxisX = defaultSectionAxisX,
            SectionAxisY = defaultSectionAxisY,
            SectionAxisZ = defaultSectionAxisZ,
            Stations = stations
        };
    }

    private static Vector3 ResolveSectionAxisPoint(BodyCandidatePartitionResult partition, double axisPosition, Vector3 fallbackAxis)
    {
        if (partition.ProvisionalBodyAxisSegments.Count == 0)
        {
            var normalizedFallback = fallbackAxis.Normalize();
            return normalizedFallback.Length > 1e-6
                ? normalizedFallback * axisPosition
                : Vector3.Zero;
        }

        var segment = partition.ProvisionalBodyAxisSegments
            .Where(item => axisPosition >= item.AxisStart - 1e-6 && axisPosition <= item.AxisEnd + 1e-6)
            .OrderBy(item => Math.Abs(((item.AxisStart + item.AxisEnd) * 0.5) - axisPosition))
            .FirstOrDefault();

        if (segment.Direction.Length <= 1e-6)
        {
            segment = partition.ProvisionalBodyAxisSegments
                .OrderBy(item => Math.Min(Math.Abs(item.AxisStart - axisPosition), Math.Abs(item.AxisEnd - axisPosition)))
                .First();
        }

        if (segment.Direction.Length <= 1e-6)
        {
            var normalizedFallback = fallbackAxis.Normalize();
            return normalizedFallback.Length > 1e-6
                ? normalizedFallback * axisPosition
                : Vector3.Zero;
        }

        var local = Math.Clamp(axisPosition - segment.AxisStart, 0.0, segment.Length);
        return segment.StartPoint + (segment.Direction * local);
    }

    private sealed record TraceGeometry(
        double CenterY,
        double CenterZ,
        double StartY,
        double StartZ,
        double EndY,
        double EndZ,
        string Reason);

    private bool TryResolveTraceGeometries(
        PartFeature part,
        double stationAxisPosition,
        Vector3 sectionPlanePoint,
        Vector3 sectionAxisX,
        Vector3 sectionAxisY,
        Vector3 sectionAxisZ,
        out IReadOnlyList<TraceGeometry> traceGeometries)
    {
        if (TryResolveTraceGeometriesFromSolidEdges(
                part,
                stationAxisPosition,
                sectionPlanePoint,
                sectionAxisX,
                sectionAxisY,
                sectionAxisZ,
                out traceGeometries))
        {
            return true;
        }

        if (!TryResolveTraceVectorAndSpan(part, sectionAxisX, out var traceVector, out var traceLength, out var vectorReason))
        {
            traceGeometries = Array.Empty<TraceGeometry>();
            return false;
        }

        ResolveTraceCenter(
            part,
            stationAxisPosition,
            sectionPlanePoint,
            sectionAxisX,
            sectionAxisY,
            sectionAxisZ,
            out var centerY,
            out var centerZ);
        var halfSpan = Math.Max(Math.Abs(traceLength) * 0.5, 1.0);
        var traceY = traceVector.Dot(sectionAxisY);
        var traceZ = traceVector.Dot(sectionAxisZ);
        var startY = centerY - (traceY * halfSpan);
        var startZ = centerZ - (traceZ * halfSpan);
        var endY = centerY + (traceY * halfSpan);
        var endZ = centerZ + (traceZ * halfSpan);
        traceGeometries =
        [
            new TraceGeometry(
                centerY,
                centerZ,
                startY,
                startZ,
                endY,
                endZ,
                vectorReason)
        ];
        return true;
    }

    private static Vector3 ResolveSectionAxisX(BodyCandidatePartitionResult partition, double axisPosition, Vector3 fallbackAxis)
    {
        if (partition.ProvisionalBodyAxisSegments.Count == 0)
        {
            return fallbackAxis;
        }

        var segment = partition.ProvisionalBodyAxisSegments
            .Where(item => axisPosition >= item.AxisStart - 1e-6 && axisPosition <= item.AxisEnd + 1e-6)
            .OrderBy(item => Math.Abs(((item.AxisStart + item.AxisEnd) * 0.5) - axisPosition))
            .FirstOrDefault();

        if (segment.Direction.Length > 1e-6)
        {
            return segment.Direction;
        }

        segment = partition.ProvisionalBodyAxisSegments
            .OrderBy(item => Math.Min(Math.Abs(item.AxisStart - axisPosition), Math.Abs(item.AxisEnd - axisPosition)))
            .First();
        return segment.Direction.Length > 1e-6 ? segment.Direction : fallbackAxis;
    }

    private static Vector3 ResolveSectionAxisY(IEnumerable<BodyCandidatePartitionItem> items, PartGraph graph, Vector3 sectionAxisX)
    {
        var bodyCandidateParts = items
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .OrderByDescending(item => item.IsInputMainPart)
            .ThenByDescending(item => item.PartitionConfidence)
            .Select(
                item =>
                {
                    graph.Parts.TryGetValue(item.PartId, out var part);
                    return part;
                })
            .Where(static part => part is not null)
            .Cast<PartFeature>()
            .ToArray();

        var layoutAxis = ResolveLayoutDrivenSectionAxisY(bodyCandidateParts, sectionAxisX);
        if (layoutAxis.Length > 1e-6)
        {
            return layoutAxis;
        }

        foreach (var part in bodyCandidateParts)
        {
            var axisY = part.PlateNormal.ProjectToPlane(sectionAxisX).Normalize();
            if (axisY.Length > 1e-6)
            {
                return axisY;
            }

            axisY = part.PlateWidthDirection.ProjectToPlane(sectionAxisX).Normalize();
            if (axisY.Length > 1e-6)
            {
                return axisY;
            }
        }

        var fallback = Math.Abs(sectionAxisX.Dot(Vector3.UnitZ)) < 0.9
            ? Vector3.UnitZ.ProjectToPlane(sectionAxisX).Normalize()
            : Vector3.UnitY.ProjectToPlane(sectionAxisX).Normalize();
        return fallback.Length > 1e-6 ? fallback : Vector3.UnitY;
    }

    private static void ResolveTraceCenter(
        PartFeature part,
        double stationAxisPosition,
        Vector3 sectionPlanePoint,
        Vector3 sectionAxisX,
        Vector3 sectionAxisY,
        Vector3 sectionAxisZ,
        out double centerY,
        out double centerZ)
    {
        var centroidX = part.Centroid.Dot(sectionAxisX);
        var centroidY = part.Centroid.Dot(sectionAxisY);
        var centroidZ = part.Centroid.Dot(sectionAxisZ);

        var normalX = part.PlateNormal.Dot(sectionAxisX);
        var normalY = part.PlateNormal.Dot(sectionAxisY);
        var normalZ = part.PlateNormal.Dot(sectionAxisZ);
        var yzNormalLengthSquared = (normalY * normalY) + (normalZ * normalZ);

        if (yzNormalLengthSquared <= 1e-9)
        {
            centerY = centroidY;
            centerZ = centroidZ;
            return;
        }

        var stationPlaneCoordinate = sectionPlanePoint.Dot(sectionAxisX);
        var offset = -normalX * (stationPlaneCoordinate - centroidX) / yzNormalLengthSquared;
        centerY = centroidY + (normalY * offset);
        centerZ = centroidZ + (normalZ * offset);
    }

    private static Vector3 ResolveLayoutDrivenSectionAxisY(IReadOnlyList<PartFeature> parts, Vector3 sectionAxisX)
    {
        if (parts.Count < 2)
        {
            return Vector3.Zero;
        }

        var projectedCentroids = parts
            .Select(
                part => new
                {
                    part.PartId,
                    Projected = part.Centroid.ProjectToPlane(sectionAxisX)
                })
            .Where(item => item.Projected.Length > 1e-6)
            .ToArray();
        if (projectedCentroids.Length < 2)
        {
            return Vector3.Zero;
        }

        var bestVector = Vector3.Zero;
        var bestDistance = 0.0;
        for (var i = 0; i < projectedCentroids.Length; i++)
        {
            for (var j = i + 1; j < projectedCentroids.Length; j++)
            {
                var candidate = projectedCentroids[j].Projected - projectedCentroids[i].Projected;
                var distance = candidate.Length;
                if (distance <= bestDistance || distance <= 1e-6)
                {
                    continue;
                }

                bestDistance = distance;
                bestVector = candidate;
            }
        }

        return bestVector.Normalize();
    }

    private bool TryResolveTraceGeometriesFromSolidEdges(
        PartFeature part,
        double stationAxisPosition,
        Vector3 sectionPlanePoint,
        Vector3 sectionAxisX,
        Vector3 sectionAxisY,
        Vector3 sectionAxisZ,
        out IReadOnlyList<TraceGeometry> traceGeometries)
    {
        var edges = part.SolidEdges;
        if (edges.Count == 0)
        {
            traceGeometries = Array.Empty<TraceGeometry>();
            return false;
        }

        var worldPoints = new List<Vector3>();
        var pointTolerance = Math.Max(_options.ContactDistanceToleranceMm * 0.2, 0.5);
        foreach (var edge in edges)
        {
            CollectPlaneIntersectionPoints(edge, sectionPlanePoint, sectionAxisX, pointTolerance, worldPoints);
        }

        var uniquePoints = DeduplicatePoints(worldPoints, pointTolerance);
        if (uniquePoints.Count < 2)
        {
            traceGeometries = Array.Empty<TraceGeometry>();
            return false;
        }

        var yzPoints = uniquePoints
            .Select(
                point => new
                {
                    Y = point.Dot(sectionAxisY),
                    Z = point.Dot(sectionAxisZ)
                })
            .ToArray();

        var direction = ResolveTraceDirectionFromPoints(part, sectionAxisX, yzPoints);
        if (direction.Length <= 1e-6)
        {
            traceGeometries = Array.Empty<TraceGeometry>();
            return false;
        }

        traceGeometries = BuildTraceGeometriesFromPointClusters(direction, yzPoints, pointTolerance, "SECTION_TRACE_USING_SOLID_EDGE_INTERSECTION");
        return traceGeometries.Count > 0;
    }

    private static IReadOnlyList<TraceGeometry> BuildTraceGeometriesFromPointClusters(
        Vector3 direction,
        IReadOnlyList<dynamic> yzPoints,
        double pointTolerance,
        string reason)
    {
        var alongAxis = direction;
        var normalAxis = new Vector3(-alongAxis.Y, alongAxis.X, 0d);
        var clusteredPoints = yzPoints
            .Select(
                point => new PointProjection(
                    (double)point.Y,
                    (double)point.Z,
                    (((double)point.Y * alongAxis.X) + ((double)point.Z * alongAxis.Y)),
                    (((double)point.Y * normalAxis.X) + ((double)point.Z * normalAxis.Y))))
            .OrderBy(item => item.Normal)
            .ThenBy(item => item.Along)
            .ToArray();

        var clusters = new List<List<PointProjection>>();
        var clusterTolerance = Math.Max(pointTolerance * 6.0, 6.0);
        foreach (var point in clusteredPoints)
        {
            var cluster = clusters.FirstOrDefault(item => Math.Abs(item.Average(existing => existing.Normal) - point.Normal) <= clusterTolerance);
            if (cluster is null)
            {
                clusters.Add([point]);
                continue;
            }

            cluster.Add(point);
        }

        var geometries = clusters
            .Where(cluster => cluster.Count >= 2)
            .Select(
                cluster =>
                {
                    var minAlong = cluster.Min(item => item.Along);
                    var maxAlong = cluster.Max(item => item.Along);
                    var span = maxAlong - minAlong;
                    if (span <= Math.Max(pointTolerance * 2.0, 1.0))
                    {
                        return null;
                    }

                    var centerAlong = (minAlong + maxAlong) * 0.5d;
                    var centerNormal = cluster.Average(item => item.Normal);
                    var centerY = (alongAxis.X * centerAlong) + (normalAxis.X * centerNormal);
                    var centerZ = (alongAxis.Y * centerAlong) + (normalAxis.Y * centerNormal);
                    var startY = (alongAxis.X * minAlong) + (normalAxis.X * centerNormal);
                    var startZ = (alongAxis.Y * minAlong) + (normalAxis.Y * centerNormal);
                    var endY = (alongAxis.X * maxAlong) + (normalAxis.X * centerNormal);
                    var endZ = (alongAxis.Y * maxAlong) + (normalAxis.Y * centerNormal);
                    return new TraceGeometry(centerY, centerZ, startY, startZ, endY, endZ, reason);
                })
            .Where(static item => item is not null)
            .Cast<TraceGeometry>()
            .OrderByDescending(item => Math.Sqrt(Math.Pow(item.EndY - item.StartY, 2) + Math.Pow(item.EndZ - item.StartZ, 2)))
            .ThenBy(item => item.CenterY)
            .ThenBy(item => item.CenterZ)
            .ToArray();

        if (geometries.Length > 0)
        {
            return geometries;
        }

        var alongValues = clusteredPoints.Select(item => item.Along).ToArray();
        var normalValues = clusteredPoints.Select(item => item.Normal).ToArray();
        var fallbackMinAlong = alongValues.Min();
        var fallbackMaxAlong = alongValues.Max();
        if (fallbackMaxAlong - fallbackMinAlong <= 1e-6)
        {
            return Array.Empty<TraceGeometry>();
        }

        var fallbackCenterAlong = (fallbackMinAlong + fallbackMaxAlong) * 0.5d;
        var fallbackCenterNormal = normalValues.Average();
        return
        [
            new TraceGeometry(
                (alongAxis.X * fallbackCenterAlong) + (normalAxis.X * fallbackCenterNormal),
                (alongAxis.Y * fallbackCenterAlong) + (normalAxis.Y * fallbackCenterNormal),
                (alongAxis.X * fallbackMinAlong) + (normalAxis.X * fallbackCenterNormal),
                (alongAxis.Y * fallbackMinAlong) + (normalAxis.Y * fallbackCenterNormal),
                (alongAxis.X * fallbackMaxAlong) + (normalAxis.X * fallbackCenterNormal),
                (alongAxis.Y * fallbackMaxAlong) + (normalAxis.Y * fallbackCenterNormal),
                reason)
        ];
    }

    private sealed record PointProjection(double Y, double Z, double Along, double Normal);

    private static bool TryResolveTraceVectorAndSpan(
        PartFeature part,
        Vector3 sectionAxisX,
        out Vector3 traceVector,
        out double traceLength,
        out string traceReason)
    {
        var dims = new[] { Math.Abs(part.ObbDims.X), Math.Abs(part.ObbDims.Y), Math.Abs(part.ObbDims.Z) }
            .OrderByDescending(value => value)
            .ToArray();
        var thickness = Math.Max(Math.Abs(part.Thickness ?? dims.LastOrDefault()), 1.0);
        var primaryInPlaneLength = Math.Max(dims.FirstOrDefault(), thickness);
        var secondaryInPlaneLength = Math.Max(dims.Skip(1).FirstOrDefault(), thickness);

        var candidates = new List<(Vector3 Vector, double Span, string Reason)>();

        var planeIntersection = part.PlateNormal.Cross(sectionAxisX);
        if (planeIntersection.Length > 1e-6)
        {
            var intersectionVector = planeIntersection.Normalize();
            var longAlignment = Math.Abs(intersectionVector.Dot(part.PlateLongDirection.Normalize()));
            var widthAlignment = Math.Abs(intersectionVector.Dot(part.PlateWidthDirection.Normalize()));
            var alignedSpan = Math.Max(primaryInPlaneLength * longAlignment, secondaryInPlaneLength * widthAlignment);
            candidates.Add(
                (
                    intersectionVector,
                    Math.Max(alignedSpan, thickness),
                    "SECTION_TRACE_USING_PLANE_INTERSECTION"));
        }

        var longProjection = part.PlateLongDirection.ProjectToPlane(sectionAxisX);
        if (longProjection.Length > 1e-6)
        {
            candidates.Add(
                (
                    longProjection.Normalize(),
                    Math.Max(primaryInPlaneLength * longProjection.Length, thickness),
                    "SECTION_TRACE_USING_LONG_DIRECTION"));
        }

        var widthProjection = part.PlateWidthDirection.ProjectToPlane(sectionAxisX);
        if (widthProjection.Length > 1e-6)
        {
            candidates.Add(
                (
                    widthProjection.Normalize(),
                    Math.Max(secondaryInPlaneLength * widthProjection.Length, thickness),
                    "SECTION_TRACE_USING_WIDTH_DIRECTION"));
        }

        var normalProjection = part.PlateNormal.ProjectToPlane(sectionAxisX);
        if (normalProjection.Length > 1e-6)
        {
            candidates.Add(
                (
                    normalProjection.Normalize(),
                    Math.Max(thickness * normalProjection.Length, 1.0),
                    "SECTION_TRACE_USING_NORMAL_FALLBACK"));
        }

        var selected = candidates
            .OrderByDescending(candidate => candidate.Span)
            .FirstOrDefault();

        if (selected.Vector.Length <= 1e-6 || selected.Span <= 1e-6)
        {
            traceVector = Vector3.Zero;
            traceLength = 0;
            traceReason = string.Empty;
            return false;
        }

        traceVector = selected.Vector;
        traceLength = selected.Span;
        traceReason = selected.Reason;
        return true;
    }

    private static void CollectPlaneIntersectionPoints(
        LineSegment3 edge,
        Vector3 sectionPlanePoint,
        Vector3 sectionAxisX,
        double tolerance,
        ICollection<Vector3> points)
    {
        var startDistance = (edge.Start - sectionPlanePoint).Dot(sectionAxisX);
        var endDistance = (edge.End - sectionPlanePoint).Dot(sectionAxisX);
        var startOnPlane = Math.Abs(startDistance) <= tolerance;
        var endOnPlane = Math.Abs(endDistance) <= tolerance;

        if (startOnPlane && endOnPlane)
        {
            points.Add(edge.Start);
            points.Add(edge.End);
            return;
        }

        if (startOnPlane)
        {
            points.Add(edge.Start);
            return;
        }

        if (endOnPlane)
        {
            points.Add(edge.End);
            return;
        }

        if ((startDistance < 0d && endDistance > 0d) || (startDistance > 0d && endDistance < 0d))
        {
            var ratio = startDistance / (startDistance - endDistance);
            points.Add(edge.Start + ((edge.End - edge.Start) * ratio));
        }
    }

    private static List<Vector3> DeduplicatePoints(IEnumerable<Vector3> points, double tolerance)
    {
        var unique = new List<Vector3>();
        foreach (var point in points)
        {
            if (unique.Any(existing => (existing - point).Length <= tolerance))
            {
                continue;
            }

            unique.Add(point);
        }

        return unique;
    }

    private static Vector3 ResolveTraceDirectionFromPoints(PartFeature part, Vector3 sectionAxisX, IReadOnlyList<dynamic> yzPoints)
    {
        var candidates = new List<Vector3>();
        AddPointDerivedDirectionCandidate(yzPoints, candidates);
        AddPartDerivedDirectionCandidates(part, sectionAxisX, candidates);

        var bestDirection = Vector3.Zero;
        var bestNormalSpreadRatio = double.PositiveInfinity;
        var bestAlongSpan = 0d;
        foreach (var candidate in candidates)
        {
            var direction = NormalizeDirection2D(candidate);
            if (direction.Length <= 1e-6)
            {
                continue;
            }

            var normal = new Vector3(-direction.Y, direction.X, 0d);
            var alongValues = yzPoints
                .Select(point => ((double)point.Y * direction.X) + ((double)point.Z * direction.Y))
                .ToArray();
            var normalValues = yzPoints
                .Select(point => ((double)point.Y * normal.X) + ((double)point.Z * normal.Y))
                .ToArray();
            var alongSpan = alongValues.Max() - alongValues.Min();
            if (alongSpan <= 1e-6)
            {
                continue;
            }

            var normalSpread = normalValues.Max() - normalValues.Min();
            var normalSpreadRatio = normalSpread / alongSpan;
            if (normalSpreadRatio < bestNormalSpreadRatio - 1e-6 ||
                (Math.Abs(normalSpreadRatio - bestNormalSpreadRatio) <= 1e-6 && alongSpan > bestAlongSpan))
            {
                bestNormalSpreadRatio = normalSpreadRatio;
                bestAlongSpan = alongSpan;
                bestDirection = direction;
            }
        }

        return bestDirection;
    }

    private static void AddPointDerivedDirectionCandidate(IReadOnlyList<dynamic> yzPoints, ICollection<Vector3> candidates)
    {
        var bestLength = 0d;
        var bestDirection = Vector3.Zero;

        for (var i = 0; i < yzPoints.Count; i++)
        {
            for (var j = i + 1; j < yzPoints.Count; j++)
            {
                var dy = (double)yzPoints[j].Y - (double)yzPoints[i].Y;
                var dz = (double)yzPoints[j].Z - (double)yzPoints[i].Z;
                var length = Math.Sqrt((dy * dy) + (dz * dz));
                if (length <= bestLength || length <= 1e-6)
                {
                    continue;
                }

                bestLength = length;
                bestDirection = new Vector3(dy / length, dz / length, 0d);
            }
        }

        if (bestDirection.Length > 1e-6)
        {
            candidates.Add(bestDirection);
        }
    }

    private static void AddPartDerivedDirectionCandidates(PartFeature part, Vector3 sectionAxisX, ICollection<Vector3> candidates)
    {
        AddInPlaneDirectionCandidate(part.PlateNormal.Cross(sectionAxisX), candidates);
        AddInPlaneDirectionCandidate(part.PlateLongDirection.ProjectToPlane(sectionAxisX), candidates);
        AddInPlaneDirectionCandidate(part.PlateWidthDirection.ProjectToPlane(sectionAxisX), candidates);
        AddInPlaneDirectionCandidate(part.PlateNormal.ProjectToPlane(sectionAxisX), candidates);
    }

    private static void AddInPlaneDirectionCandidate(Vector3 vector, ICollection<Vector3> candidates)
    {
        var normalized = NormalizeDirection2D(vector);
        if (normalized.Length <= 1e-6)
        {
            return;
        }

        if (candidates.Any(existing => Math.Abs(existing.Dot(normalized)) >= 0.995d))
        {
            return;
        }

        candidates.Add(normalized);
    }

    private static Vector3 NormalizeDirection2D(Vector3 direction)
    {
        var normalized = new Vector3(direction.X, direction.Y, 0d).Normalize();
        if (normalized.Length <= 1e-6)
        {
            return Vector3.Zero;
        }

        if (normalized.X < 0d || (Math.Abs(normalized.X) <= 1e-6 && normalized.Y < 0d))
        {
            normalized = -normalized;
        }

        return normalized;
    }
}
