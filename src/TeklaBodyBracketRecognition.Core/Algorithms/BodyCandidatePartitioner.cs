using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class BodyCandidatePartitioner
{
    private readonly RecognitionOptions _options;

    public BodyCandidatePartitioner(RecognitionOptions options)
    {
        _options = options;
    }

    public BodyCandidatePartitionResult Partition(
        string assemblyId,
        PartGraph graph,
        int inputMainPartId,
        IReadOnlyList<LongitudinalAxisSegment>? importedLongitudinalAxisSegments = null)
    {
        var parts = graph.Parts.Values.OrderBy(part => part.PartId).ToArray();
        var fallbackBodyAxis = EstimateBodyAxis(parts, inputMainPartId);
        var provisionalBodyAxisSegments = importedLongitudinalAxisSegments is { Count: > 0 }
            ? importedLongitudinalAxisSegments
            : EstimateBodyAxisSegments(parts, inputMainPartId, fallbackBodyAxis);
        var provisionalBodyAxis = provisionalBodyAxisSegments.Count > 0
            ? provisionalBodyAxisSegments[0].Direction.Normalize()
            : fallbackBodyAxis;
        var inputMainComponentPartIds = CollectConnectedComponentPartIds(graph, inputMainPartId);
        var componentParts = parts
            .Where(part => inputMainComponentPartIds.Contains(part.PartId))
            .ToArray();
        var assemblySpan = EstimateAssemblySpan(
            componentParts.Length > 0 ? componentParts : parts,
            provisionalBodyAxisSegments,
            provisionalBodyAxis,
            inputMainPartId);

        var items = parts
            .Select(
                part => PartitionPart(
                    part,
                    inputMainPartId,
                    provisionalBodyAxisSegments,
                    provisionalBodyAxis,
                    assemblySpan,
                    inputMainComponentPartIds.Count == 0 || inputMainComponentPartIds.Contains(part.PartId)))
            .ToArray();

        return new BodyCandidatePartitionResult
        {
            AssemblyId = assemblyId,
            InputMainPartId = inputMainPartId,
            ProvisionalBodyAxis = provisionalBodyAxis,
            ProvisionalBodyAxisSegments = provisionalBodyAxisSegments,
            Items = items
        };
    }

    private BodyCandidatePartitionItem PartitionPart(
        PartFeature part,
        int inputMainPartId,
        IReadOnlyList<LongitudinalAxisSegment> provisionalBodyAxisSegments,
        Vector3 provisionalBodyAxis,
        double assemblySpan,
        bool isInInputMainComponent)
    {
        var projectedInterval = EstimateProjectedInterval(part, provisionalBodyAxisSegments, provisionalBodyAxis);
        var coverage = EstimateLongitudinalCoverage(projectedInterval, assemblySpan);
        var endProximity = EstimateEndProximity(projectedInterval, assemblySpan);
        var nearStableZone = !IsSingleEndedLocalPart(endProximity.NearStart, endProximity.NearEnd);
        var reasons = new List<string>();
        var isInputMainPart = part.PartId == inputMainPartId;

        BodyCandidatePartitionClass partitionClass;
        double confidence;

        if (part.IsSpecialShape || part.RuntimeType == RuntimePartType.BentPlate)
        {
            if (ShouldPromoteSpecialShapeToBodyCandidate(
                    part,
                    coverage,
                    nearStableZone,
                    isInputMainPart,
                    isInInputMainComponent))
            {
                partitionClass = BodyCandidatePartitionClass.BodyCandidate;
                confidence = isInputMainPart
                    ? 0.82
                    : IsPrimaryBodyRole(part.SemanticRole)
                        ? 0.78
                        : 0.68;
                reasons.Add("SPECIAL_SHAPE_LONGITUDINAL_BODY_CANDIDATE");
                reasons.Add("LOW_CONFIDENCE_PARTITION");
                if (IsPrimaryBodyRole(part.SemanticRole) || part.OuterSideCandidate || isInputMainPart)
                {
                    reasons.Add("LIKELY_MAIN_SECTION_PART");
                }

                if (isInputMainPart)
                {
                    reasons.Add("INPUT_MAIN_PART");
                }
            }
            else
            {
                partitionClass = BodyCandidatePartitionClass.SpecialShape;
                confidence = 0.95;
                reasons.Add("SPECIAL_SHAPE_PART");
            }
        }
        else if (part.IsTinyPart)
        {
            partitionClass = BodyCandidatePartitionClass.TinyPart;
            confidence = 0.95;
            reasons.Add("TOO_SMALL_FOR_BODY");
        }
        else if (part.SemanticRole == PartSemanticRole.EndPlateCandidate || part.LikelyConnectionPart)
        {
            partitionClass = BodyCandidatePartitionClass.EndConnectionCandidate;
            confidence = 0.90;
            reasons.Add("LIKELY_END_CONNECTION");
            if (part.LikelyConnectionPart)
            {
                reasons.Add("BOLT_DOMINATED_LOCAL_PART");
            }

            if (!nearStableZone)
            {
                reasons.Add("NEAR_END_LOCAL_PART");
            }
        }
        else if (part.SemanticRole == PartSemanticRole.StiffenerCandidate)
        {
            if (coverage >= 0.35 && nearStableZone)
            {
                partitionClass = BodyCandidatePartitionClass.BodyAccessoryCandidate;
                confidence = 0.78;
                reasons.Add("LIKELY_BODY_ACCESSORY_LONGITUDINAL");
            }
            else
            {
                partitionClass = BodyCandidatePartitionClass.LocalStiffenerCandidate;
                confidence = 0.86;
                reasons.Add("LIKELY_LOCAL_STIFFENER");
            }

            if (!nearStableZone)
            {
                reasons.Add("NEAR_END_LOCAL_PART");
            }
        }
        else if (!isInInputMainComponent)
        {
            partitionClass = BodyCandidatePartitionClass.Unknown;
            confidence = 0.30;
            reasons.Add("OUTSIDE_INPUT_MAIN_COMPONENT");
            reasons.Add("LOW_CONFIDENCE_PARTITION");
        }
        else if (isInputMainPart && IsPrimaryBodyRole(part.SemanticRole))
        {
            if (coverage >= 0.20)
            {
                partitionClass = BodyCandidatePartitionClass.BodyCandidate;
                confidence = Math.Min(0.90, 0.70 + (0.25 * Math.Min(1.0, coverage)) + (0.08 * part.SemanticRoleScore));
                reasons.Add("LIKELY_MAIN_SECTION_PART");
                reasons.Add("LOW_CONFIDENCE_PARTITION");
                reasons.Add("INPUT_MAIN_PART");
                if (!nearStableZone)
                {
                    reasons.Add("NEAR_END_LOCAL_PART");
                }
            }
            else if (nearStableZone)
            {
                partitionClass = BodyCandidatePartitionClass.BodyAccessoryCandidate;
                confidence = 0.60;
                reasons.Add("INPUT_MAIN_PART");
                reasons.Add("LOW_CONFIDENCE_PARTITION");
            }
            else
            {
                partitionClass = BodyCandidatePartitionClass.LocalStiffenerCandidate;
                confidence = 0.70;
                reasons.Add("INPUT_MAIN_PART");
                reasons.Add("NEAR_END_LOCAL_PART");
                reasons.Add("LOW_CONFIDENCE_PARTITION");
            }
        }
        else if (IsPrimaryBodyRole(part.SemanticRole))
        {
            if (coverage >= 0.30)
            {
                partitionClass = BodyCandidatePartitionClass.BodyCandidate;
                confidence = Math.Min(0.95, 0.72 + (0.30 * Math.Min(1.0, coverage)) + (0.10 * part.SemanticRoleScore));
                reasons.Add("LONGITUDINAL_PRIMARY_PLATE");
                if (part.OuterSideCandidate || part.SemanticRole == PartSemanticRole.WallCandidate)
                {
                    reasons.Add("LIKELY_MAIN_SECTION_PART");
                }
            }
            else if (nearStableZone)
            {
                partitionClass = BodyCandidatePartitionClass.BodyAccessoryCandidate;
                confidence = 0.62;
                reasons.Add("LOW_CONFIDENCE_PARTITION");
            }
            else
            {
                partitionClass = BodyCandidatePartitionClass.LocalStiffenerCandidate;
                confidence = 0.72;
                reasons.Add("NEAR_END_LOCAL_PART");
                reasons.Add("LOW_CONFIDENCE_PARTITION");
            }
        }
        else if (part.IsPlateLike && coverage >= 0.40 && nearStableZone)
        {
            partitionClass = BodyCandidatePartitionClass.BodyCandidate;
            confidence = 0.58;
            reasons.Add("LIKELY_MAIN_SECTION_PART");
            reasons.Add("LOW_CONFIDENCE_PARTITION");
        }
        else if (part.IsPlateLike && coverage >= 0.25)
        {
            partitionClass = BodyCandidatePartitionClass.BodyAccessoryCandidate;
            confidence = 0.54;
            reasons.Add("LIKELY_BODY_ACCESSORY_LONGITUDINAL");
            reasons.Add("LOW_CONFIDENCE_PARTITION");
        }
        else if (!nearStableZone)
        {
            partitionClass = BodyCandidatePartitionClass.EndConnectionCandidate;
            confidence = 0.55;
            reasons.Add("NEAR_END_LOCAL_PART");
            reasons.Add("LOW_CONFIDENCE_PARTITION");
        }
        else
        {
            partitionClass = BodyCandidatePartitionClass.Unknown;
            confidence = 0.35;
            reasons.Add("LOW_CONFIDENCE_PARTITION");
        }

        return new BodyCandidatePartitionItem
        {
            PartId = part.PartId,
            PartName = string.IsNullOrWhiteSpace(part.Name) ? "<unnamed>" : part.Name,
            ProfileString = part.ProfileString,
            IsInputMainPart = isInputMainPart,
            PartitionClass = partitionClass,
            PartitionConfidence = confidence,
            PartitionReasons = reasons.Distinct().ToArray(),
            UpstreamRoleHint = part.SemanticRole.ToString(),
            NearStableZone = nearStableZone,
            LongitudinalCoverageEstimate = Math.Round(coverage, 4),
            AxisIntervalMin = Math.Round(projectedInterval.Min, 3),
            AxisIntervalMax = Math.Round(projectedInterval.Max, 3)
        };
    }

    private static bool ShouldPromoteSpecialShapeToBodyCandidate(
        PartFeature part,
        double coverage,
        bool nearStableZone,
        bool isInputMainPart,
        bool isInInputMainComponent)
    {
        if (!isInInputMainComponent || !part.IsPlateLike || part.IsTinyPart || !nearStableZone)
        {
            return false;
        }

        if (coverage >= 0.55)
        {
            return true;
        }

        var hasBodyRoleSignal =
            isInputMainPart ||
            IsPrimaryBodyRole(part.SemanticRole) ||
            part.OuterSideCandidate;
        if (!hasBodyRoleSignal)
        {
            return false;
        }

        return isInputMainPart &&
               IsPrimaryBodyRole(part.SemanticRole) &&
               coverage >= 0.35;
    }

    private static Vector3 EstimateBodyAxis(IReadOnlyList<PartFeature> parts, int inputMainPartId)
    {
        var nonTinyParts = parts
            .Where(part => !part.IsTinyPart)
            .ToArray();
        var referenceLength = nonTinyParts.Length > 0
            ? nonTinyParts.Max(GetPrimaryLength)
            : 0.0;
        var preferred = parts.FirstOrDefault(part => part.PartId == inputMainPartId);
        if (preferred is not null &&
            preferred.PlateLongDirection.Length > 1e-6 &&
            (referenceLength <= 1e-6 || GetPrimaryLength(preferred) >= referenceLength * 0.60))
        {
            var axis = preferred.PlateLongDirection.Normalize();
            if (axis.Length > 1e-6)
            {
                return axis;
            }
        }

        var fallback = nonTinyParts
            .OrderByDescending(part => GetPrimaryLength(part))
            .ThenByDescending(part => IsPrimaryBodyRole(part.SemanticRole))
            .ThenByDescending(part => GetPrimaryLength(part))
            .FirstOrDefault();

        if (fallback is not null)
        {
            var axis = fallback.PlateLongDirection.Normalize();
            if (axis.Length > 1e-6)
            {
                return axis;
            }
        }

        return Vector3.UnitX;
    }

    private static double EstimateAssemblySpan(IReadOnlyList<PartFeature> parts, Vector3 axis, int inputMainPartId)
    {
        if (parts.Count == 0)
        {
            return 1.0;
        }

        var usableParts = parts
            .Where(part => HasUsableSpanGeometry(part, BuildSingleAxisSegment(axis, parts), axis))
            .ToArray();
        if (usableParts.Length == 0)
        {
            usableParts = parts.ToArray();
        }

        var primaryBodyParts = usableParts
            .Where(part => IsPrimaryBodyRole(part.SemanticRole))
            .ToArray();
        var usableIntervals = usableParts
            .Select(part => EstimateProjectedInterval(part, BuildSingleAxisSegment(axis, usableParts), axis))
            .ToArray();
        var usableSpanMin = usableIntervals.Min(item => item.Min);
        var usableSpanMax = usableIntervals.Max(item => item.Max);
        var usableAssemblySpan = Math.Max(usableSpanMax - usableSpanMin, 1.0);
        var anchoredParts = usableParts
            .Where(
                part =>
                {
                    if (part.PartId == inputMainPartId)
                    {
                        return true;
                    }

                    var interval = TranslateIntervalToAssemblyOrigin(
                        EstimateProjectedInterval(part, BuildSingleAxisSegment(axis, usableParts), axis),
                        usableSpanMin);
                    var endProximity = EstimateEndProximity(interval, usableAssemblySpan);
                    return !IsSingleEndedLocalPart(endProximity.NearStart, endProximity.NearEnd);
                })
            .ToArray();
        var spanSource = SelectSpanSourceParts(
            primaryBodyParts,
            anchoredParts,
            usableParts,
            part => EstimateProjectedInterval(part, BuildSingleAxisSegment(axis, usableParts), axis));

        var intervals = spanSource
            .Select(part => EstimateProjectedInterval(part, BuildSingleAxisSegment(axis, spanSource), axis))
            .ToArray();

        var span = intervals.Max(item => item.Max) - intervals.Min(item => item.Min);
        return Math.Max(span, 1.0);
    }

    private static double EstimateLongitudinalCoverage((double Min, double Max) projectedInterval, double assemblySpan)
    {
        if (assemblySpan <= 1e-6)
        {
            return 0.0;
        }

        var projectedLength = Math.Max(projectedInterval.Max - projectedInterval.Min, 0.0);
        return Math.Clamp(projectedLength / assemblySpan, 0.0, 1.0);
    }

    private static double EstimateAssemblySpan(
        IReadOnlyList<PartFeature> parts,
        IReadOnlyList<LongitudinalAxisSegment> axisSegments,
        Vector3 axis,
        int inputMainPartId)
    {
        if (parts.Count == 0)
        {
            return 1.0;
        }

        var usableParts = parts
            .Where(part => HasUsableSpanGeometry(part, axisSegments, axis))
            .ToArray();
        if (usableParts.Length == 0)
        {
            usableParts = parts.ToArray();
        }

        var primaryBodyParts = usableParts
            .Where(part => IsPrimaryBodyRole(part.SemanticRole))
            .ToArray();
        var usableIntervals = usableParts
            .Select(part => EstimateProjectedInterval(part, axisSegments, axis))
            .ToArray();
        var usableSpanMin = usableIntervals.Min(item => item.Min);
        var usableSpanMax = usableIntervals.Max(item => item.Max);
        var usableAssemblySpan = Math.Max(usableSpanMax - usableSpanMin, 1.0);
        var anchoredParts = usableParts
            .Where(
                part =>
                {
                    if (part.PartId == inputMainPartId)
                    {
                        return true;
                    }

                    var interval = TranslateIntervalToAssemblyOrigin(
                        EstimateProjectedInterval(part, axisSegments, axis),
                        usableSpanMin);
                    var endProximity = EstimateEndProximity(interval, usableAssemblySpan);
                    return !IsSingleEndedLocalPart(endProximity.NearStart, endProximity.NearEnd);
                })
            .ToArray();
        var spanSource = SelectSpanSourceParts(
            primaryBodyParts,
            anchoredParts,
            usableParts,
            part => EstimateProjectedInterval(part, axisSegments, axis));

        var intervals = spanSource
            .Select(part => EstimateProjectedInterval(part, axisSegments, axis))
            .ToArray();

        var span = intervals.Max(item => item.Max) - intervals.Min(item => item.Min);
        return Math.Max(span, 1.0);
    }

    private static IReadOnlyList<PartFeature> SelectSpanSourceParts(
        IReadOnlyList<PartFeature> primaryBodyParts,
        IReadOnlyList<PartFeature> anchoredParts,
        IReadOnlyList<PartFeature> usableParts,
        Func<PartFeature, (double Min, double Max)> intervalSelector)
    {
        if (primaryBodyParts.Count == 0)
        {
            return anchoredParts.Count > 0 ? anchoredParts : usableParts;
        }

        var primarySpan = EstimateIntervalSpan(primaryBodyParts, intervalSelector);
        var anchoredSpan = anchoredParts.Count > 0 ? EstimateIntervalSpan(anchoredParts, intervalSelector) : 0.0;
        var usableSpan = EstimateIntervalSpan(usableParts, intervalSelector);
        var referenceSpan = Math.Max(anchoredSpan, usableSpan);

        if (referenceSpan <= 1e-6 || primarySpan >= referenceSpan * 0.60)
        {
            return primaryBodyParts;
        }

        return anchoredParts.Count > 0 ? anchoredParts : usableParts;
    }

    private static double EstimateIntervalSpan(
        IReadOnlyList<PartFeature> parts,
        Func<PartFeature, (double Min, double Max)> intervalSelector)
    {
        if (parts.Count == 0)
        {
            return 0.0;
        }

        var intervals = parts.Select(intervalSelector).ToArray();
        return Math.Max(intervals.Max(item => item.Max) - intervals.Min(item => item.Min), 0.0);
    }

    private static (double Min, double Max) EstimateProjectedInterval(
        PartFeature part,
        IReadOnlyList<LongitudinalAxisSegment> axisSegments,
        Vector3 fallbackAxis)
    {
        var sourcePoints = CollectRepresentativePoints(part);
        if (sourcePoints.Length == 0)
        {
            var center = EstimateAxisCoordinate(part.Centroid, axisSegments, fallbackAxis);
            return (center, center);
        }

        var coordinates = sourcePoints
            .Select(point => EstimateAxisCoordinate(point, axisSegments, fallbackAxis))
            .ToArray();

        if (coordinates.Length == 0)
        {
            var center = EstimateAxisCoordinate(part.Centroid, axisSegments, fallbackAxis);
            return (center, center);
        }

        return (coordinates.Min(), coordinates.Max());
    }

    private static double GetPrimaryLength(PartFeature part) =>
        new[] { Math.Abs(part.ObbDims.X), Math.Abs(part.ObbDims.Y), Math.Abs(part.ObbDims.Z) }.Max();

    private static HashSet<int> CollectConnectedComponentPartIds(PartGraph graph, int seedPartId)
    {
        if (!graph.Parts.ContainsKey(seedPartId))
        {
            return new HashSet<int>();
        }

        var visited = new HashSet<int> { seedPartId };
        var queue = new Queue<int>();
        queue.Enqueue(seedPartId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited;
    }

    private static (bool NearStart, bool NearEnd) EstimateEndProximity(
        (double Min, double Max) projectedInterval,
        double assemblySpan)
    {
        if (assemblySpan <= 1e-6)
        {
            return (false, false);
        }

        var tolerance = assemblySpan * 0.05;
        var nearStart = projectedInterval.Min <= tolerance;
        var nearEnd = projectedInterval.Max >= assemblySpan - tolerance;
        return (nearStart, nearEnd);
    }

    private static bool IsSingleEndedLocalPart(bool nearStart, bool nearEnd) =>
        nearStart ^ nearEnd;

    private static (double Min, double Max) TranslateIntervalToAssemblyOrigin(
        (double Min, double Max) projectedInterval,
        double assemblyMin)
    {
        return (projectedInterval.Min - assemblyMin, projectedInterval.Max - assemblyMin);
    }

    private static bool HasUsableSpanGeometry(PartFeature part, IReadOnlyList<LongitudinalAxisSegment> axisSegments, Vector3 axis)
    {
        var interval = EstimateProjectedInterval(part, axisSegments, axis);
        var span = Math.Max(interval.Max - interval.Min, 0.0);
        var boxSize = part.BoundingBox.Size;
        var hasBoundingBox = Math.Abs(boxSize.X) > 1e-6 || Math.Abs(boxSize.Y) > 1e-6 || Math.Abs(boxSize.Z) > 1e-6;
        return span > 1e-3 && (hasBoundingBox || part.Volume > 1e-3 || part.SurfaceArea > 1e-3);
    }

    private static IReadOnlyList<LongitudinalAxisSegment> EstimateBodyAxisSegments(
        IReadOnlyList<PartFeature> parts,
        int inputMainPartId,
        Vector3 fallbackAxis)
    {
        var rawCandidates = parts
            .Where(HasUsableGuideSegments)
            .Select(
                part => new
                {
                    Part = part,
                    Polyline = BuildGuidePolyline(part)
                })
            .Select(
                item => new
                {
                    item.Part,
                    item.Polyline,
                    Length = EstimatePolylineLength(item.Polyline),
                    ProgressRatio = EstimateProgressRatio(item.Polyline, fallbackAxis)
                })
            .Where(item => item.Polyline.Count >= 2 && item.Length > 1e-3)
            .ToArray();

        var maxLength = rawCandidates.Length > 0
            ? rawCandidates.Max(item => item.Length)
            : 0.0;
        var candidates = rawCandidates
            .Select(
                item => new
                {
                    item.Part,
                    item.Polyline,
                    item.Length,
                    item.ProgressRatio,
                    LengthRatio = maxLength <= 1e-6 ? 0.0 : item.Length / maxLength,
                    ComparableToLongest = maxLength <= 1e-6 || item.Length >= maxLength * 0.60
                })
            .OrderByDescending(item => item.ProgressRatio)
            .ThenByDescending(item => item.Length)
            .ThenByDescending(item => item.ComparableToLongest && item.Part.PartId == inputMainPartId)
            .ThenByDescending(item => item.ComparableToLongest && IsPrimaryBodyRole(item.Part.SemanticRole))
            .ToArray();

        if (candidates.Length == 0)
        {
            return BuildSingleAxisSegment(fallbackAxis, parts);
        }

        var polyline = candidates[0].Polyline;

        var segments = new List<LongitudinalAxisSegment>();
        var axisPosition = 0.0;
        for (var index = 0; index < polyline.Count - 1; index++)
        {
            var start = polyline[index];
            var end = polyline[index + 1];
            var direction = (end - start).Normalize();
            var length = (end - start).Length;
            if (direction.Length <= 1e-6 || length <= 1e-3)
            {
                continue;
            }

            segments.Add(new LongitudinalAxisSegment(index, start, end, direction, axisPosition, axisPosition + length));
            axisPosition += length;
        }

        return segments.Count > 0 ? segments : BuildSingleAxisSegment(fallbackAxis, parts);
    }

    private static IReadOnlyList<LongitudinalAxisSegment> BuildSingleAxisSegment(Vector3 axis, IReadOnlyList<PartFeature> parts)
    {
        var normalizedAxis = axis.Normalize();
        if (normalizedAxis.Length <= 1e-9)
        {
            normalizedAxis = Vector3.UnitX;
        }

        var points = parts
            .SelectMany(CollectRepresentativePoints)
            .ToArray();
        if (points.Length == 0)
        {
            points = new[] { Vector3.Zero };
        }

        var projections = points.Select(point => point.Dot(normalizedAxis)).ToArray();
        var min = projections.Min();
        var max = projections.Max();
        if ((max - min) <= 1e-6)
        {
            max = min + 1.0;
        }

        var start = normalizedAxis * min;
        var end = normalizedAxis * max;
        return new[]
        {
            new LongitudinalAxisSegment(0, start, end, normalizedAxis, 0.0, max - min)
        };
    }

    private static bool HasUsableGuideSegments(PartFeature part) =>
        part.SolidEdges.Count > 0 &&
        (part.RuntimeType == RuntimePartType.PolyBeam || part.RuntimeType == RuntimePartType.Beam);

    private static IReadOnlyList<Vector3> BuildGuidePolyline(PartFeature part)
    {
        var candidates = part.SolidEdges
            .Select(segment => NormalizeGuideSegment(part, segment))
            .Where(item => item.Length > 50.0)
            .Where(item => item.Alignment >= 0.45)
            .GroupBy(
                item => BuildSegmentKey(item.Start, item.End),
                StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(item => item.Length).First())
            .ToArray();

        if (candidates.Length == 0)
        {
            return Array.Empty<Vector3>();
        }

        var points = new Dictionary<string, Vector3>(StringComparer.Ordinal);
        foreach (var candidate in candidates)
        {
            points[BuildPointKey(candidate.Start)] = candidate.Start;
            points[BuildPointKey(candidate.End)] = candidate.End;
        }

        if (points.Count < 2)
        {
            return Array.Empty<Vector3>();
        }

        var overallAxis = part.PlateLongDirection.Normalize();
        if (overallAxis.Length <= 1e-6)
        {
            var pair = FindFarthestPair(points.Values.ToArray());
            overallAxis = (pair.End - pair.Start).Normalize();
        }

        if (overallAxis.Length <= 1e-6)
        {
            overallAxis = Vector3.UnitX;
        }

        return points.Values
            .OrderBy(point => point.Dot(overallAxis))
            .ToArray();
    }

    private static (Vector3 Start, Vector3 End, double Length, double Alignment) NormalizeGuideSegment(PartFeature part, LineSegment3 segment)
    {
        var normal = part.PlateNormal.Normalize();
        var start = ProjectToMidPlane(segment.Start, part.Centroid, normal);
        var end = ProjectToMidPlane(segment.End, part.Centroid, normal);
        var vector = end - start;
        var length = vector.Length;
        if (length <= 1e-6)
        {
            return (start, end, 0.0, 0.0);
        }

        var direction = vector.Normalize();
        var guide = part.PlateLongDirection.Normalize();
        var alignment = Math.Abs(direction.Dot(guide));
        if (start.Dot(guide) > end.Dot(guide))
        {
            (start, end) = (end, start);
        }

        return (start, end, length, alignment);
    }

    private static Vector3 ProjectToMidPlane(Vector3 point, Vector3 origin, Vector3 normal)
    {
        var unitNormal = normal.Normalize();
        if (unitNormal.Length <= 1e-9)
        {
            return point;
        }

        var offset = (point - origin).Dot(unitNormal);
        return point - (unitNormal * offset);
    }

    private static Vector3[] CollectRepresentativePoints(PartFeature part)
    {
        var points = new List<Vector3>();
        points.AddRange(EnumerateBoundingBoxCorners(part.BoundingBox));
        points.Add(part.Centroid);
        if (part.SolidEdges.Count > 0)
        {
            points.AddRange(part.SolidEdges.Select(edge => edge.Start));
            points.AddRange(part.SolidEdges.Select(edge => edge.End));
        }

        return points.ToArray();
    }

    private static double EstimateAxisCoordinate(Vector3 point, IReadOnlyList<LongitudinalAxisSegment> axisSegments, Vector3 fallbackAxis)
    {
        if (axisSegments.Count == 0)
        {
            var normalizedAxis = fallbackAxis.Normalize();
            if (normalizedAxis.Length <= 1e-9)
            {
                normalizedAxis = Vector3.UnitX;
            }

            return point.Dot(normalizedAxis);
        }

        var bestDistance = double.MaxValue;
        var bestCoordinate = 0.0;
        foreach (var segment in axisSegments)
        {
            var vector = point - segment.StartPoint;
            var local = vector.Dot(segment.Direction);
            var clamped = Math.Clamp(local, 0.0, segment.Length);
            var closestPoint = segment.StartPoint + (segment.Direction * clamped);
            var distance = (point - closestPoint).Length;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCoordinate = segment.AxisStart + clamped;
            }
        }

        return bestCoordinate;
    }

    private static string BuildPointKey(Vector3 point) =>
        $"{Math.Round(point.X, 3):0.###}|{Math.Round(point.Y, 3):0.###}|{Math.Round(point.Z, 3):0.###}";

    private static string BuildSegmentKey(Vector3 start, Vector3 end)
    {
        var startKey = BuildPointKey(start);
        var endKey = BuildPointKey(end);
        return string.CompareOrdinal(startKey, endKey) <= 0 ? $"{startKey}->{endKey}" : $"{endKey}->{startKey}";
    }

    private static double EstimatePolylineLength(IReadOnlyList<Vector3> polyline)
    {
        double total = 0.0;
        for (var index = 0; index < polyline.Count - 1; index++)
        {
            total += (polyline[index + 1] - polyline[index]).Length;
        }

        return total;
    }

    private static double EstimateProgressRatio(IReadOnlyList<Vector3> polyline, Vector3 fallbackAxis)
    {
        if (polyline.Count < 2)
        {
            return 0.0;
        }

        var axis = fallbackAxis.Normalize();
        if (axis.Length <= 1e-6)
        {
            axis = (polyline[^1] - polyline[0]).Normalize();
        }

        if (axis.Length <= 1e-6)
        {
            axis = Vector3.UnitX;
        }

        var progress = Math.Abs((polyline[^1] - polyline[0]).Dot(axis));
        var length = EstimatePolylineLength(polyline);
        if (length <= 1e-6)
        {
            return 0.0;
        }

        return progress / length;
    }

    private static (Vector3 Start, Vector3 End) FindFarthestPair(IReadOnlyList<Vector3> points)
    {
        var bestStart = Vector3.Zero;
        var bestEnd = Vector3.Zero;
        var bestDistance = -1.0;

        for (var i = 0; i < points.Count; i++)
        {
            for (var j = i + 1; j < points.Count; j++)
            {
                var distance = (points[j] - points[i]).Length;
                if (distance <= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestStart = points[i];
                bestEnd = points[j];
            }
        }

        return (bestStart, bestEnd);
    }

    private static IEnumerable<Vector3> EnumerateBoundingBoxCorners(BoundingBox boundingBox)
    {
        foreach (var x in new[] { boundingBox.Min.X, boundingBox.Max.X })
        {
            foreach (var y in new[] { boundingBox.Min.Y, boundingBox.Max.Y })
            {
                foreach (var z in new[] { boundingBox.Min.Z, boundingBox.Max.Z })
                {
                    yield return new Vector3(x, y, z);
                }
            }
        }
    }

    private static bool IsPrimaryBodyRole(PartSemanticRole role) =>
        role is PartSemanticRole.WebCandidate or PartSemanticRole.FlangeCandidate or PartSemanticRole.WallCandidate;
}
