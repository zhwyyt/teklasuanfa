using System.Text.Json;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class FoundationGeometryHealthAuditCollector
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string[] EnabledChecks =
    {
        "AxisConsistency",
        "CandidateSetConsistency",
        "SampleTraceConsistency"
    };

    public static List<FoundationGeometryHealthAuditMemberRow> Collect(BatchArtifacts artifacts)
    {
        var partitionByAssembly = artifacts.RealInputPartitions.ToDictionary(item => item.AssemblyId, StringComparer.Ordinal);
        var rawTraceByAssembly = artifacts.RealInputSectionTraces.ToDictionary(item => item.AssemblyId, StringComparer.Ordinal);
        var cleanTraceByAssembly = artifacts.RealInputTraceCleaning.ToDictionary(item => item.AssemblyId, StringComparer.Ordinal);
        var snapshotBySourceFile = new Dictionary<string, XingcaiMemberSnapshot?>(StringComparer.OrdinalIgnoreCase);

        var rows = new List<FoundationGeometryHealthAuditMemberRow>();
        foreach (var summary in artifacts.BodyMaterialSummaries.OrderBy(item => item.MemberId, StringComparer.Ordinal))
        {
            partitionByAssembly.TryGetValue(summary.AssemblyId, out var partitionView);
            rawTraceByAssembly.TryGetValue(summary.AssemblyId, out var rawTraceView);
            cleanTraceByAssembly.TryGetValue(summary.AssemblyId, out var cleanTraceView);

            var snapshot = LoadSnapshot(summary.SourceFile, snapshotBySourceFile);
            rows.Add(BuildMemberRow(summary, snapshot, partitionView, rawTraceView, cleanTraceView));
        }

        return rows;
    }

    private static FoundationGeometryHealthAuditMemberRow BuildMemberRow(
        BodyMaterialSummary summary,
        XingcaiMemberSnapshot? snapshot,
        BodyCandidatePartitionViewOutput? partitionView,
        SectionTraceViewOutput? rawTraceView,
        SectionTraceCleaningViewOutput? cleanTraceView)
    {
        var partitionItems = partitionView?.Partition.Items ?? Array.Empty<BodyCandidatePartitionItem>();
        var selectedCandidates = SelectCandidateParts(partitionItems);
        var selectedCandidateIds = selectedCandidates.Select(item => item.PartId).OrderBy(static item => item).ToArray();
        var bodyCandidateIds = partitionItems
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .Select(item => item.PartId)
            .Distinct()
            .OrderBy(static item => item)
            .ToArray();

        var axisEvidence = EvaluateAxisConsistency(snapshot, selectedCandidates);
        var candidateEvidence = EvaluateCandidateSetConsistency(snapshot, partitionItems, selectedCandidates);
        var traceEvidence = EvaluateSampleTraceConsistency(rawTraceView, cleanTraceView, selectedCandidateIds);

        var checkResults = new (string CheckCode, string StatusCode, IReadOnlyList<string> ReasonCodes)[]
        {
            ("AxisConsistency", axisEvidence.StatusCode, axisEvidence.ReasonCodes),
            ("CandidateSetConsistency", candidateEvidence.StatusCode, candidateEvidence.ReasonCodes),
            ("SampleTraceConsistency", traceEvidence.StatusCode, traceEvidence.ReasonCodes)
        };

        var overallStatusCode = ResolveOverallStatus(checkResults.Select(item => item.StatusCode));
        var primaryReasonCode = ResolvePrimaryReasonCode(checkResults);
        var suggestedLayerCode = ResolveSuggestedLayerCode(checkResults);
        var reasonCodes = checkResults
            .SelectMany(item => item.ReasonCodes)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (reasonCodes.Count == 0)
        {
            reasonCodes.Add(primaryReasonCode);
        }

        return new FoundationGeometryHealthAuditMemberRow
        {
            AssemblyId = summary.AssemblyId,
            AssemblyNumber = summary.MemberId,
            MemberId = summary.MemberId,
            MemberName = string.Empty,
            Profile = summary.MemberProfileString,
            SourceFileName = summary.SourceFile,
            OverallHealthStatusCode = overallStatusCode,
            OverallHealthStatusLabelZh = ToStatusLabelZh(overallStatusCode),
            PrimaryReasonCode = primaryReasonCode,
            PrimaryReasonLabelZh = ToReasonLabelZh(primaryReasonCode),
            SuggestedInvestigationLayerCode = suggestedLayerCode,
            SuggestedInvestigationLayerLabelZh = ToLayerLabelZh(suggestedLayerCode),
            ReasonCodes = reasonCodes,
            EvidenceSummaryZh = BuildEvidenceSummaryZh(axisEvidence, candidateEvidence, traceEvidence),
            AxisConsistencyStatusCode = axisEvidence.StatusCode,
            AxisConsistencyStatusLabelZh = ToStatusLabelZh(axisEvidence.StatusCode),
            CandidateSetConsistencyStatusCode = candidateEvidence.StatusCode,
            CandidateSetConsistencyStatusLabelZh = ToStatusLabelZh(candidateEvidence.StatusCode),
            SampleTraceConsistencyStatusCode = traceEvidence.StatusCode,
            SampleTraceConsistencyStatusLabelZh = ToStatusLabelZh(traceEvidence.StatusCode),
            AxisSegmentsLength = axisEvidence.AxisSegmentsLength,
            MemberMainAxisLength = axisEvidence.MemberMainAxisLength,
            LongestMainPlateAxisProjectionLength = axisEvidence.LongestMainPlateAxisProjectionLength,
            CandidateSetSpanLength = axisEvidence.CandidateSetSpanLength,
            AxisLengthToMemberMainAxisRatio = axisEvidence.AxisLengthToMemberMainAxisRatio,
            AxisLengthToLongestMainPlateRatio = axisEvidence.AxisLengthToLongestMainPlateRatio,
            AxisLengthToCandidateSetSpanRatio = axisEvidence.AxisLengthToCandidateSetSpanRatio,
            AxisEvidenceReasonCodes = axisEvidence.ReasonCodes,
            TotalPartCount = snapshot?.Parts.Count ?? 0,
            CandidatePartCount = selectedCandidateIds.Length,
            BodyCandidatePartCount = bodyCandidateIds.Length,
            CandidatePartIds = selectedCandidateIds.ToList(),
            BodyCandidatePartIds = bodyCandidateIds.ToList(),
            SuspectMissingMainPartIds = candidateEvidence.SuspectMissingMainPartIds,
            SuspectAccessoryDominantPartIds = candidateEvidence.SuspectAccessoryDominantPartIds,
            CandidateSetCoverageRatio = candidateEvidence.CandidateSetCoverageRatio,
            CandidateEvidenceReasonCodes = candidateEvidence.ReasonCodes,
            EvaluatedStationCount = traceEvidence.EvaluatedStationCount,
            EvaluatedTraceCount = traceEvidence.EvaluatedTraceCount,
            TraceDirectionDriftCount = traceEvidence.TraceDirectionDriftCount,
            TraceCenterDriftCount = traceEvidence.TraceCenterDriftCount,
            TraceDiagonalizedCount = traceEvidence.TraceDiagonalizedCount,
            TraceParallelRelationLostCount = traceEvidence.TraceParallelRelationLostCount,
            TraceConnectorRelationLostCount = traceEvidence.TraceConnectorRelationLostCount,
            MaxTraceDirectionDeviationDegrees = traceEvidence.MaxTraceDirectionDeviationDegrees,
            SampleTraceEvidenceReasonCodes = traceEvidence.ReasonCodes
        };
    }

    private static AxisConsistencyEvidence EvaluateAxisConsistency(
        XingcaiMemberSnapshot? snapshot,
        IReadOnlyList<BodyCandidatePartitionItem> selectedCandidates)
    {
        var axisLength = SumAxisSegmentLength(snapshot?.AxisSegments);
        var memberMainAxisLength = snapshot?.Member.MainAxis?.Length;
        var longestMainPlateAxisProjection = ResolveLongestMainPlateAxisProjectionLength(snapshot);
        var candidateSetSpanLength = ResolveCandidateSetSpanLength(selectedCandidates);

        var reasonCodes = new List<string>();
        var statusCode = "PASS";
        var axisToMemberRatio = Ratio(axisLength, memberMainAxisLength);
        var axisToPlateRatio = Ratio(axisLength, longestMainPlateAxisProjection);
        var axisToCandidateRatio = Ratio(axisLength, candidateSetSpanLength);

        if (axisLength is null && memberMainAxisLength is > 1e-6)
        {
            statusCode = "FAIL";
            reasonCodes.Add("AXIS_SEGMENTS_MISSING_BUT_MAIN_AXIS_AVAILABLE");
        }

        if (axisToMemberRatio is < 0.55d)
        {
            statusCode = "FAIL";
            reasonCodes.Add("AXIS_TOO_SHORT_FOR_MEMBER");
        }
        else if (axisToMemberRatio is < 0.75d && statusCode != "FAIL")
        {
            statusCode = "WARNING";
            reasonCodes.Add("AXIS_TOO_SHORT_FOR_MEMBER");
        }

        if (axisToPlateRatio is < 0.55d)
        {
            statusCode = "FAIL";
            reasonCodes.Add("AXIS_TOO_SHORT_FOR_MAIN_PLATE");
        }
        else if (axisToPlateRatio is < 0.75d && statusCode == "PASS")
        {
            statusCode = "WARNING";
            reasonCodes.Add("AXIS_TOO_SHORT_FOR_MAIN_PLATE");
        }

        if (axisToCandidateRatio is < 0.60d)
        {
            statusCode = "FAIL";
            reasonCodes.Add("AXIS_SPAN_NOT_COVERING_CANDIDATE_SET");
        }
        else if (axisToCandidateRatio is < 0.80d && statusCode == "PASS")
        {
            statusCode = "WARNING";
            reasonCodes.Add("AXIS_SPAN_NOT_COVERING_CANDIDATE_SET");
        }

        if ((axisToMemberRatio is < 0.45d || axisToPlateRatio is < 0.45d) &&
            candidateSetSpanLength is > 0d)
        {
            statusCode = "FAIL";
            reasonCodes.Add("AXIS_SOURCE_HIJACKED_BY_LOCAL_PART");
        }

        if (reasonCodes.Count == 0)
        {
            if (axisLength is null && memberMainAxisLength is null && longestMainPlateAxisProjection is null)
            {
                statusCode = "NOT_EVALUATED";
                reasonCodes.Add("INPUT_DATA_MISSING");
            }
            else
            {
                reasonCodes.Add("AXIS_OK");
            }
        }

        return new AxisConsistencyEvidence(
            statusCode,
            reasonCodes.Distinct(StringComparer.Ordinal).ToList(),
            axisLength,
            memberMainAxisLength,
            longestMainPlateAxisProjection,
            candidateSetSpanLength,
            axisToMemberRatio,
            axisToPlateRatio,
            axisToCandidateRatio);
    }

    private static CandidateSetConsistencyEvidence EvaluateCandidateSetConsistency(
        XingcaiMemberSnapshot? snapshot,
        IReadOnlyList<BodyCandidatePartitionItem> partitionItems,
        IReadOnlyList<BodyCandidatePartitionItem> selectedCandidates)
    {
        var candidatePartIds = selectedCandidates.Select(item => item.PartId).ToHashSet();
        var candidateSetSpanLength = ResolveCandidateSetSpanLength(selectedCandidates);
        var memberMainAxisLength = snapshot?.Member.MainAxis?.Length;
        var candidateSetCoverageRatio = Ratio(candidateSetSpanLength, memberMainAxisLength);
        var suspectMissingMainPartIds = ResolveSuspectMissingMainPartIds(snapshot, candidatePartIds);
        var suspectAccessoryDominantPartIds = ResolveSuspectAccessoryDominantPartIds(snapshot, partitionItems, candidatePartIds);

        var reasonCodes = new List<string>();
        var statusCode = "PASS";

        if (selectedCandidates.Count == 0)
        {
            statusCode = "FAIL";
            reasonCodes.Add("CANDIDATE_SET_EMPTY");
        }

        if (suspectMissingMainPartIds.Count > 0)
        {
            statusCode = "FAIL";
            reasonCodes.Add("MAIN_PLATE_MISSING_FROM_CANDIDATE_SET");
        }

        if (candidateSetCoverageRatio is < 0.55d)
        {
            statusCode = "FAIL";
            reasonCodes.Add("CANDIDATE_SET_SPAN_TOO_SHORT");
        }
        else if (candidateSetCoverageRatio is < 0.75d && statusCode == "PASS")
        {
            statusCode = "WARNING";
            reasonCodes.Add("CANDIDATE_SET_SPAN_TOO_SHORT");
        }

        if (suspectMissingMainPartIds.Any(id => IsSpecialShapePart(snapshot, id)))
        {
            statusCode = "FAIL";
            reasonCodes.Add("SPECIAL_SHAPE_BODY_LIKE_PART_EXCLUDED");
        }

        if (suspectAccessoryDominantPartIds.Count > 0)
        {
            if (statusCode == "PASS")
            {
                statusCode = "WARNING";
            }

            reasonCodes.Add("ACCESSORY_DOMINATES_CANDIDATE_SET");
        }

        if (reasonCodes.Count == 0)
        {
            if (snapshot is null || memberMainAxisLength is null)
            {
                statusCode = "NOT_EVALUATED";
                reasonCodes.Add("INPUT_DATA_MISSING");
            }
            else
            {
                reasonCodes.Add("CANDIDATE_SET_OK");
            }
        }

        return new CandidateSetConsistencyEvidence(
            statusCode,
            reasonCodes.Distinct(StringComparer.Ordinal).ToList(),
            suspectMissingMainPartIds,
            suspectAccessoryDominantPartIds,
            candidateSetCoverageRatio);
    }

    private static SampleTraceConsistencyEvidence EvaluateSampleTraceConsistency(
        SectionTraceViewOutput? rawTraceView,
        SectionTraceCleaningViewOutput? cleanTraceView,
        IReadOnlyList<int> candidatePartIds)
    {
        if (rawTraceView is null || cleanTraceView is null)
        {
            return new SampleTraceConsistencyEvidence(
                "NOT_EVALUATED",
                new List<string> { "INPUT_DATA_MISSING" },
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                null);
        }

        var candidatePartSet = candidatePartIds.ToHashSet();
        var cleanByStation = cleanTraceView.Result.Stations.ToDictionary(item => item.StationId, StringComparer.Ordinal);
        var evaluatedStationCount = 0;
        var evaluatedTraceCount = 0;
        var traceDirectionDriftCount = 0;
        var traceCenterDriftCount = 0;
        var traceDiagonalizedCount = 0;
        var traceParallelRelationLostCount = 0;
        var traceConnectorRelationLostCount = 0;
        double maxDeviation = 0d;

        foreach (var rawStation in rawTraceView.Result.Stations.Where(item => item.StationKind != SectionStationKind.LowPriority))
        {
            if (!cleanByStation.TryGetValue(rawStation.StationId, out var cleanStation))
            {
                continue;
            }

            var rawSegments = FilterAuditSegments(rawStation.Segments, candidatePartSet);
            var cleanSegments = FilterAuditSegments(cleanStation.Segments, candidatePartSet);
            if (rawSegments.Count == 0 || cleanSegments.Count == 0)
            {
                continue;
            }

            evaluatedStationCount++;
            var sharedPartIds = rawSegments.Keys.Intersect(cleanSegments.Keys).ToArray();
            foreach (var partId in sharedPartIds)
            {
                evaluatedTraceCount++;
                var rawSegment = rawSegments[partId];
                var cleanSegment = cleanSegments[partId];
                var deviation = ResolveDirectionDeviationDegrees(rawSegment, cleanSegment);
                var centerDrift = ResolveCenterDrift(rawSegment, cleanSegment);
                maxDeviation = Math.Max(maxDeviation, deviation);

                if (deviation >= 20d)
                {
                    traceDirectionDriftCount++;
                }

                if (centerDrift >= Math.Max(60d, rawSegment.WidthLength * 0.45d))
                {
                    traceCenterDriftCount++;
                }

                if (IsDiagonalization(rawSegment, cleanSegment, deviation))
                {
                    traceDiagonalizedCount++;
                }
            }

            if (HasParallelRelation(rawSegments.Values) && !HasParallelRelation(cleanSegments.Values))
            {
                traceParallelRelationLostCount++;
            }

            if (HasConnectorRelation(rawSegments.Values) && !HasConnectorRelation(cleanSegments.Values))
            {
                traceConnectorRelationLostCount++;
            }
        }

        var reasonCodes = new List<string>();
        var statusCode = "PASS";

        if (evaluatedStationCount == 0 || evaluatedTraceCount == 0)
        {
            statusCode = "NOT_EVALUATED";
            reasonCodes.Add("EVIDENCE_INCOMPLETE");
        }
        else
        {
            if (traceDiagonalizedCount > 0)
            {
                statusCode = "FAIL";
                reasonCodes.Add("TRACE_DIAGONALIZED_BY_CORNER_GEOMETRY");
            }

            if (traceParallelRelationLostCount > 0)
            {
                statusCode = "FAIL";
                reasonCodes.Add("TRACE_PARALLEL_RELATION_LOST");
            }

            if (traceDirectionDriftCount > 0 && statusCode != "FAIL")
            {
                statusCode = "WARNING";
                reasonCodes.Add("TRACE_DIRECTION_DRIFT_FROM_SAMPLE");
            }

            if (traceCenterDriftCount > 0 && statusCode == "PASS")
            {
                statusCode = "WARNING";
                reasonCodes.Add("TRACE_CENTER_DRIFT_FROM_SAMPLE");
            }

            if (traceConnectorRelationLostCount > 0 && statusCode == "PASS")
            {
                statusCode = "WARNING";
                reasonCodes.Add("TRACE_CONNECTOR_RELATION_LOST");
            }

            if (reasonCodes.Count == 0)
            {
                reasonCodes.Add("SAMPLE_TRACE_OK");
            }
        }

        return new SampleTraceConsistencyEvidence(
            statusCode,
            reasonCodes.Distinct(StringComparer.Ordinal).ToList(),
            evaluatedStationCount,
            evaluatedTraceCount,
            traceDirectionDriftCount,
            traceCenterDriftCount,
            traceDiagonalizedCount,
            traceParallelRelationLostCount,
            traceConnectorRelationLostCount,
            evaluatedTraceCount > 0 ? maxDeviation : null);
    }

    private static IReadOnlyList<BodyCandidatePartitionItem> SelectCandidateParts(
        IReadOnlyList<BodyCandidatePartitionItem> partitionItems)
    {
        var selected = partitionItems
            .Where(item => IsBodyLikePartition(item.PartitionClass) && item.LongitudinalCoverageEstimate >= 0.30d)
            .OrderByDescending(item => item.LongitudinalCoverageEstimate)
            .ThenByDescending(item => item.PartitionConfidence)
            .ThenBy(item => item.PartId)
            .ToArray();

        if (selected.Length > 0)
        {
            return selected;
        }

        return partitionItems
            .Where(item => IsBodyLikePartition(item.PartitionClass))
            .OrderByDescending(item => item.LongitudinalCoverageEstimate)
            .ThenBy(item => item.PartId)
            .Take(4)
            .ToArray();
    }

    private static Dictionary<int, SectionTraceSegment> FilterAuditSegments(
        IEnumerable<SectionTraceSegment> segments,
        IReadOnlySet<int> candidatePartIds)
    {
        return segments
            .Where(item => candidatePartIds.Count == 0 || candidatePartIds.Contains(item.PartId))
            .Where(item => IsBodyLikePartition(item.PartitionClass))
            .GroupBy(item => item.PartId)
            .Select(group => group.OrderByDescending(item => item.WidthLength).First())
            .ToDictionary(item => item.PartId, item => item);
    }

    private static Dictionary<int, SectionTraceCleanSegment> FilterAuditSegments(
        IEnumerable<SectionTraceCleanSegment> segments,
        IReadOnlySet<int> candidatePartIds)
    {
        return segments
            .Where(item => !item.IsSuppressed)
            .Where(item => candidatePartIds.Count == 0 || candidatePartIds.Contains(item.PartId))
            .Where(item => IsBodyLikePartition(item.PartitionClass))
            .GroupBy(item => item.PartId)
            .Select(group => group.OrderByDescending(item => item.WidthLength).First())
            .ToDictionary(item => item.PartId, item => item);
    }

    private static bool HasParallelRelation(IEnumerable<SectionTraceSegment> segments)
    {
        return HasParallelRelationCore(
            segments.Select(
                item => (
                    item.StartY,
                    item.StartZ,
                    item.EndY,
                    item.EndZ)));
    }

    private static bool HasParallelRelation(IEnumerable<SectionTraceCleanSegment> segments)
    {
        return HasParallelRelationCore(
            segments.Select(
                item => (
                    item.StartY,
                    item.StartZ,
                    item.EndY,
                    item.EndZ)));
    }

    private static bool HasParallelRelationCore(IEnumerable<(double StartY, double StartZ, double EndY, double EndZ)> segments)
    {
        var directions = segments
            .Select(item => TryGetSegmentDirection(item.StartY, item.StartZ, item.EndY, item.EndZ, out var dy, out var dz) ? (IsValid: true, dy, dz) : (IsValid: false, dy: 0d, dz: 0d))
            .Where(item => item.IsValid)
            .ToArray();

        for (var i = 0; i < directions.Length; i++)
        {
            for (var j = i + 1; j < directions.Length; j++)
            {
                if (Math.Abs((directions[i].dy * directions[j].dy) + (directions[i].dz * directions[j].dz)) >= 0.92d)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasConnectorRelation(IEnumerable<SectionTraceSegment> segments)
    {
        return HasConnectorRelationCore(
            segments.Select(
                item => (
                    item.StartY,
                    item.StartZ,
                    item.EndY,
                    item.EndZ)));
    }

    private static bool HasConnectorRelation(IEnumerable<SectionTraceCleanSegment> segments)
    {
        return HasConnectorRelationCore(
            segments.Select(
                item => (
                    item.StartY,
                    item.StartZ,
                    item.EndY,
                    item.EndZ)));
    }

    private static bool HasConnectorRelationCore(IEnumerable<(double StartY, double StartZ, double EndY, double EndZ)> segments)
    {
        var directions = segments
            .Select(item => TryGetSegmentDirection(item.StartY, item.StartZ, item.EndY, item.EndZ, out var dy, out var dz) ? (IsValid: true, dy, dz) : (IsValid: false, dy: 0d, dz: 0d))
            .Where(item => item.IsValid)
            .ToArray();

        for (var i = 0; i < directions.Length; i++)
        {
            for (var j = i + 1; j < directions.Length; j++)
            {
                var dot = Math.Abs((directions[i].dy * directions[j].dy) + (directions[i].dz * directions[j].dz));
                if (dot >= 0.92d)
                {
                    continue;
                }

                if (dot <= 0.35d)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static double ResolveDirectionDeviationDegrees(SectionTraceSegment rawSegment, SectionTraceCleanSegment cleanSegment)
    {
        if (!TryGetSegmentDirection(rawSegment.StartY, rawSegment.StartZ, rawSegment.EndY, rawSegment.EndZ, out var rawY, out var rawZ) ||
            !TryGetSegmentDirection(cleanSegment.StartY, cleanSegment.StartZ, cleanSegment.EndY, cleanSegment.EndZ, out var cleanY, out var cleanZ))
        {
            return 0d;
        }

        var dot = Math.Clamp(Math.Abs((rawY * cleanY) + (rawZ * cleanZ)), -1d, 1d);
        return Math.Acos(dot) * (180d / Math.PI);
    }

    private static double ResolveCenterDrift(SectionTraceSegment rawSegment, SectionTraceCleanSegment cleanSegment)
    {
        var dy = cleanSegment.CenterY - rawSegment.CenterY;
        var dz = cleanSegment.CenterZ - rawSegment.CenterZ;
        return Math.Sqrt((dy * dy) + (dz * dz));
    }

    private static bool IsDiagonalization(SectionTraceSegment rawSegment, SectionTraceCleanSegment cleanSegment, double deviationDegrees)
    {
        var rawIsAxisLike = IsAxisLike(rawSegment.StartY, rawSegment.StartZ, rawSegment.EndY, rawSegment.EndZ);
        var cleanIsAxisLike = IsAxisLike(cleanSegment.StartY, cleanSegment.StartZ, cleanSegment.EndY, cleanSegment.EndZ);
        return rawIsAxisLike && !cleanIsAxisLike && deviationDegrees >= 20d;
    }

    private static bool IsAxisLike(double startY, double startZ, double endY, double endZ)
    {
        if (!TryGetSegmentDirection(startY, startZ, endY, endZ, out var dy, out var dz))
        {
            return false;
        }

        return Math.Abs(dy) >= 0.94d || Math.Abs(dz) >= 0.94d;
    }

    private static bool TryGetSegmentDirection(
        double startY,
        double startZ,
        double endY,
        double endZ,
        out double directionY,
        out double directionZ)
    {
        var dy = endY - startY;
        var dz = endZ - startZ;
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

    private static List<int> ResolveSuspectMissingMainPartIds(
        XingcaiMemberSnapshot? snapshot,
        IReadOnlySet<int> candidatePartIds)
    {
        if (snapshot is null)
        {
            return new List<int>();
        }

        var roleLookup = (snapshot.Classification?.PartRoles ?? new List<XingcaiPartRoleAssignment>())
            .Where(item => !string.IsNullOrWhiteSpace(item.PartId))
            .GroupBy(item => ParseInt(item.PartId))
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.Score).First());
        var memberMainAxisLength = snapshot.Member.MainAxis?.Length ?? 0d;

        return snapshot.Parts
            .Where(item => !candidatePartIds.Contains(ParseInt(item.PartId)))
            .Where(
                item =>
                {
                    var partId = ParseInt(item.PartId);
                    var axisProjectionLength = item.AxisProjection?.Length ?? 0d;
                    var hasPrimaryRole =
                        roleLookup.TryGetValue(partId, out var role) &&
                        IsPrimaryRole(role.Role);
                    var plateLikeSpan = axisProjectionLength >= Math.Max(800d, memberMainAxisLength * 0.45d);
                    var bodyLikeSpecialShape =
                        IsSpecialShape(item) &&
                        axisProjectionLength >= Math.Max(600d, memberMainAxisLength * 0.35d);
                    return hasPrimaryRole || plateLikeSpan || bodyLikeSpecialShape;
                })
            .Select(item => ParseInt(item.PartId))
            .Where(item => item != 0)
            .Distinct()
            .OrderBy(item => item)
            .ToList();
    }

    private static List<int> ResolveSuspectAccessoryDominantPartIds(
        XingcaiMemberSnapshot? snapshot,
        IReadOnlyList<BodyCandidatePartitionItem> partitionItems,
        IReadOnlySet<int> candidatePartIds)
    {
        if (snapshot is null)
        {
            return new List<int>();
        }

        var candidateSet = partitionItems
            .Where(item => candidatePartIds.Contains(item.PartId))
            .ToArray();
        var longestCandidate = candidateSet.Length == 0
            ? 0d
            : candidateSet.Max(item => Math.Max(0d, item.AxisIntervalMax - item.AxisIntervalMin));

        return snapshot.Parts
            .Where(item => !candidatePartIds.Contains(ParseInt(item.PartId)))
            .Where(item => (item.AxisProjection?.Length ?? 0d) > Math.Max(1200d, longestCandidate * 1.15d))
            .Where(item => !IsPrimaryRole(snapshot.Classification?.PartRoles.FirstOrDefault(role => ParseInt(role.PartId) == ParseInt(item.PartId))?.Role))
            .Select(item => ParseInt(item.PartId))
            .Where(item => item != 0)
            .Distinct()
            .OrderBy(item => item)
            .ToList();
    }

    private static bool IsSpecialShapePart(XingcaiMemberSnapshot? snapshot, int partId)
    {
        var part = snapshot?.Parts.FirstOrDefault(item => ParseInt(item.PartId) == partId);
        return part is not null && IsSpecialShape(part);
    }

    private static bool IsSpecialShape(XingcaiPartSnapshot part)
    {
        return string.Equals(part.PartType, "BentPlate", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(part.PartType, "PolyBeam", StringComparison.OrdinalIgnoreCase) ||
               part.GeometryHints?.IsBentLike == true ||
               part.GeometryHints?.IsFoldedLike == true;
    }

    private static bool IsPrimaryRole(string? role)
    {
        return role?.Trim().ToUpperInvariant() is "WEB_CANDIDATE" or "FLANGE_CANDIDATE" or "WALL_CANDIDATE";
    }

    private static bool IsBodyLikePartition(BodyCandidatePartitionClass partitionClass)
    {
        return partitionClass is BodyCandidatePartitionClass.BodyCandidate or BodyCandidatePartitionClass.SpecialShape;
    }

    private static double? ResolveLongestMainPlateAxisProjectionLength(XingcaiMemberSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return null;
        }

        var roleLookup = (snapshot.Classification?.PartRoles ?? new List<XingcaiPartRoleAssignment>())
            .Where(item => !string.IsNullOrWhiteSpace(item.PartId))
            .GroupBy(item => ParseInt(item.PartId))
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.Score).First());

        var candidateLengths = snapshot.Parts
            .Where(
                item =>
                {
                    var partId = ParseInt(item.PartId);
                    return roleLookup.TryGetValue(partId, out var role) && IsPrimaryRole(role.Role);
                })
            .Select(item => item.AxisProjection?.Length ?? 0d)
            .Where(item => item > 1e-6)
            .ToArray();

        if (candidateLengths.Length > 0)
        {
            return candidateLengths.Max();
        }

        var fallback = snapshot.Parts
            .Select(item => item.AxisProjection?.Length ?? 0d)
            .Where(item => item > 1e-6)
            .ToArray();
        return fallback.Length == 0 ? null : fallback.Max();
    }

    private static double? ResolveCandidateSetSpanLength(IReadOnlyList<BodyCandidatePartitionItem> candidates)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var min = candidates.Min(item => item.AxisIntervalMin);
        var max = candidates.Max(item => item.AxisIntervalMax);
        return max > min ? max - min : null;
    }

    private static double? SumAxisSegmentLength(IEnumerable<XingcaiLongitudinalAxisSegment>? segments)
    {
        if (segments is null)
        {
            return null;
        }

        var lengths = segments
            .Select(item => Math.Max(0d, item.CumulativeEnd - item.CumulativeStart))
            .Where(item => item > 1e-6)
            .ToArray();
        return lengths.Length == 0 ? null : lengths.Sum();
    }

    private static double? Ratio(double? numerator, double? denominator)
    {
        return numerator.HasValue && denominator is > 1e-6 ? numerator.Value / denominator.Value : null;
    }

    private static string ResolveOverallStatus(IEnumerable<string> statusCodes)
    {
        var enabledStatuses = statusCodes.ToArray();
        if (enabledStatuses.All(item => item == "NOT_EVALUATED"))
        {
            return "NOT_EVALUATED";
        }

        if (enabledStatuses.Any(item => item == "FAIL"))
        {
            return "FAIL";
        }

        if (enabledStatuses.Any(item => item == "WARNING"))
        {
            return "WARNING";
        }

        return enabledStatuses.All(item => item == "PASS") ? "PASS" : "NOT_EVALUATED";
    }

    private static string ResolvePrimaryReasonCode(IEnumerable<(string CheckCode, string StatusCode, IReadOnlyList<string> ReasonCodes)> checkResults)
    {
        foreach (var checkCode in EnabledChecks)
        {
            var result = checkResults.First(item => item.CheckCode == checkCode);
            if (result.StatusCode == "FAIL" && result.ReasonCodes.Count > 0)
            {
                return result.ReasonCodes[0];
            }
        }

        foreach (var checkCode in EnabledChecks)
        {
            var result = checkResults.First(item => item.CheckCode == checkCode);
            if (result.StatusCode == "WARNING" && result.ReasonCodes.Count > 0)
            {
                return result.ReasonCodes[0];
            }
        }

        return checkResults.All(item => item.StatusCode == "NOT_EVALUATED")
            ? "CHECK_NOT_EVALUATED"
            : "NONE";
    }

    private static string ResolveSuggestedLayerCode(IEnumerable<(string CheckCode, string StatusCode, IReadOnlyList<string> ReasonCodes)> checkResults)
    {
        foreach (var checkCode in EnabledChecks)
        {
            var result = checkResults.First(item => item.CheckCode == checkCode);
            if (result.StatusCode == "FAIL")
            {
                return ToLayerCode(checkCode);
            }
        }

        foreach (var checkCode in EnabledChecks)
        {
            var result = checkResults.First(item => item.CheckCode == checkCode);
            if (result.StatusCode == "WARNING")
            {
                return ToLayerCode(checkCode);
            }
        }

        return checkResults.All(item => item.StatusCode == "NOT_EVALUATED")
            ? "INSUFFICIENT_EVIDENCE"
            : "NONE";
    }

    private static string ToLayerCode(string checkCode)
    {
        return checkCode switch
        {
            "AxisConsistency" => "INPUT_LAYER",
            "CandidateSetConsistency" => "BODY_CANDIDATE_LAYER",
            "SampleTraceConsistency" => "SECTION_TOPOLOGY_OBSERVATION_LAYER",
            "SectionFrameConsistency" => "SECTION_FRAME_LAYER",
            "TopologyInputConsistency" => "TOPOLOGY_INPUT_LAYER",
            _ => "INSUFFICIENT_EVIDENCE"
        };
    }

    private static string BuildEvidenceSummaryZh(
        AxisConsistencyEvidence axisEvidence,
        CandidateSetConsistencyEvidence candidateEvidence,
        SampleTraceConsistencyEvidence traceEvidence)
    {
        return string.Join(
            "；",
            new[]
            {
                $"轴线：{ToReasonLabelZh(axisEvidence.ReasonCodes.FirstOrDefault() ?? "NONE")}",
                $"候选：{ToReasonLabelZh(candidateEvidence.ReasonCodes.FirstOrDefault() ?? "NONE")}",
                $"trace：{ToReasonLabelZh(traceEvidence.ReasonCodes.FirstOrDefault() ?? "NONE")}"
            });
    }

    private static string ToStatusLabelZh(string statusCode)
    {
        return statusCode switch
        {
            "PASS" => "通过",
            "WARNING" => "可疑",
            "FAIL" => "失败",
            "NOT_EVALUATED" => "未评估",
            _ => "未评估"
        };
    }

    private static string ToLayerLabelZh(string layerCode)
    {
        return layerCode switch
        {
            "NONE" => "暂无明显异常层级",
            "INPUT_LAYER" => "输入层",
            "BODY_CANDIDATE_LAYER" => "主体候选层",
            "SECTION_TOPOLOGY_OBSERVATION_LAYER" => "截面拓扑观察层",
            "SECTION_FRAME_LAYER" => "截面坐标系层",
            "TOPOLOGY_INPUT_LAYER" => "拓扑输入层",
            "INSUFFICIENT_EVIDENCE" => "证据不足",
            _ => "证据不足"
        };
    }

    private static string ToReasonLabelZh(string reasonCode)
    {
        return reasonCode switch
        {
            "NONE" => "无明显异常",
            "CHECK_NOT_EVALUATED" => "检查项未评估",
            "INPUT_DATA_MISSING" => "输入数据缺失",
            "EVIDENCE_INCOMPLETE" => "证据不完整",
            "AXIS_OK" => "轴线一致性通过",
            "AXIS_TOO_SHORT_FOR_MEMBER" => "导出轴线明显短于构件主跨度",
            "AXIS_TOO_SHORT_FOR_MAIN_PLATE" => "导出轴线明显短于最长长向主板",
            "AXIS_SPAN_NOT_COVERING_CANDIDATE_SET" => "导出轴线未覆盖主体候选集合跨度",
            "AXIS_SOURCE_HIJACKED_BY_LOCAL_PART" => "轴线疑似被局部短件劫持",
            "AXIS_SEGMENTS_MISSING_BUT_MAIN_AXIS_AVAILABLE" => "导出轴线缺失但原始主轴可用",
            "CANDIDATE_SET_OK" => "主体候选集合一致性通过",
            "CANDIDATE_SET_EMPTY" => "主体候选集合为空",
            "MAIN_PLATE_MISSING_FROM_CANDIDATE_SET" => "疑似长向主板未进入候选集合",
            "CANDIDATE_SET_SPAN_TOO_SHORT" => "主体候选集合跨度过短",
            "SPECIAL_SHAPE_BODY_LIKE_PART_EXCLUDED" => "类似主体的 SpecialShape/BentPlate 被排除",
            "ACCESSORY_DOMINATES_CANDIDATE_SET" => "附件疑似主导候选集合",
            "SAMPLE_TRACE_OK" => "sample 与 trace 一致性通过",
            "TRACE_DIRECTION_DRIFT_FROM_SAMPLE" => "trace 方向相对 sample 明显漂移",
            "TRACE_CENTER_DRIFT_FROM_SAMPLE" => "trace 中心相对 sample 明显漂移",
            "TRACE_DIAGONALIZED_BY_CORNER_GEOMETRY" => "trace 疑似被倒角或角部几何拉成对角线",
            "TRACE_PARALLEL_RELATION_LOST" => "sample 中平行关系在 trace 中丢失",
            "TRACE_CONNECTOR_RELATION_LOST" => "sample 中连接板关系在 trace 中丢失",
            _ => reasonCode
        };
    }

    private static XingcaiMemberSnapshot? LoadSnapshot(
        string sourceFile,
        IDictionary<string, XingcaiMemberSnapshot?> cache)
    {
        if (cache.TryGetValue(sourceFile, out var cached))
        {
            return cached;
        }

        XingcaiMemberSnapshot? snapshot = null;
        if (File.Exists(sourceFile))
        {
            using var stream = File.OpenRead(sourceFile);
            snapshot = JsonSerializer.Deserialize<XingcaiMemberSnapshot>(stream, SnapshotJsonOptions);
        }

        cache[sourceFile] = snapshot;
        return snapshot;
    }

    private static int ParseInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : 0;
    }

    private sealed record AxisConsistencyEvidence(
        string StatusCode,
        List<string> ReasonCodes,
        double? AxisSegmentsLength,
        double? MemberMainAxisLength,
        double? LongestMainPlateAxisProjectionLength,
        double? CandidateSetSpanLength,
        double? AxisLengthToMemberMainAxisRatio,
        double? AxisLengthToLongestMainPlateRatio,
        double? AxisLengthToCandidateSetSpanRatio);

    private sealed record CandidateSetConsistencyEvidence(
        string StatusCode,
        List<string> ReasonCodes,
        List<int> SuspectMissingMainPartIds,
        List<int> SuspectAccessoryDominantPartIds,
        double? CandidateSetCoverageRatio);

    private sealed record SampleTraceConsistencyEvidence(
        string StatusCode,
        List<string> ReasonCodes,
        int EvaluatedStationCount,
        int EvaluatedTraceCount,
        int TraceDirectionDriftCount,
        int TraceCenterDriftCount,
        int TraceDiagonalizedCount,
        int TraceParallelRelationLostCount,
        int TraceConnectorRelationLostCount,
        double? MaxTraceDirectionDeviationDegrees);
}
