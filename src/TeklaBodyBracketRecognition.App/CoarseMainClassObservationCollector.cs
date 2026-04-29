using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class CoarseMainClassObservationCollector
{
    private sealed record CandidatePart(
        int PartId,
        double PresenceRatio,
        double EnvelopeSupportRatio);

    private sealed record StationObservation(
        bool Eligible,
        bool IsBox,
        bool IsH,
        bool IsPrimaryPlate,
        bool HasClosedLoop,
        int DistinctPartCount,
        int CandidateSegmentCount,
        double SpanY,
        double SpanZ);

    public static List<CoarseMainClassObservationRow> Collect(BatchArtifacts artifacts)
    {
        var partitionByAssembly = artifacts.RealInputPartitions.ToDictionary(item => item.AssemblyId, StringComparer.Ordinal);
        var traceByAssembly = artifacts.RealInputTraceCleaning.ToDictionary(item => item.AssemblyId, StringComparer.Ordinal);
        var topologyByAssembly = artifacts.RealInputTopology.ToDictionary(item => item.AssemblyId, StringComparer.Ordinal);

        var rows = new List<CoarseMainClassObservationRow>();
        foreach (var summary in artifacts.BodyMaterialSummaries.OrderBy(item => item.MemberId, StringComparer.Ordinal))
        {
            partitionByAssembly.TryGetValue(summary.AssemblyId, out var partitionView);
            traceByAssembly.TryGetValue(summary.AssemblyId, out var traceView);
            topologyByAssembly.TryGetValue(summary.AssemblyId, out var topologyView);

            rows.Add(BuildRow(summary, partitionView, traceView, topologyView));
        }

        return rows;
    }

    private static CoarseMainClassObservationRow BuildRow(
        BodyMaterialSummary summary,
        BodyCandidatePartitionViewOutput? partitionView,
        SectionTraceCleaningViewOutput? traceView,
        SectionTopologyViewOutput? topologyView)
    {
        var priorityStationCount = traceView?.Result.Stations.Count(item => item.StationKind != SectionStationKind.LowPriority) ?? 0;
        var bodyCandidateIds = new HashSet<int>(
            partitionView?.Partition.Items
                .Where(IsBodyLikePartition)
                .Select(item => item.PartId) ?? Enumerable.Empty<int>());
        var partitionItems = partitionView?.Partition.Items ?? Array.Empty<BodyCandidatePartitionItem>();
        var candidateParts = SelectCandidateParts(partitionItems, bodyCandidateIds, priorityStationCount);
        var candidatePartIds = candidateParts.Select(item => item.PartId).OrderBy(static item => item).ToArray();
        var candidatePartSet = candidatePartIds.ToHashSet();
        var stations = BuildStationObservations(traceView, topologyView, candidatePartSet);

        var eligibleStationCount = stations.Count(item => item.Eligible);
        var boxStationCount = stations.Count(item => item.IsBox);
        var hStationCount = stations.Count(item => item.IsH);
        var primaryPlateStationCount = stations.Count(item => item.IsPrimaryPlate);
        var closedLoopStationCount = stations.Count(item => item.HasClosedLoop);

        var boxStationRatio = Ratio(boxStationCount, eligibleStationCount);
        var hStationRatio = Ratio(hStationCount, eligibleStationCount);
        var primaryPlateStationRatio = Ratio(primaryPlateStationCount, eligibleStationCount);
        var closedLoopStationRatio = Ratio(closedLoopStationCount, eligibleStationCount);

        var directCode = ResolveSourceMemberMainClassCode(summary.SourceFile);
        var directLabel = ToMainClassLabelZh(directCode);
        var attributeDirectBypass = IsAttributeDirectBypass(summary.BodyDescriptorFamily);

        var decision = ResolveDecision(
            stations,
            eligibleStationCount,
            candidatePartIds.Length,
            boxStationRatio,
            hStationRatio,
            primaryPlateStationRatio,
            closedLoopStationRatio,
            attributeDirectBypass);

        return new CoarseMainClassObservationRow
        {
            SourceFile = summary.SourceFile,
            MemberId = summary.MemberId,
            AssemblyId = summary.AssemblyId,
            SourceMemberMainClassCode = directCode,
            SourceMemberMainClassLabelZh = directLabel,
            BodyDescriptorFamily = summary.BodyDescriptorFamily,
            BodyDescriptorSectionType = summary.BodyDescriptorSectionType,
            ImportSynthesisKind = summary.ImportSynthesisKind ?? string.Empty,
            LongitudinalTypeCode = summary.LongitudinalTypeCode,
            LongitudinalTypeLabelZh = summary.LongitudinalTypeLabelZh,
            PriorityStationCount = priorityStationCount,
            EligibleStationCount = eligibleStationCount,
            CandidatePartCount = candidatePartIds.Length,
            CandidatePartIds = string.Join(",", candidatePartIds),
            ExcludedPartCount = Math.Max(0, bodyCandidateIds.Count - candidatePartIds.Length),
            BoxStationCount = boxStationCount,
            HStationCount = hStationCount,
            PrimaryPlateStationCount = primaryPlateStationCount,
            ClosedLoopStationCount = closedLoopStationCount,
            BoxStationRatio = boxStationRatio,
            HStationRatio = hStationRatio,
            PrimaryPlateStationRatio = primaryPlateStationRatio,
            ClosedLoopStationRatio = closedLoopStationRatio,
            CoarseMainClassCode = decision.Code,
            CoarseMainClassLabelZh = decision.LabelZh,
            CoarseMainClassConfidence = decision.Confidence,
            CoarseMainClassSubtypeCode = decision.SubtypeCode,
            CoarseMainClassSubtypeLabelZh = decision.SubtypeLabelZh,
            CoarseMainClassReasonCode = decision.ReasonCode,
            CoarseMainClassReasonLabelZh = decision.ReasonLabelZh
        };
    }

    private static IReadOnlyList<CandidatePart> SelectCandidateParts(
        IReadOnlyList<BodyCandidatePartitionItem> partitionItems,
        IReadOnlySet<int> bodyCandidateIds,
        int priorityStationCount)
    {
        var selected = partitionItems
            .Where(
                item =>
                    IsBodyLikePartition(item) &&
                    bodyCandidateIds.Contains(item.PartId) &&
                    item.LongitudinalCoverageEstimate >= ResolveCandidateCoverageThreshold(item, priorityStationCount))
            .OrderByDescending(item => item.LongitudinalCoverageEstimate)
            .ThenByDescending(item => item.PartitionConfidence)
            .ThenBy(item => item.PartId)
            .Select(
                item => new CandidatePart(
                    item.PartId,
                    item.LongitudinalCoverageEstimate,
                    item.LongitudinalCoverageEstimate))
            .ToArray();

        if (selected.Length > 0)
        {
            return selected;
        }

        return partitionItems
            .Where(
                item =>
                    IsBodyLikePartition(item) &&
                    bodyCandidateIds.Contains(item.PartId) &&
                    item.LongitudinalCoverageEstimate >= 0.30d)
            .OrderByDescending(item => item.LongitudinalCoverageEstimate)
            .ThenBy(item => item.PartId)
            .Take(4)
            .Select(item => new CandidatePart(item.PartId, item.LongitudinalCoverageEstimate, item.LongitudinalCoverageEstimate))
            .ToArray();
    }

    private static double ResolveCandidateCoverageThreshold(BodyCandidatePartitionItem item, int priorityStationCount)
    {
        if (item.PartitionClass == BodyCandidatePartitionClass.SpecialShape)
        {
            return priorityStationCount <= 2 ? 0.25d : 0.30d;
        }

        return priorityStationCount <= 2 ? 0.25d : 0.30d;
    }

    private static List<StationObservation> BuildStationObservations(
        SectionTraceCleaningViewOutput? traceView,
        SectionTopologyViewOutput? topologyView,
        IReadOnlySet<int> candidatePartIds)
    {
        if (traceView is null || topologyView is null || candidatePartIds.Count == 0)
        {
            return new List<StationObservation>();
        }

        var topologyByStation = topologyView.Result.Stations.ToDictionary(item => item.StationId, StringComparer.Ordinal);
        var observations = new List<StationObservation>();

        foreach (var station in traceView.Result.Stations.Where(item => item.StationKind != SectionStationKind.LowPriority))
        {
            if (!topologyByStation.TryGetValue(station.StationId, out var topologyStation))
            {
                continue;
            }

            var candidateSegments = station.Segments
                .Where(
                    item =>
                        !item.IsSuppressed &&
                        candidatePartIds.Contains(item.PartId) &&
                        IsBodyLikePartition(item.PartitionClass))
                .ToArray();

            if (candidateSegments.Length == 0)
            {
                observations.Add(new StationObservation(false, false, false, false, false, 0, 0, 0d, 0d));
                continue;
            }

            var distinctPartCount = candidateSegments.Select(item => item.PartId).Distinct().Count();
            var envelopeWidth = Math.Max(1d, topologyStation.EnvelopeMaxY - topologyStation.EnvelopeMinY);
            var envelopeHeight = Math.Max(1d, topologyStation.EnvelopeMaxZ - topologyStation.EnvelopeMinZ);
            var envelopeCenterY = (topologyStation.EnvelopeMaxY + topologyStation.EnvelopeMinY) * 0.5d;
            var envelopeCenterZ = (topologyStation.EnvelopeMaxZ + topologyStation.EnvelopeMinZ) * 0.5d;

            var horizontalSegments = candidateSegments.Count(IsHorizontalLike);
            var verticalSegments = candidateSegments.Count(IsVerticalLike);
            var hasUpperHorizontal = candidateSegments.Any(item => IsHorizontalLike(item) && item.CenterZ > envelopeCenterZ);
            var hasLowerHorizontal = candidateSegments.Any(item => IsHorizontalLike(item) && item.CenterZ < envelopeCenterZ);
            var hasCentralVertical = candidateSegments.Any(
                item => IsVerticalLike(item) && Math.Abs(item.CenterY - envelopeCenterY) <= Math.Max(40d, envelopeWidth * 0.18d));
            var hasClosedLoop = topologyStation.ClosedLoopCandidate;

            var isBox = hasClosedLoop && distinctPartCount >= 3 && candidateSegments.Length >= 3;
            var isAxisAlignedH =
                distinctPartCount >= 2 &&
                horizontalSegments >= 2 &&
                verticalSegments >= 1 &&
                hasUpperHorizontal &&
                hasLowerHorizontal &&
                hasCentralVertical;
            var isTopologyH = HasHLikeTopology(candidateSegments, envelopeWidth, envelopeHeight);
            var isH = !hasClosedLoop && (isAxisAlignedH || isTopologyH);
            var isPrimaryPlate = !isBox && !isH && distinctPartCount <= 2;

            observations.Add(
                new StationObservation(
                    true,
                    isBox,
                    isH,
                    isPrimaryPlate,
                    hasClosedLoop,
                    distinctPartCount,
                    candidateSegments.Length,
                    envelopeWidth,
                    envelopeHeight));
        }

        return observations;
    }

    private static bool HasHLikeTopology(
        IReadOnlyList<SectionTraceCleanSegment> segments,
        double envelopeWidth,
        double envelopeHeight)
    {
        if (segments.Count < 3)
        {
            return false;
        }

        var primaryGapFloor = Math.Max(40d, Math.Min(envelopeWidth, envelopeHeight) * 0.10d);
        var centerTolerance = Math.Max(35d, Math.Min(envelopeWidth, envelopeHeight) * 0.12d);
        var spanTolerance = Math.Max(20d, Math.Min(envelopeWidth, envelopeHeight) * 0.05d);

        foreach (var seed in segments)
        {
            if (!TryGetSegmentDirection(seed, out var seedDirectionY, out var seedDirectionZ))
            {
                continue;
            }

            var parallelFamily = segments
                .Where(
                    item =>
                        TryGetSegmentDirection(item, out var directionY, out var directionZ) &&
                        AreParallel(seedDirectionY, seedDirectionZ, directionY, directionZ))
                .ToArray();
            if (parallelFamily.Length < 2)
            {
                continue;
            }

            var normalY = -seedDirectionZ;
            var normalZ = seedDirectionY;
            var centerOffsets = parallelFamily
                .Select(item => Project(item.CenterY, item.CenterZ, normalY, normalZ))
                .OrderBy(value => value)
                .ToArray();
            var minOffset = centerOffsets.First();
            var maxOffset = centerOffsets.Last();
            if (maxOffset - minOffset < primaryGapFloor)
            {
                continue;
            }

            var familyCenterOffset = (minOffset + maxOffset) * 0.5d;
            var outerPlates = parallelFamily
                .Where(
                    item =>
                    {
                        var offset = Project(item.CenterY, item.CenterZ, normalY, normalZ);
                        return Math.Abs(offset - familyCenterOffset) >= primaryGapFloor * 0.35d;
                    })
                .OrderBy(item => Project(item.CenterY, item.CenterZ, normalY, normalZ))
                .ToArray();
            if (outerPlates.Length < 2)
            {
                continue;
            }

            var lowerOuter = outerPlates.First();
            var upperOuter = outerPlates.Last();
            var lowerCenterAlong = Project(lowerOuter.CenterY, lowerOuter.CenterZ, seedDirectionY, seedDirectionZ);
            var upperCenterAlong = Project(upperOuter.CenterY, upperOuter.CenterZ, seedDirectionY, seedDirectionZ);
            var minOuterAlong = Math.Min(lowerCenterAlong, upperCenterAlong);
            var maxOuterAlong = Math.Max(lowerCenterAlong, upperCenterAlong);

            var connectorExists = segments
                .Where(item => !parallelFamily.Contains(item))
                .Any(
                    item =>
                    {
                        if (!TryGetSegmentDirection(item, out var directionY, out var directionZ))
                        {
                            return false;
                        }

                        if (!ArePerpendicular(seedDirectionY, seedDirectionZ, directionY, directionZ))
                        {
                            return false;
                        }

                        var connectorOffset = Project(item.CenterY, item.CenterZ, normalY, normalZ);
                        if (Math.Abs(connectorOffset - familyCenterOffset) > centerTolerance)
                        {
                            return false;
                        }

                        var startAlong = Project(item.StartY, item.StartZ, seedDirectionY, seedDirectionZ);
                        var endAlong = Project(item.EndY, item.EndZ, seedDirectionY, seedDirectionZ);
                        var connectorMinAlong = Math.Min(startAlong, endAlong);
                        var connectorMaxAlong = Math.Max(startAlong, endAlong);
                        return connectorMinAlong <= minOuterAlong + spanTolerance &&
                               connectorMaxAlong >= maxOuterAlong - spanTolerance;
                    });

            if (connectorExists)
            {
                return true;
            }
        }

        return false;
    }

    private static (string Code, string LabelZh, double Confidence, string SubtypeCode, string SubtypeLabelZh, string ReasonCode, string ReasonLabelZh)
        ResolveDecision(
            IReadOnlyList<StationObservation> stations,
            int eligibleStationCount,
            int candidatePartCount,
            double boxStationRatio,
            double hStationRatio,
            double primaryPlateStationRatio,
            double closedLoopStationRatio,
            bool attributeDirectBypass)
    {
        if (attributeDirectBypass)
        {
            return (
                string.Empty,
                string.Empty,
                0d,
                string.Empty,
                string.Empty,
                "ATTRIBUTE_DIRECT_BYPASS",
                "标准截面型材已走属性直达，不进入粗分类观察路线");
        }

        if (eligibleStationCount == 0 || candidatePartCount == 0)
        {
            return (
                string.Empty,
                string.Empty,
                0d,
                string.Empty,
                string.Empty,
                "NO_ELIGIBLE_CANDIDATE_SET",
                "没有形成可观察的主板候选集合");
        }

        var familyVariability = ResolveFamilyVariability(stations);

        if (candidatePartCount >= 3 &&
            boxStationRatio >= 0.5d)
        {
            var (subtypeCode, subtypeLabelZh) = familyVariability.IsVariable
                ? ("VARIABLE_SECTION_BOX", "变化截面箱体")
                : ("CLOSED_LOOP_BOX", "恒定截面箱体");
            var confidence = Math.Min(0.99d, 0.55d + boxStationRatio * 0.25d + closedLoopStationRatio * 0.20d);
            return (
                "BOX",
                "闭合箱形主体",
                confidence,
                subtypeCode,
                subtypeLabelZh,
                "MULTI_WALL_CLOSED_LOOP_CONSENSUS",
                "多壁候选在多数切片共同形成闭合围合");
        }

        if (candidatePartCount >= 2 &&
            hStationRatio >= 0.45d)
        {
            var (subtypeCode, subtypeLabelZh) = familyVariability.IsVariable
                ? ("VARIABLE_SECTION_H", "变化截面 H")
                : ("CONST_SECTION_H", "恒定截面 H");
            var confidence = Math.Min(0.96d, 0.52d + hStationRatio * 0.40d);
            return (
                "H",
                "H型主体",
                confidence,
                subtypeCode,
                subtypeLabelZh,
                "WEB_FLANGE_SECTION_CONSENSUS",
                "多切片稳定呈现腹板与上下翼缘组合");
        }

        if (candidatePartCount <= 2 && primaryPlateStationRatio >= 0.60d)
        {
            var (subtypeCode, subtypeLabelZh) = familyVariability.IsVariable
                ? ("VARIABLE_PRIMARY_PLATE", "变化截面主板体")
                : ("CONST_PRIMARY_PLATE", "恒定截面主板体");
            var confidence = Math.Min(0.93d, 0.50d + primaryPlateStationRatio * 0.35d);
            return (
                "PRIMARY_PLATE_BODY",
                "单主板主体",
                confidence,
                subtypeCode,
                subtypeLabelZh,
                "SINGLE_PLATE_STATION_MAJORITY",
                "多数切片由单主板或双板窄组合主导");
        }

        return (
            string.Empty,
            string.Empty,
            0d,
            string.Empty,
            string.Empty,
            "TOPOLOGY_CONSENSUS_NOT_REACHED",
            "多切片还没有形成稳定的粗拓扑共识");
    }

    private static (bool IsVariable, double VariationScore) ResolveFamilyVariability(
        IReadOnlyList<StationObservation> stations)
    {
        var eligibleStations = stations.Where(item => item.Eligible).ToArray();
        if (eligibleStations.Length <= 1)
        {
            return (false, 0d);
        }

        var minSpanY = eligibleStations.Min(item => item.SpanY);
        var maxSpanY = eligibleStations.Max(item => item.SpanY);
        var minSpanZ = eligibleStations.Min(item => item.SpanZ);
        var maxSpanZ = eligibleStations.Max(item => item.SpanZ);
        var minPartCount = eligibleStations.Min(item => item.DistinctPartCount);
        var maxPartCount = eligibleStations.Max(item => item.DistinctPartCount);
        var minSegmentCount = eligibleStations.Min(item => item.CandidateSegmentCount);
        var maxSegmentCount = eligibleStations.Max(item => item.CandidateSegmentCount);

        var spanYVariation = maxSpanY > 0d ? (maxSpanY - minSpanY) / maxSpanY : 0d;
        var spanZVariation = maxSpanZ > 0d ? (maxSpanZ - minSpanZ) / maxSpanZ : 0d;
        var partCountVariation = maxPartCount != minPartCount;
        var segmentCountVariation = maxSegmentCount != minSegmentCount;

        var variationScore =
            Math.Max(spanYVariation, spanZVariation) +
            (partCountVariation ? 0.35d : 0d) +
            (segmentCountVariation ? 0.20d : 0d);

        return (variationScore >= 0.28d, variationScore);
    }

    private static bool IsHorizontalLike(SectionTraceCleanSegment segment)
    {
        var dy = Math.Abs(segment.EndY - segment.StartY);
        var dz = Math.Abs(segment.EndZ - segment.StartZ);
        return dy >= dz * 1.5d;
    }

    private static bool IsVerticalLike(SectionTraceCleanSegment segment)
    {
        var dy = Math.Abs(segment.EndY - segment.StartY);
        var dz = Math.Abs(segment.EndZ - segment.StartZ);
        return dz >= dy * 1.5d;
    }

    private static bool TryGetSegmentDirection(
        SectionTraceCleanSegment segment,
        out double directionY,
        out double directionZ)
    {
        var dy = segment.EndY - segment.StartY;
        var dz = segment.EndZ - segment.StartZ;
        var length = Math.Sqrt((dy * dy) + (dz * dz));
        if (length <= 1e-6)
        {
            directionY = 0d;
            directionZ = 0d;
            return false;
        }

        directionY = dy / length;
        directionZ = dz / length;
        if (directionY < 0d || (Math.Abs(directionY) <= 1e-6 && directionZ < 0d))
        {
            directionY = -directionY;
            directionZ = -directionZ;
        }

        return true;
    }

    private static bool AreParallel(
        double leftY,
        double leftZ,
        double rightY,
        double rightZ)
    {
        return Math.Abs((leftY * rightY) + (leftZ * rightZ)) >= 0.92d;
    }

    private static bool ArePerpendicular(
        double leftY,
        double leftZ,
        double rightY,
        double rightZ)
    {
        return Math.Abs((leftY * rightY) + (leftZ * rightZ)) <= 0.35d;
    }

    private static double Project(double y, double z, double axisY, double axisZ)
    {
        return (y * axisY) + (z * axisZ);
    }

    private static bool IsBodyLikePartition(BodyCandidatePartitionItem item)
    {
        return IsBodyLikePartition(item.PartitionClass);
    }

    private static bool IsBodyLikePartition(BodyCandidatePartitionClass partitionClass)
    {
        return partitionClass is BodyCandidatePartitionClass.BodyCandidate or BodyCandidatePartitionClass.SpecialShape;
    }

    private static bool IsAttributeDirectBypass(string bodyDescriptorFamily)
    {
        return string.Equals(bodyDescriptorFamily, "StandardSection", StringComparison.Ordinal);
    }

    private static double Ratio(int numerator, int denominator)
    {
        return denominator <= 0 ? 0d : numerator / (double)denominator;
    }

    private static string ResolveSourceMemberMainClassCode(string sourceFile)
    {
        return DefinitionClauseDecisionFullRunSourceCollector.ResolveSourceMemberMainClassCodeForObservation(sourceFile);
    }

    private static string ToMainClassLabelZh(string code)
    {
        return code switch
        {
            "H" => "H型主体",
            "BOX" => "闭合箱形主体",
            "T" => "T型主体",
            "CROSS" => "十字型主体",
            "L" => "L型主体",
            "PIPE" => "管型主体",
            "ROD" => "杆型主体",
            "IRREGULAR" => "不规则主体",
            _ => "NONE"
        };
    }
}
