using TeklaBodyBracketRecognition.Core.Config;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.Core.Algorithms;

public sealed class CoreBodyProofEngine
{
    private readonly RecognitionOptions _options;

    public CoreBodyProofEngine(RecognitionOptions options)
    {
        _options = options;
    }

    public CoreBodyProofResult Prove(
        BodyCandidatePartitionResult partition,
        SectionTraceCleaningResult cleaning,
        SectionTopologyAnalysisResult topology)
    {
        var priorityStations = cleaning.Stations
            .Where(station => station.StationKind != SectionStationKind.LowPriority)
            .ToArray();
        if (priorityStations.Length == 0)
        {
            priorityStations = cleaning.Stations.ToArray();
        }

        var topologyLookup = topology.Stations
            .ToDictionary(item => item.StationId, StringComparer.Ordinal);
        var partitionLookup = partition.Items.ToDictionary(item => item.PartId);
        var baselineStationSnapshots = priorityStations
            .Select(
                station =>
                {
                    topologyLookup.TryGetValue(station.StationId, out var topologyStation);
                    return RecomputeStationSnapshot(station, excludedPartId: null, topologyStation);
                })
            .ToDictionary(item => item.StationId, StringComparer.Ordinal);
        var initialCandidates = priorityStations
            .SelectMany(station => station.Segments.Where(segment => !segment.IsSuppressed))
            .Where(segment => partitionLookup.ContainsKey(segment.PartId))
            .GroupBy(segment => segment.PartId)
            .Select(
                group => BuildPartResult(
                    group.Key,
                    group.ToArray(),
                    priorityStations,
                    baselineStationSnapshots,
                    partitionLookup[group.Key]))
            .ToArray();

        var mainPartCore = initialCandidates.FirstOrDefault(
            item => item.IsInputMainPart && item.ProofClass == CoreBodyProofClass.CoreBodyPart);
        var candidates = initialCandidates
            .Select(item => MaybePromoteMainPartPeer(item, mainPartCore))
            .OrderByDescending(item => item.ProofClass == CoreBodyProofClass.CoreBodyPart)
            .ThenByDescending(item => item.PriorityStationPresenceRatio)
            .ThenByDescending(item => item.EnvelopeSupportStationRatio)
            .ThenBy(item => item.PartId)
            .ToArray();
        candidates = AttachTopologyRewriteShapeRoles(candidates);
        candidates = AttachFallbackTopologyRewriteBootstraps(candidates);
        candidates = AttachTopologyRewriteFamilyRisks(candidates);
        candidates = AttachTopologyRewriteProofTypes(candidates);
        candidates = AttachTopologyRewriteFamilyProofTargets(candidates);
        candidates = AttachTopologyRewriteDefinitionClauses(candidates);
        candidates = AttachTopologyRewriteDefinitionClauseEffects(candidates);

        return new CoreBodyProofResult
        {
            AssemblyId = partition.AssemblyId,
            InputMainPartId = partition.InputMainPartId,
            PriorityStationCount = priorityStations.Length,
            BootstrapOnly = true,
            CoreBodyPartIds = candidates.Where(item => item.ProofClass == CoreBodyProofClass.CoreBodyPart).Select(item => item.PartId).ToArray(),
            BodyAccessoryPartIds = candidates.Where(item => item.ProofClass == CoreBodyProofClass.BodyAccessoryPart).Select(item => item.PartId).ToArray(),
            ReviewPartIds = candidates.Where(item => item.ProofClass == CoreBodyProofClass.ReviewRequired).Select(item => item.PartId).ToArray(),
            Parts = candidates
        };
    }

    private static CoreBodyProofPartResult MaybePromoteMainPartPeer(
        CoreBodyProofPartResult candidate,
        CoreBodyProofPartResult? mainPartCore)
    {
        if (mainPartCore is null ||
            candidate.IsInputMainPart ||
            candidate.ProofClass != CoreBodyProofClass.ReviewRequired ||
            candidate.PartitionClass != BodyCandidatePartitionClass.BodyCandidate)
        {
            return candidate;
        }

        if (candidate.PriorityStationPresenceRatio < 0.95 ||
            candidate.EnvelopeSupportStationRatio < 0.95)
        {
            return candidate;
        }

        if (!string.Equals(
                NormalizeProfileFamily(candidate.ProfileString),
                NormalizeProfileFamily(mainPartCore.ProfileString),
                StringComparison.OrdinalIgnoreCase))
        {
            return candidate;
        }

        if (Math.Abs(candidate.RemovalImpactScore - mainPartCore.RemovalImpactScore) > 0.015 ||
            Math.Abs(candidate.BodyWidthRetentionRatio - mainPartCore.BodyWidthRetentionRatio) > 0.05)
        {
            return candidate;
        }

        var reasons = candidate.Reasons
            .Concat(
                new[]
                {
                    "MAIN_PART_PEER_GROUP_PROMOTION",
                    $"PEER_OF_MAIN_PART_{mainPartCore.PartId}"
                })
            .Distinct()
            .ToArray();

        return candidate with
        {
            ProofClass = CoreBodyProofClass.CoreBodyPart,
            Reasons = reasons
        };
    }

    private CoreBodyProofPartResult BuildPartResult(
        int partId,
        IReadOnlyList<SectionTraceCleanSegment> segments,
        IReadOnlyList<SectionTraceCleanStationResult> priorityStations,
        IReadOnlyDictionary<string, StationRecomputeSnapshot> baselineStationSnapshots,
        BodyCandidatePartitionItem partitionItem)
    {
        var presenceStationIds = segments
            .Select(item => item.StationId)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
        var presenceCount = presenceStationIds.Count;
        var presenceRatio = priorityStations.Count == 0 ? 0.0 : presenceCount / (double)priorityStations.Count;

        var envelopeSupportStationIds = priorityStations
            .Where(station => StationTouchesEnvelope(station, partId))
            .Select(station => station.StationId)
            .ToHashSet(StringComparer.Ordinal);
        var envelopeSupportCount = envelopeSupportStationIds.Count;
        var envelopeSupportRatio = priorityStations.Count == 0 ? 0.0 : envelopeSupportCount / (double)priorityStations.Count;
        var baselineClosedLoopStations = baselineStationSnapshots.Values.Count(item => item.ClosedLoopCandidate);
        var baselineEnvelopeBodyCoverageStations = baselineStationSnapshots.Values.Count(item => item.BodyCandidateEnvelopeTraceCount > 0);
        var baselineAverageBodyWidth = baselineStationSnapshots.Count == 0
            ? 0.0
            : baselineStationSnapshots.Values.Average(item => item.BodyWidth);

        var recomputedAfterRemoval = priorityStations
            .Select(station => RecomputeStationSnapshot(station, partId, baselineTopologyStation: null))
            .ToArray();
        var recomputedLookup = recomputedAfterRemoval.ToDictionary(item => item.StationId, StringComparer.Ordinal);

        var priorityStationsWithBodyCandidateAfterRemoval = recomputedAfterRemoval.Count(item => item.BodyCandidateTraceCount > 0);
        var priorityBodyCoverageAfterRemovalRatio = priorityStations.Count == 0
            ? 0.0
            : priorityStationsWithBodyCandidateAfterRemoval / (double)priorityStations.Count;
        var priorityStationsWithEnvelopeBodyCandidateAfterRemoval = recomputedAfterRemoval.Count(item => item.BodyCandidateEnvelopeTraceCount > 0);
        var envelopeBodyCoverageAfterRemovalRatio = priorityStations.Count == 0
            ? 0.0
            : priorityStationsWithEnvelopeBodyCandidateAfterRemoval / (double)priorityStations.Count;
        var closedLoopStationsAfterRemoval = recomputedAfterRemoval.Count(item => item.ClosedLoopCandidate);
        var averageBodyWidthAfterRemoval = recomputedAfterRemoval.Length == 0
            ? 0.0
            : recomputedAfterRemoval.Average(item => item.BodyWidth);
        var bodyWidthRetentionRatio = baselineAverageBodyWidth <= 1e-6
            ? 1.0
            : averageBodyWidthAfterRemoval / baselineAverageBodyWidth;
        var priorityCoverageDrop = 1.0 - priorityBodyCoverageAfterRemovalRatio;
        var envelopeCoverageDrop = baselineEnvelopeBodyCoverageStations <= 0
            ? 0.0
            : (baselineEnvelopeBodyCoverageStations - priorityStationsWithEnvelopeBodyCandidateAfterRemoval) / (double)baselineEnvelopeBodyCoverageStations;
        var closedLoopCoverageDrop = baselineClosedLoopStations <= 0
            ? 0.0
            : (baselineClosedLoopStations - closedLoopStationsAfterRemoval) / (double)baselineClosedLoopStations;
        var bodyWidthDrop = 1.0 - bodyWidthRetentionRatio;
        var removalImpactScore = Math.Round((priorityCoverageDrop * 0.30) + (envelopeCoverageDrop * 0.20) + (closedLoopCoverageDrop * 0.20) + (bodyWidthDrop * 0.30), 4);

        var missingStations = priorityStations
            .Select(station => station.StationId)
            .Where(stationId => !presenceStationIds.Contains(stationId))
            .ToArray();
        var lostBodyCoverageStationIds = baselineStationSnapshots.Values
            .Where(item => item.BodyCandidateTraceCount > 0)
            .Where(item => recomputedLookup.TryGetValue(item.StationId, out var recomputed) && recomputed.BodyCandidateTraceCount == 0)
            .Select(item => item.StationId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var lostEnvelopeSupportStationIds = baselineStationSnapshots.Values
            .Where(item => item.BodyCandidateEnvelopeTraceCount > 0)
            .Where(item => recomputedLookup.TryGetValue(item.StationId, out var recomputed) && recomputed.BodyCandidateEnvelopeTraceCount == 0)
            .Select(item => item.StationId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var lostClosedLoopStationIds = baselineStationSnapshots.Values
            .Where(item => item.ClosedLoopCandidate)
            .Where(item => recomputedLookup.TryGetValue(item.StationId, out var recomputed) && !recomputed.ClosedLoopCandidate)
            .Select(item => item.StationId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var topologyRewriteSpanYStationIds = baselineStationSnapshots.Values
            .Where(
                item =>
                {
                    if (!recomputedLookup.TryGetValue(item.StationId, out var recomputed))
                    {
                        return false;
                    }

                    var lostBodyCoverage = item.BodyCandidateTraceCount > 0 && recomputed.BodyCandidateTraceCount == 0;
                    var lostEnvelopeSupport = item.BodyCandidateEnvelopeTraceCount > 0 && recomputed.BodyCandidateEnvelopeTraceCount == 0;
                    var lostClosedLoop = item.ClosedLoopCandidate && !recomputed.ClosedLoopCandidate;
                    if (lostBodyCoverage || lostEnvelopeSupport || lostClosedLoop)
                    {
                        return false;
                    }

                    var hasEngineeringSignificantBaseline =
                        item.ClosedLoopCandidate ||
                        item.BodyCandidateTraceCount >= 3 ||
                        item.BodyCandidateEnvelopeTraceCount >= 3;
                    if (!hasEngineeringSignificantBaseline)
                    {
                        return false;
                    }

                    return Math.Abs(item.EnvelopeSpanY - recomputed.EnvelopeSpanY) > _options.ContactDistanceToleranceMm;
                })
            .Select(item => item.StationId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var topologyRewriteSpanZStationIds = baselineStationSnapshots.Values
            .Where(
                item =>
                {
                    if (!recomputedLookup.TryGetValue(item.StationId, out var recomputed))
                    {
                        return false;
                    }

                    var lostBodyCoverage = item.BodyCandidateTraceCount > 0 && recomputed.BodyCandidateTraceCount == 0;
                    var lostEnvelopeSupport = item.BodyCandidateEnvelopeTraceCount > 0 && recomputed.BodyCandidateEnvelopeTraceCount == 0;
                    var lostClosedLoop = item.ClosedLoopCandidate && !recomputed.ClosedLoopCandidate;
                    if (lostBodyCoverage || lostEnvelopeSupport || lostClosedLoop)
                    {
                        return false;
                    }

                    var hasEngineeringSignificantBaseline =
                        item.ClosedLoopCandidate ||
                        item.BodyCandidateTraceCount >= 3 ||
                        item.BodyCandidateEnvelopeTraceCount >= 3;
                    if (!hasEngineeringSignificantBaseline)
                    {
                        return false;
                    }

                    return Math.Abs(item.EnvelopeSpanZ - recomputed.EnvelopeSpanZ) > _options.ContactDistanceToleranceMm;
                })
            .Select(item => item.StationId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var topologyRewriteEnvelopeControllerSwitchStationIds = baselineStationSnapshots.Values
            .Where(
                item =>
                {
                    if (!recomputedLookup.TryGetValue(item.StationId, out var recomputed))
                    {
                        return false;
                    }

                    var lostBodyCoverage = item.BodyCandidateTraceCount > 0 && recomputed.BodyCandidateTraceCount == 0;
                    var lostEnvelopeSupport = item.BodyCandidateEnvelopeTraceCount > 0 && recomputed.BodyCandidateEnvelopeTraceCount == 0;
                    var lostClosedLoop = item.ClosedLoopCandidate && !recomputed.ClosedLoopCandidate;
                    if (lostBodyCoverage || lostEnvelopeSupport || lostClosedLoop)
                    {
                        return false;
                    }

                    var hasEngineeringSignificantBaseline =
                        item.ClosedLoopCandidate ||
                        item.BodyCandidateTraceCount >= 3 ||
                        item.BodyCandidateEnvelopeTraceCount >= 3;
                    if (!hasEngineeringSignificantBaseline)
                    {
                        return false;
                    }

                    return !item.DominantEnvelopeBodyPartIds.SequenceEqual(recomputed.DominantEnvelopeBodyPartIds);
                })
            .Select(item => item.StationId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var topologyRewriteStationIds = topologyRewriteSpanYStationIds
            .Concat(topologyRewriteSpanZStationIds)
            .Concat(topologyRewriteEnvelopeControllerSwitchStationIds)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var topologyRewriteDirectControllerStationIds = topologyRewriteStationIds
            .Where(
                stationId =>
                    baselineStationSnapshots.TryGetValue(stationId, out var baseline) &&
                    baseline.DominantEnvelopeBodyPartIds.Contains(partId))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var topologyRewriteIndirectControllerStationIds = topologyRewriteStationIds
            .Except(topologyRewriteDirectControllerStationIds, StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var topologyRewriteControllerRoleCode = ResolveTopologyRewriteControllerRoleCode(
            topologyRewriteDirectControllerStationIds.Length > 0,
            topologyRewriteIndirectControllerStationIds.Length > 0);
        var topologyRewriteControllerRoleLabelZh = ResolveTopologyRewriteControllerRoleLabelZh(topologyRewriteControllerRoleCode);
        var topologyRewritePatternCode = ResolveTopologyRewritePatternCode(
            topologyRewriteSpanYStationIds.Length > 0,
            topologyRewriteSpanZStationIds.Length > 0,
            topologyRewriteEnvelopeControllerSwitchStationIds.Length > 0);
        var topologyRewritePatternLabelZh = ResolveTopologyRewritePatternLabelZh(topologyRewritePatternCode);
        var dominantDimensionSwitchStationIds = baselineStationSnapshots.Values
            .Where(
                item => recomputedLookup.TryGetValue(item.StationId, out var recomputed) &&
                        !item.DominantBodyPartIds.Contains(partId) &&
                        !item.DominantBodyPartIds.SequenceEqual(recomputed.DominantBodyPartIds))
            .Select(item => item.StationId)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();

        var reasons = new List<string>
        {
            $"PARTITION_{partitionItem.PartitionClass}",
            $"PRIORITY_STATION_PRESENCE_{presenceCount}/{priorityStations.Count}",
            $"ENVELOPE_SUPPORT_{envelopeSupportCount}/{priorityStations.Count}",
            $"BODY_COVERAGE_AFTER_REMOVAL_{priorityStationsWithBodyCandidateAfterRemoval}/{priorityStations.Count}",
            $"ENVELOPE_BODY_COVERAGE_AFTER_REMOVAL_{priorityStationsWithEnvelopeBodyCandidateAfterRemoval}/{Math.Max(baselineEnvelopeBodyCoverageStations, 0)}",
            $"CLOSED_LOOP_AFTER_REMOVAL_{closedLoopStationsAfterRemoval}/{baselineClosedLoopStations}",
            $"BODY_WIDTH_RETENTION_{bodyWidthRetentionRatio:0.000}",
            $"REMOVAL_IMPACT_SCORE_{removalImpactScore:0.000}"
        };

        CoreBodyProofClass proofClass;
        var mainPartStructuralImpact =
            partitionItem.IsInputMainPart &&
            presenceRatio >= 0.60 &&
            (bodyWidthRetentionRatio <= 0.80 || removalImpactScore >= 0.07);

        if (partitionItem.PartitionClass == BodyCandidatePartitionClass.BodyCandidate &&
            presenceRatio >= 0.60 &&
            ((envelopeSupportRatio >= 0.40 &&
              (bodyWidthRetentionRatio <= 0.72 ||
               priorityCoverageDrop >= 0.25 ||
               closedLoopCoverageDrop >= 0.25 ||
               removalImpactScore >= 0.18)) ||
             mainPartStructuralImpact))
        {
            proofClass = CoreBodyProofClass.CoreBodyPart;
            reasons.Add("PERSISTENT_BODY_TRACE_SUPPORT");
            if (envelopeSupportRatio >= 0.40)
            {
                reasons.Add("ENVELOPE_SUPPORTED_BODY_PART");
            }

            if (mainPartStructuralImpact)
            {
                reasons.Add("INPUT_MAIN_PART_WITH_STRUCTURAL_IMPACT");
            }

            if (bodyWidthRetentionRatio <= 0.72)
            {
                reasons.Add("BODY_WIDTH_COLLAPSES_AFTER_REMOVAL");
            }

            if (priorityCoverageDrop >= 0.25)
            {
                reasons.Add("PRIORITY_BODY_COVERAGE_DROPS_AFTER_REMOVAL");
            }

            if (closedLoopCoverageDrop >= 0.25)
            {
                reasons.Add("CLOSED_LOOP_EVIDENCE_DROPS_AFTER_REMOVAL");
            }

            if (envelopeCoverageDrop >= 0.25)
            {
                reasons.Add("ENVELOPE_COVERAGE_DROPS_AFTER_REMOVAL");
            }
        }
        else if (partitionItem.PartitionClass is BodyCandidatePartitionClass.BodyAccessoryCandidate or
                 BodyCandidatePartitionClass.EndConnectionCandidate or
                 BodyCandidatePartitionClass.LocalStiffenerCandidate or
                 BodyCandidatePartitionClass.TinyPart)
        {
            proofClass = CoreBodyProofClass.BodyAccessoryPart;
            reasons.Add("NON_PRIMARY_PARTITION_CLASS");
        }
        else
        {
            proofClass = CoreBodyProofClass.ReviewRequired;
            reasons.Add("INSUFFICIENT_PERSISTENT_PROOF");
        }

        if (missingStations.Length > 0)
        {
            reasons.Add("MISSING_ON_PRIORITY_STATIONS");
        }

        if (topologyRewriteStationIds.Length > 0)
        {
            reasons.Add("ENVELOPE_TOPOLOGY_REWRITES_AFTER_REMOVAL");
        }

        if (topologyRewriteDirectControllerStationIds.Length > 0)
        {
            reasons.Add("PART_IS_DIRECT_ENVELOPE_CONTROLLER_ON_REWRITE_STATIONS");
        }

        if (topologyRewriteIndirectControllerStationIds.Length > 0)
        {
            reasons.Add("REWRITE_STATIONS_INCLUDE_PEER_CONTROLLER_SWITCH");
        }

        if (dominantDimensionSwitchStationIds.Length > 0)
        {
            reasons.Add("DOMINANT_DIMENSION_SWITCHES_AFTER_REMOVAL");
        }

        var representative = segments
            .OrderByDescending(item => item.WidthLength)
            .ThenBy(item => item.TraceId, StringComparer.Ordinal)
            .First();

        return new CoreBodyProofPartResult
        {
            PartId = partId,
            PartName = representative.PartName,
            ProfileString = representative.ProfileString,
            IsInputMainPart = partitionItem.IsInputMainPart,
            PartitionClass = partitionItem.PartitionClass,
            PriorityStationPresenceCount = presenceCount,
            PriorityStationPresenceRatio = Math.Round(presenceRatio, 4),
            EnvelopeSupportStationCount = envelopeSupportCount,
            EnvelopeSupportStationRatio = Math.Round(envelopeSupportRatio, 4),
            PriorityStationsWithBodyCandidateAfterRemoval = priorityStationsWithBodyCandidateAfterRemoval,
            PriorityBodyCoverageAfterRemovalRatio = Math.Round(priorityBodyCoverageAfterRemovalRatio, 4),
            PriorityStationsWithEnvelopeBodyCandidateAfterRemoval = priorityStationsWithEnvelopeBodyCandidateAfterRemoval,
            EnvelopeBodyCoverageAfterRemovalRatio = Math.Round(envelopeBodyCoverageAfterRemovalRatio, 4),
            ClosedLoopStationCountBeforeRemoval = baselineClosedLoopStations,
            ClosedLoopStationCountAfterRemoval = closedLoopStationsAfterRemoval,
            AverageBodyWidthBeforeRemoval = Math.Round(baselineAverageBodyWidth, 4),
            AverageBodyWidthAfterRemoval = Math.Round(averageBodyWidthAfterRemoval, 4),
            BodyWidthRetentionRatio = Math.Round(bodyWidthRetentionRatio, 4),
            RemovalImpactScore = removalImpactScore,
            MissingPriorityStationIds = missingStations,
            LostBodyCoverageStationIds = lostBodyCoverageStationIds,
            LostEnvelopeSupportStationIds = lostEnvelopeSupportStationIds,
            LostClosedLoopStationIds = lostClosedLoopStationIds,
            TopologyRewriteStationIds = topologyRewriteStationIds,
            TopologyRewriteSpanYStationIds = topologyRewriteSpanYStationIds,
            TopologyRewriteSpanZStationIds = topologyRewriteSpanZStationIds,
            TopologyRewriteEnvelopeControllerSwitchStationIds = topologyRewriteEnvelopeControllerSwitchStationIds,
            TopologyRewriteDirectControllerStationIds = topologyRewriteDirectControllerStationIds,
            TopologyRewriteIndirectControllerStationIds = topologyRewriteIndirectControllerStationIds,
            TopologyRewriteControllerRoleCode = topologyRewriteControllerRoleCode,
            TopologyRewriteControllerRoleLabelZh = topologyRewriteControllerRoleLabelZh,
            TopologyRewritePatternCode = topologyRewritePatternCode,
            TopologyRewritePatternLabelZh = topologyRewritePatternLabelZh,
            TopologyRewriteCohortPartIds = Array.Empty<int>(),
            TopologyRewriteCohortPartCount = 0,
            TopologyRewriteShapeRoleCode = "NONE",
            TopologyRewriteShapeRoleLabelZh = "无主体板组角色",
            TopologyRewriteFamilyRiskCode = "NONE",
            TopologyRewriteFamilyRiskLabelZh = "无家族证明风险",
            TopologyRewriteProofTypeCode = "NONE",
            TopologyRewriteProofTypeLabelZh = "无主截面证明对象",
            TopologyRewriteFamilyProofTargetCode = "NONE",
            TopologyRewriteFamilyProofTargetLabelZh = "无家族证明目标",
            TopologyRewriteDefinitionClauseCode = "NONE",
            TopologyRewriteDefinitionClauseLabelZh = "无定义条款提示",
            TopologyRewriteDefinitionClauseEffectCode = "NONE",
            TopologyRewriteDefinitionClauseEffectLabelZh = "无定义条款效果",
            DominantDimensionSwitchStationIds = dominantDimensionSwitchStationIds,
            ProofClass = proofClass,
            Reasons = reasons
        };
    }

    private static CoreBodyProofPartResult[] AttachTopologyRewriteShapeRoles(
        IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        var cohortLookup = candidates
            .Where(item =>
                item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate &&
                item.TopologyRewriteStationIds.Count > 0 &&
                !string.Equals(item.TopologyRewritePatternCode, "NONE", StringComparison.Ordinal))
            .GroupBy(
                item => BuildTopologyRewriteCohortKey(
                    item.TopologyRewritePatternCode,
                    item.TopologyRewriteControllerRoleCode),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)group
                    .Select(item => item.PartId)
                    .Distinct()
                    .OrderBy(item => item)
                    .ToArray());

        return candidates
            .Select(
                item =>
                {
                    if (item.PartitionClass != BodyCandidatePartitionClass.BodyCandidate ||
                        item.TopologyRewriteStationIds.Count == 0 ||
                        string.Equals(item.TopologyRewritePatternCode, "NONE", StringComparison.Ordinal))
                    {
                        return item with
                        {
                            TopologyRewriteCohortPartIds = Array.Empty<int>(),
                            TopologyRewriteCohortPartCount = 0,
                            TopologyRewriteShapeRoleCode = "NONE",
                            TopologyRewriteShapeRoleLabelZh = "无主体板组角色"
                        };
                    }

                    if (!cohortLookup.TryGetValue(
                            BuildTopologyRewriteCohortKey(
                                item.TopologyRewritePatternCode,
                                item.TopologyRewriteControllerRoleCode),
                            out var cohortPartIds))
                    {
                        cohortPartIds = new[] { item.PartId };
                    }

                    var shapeRoleCode = ResolveTopologyRewriteShapeRoleCode(cohortPartIds.Count, item.IsInputMainPart);
                    var shapeRoleLabelZh = ResolveTopologyRewriteShapeRoleLabelZh(shapeRoleCode);
                    var reasons = item.Reasons
                        .Concat(new[] { $"TOPOLOGY_REWRITE_SHAPE_ROLE_{shapeRoleCode}" })
                        .Distinct()
                        .ToArray();

                    return item with
                    {
                        TopologyRewriteCohortPartIds = cohortPartIds,
                        TopologyRewriteCohortPartCount = cohortPartIds.Count,
                        TopologyRewriteShapeRoleCode = shapeRoleCode,
                        TopologyRewriteShapeRoleLabelZh = shapeRoleLabelZh,
                        Reasons = reasons
                    };
                })
            .ToArray();
    }

    private static CoreBodyProofPartResult[] AttachTopologyRewriteFamilyRisks(
        IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        return candidates
            .Select(
                item =>
                {
                    var familyRiskCode = string.Equals(item.TopologyRewriteFamilyRiskCode, "NONE", StringComparison.Ordinal)
                        ? ResolveTopologyRewriteFamilyRiskCode(
                            item.TopologyRewritePatternCode,
                            item.TopologyRewriteControllerRoleCode,
                            item.TopologyRewriteShapeRoleCode)
                        : item.TopologyRewriteFamilyRiskCode;
                    familyRiskCode = RefineTopologyRewriteFamilyRiskCode(item, candidates, familyRiskCode);
                    var familyRiskLabelZh = string.Equals(item.TopologyRewriteFamilyRiskLabelZh, "无家族证明风险", StringComparison.Ordinal)
                        ? ResolveTopologyRewriteFamilyRiskLabelZh(familyRiskCode)
                        : item.TopologyRewriteFamilyRiskLabelZh;
                    var reasons = item.Reasons;

                    if (!string.Equals(familyRiskCode, "NONE", StringComparison.Ordinal))
                    {
                        reasons = item.Reasons
                            .Concat(new[] { $"TOPOLOGY_REWRITE_FAMILY_RISK_{familyRiskCode}" })
                            .Distinct()
                            .ToArray();
                    }

                    return item with
                    {
                        TopologyRewriteFamilyRiskCode = familyRiskCode,
                        TopologyRewriteFamilyRiskLabelZh = familyRiskLabelZh,
                        Reasons = reasons
                    };
                })
            .ToArray();
    }

    private static CoreBodyProofPartResult[] AttachFallbackTopologyRewriteBootstraps(
        IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        if (candidates.Count == 0)
        {
            return Array.Empty<CoreBodyProofPartResult>();
        }

        var embeddedPlatePrimaryPartId = ResolveEmbeddedPlatePrimaryPartId(candidates);
        var closedBoxShellPartIds = ResolveClosedBoxShellPartIds(candidates);
        var bentFlangeHPartIds = ResolveBentFlangeHPartIds(candidates);
        var foldedThreePlateHPartIds = ResolveFoldedThreePlateHPartIds(candidates);
        if (embeddedPlatePrimaryPartId is null &&
            closedBoxShellPartIds.Count == 0 &&
            bentFlangeHPartIds.Count == 0 &&
            foldedThreePlateHPartIds.Count == 0)
        {
            return candidates.ToArray();
        }

        return candidates
            .Select(
                item => ApplyFallbackTopologyRewriteBootstrap(
                    item,
                    embeddedPlatePrimaryPartId,
                    closedBoxShellPartIds,
                    bentFlangeHPartIds,
                    foldedThreePlateHPartIds))
            .ToArray();
    }

    private static CoreBodyProofPartResult[] AttachTopologyRewriteProofTypes(
        IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        return candidates
            .Select(
                item =>
                {
                    var proofTypeCode = string.Equals(item.TopologyRewriteProofTypeCode, "NONE", StringComparison.Ordinal)
                        ? ResolveTopologyRewriteProofTypeCode(
                            item.TopologyRewriteFamilyRiskCode,
                            item.TopologyRewritePatternCode,
                            item.TopologyRewriteShapeRoleCode)
                        : item.TopologyRewriteProofTypeCode;
                    var proofTypeLabelZh = string.Equals(item.TopologyRewriteProofTypeLabelZh, "无主截面证明对象", StringComparison.Ordinal)
                        ? ResolveTopologyRewriteProofTypeLabelZh(proofTypeCode)
                        : item.TopologyRewriteProofTypeLabelZh;
                    var reasons = item.Reasons;

                    if (!string.Equals(proofTypeCode, "NONE", StringComparison.Ordinal))
                    {
                        reasons = item.Reasons
                            .Concat(new[] { $"TOPOLOGY_REWRITE_PROOF_TYPE_{proofTypeCode}" })
                            .Distinct()
                            .ToArray();
                    }

                    return item with
                    {
                        TopologyRewriteProofTypeCode = proofTypeCode,
                        TopologyRewriteProofTypeLabelZh = proofTypeLabelZh,
                        Reasons = reasons
                    };
                })
            .ToArray();
    }

    private static CoreBodyProofPartResult[] AttachTopologyRewriteFamilyProofTargets(
        IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        return candidates
            .Select(
                item =>
                {
                    var familyProofTargetCode = string.Equals(item.TopologyRewriteFamilyProofTargetCode, "NONE", StringComparison.Ordinal)
                        ? ResolveTopologyRewriteFamilyProofTargetCode(
                            item.TopologyRewriteProofTypeCode,
                            item.TopologyRewriteFamilyRiskCode,
                            item.TopologyRewritePatternCode)
                        : item.TopologyRewriteFamilyProofTargetCode;
                    var familyProofTargetLabelZh = string.Equals(item.TopologyRewriteFamilyProofTargetLabelZh, "无家族证明目标", StringComparison.Ordinal)
                        ? ResolveTopologyRewriteFamilyProofTargetLabelZh(familyProofTargetCode)
                        : item.TopologyRewriteFamilyProofTargetLabelZh;
                    var reasons = item.Reasons;

                    if (!string.Equals(familyProofTargetCode, "NONE", StringComparison.Ordinal))
                    {
                        reasons = item.Reasons
                            .Concat(new[] { $"TOPOLOGY_REWRITE_FAMILY_PROOF_TARGET_{familyProofTargetCode}" })
                            .Distinct()
                            .ToArray();
                    }

                    return item with
                    {
                        TopologyRewriteFamilyProofTargetCode = familyProofTargetCode,
                        TopologyRewriteFamilyProofTargetLabelZh = familyProofTargetLabelZh,
                        Reasons = reasons
                    };
                })
            .ToArray();
    }

    private static CoreBodyProofPartResult[] AttachTopologyRewriteDefinitionClauses(
        IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        return candidates
            .Select(
                item =>
                {
                    var definitionClauseCode = string.Equals(item.TopologyRewriteDefinitionClauseCode, "NONE", StringComparison.Ordinal)
                        ? ResolveTopologyRewriteDefinitionClauseCode(
                            item.TopologyRewriteFamilyProofTargetCode,
                            item.TopologyRewritePatternCode,
                            item.TopologyRewriteControllerRoleCode)
                        : item.TopologyRewriteDefinitionClauseCode;
                    var definitionClauseLabelZh = string.Equals(item.TopologyRewriteDefinitionClauseLabelZh, "无定义条款提示", StringComparison.Ordinal)
                        ? ResolveTopologyRewriteDefinitionClauseLabelZh(definitionClauseCode)
                        : item.TopologyRewriteDefinitionClauseLabelZh;
                    var reasons = item.Reasons;

                    if (!string.Equals(definitionClauseCode, "NONE", StringComparison.Ordinal))
                    {
                        reasons = item.Reasons
                            .Concat(new[] { $"TOPOLOGY_REWRITE_DEFINITION_CLAUSE_{definitionClauseCode}" })
                            .Distinct()
                            .ToArray();
                    }

                    return item with
                    {
                        TopologyRewriteDefinitionClauseCode = definitionClauseCode,
                        TopologyRewriteDefinitionClauseLabelZh = definitionClauseLabelZh,
                        Reasons = reasons
                    };
                })
            .ToArray();
    }

    private static CoreBodyProofPartResult[] AttachTopologyRewriteDefinitionClauseEffects(
        IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        return candidates
            .Select(
                item =>
                {
                    var definitionClauseEffectCode = ResolveTopologyRewriteDefinitionClauseEffectCode(item);
                    var definitionClauseEffectLabelZh =
                        ResolveTopologyRewriteDefinitionClauseEffectLabelZh(definitionClauseEffectCode);
                    var reasons = item.Reasons;

                    if (!string.Equals(definitionClauseEffectCode, "NONE", StringComparison.Ordinal))
                    {
                        reasons = item.Reasons
                            .Concat(new[] { $"TOPOLOGY_REWRITE_DEFINITION_CLAUSE_EFFECT_{definitionClauseEffectCode}" })
                            .Distinct()
                            .ToArray();
                    }

                    return item with
                    {
                        TopologyRewriteDefinitionClauseEffectCode = definitionClauseEffectCode,
                        TopologyRewriteDefinitionClauseEffectLabelZh = definitionClauseEffectLabelZh,
                        Reasons = reasons
                    };
                })
            .ToArray();
    }

    private StationRecomputeSnapshot RecomputeStationSnapshot(
        SectionTraceCleanStationResult station,
        int? excludedPartId,
        SectionTopologyStationResult? baselineTopologyStation)
    {
        var retained = station.Segments
            .Where(item => !item.IsSuppressed)
            .Where(item => excludedPartId is null || item.PartId != excludedPartId.Value)
            .ToArray();
        if (retained.Length == 0)
        {
            return new StationRecomputeSnapshot
            {
                StationId = station.StationId,
                BodyCandidateTraceCount = 0,
                BodyCandidateEnvelopeTraceCount = 0,
                ClosedLoopCandidate = false,
                BodyWidth = 0.0,
                EnvelopeSpanY = 0.0,
                EnvelopeSpanZ = 0.0,
                DominantBodyPartIds = Array.Empty<int>(),
                DominantEnvelopeBodyPartIds = Array.Empty<int>()
            };
        }

        var retainedBodyCandidates = retained
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .ToArray();
        var minY = retained.Min(item => Math.Min(item.StartY, item.EndY));
        var maxY = retained.Max(item => Math.Max(item.StartY, item.EndY));
        var minZ = retained.Min(item => Math.Min(item.StartZ, item.EndZ));
        var maxZ = retained.Max(item => Math.Max(item.StartZ, item.EndZ));
        var tolerance = Math.Max(_options.ContactDistanceToleranceMm * 2.0, 8.0);
        var bodyCandidateEnvelopeTraceCount = retainedBodyCandidates.Count(
            item => TouchesEnvelope(item, minY, maxY, minZ, maxZ, tolerance));
        var touchesMinY = retained.Any(item => Math.Min(item.StartY, item.EndY) <= minY + tolerance);
        var touchesMaxY = retained.Any(item => Math.Max(item.StartY, item.EndY) >= maxY - tolerance);
        var touchesMinZ = retained.Any(item => Math.Min(item.StartZ, item.EndZ) <= minZ + tolerance);
        var touchesMaxZ = retained.Any(item => Math.Max(item.StartZ, item.EndZ) >= maxZ - tolerance);
        var recomputedClosedLoopCandidate = retained.Length >= 4 &&
                                            retainedBodyCandidates.Length >= 2 &&
                                            touchesMinY &&
                                            touchesMaxY &&
                                            touchesMinZ &&
                                            touchesMaxZ;
        var dominantBodyPartIds = ResolveDominantPartIds(retainedBodyCandidates);
        var dominantEnvelopeBodyPartIds = ResolveDominantPartIds(
            retainedBodyCandidates.Where(item => TouchesEnvelope(item, minY, maxY, minZ, maxZ, tolerance)).ToArray());
        return new StationRecomputeSnapshot
        {
            StationId = station.StationId,
            BodyCandidateTraceCount = retainedBodyCandidates.Length,
            BodyCandidateEnvelopeTraceCount = bodyCandidateEnvelopeTraceCount,
            ClosedLoopCandidate = excludedPartId is null && baselineTopologyStation is not null
                ? baselineTopologyStation.ClosedLoopCandidate
                : recomputedClosedLoopCandidate,
            BodyWidth = retainedBodyCandidates.Sum(item => item.WidthLength),
            EnvelopeSpanY = maxY - minY,
            EnvelopeSpanZ = maxZ - minZ,
            DominantBodyPartIds = dominantBodyPartIds,
            DominantEnvelopeBodyPartIds = dominantEnvelopeBodyPartIds
        };
    }

    private bool StationTouchesEnvelope(SectionTraceCleanStationResult station, int partId)
    {
        var retained = station.Segments.Where(item => !item.IsSuppressed).ToArray();
        var current = retained.FirstOrDefault(item => item.PartId == partId);
        if (current is null)
        {
            return false;
        }

        var minY = retained.Min(item => Math.Min(item.StartY, item.EndY));
        var maxY = retained.Max(item => Math.Max(item.StartY, item.EndY));
        var minZ = retained.Min(item => Math.Min(item.StartZ, item.EndZ));
        var maxZ = retained.Max(item => Math.Max(item.StartZ, item.EndZ));
        var tolerance = Math.Max(_options.ContactDistanceToleranceMm * 2.0, 8.0);
        var currentMinY = Math.Min(current.StartY, current.EndY);
        var currentMaxY = Math.Max(current.StartY, current.EndY);
        var currentMinZ = Math.Min(current.StartZ, current.EndZ);
        var currentMaxZ = Math.Max(current.StartZ, current.EndZ);
        return currentMinY <= minY + tolerance ||
               currentMaxY >= maxY - tolerance ||
               currentMinZ <= minZ + tolerance ||
               currentMaxZ >= maxZ - tolerance;
    }

    private static string NormalizeProfileFamily(string profileString)
    {
        if (string.IsNullOrWhiteSpace(profileString))
        {
            return string.Empty;
        }

        var normalized = profileString.Trim().ToUpperInvariant();
        var starIndex = normalized.IndexOf('*');
        return starIndex > 0 ? normalized[..starIndex] : normalized;
    }

    private static bool TouchesEnvelope(
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
        return segmentMinY <= minY + tolerance ||
               segmentMaxY >= maxY - tolerance ||
               segmentMinZ <= minZ + tolerance ||
               segmentMaxZ >= maxZ - tolerance;
    }

    private static IReadOnlyList<int> ResolveDominantPartIds(IReadOnlyList<SectionTraceCleanSegment> segments)
    {
        if (segments.Count == 0)
        {
            return Array.Empty<int>();
        }

        var maxLength = segments.Max(item => item.WidthLength);
        return segments
            .Where(item => Math.Abs(item.WidthLength - maxLength) <= 1e-6)
            .Select(item => item.PartId)
            .Distinct()
            .OrderBy(item => item)
            .ToArray();
    }

    private static string ResolveTopologyRewritePatternCode(
        bool hasSpanYRewrite,
        bool hasSpanZRewrite,
        bool hasControllerSwitch)
    {
        if (hasSpanYRewrite && hasSpanZRewrite && hasControllerSwitch)
        {
            return "SPAN_YZ_PLUS_CONTROLLER";
        }

        if (hasSpanYRewrite && hasControllerSwitch)
        {
            return "SPAN_Y_PLUS_CONTROLLER";
        }

        if (hasSpanZRewrite && hasControllerSwitch)
        {
            return "SPAN_Z_PLUS_CONTROLLER";
        }

        if (hasSpanYRewrite && hasSpanZRewrite)
        {
            return "SPAN_YZ_ONLY";
        }

        if (hasSpanYRewrite)
        {
            return "SPAN_Y_ONLY";
        }

        if (hasSpanZRewrite)
        {
            return "SPAN_Z_ONLY";
        }

        if (hasControllerSwitch)
        {
            return "CONTROLLER_SWITCH_ONLY";
        }

        return "NONE";
    }

    private static string ResolveTopologyRewriteControllerRoleCode(
        bool hasDirectControllerRole,
        bool hasIndirectControllerRole)
    {
        if (hasDirectControllerRole && hasIndirectControllerRole)
        {
            return "MIXED_CONTROLLER_ROLE";
        }

        if (hasDirectControllerRole)
        {
            return "DIRECT_ENVELOPE_CONTROLLER";
        }

        if (hasIndirectControllerRole)
        {
            return "INDIRECT_ENVELOPE_CONTROLLER";
        }

        return "NONE";
    }

    private static int? ResolveEmbeddedPlatePrimaryPartId(IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        var primaryCandidates = candidates
            .Where(item => item.IsInputMainPart)
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .Where(item => item.ProofClass == CoreBodyProofClass.CoreBodyPart)
            .Where(item => !HasTopologyRewriteProofChain(item))
            .Where(item => NormalizeProfileFamily(item.ProfileString).StartsWith("PL", StringComparison.OrdinalIgnoreCase))
            .Where(item => item.PriorityStationPresenceRatio >= 0.95)
            .Where(item => item.PriorityBodyCoverageAfterRemovalRatio <= 0.001)
            .Where(item => item.EnvelopeBodyCoverageAfterRemovalRatio <= 0.001)
            .Where(item => item.ClosedLoopStationCountBeforeRemoval == 0)
            .Where(item => item.ClosedLoopStationCountAfterRemoval == 0)
            .ToArray();
        if (primaryCandidates.Length != 1)
        {
            return null;
        }

        var primaryCandidate = primaryCandidates[0];
        var otherBodyCandidates = candidates
            .Where(item => item.PartId != primaryCandidate.PartId)
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .Where(item => item.ProofClass != CoreBodyProofClass.BodyAccessoryPart)
            .ToArray();
        if (otherBodyCandidates.Length > 0)
        {
            return null;
        }

        return primaryCandidate.PartId;
    }

    private static HashSet<int> ResolveClosedBoxShellPartIds(IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        var shellCandidates = candidates
            .Where(IsClosedBoxShellCandidate)
            .ToArray();
        var strongEnvelopeShellCandidates = shellCandidates
            .Where(item => item.EnvelopeSupportStationRatio >= 0.95)
            .ToArray();
        var weakEnvelopeShellCandidates = shellCandidates
            .Where(item => item.EnvelopeSupportStationRatio < 0.95)
            .ToArray();
        var bodyShellCandidates = shellCandidates
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .ToArray();
        var bentVariantShellCandidates = shellCandidates
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.SpecialShape)
            .ToArray();

        var isCanonicalClosedBoxShell = bodyShellCandidates.Length >= 3 &&
                                        strongEnvelopeShellCandidates.Length >= 3;
        var isBentFlangeClosedBoxVariant = bodyShellCandidates.Length >= 2 &&
                                           bentVariantShellCandidates.Length >= 1 &&
                                           shellCandidates.Length >= 4 &&
                                           strongEnvelopeShellCandidates.Length >= 2;
        var isFourWallClosedBoxVariant = bodyShellCandidates.Length == 4 &&
                                         shellCandidates.Length == 4 &&
                                         bentVariantShellCandidates.Length == 0 &&
                                         strongEnvelopeShellCandidates.Length == 2 &&
                                         weakEnvelopeShellCandidates.Length == 2;
        if (!isCanonicalClosedBoxShell &&
            !isBentFlangeClosedBoxVariant &&
            !isFourWallClosedBoxVariant)
        {
            return new HashSet<int>();
        }

        return shellCandidates
            .Select(item => item.PartId)
            .ToHashSet();
    }

    private static bool IsClosedBoxShellCandidate(CoreBodyProofPartResult item)
    {
        if (HasTopologyRewriteProofChain(item))
        {
            return false;
        }

        if (item.PartitionClass != BodyCandidatePartitionClass.BodyCandidate &&
            item.PartitionClass != BodyCandidatePartitionClass.SpecialShape)
        {
            return false;
        }

        if (!NormalizeProfileFamily(item.ProfileString).StartsWith("PL", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HasPersistentClosedLoopEvidence(item) &&
               item.ClosedLoopStationCountAfterRemoval == 0 &&
               item.PriorityStationPresenceRatio >= 0.95;
    }

    private static HashSet<int> ResolveBentFlangeHPartIds(IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        var hCandidates = candidates
            .Where(item => !HasTopologyRewriteProofChain(item))
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate ||
                           item.PartitionClass == BodyCandidatePartitionClass.SpecialShape)
            .Where(item => NormalizeProfileFamily(item.ProfileString).StartsWith("PL", StringComparison.OrdinalIgnoreCase))
            .Where(item => item.PriorityStationPresenceRatio >= 0.95)
            .ToArray();
        if (hCandidates.Length != 3)
        {
            return new HashSet<int>();
        }

        var bodyCandidates = hCandidates
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .ToArray();
        var specialShapeCandidates = hCandidates
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.SpecialShape)
            .ToArray();
        if (bodyCandidates.Length != 2 || specialShapeCandidates.Length != 1)
        {
            return new HashSet<int>();
        }

        var specialShapeCandidate = specialShapeCandidates[0];
        var hasStrongEnvelopeOnAllCandidates = hCandidates.All(item => item.EnvelopeSupportStationRatio >= 0.95);
        var isOpenReviewFlangeBentFlangeH =
            bodyCandidates.All(item => item.EnvelopeSupportStationRatio >= 0.95) &&
            specialShapeCandidate.EnvelopeSupportStationRatio <= 0.05 &&
            specialShapeCandidate.PriorityBodyCoverageAfterRemovalRatio >= 0.95 &&
            specialShapeCandidate.EnvelopeBodyCoverageAfterRemovalRatio >= 0.95 &&
            specialShapeCandidate.BodyWidthRetentionRatio >= 0.95;
        var isSymmetricPureOpenBentFlangeH =
            bodyCandidates.All(item => item.EnvelopeSupportStationRatio >= 0.95) &&
            specialShapeCandidate.EnvelopeSupportStationRatio >= 0.95 &&
            bodyCandidates.All(
                item =>
                    item.BodyWidthRetentionRatio >= 0.45 &&
                    item.BodyWidthRetentionRatio <= 0.55) &&
            specialShapeCandidate.BodyWidthRetentionRatio >= 0.95;
        var isAsymmetricMainFlangeOpenBentFlangeH =
            specialShapeCandidate.IsInputMainPart &&
            specialShapeCandidate.EnvelopeSupportStationRatio >= 0.95 &&
            specialShapeCandidate.PriorityBodyCoverageAfterRemovalRatio >= 0.95 &&
            specialShapeCandidate.EnvelopeBodyCoverageAfterRemovalRatio >= 0.95 &&
            specialShapeCandidate.BodyWidthRetentionRatio >= 0.95 &&
            bodyCandidates.Count(item => item.EnvelopeSupportStationRatio >= 0.95) == 1 &&
            bodyCandidates.Count(item => item.EnvelopeSupportStationRatio <= 0.05) == 1 &&
            bodyCandidates.All(
                item =>
                    item.PriorityBodyCoverageAfterRemovalRatio >= 0.95 &&
                    item.EnvelopeBodyCoverageAfterRemovalRatio >= 0.95 &&
                    item.BodyWidthRetentionRatio >= 0.44 &&
                    item.BodyWidthRetentionRatio <= 0.56);
        var isPureOpenBentFlangeH = hCandidates.All(
            item =>
                item.ClosedLoopStationCountBeforeRemoval == 0 &&
                item.ClosedLoopStationCountAfterRemoval == 0);
        var isWeakSingleStationClosedLoopBentFlangeH =
            hCandidates.All(item => item.ClosedLoopStationCountBeforeRemoval <= 1) &&
            hCandidates.All(item => item.ClosedLoopStationCountAfterRemoval <= 1) &&
            hCandidates.Any(item => item.ClosedLoopStationCountBeforeRemoval > 0) &&
            bodyCandidates.All(item => item.ClosedLoopStationCountAfterRemoval == 0) &&
            specialShapeCandidate.ClosedLoopStationCountAfterRemoval <= 1;
        var isWeakSingleStationOpenReviewFlangeBentFlangeH =
            isWeakSingleStationClosedLoopBentFlangeH &&
            bodyCandidates.All(item => item.EnvelopeSupportStationRatio >= 0.95) &&
            specialShapeCandidate.EnvelopeSupportStationRatio <= 0.05 &&
            specialShapeCandidate.PriorityBodyCoverageAfterRemovalRatio >= 0.95 &&
            specialShapeCandidate.EnvelopeBodyCoverageAfterRemovalRatio >= 0.95 &&
            specialShapeCandidate.BodyWidthRetentionRatio >= 0.95;
        var isCanonicalEnvelopeBentFlangeH =
            hasStrongEnvelopeOnAllCandidates &&
            (isPureOpenBentFlangeH || isWeakSingleStationClosedLoopBentFlangeH);
        var isOpenReviewFlangeVariant =
            isPureOpenBentFlangeH &&
            isOpenReviewFlangeBentFlangeH;
        var isSymmetricPureOpenVariant =
            isPureOpenBentFlangeH &&
            isSymmetricPureOpenBentFlangeH;
        var isAsymmetricMainFlangeOpenVariant =
            isPureOpenBentFlangeH &&
            isAsymmetricMainFlangeOpenBentFlangeH;
        if (!isCanonicalEnvelopeBentFlangeH &&
            !isOpenReviewFlangeVariant &&
            !isSymmetricPureOpenVariant &&
            !isAsymmetricMainFlangeOpenVariant &&
            !isWeakSingleStationOpenReviewFlangeBentFlangeH)
        {
            return new HashSet<int>();
        }

        // Keep the folded-flange H fallback narrow, but allow the real GL variants whose
        // web/flange split is slightly softer, whose local folded flange introduces a
        // single-station pseudo closed-loop at one section, or whose lower flange is
        // expressed as a segmented/special-shape review part while the open H contour
        // still stays stable on the two main body plates.
        var useSoftOpenThresholds =
            isWeakSingleStationClosedLoopBentFlangeH ||
            isOpenReviewFlangeVariant ||
            isSymmetricPureOpenVariant ||
            isAsymmetricMainFlangeOpenVariant ||
            isWeakSingleStationOpenReviewFlangeBentFlangeH;
        var strongWebThreshold = useSoftOpenThresholds ? 0.50 : 0.45;
        var flangeLikeThreshold = useSoftOpenThresholds ? 0.50 : 0.55;
        var strongWebCandidateCount = hCandidates.Count(item => item.BodyWidthRetentionRatio <= strongWebThreshold);
        var flangeLikeCandidateCount = hCandidates.Count(
            item => item.BodyWidthRetentionRatio >= flangeLikeThreshold && item.BodyWidthRetentionRatio <= 1.001);
        var flangeAnchorCandidateCount = hCandidates.Count(
            item => item.BodyWidthRetentionRatio >= 0.70 && item.BodyWidthRetentionRatio <= 1.001);
        var inputMainCount = hCandidates.Count(item => item.IsInputMainPart);
        if (strongWebCandidateCount != 1 ||
            flangeLikeCandidateCount < 2 ||
            flangeAnchorCandidateCount < 1 ||
            inputMainCount != 1)
        {
            return new HashSet<int>();
        }

        return hCandidates
            .Select(item => item.PartId)
            .ToHashSet();
    }

    private static HashSet<int> ResolveFoldedThreePlateHPartIds(IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        var hCandidates = candidates
            .Where(item => !HasTopologyRewriteProofChain(item))
            .Where(item => item.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
            .Where(item => NormalizeProfileFamily(item.ProfileString).StartsWith("PL", StringComparison.OrdinalIgnoreCase))
            .Where(item => item.PriorityStationPresenceRatio >= 0.95)
            .ToArray();
        if (hCandidates.Length != 3)
        {
            return new HashSet<int>();
        }

        var strongEnvelopeCandidates = hCandidates
            .Where(item => item.EnvelopeSupportStationRatio >= 0.95)
            .ToArray();
        var weakEnvelopeCandidates = hCandidates
            .Where(item => item.EnvelopeSupportStationRatio >= 0.20 && item.EnvelopeSupportStationRatio < 0.95)
            .ToArray();
        if (strongEnvelopeCandidates.Length != 2 || weakEnvelopeCandidates.Length != 1)
        {
            return new HashSet<int>();
        }

        if (!hCandidates.All(
                item =>
                    item.PriorityBodyCoverageAfterRemovalRatio >= 0.95 &&
                    item.EnvelopeBodyCoverageAfterRemovalRatio >= 0.95 &&
                    item.ClosedLoopStationCountBeforeRemoval <= 1 &&
                    item.ClosedLoopStationCountAfterRemoval <= 1))
        {
            return new HashSet<int>();
        }

        if (hCandidates.All(item => item.ClosedLoopStationCountBeforeRemoval == 0) &&
            hCandidates.All(item => string.Equals(item.TopologyRewritePatternCode, "NONE", StringComparison.Ordinal)))
        {
            return new HashSet<int>();
        }

        var pairedRewriteCount = hCandidates.Count(
            item => string.Equals(item.TopologyRewriteShapeRoleCode, "PAIRED_BODY_PLATE_REWRITE", StringComparison.Ordinal));
        var singlePrimaryCount = hCandidates.Count(
            item => string.Equals(item.TopologyRewriteShapeRoleCode, "SINGLE_PRIMARY_BODY_PLATE_REWRITE", StringComparison.Ordinal));
        if (pairedRewriteCount < 2 || singlePrimaryCount != 1)
        {
            return new HashSet<int>();
        }

        if (strongEnvelopeCandidates.Any(
                item =>
                    item.BodyWidthRetentionRatio < 0.60 ||
                    item.BodyWidthRetentionRatio > 0.75))
        {
            return new HashSet<int>();
        }

        var weakEnvelopeCandidate = weakEnvelopeCandidates[0];
        if (weakEnvelopeCandidate.BodyWidthRetentionRatio < 0.60 ||
            weakEnvelopeCandidate.BodyWidthRetentionRatio > 0.85)
        {
            return new HashSet<int>();
        }

        return hCandidates
            .Select(item => item.PartId)
            .ToHashSet();
    }

    private static CoreBodyProofPartResult ApplyFallbackTopologyRewriteBootstrap(
        CoreBodyProofPartResult item,
        int? embeddedPlatePrimaryPartId,
        IReadOnlySet<int> closedBoxShellPartIds,
        IReadOnlySet<int> bentFlangeHPartIds,
        IReadOnlySet<int> foldedThreePlateHPartIds)
    {
        if (HasTopologyRewriteProofChain(item))
        {
            return item;
        }

        if (embeddedPlatePrimaryPartId.HasValue && item.PartId == embeddedPlatePrimaryPartId.Value)
        {
            var reasons = item.Reasons
                .Concat(
                    new[]
                    {
                        "FALLBACK_SINGLE_PRIMARY_PLATE_BOOTSTRAP",
                        "FALLBACK_EMBEDDED_PLATE_PRIMARY_CHAIN"
                    })
                .Distinct()
                .ToArray();
            return item with
            {
                TopologyRewriteCohortPartIds = new[] { item.PartId },
                TopologyRewriteCohortPartCount = 1,
                TopologyRewriteShapeRoleCode = "SINGLE_PRIMARY_BODY_PLATE_REWRITE",
                TopologyRewriteShapeRoleLabelZh = ResolveTopologyRewriteShapeRoleLabelZh("SINGLE_PRIMARY_BODY_PLATE_REWRITE"),
                TopologyRewriteFamilyRiskCode = "SINGLE_PRIMARY_PLATE_SYSTEM_RISK",
                TopologyRewriteFamilyRiskLabelZh = ResolveTopologyRewriteFamilyRiskLabelZh("SINGLE_PRIMARY_PLATE_SYSTEM_RISK"),
                TopologyRewriteProofTypeCode = "SINGLE_PRIMARY_PLATE_PROOF",
                TopologyRewriteProofTypeLabelZh = ResolveTopologyRewriteProofTypeLabelZh("SINGLE_PRIMARY_PLATE_PROOF"),
                TopologyRewriteFamilyProofTargetCode = "SINGLE_PRIMARY_PLATE_SYSTEM_TARGET",
                TopologyRewriteFamilyProofTargetLabelZh = ResolveTopologyRewriteFamilyProofTargetLabelZh("SINGLE_PRIMARY_PLATE_SYSTEM_TARGET"),
                TopologyRewriteDefinitionClauseCode = "PRIMARY_PLATE_CONTINUITY_CLAUSE",
                TopologyRewriteDefinitionClauseLabelZh = ResolveTopologyRewriteDefinitionClauseLabelZh("PRIMARY_PLATE_CONTINUITY_CLAUSE"),
                Reasons = reasons
            };
        }

        if (closedBoxShellPartIds.Contains(item.PartId))
        {
            var cohortPartIds = closedBoxShellPartIds
                .OrderBy(partId => partId)
                .ToArray();
            var reasons = item.Reasons
                .Concat(
                    new[]
                    {
                        "FALLBACK_CLOSED_BOX_SECTION_BOOTSTRAP",
                        "FALLBACK_CLOSED_BOX_WALL_CHAIN"
                    })
                .Distinct()
                .ToArray();
            return item with
            {
                TopologyRewriteCohortPartIds = cohortPartIds,
                TopologyRewriteCohortPartCount = cohortPartIds.Length,
                TopologyRewriteShapeRoleCode = "CLOSED_BOX_WALL_SHELL_REWRITE",
                TopologyRewriteShapeRoleLabelZh = ResolveTopologyRewriteShapeRoleLabelZh("CLOSED_BOX_WALL_SHELL_REWRITE"),
                TopologyRewriteFamilyRiskCode = "PAIRED_PRIMARY_WALL_SYSTEM_RISK",
                TopologyRewriteFamilyRiskLabelZh = ResolveTopologyRewriteFamilyRiskLabelZh("PAIRED_PRIMARY_WALL_SYSTEM_RISK"),
                TopologyRewriteProofTypeCode = "PAIRED_WALL_MAIN_CONTOUR_PROOF",
                TopologyRewriteProofTypeLabelZh = ResolveTopologyRewriteProofTypeLabelZh("PAIRED_WALL_MAIN_CONTOUR_PROOF"),
                TopologyRewriteFamilyProofTargetCode = "BOX_WALL_PAIR_PROOF_TARGET",
                TopologyRewriteFamilyProofTargetLabelZh = ResolveTopologyRewriteFamilyProofTargetLabelZh("BOX_WALL_PAIR_PROOF_TARGET"),
                TopologyRewriteDefinitionClauseCode = "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE",
                TopologyRewriteDefinitionClauseLabelZh = ResolveTopologyRewriteDefinitionClauseLabelZh("BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE"),
                Reasons = reasons
            };
        }

        if (bentFlangeHPartIds.Contains(item.PartId))
        {
            var cohortPartIds = bentFlangeHPartIds
                .OrderBy(partId => partId)
                .ToArray();
            var reasons = item.Reasons
                .Concat(
                    new[]
                    {
                        "FALLBACK_H_SECTION_BOOTSTRAP",
                        "FALLBACK_BENT_FLANGE_H_CHAIN"
                    })
                .Distinct()
                .ToArray();
            return item with
            {
                TopologyRewriteCohortPartIds = cohortPartIds,
                TopologyRewriteCohortPartCount = cohortPartIds.Length,
                TopologyRewriteShapeRoleCode = "H_BENT_FLANGE_SECTION_REWRITE",
                TopologyRewriteShapeRoleLabelZh = ResolveTopologyRewriteShapeRoleLabelZh("H_BENT_FLANGE_SECTION_REWRITE"),
                TopologyRewriteFamilyRiskCode = "H_WEB_FLANGE_SYSTEM_RISK",
                TopologyRewriteFamilyRiskLabelZh = ResolveTopologyRewriteFamilyRiskLabelZh("H_WEB_FLANGE_SYSTEM_RISK"),
                TopologyRewriteProofTypeCode = "H_MAIN_CONTOUR_PROOF",
                TopologyRewriteProofTypeLabelZh = ResolveTopologyRewriteProofTypeLabelZh("H_MAIN_CONTOUR_PROOF"),
                TopologyRewriteFamilyProofTargetCode = "H_WEB_FLANGE_PROOF_TARGET",
                TopologyRewriteFamilyProofTargetLabelZh = ResolveTopologyRewriteFamilyProofTargetLabelZh("H_WEB_FLANGE_PROOF_TARGET"),
                TopologyRewriteDefinitionClauseCode = "H_WEB_FLANGE_CONTINUITY_CLAUSE",
                TopologyRewriteDefinitionClauseLabelZh = ResolveTopologyRewriteDefinitionClauseLabelZh("H_WEB_FLANGE_CONTINUITY_CLAUSE"),
                Reasons = reasons
            };
        }

        if (foldedThreePlateHPartIds.Contains(item.PartId))
        {
            var cohortPartIds = foldedThreePlateHPartIds
                .OrderBy(partId => partId)
                .ToArray();
            var reasons = item.Reasons
                .Concat(
                    new[]
                    {
                        "FALLBACK_H_SECTION_BOOTSTRAP",
                        "FALLBACK_FOLDED_THREE_PLATE_H_CHAIN"
                    })
                .Distinct()
                .ToArray();
            return item with
            {
                TopologyRewriteCohortPartIds = cohortPartIds,
                TopologyRewriteCohortPartCount = cohortPartIds.Length,
                TopologyRewriteShapeRoleCode = "H_BENT_FLANGE_SECTION_REWRITE",
                TopologyRewriteShapeRoleLabelZh = ResolveTopologyRewriteShapeRoleLabelZh("H_BENT_FLANGE_SECTION_REWRITE"),
                TopologyRewriteFamilyRiskCode = "H_WEB_FLANGE_SYSTEM_RISK",
                TopologyRewriteFamilyRiskLabelZh = ResolveTopologyRewriteFamilyRiskLabelZh("H_WEB_FLANGE_SYSTEM_RISK"),
                TopologyRewriteProofTypeCode = "H_MAIN_CONTOUR_PROOF",
                TopologyRewriteProofTypeLabelZh = ResolveTopologyRewriteProofTypeLabelZh("H_MAIN_CONTOUR_PROOF"),
                TopologyRewriteFamilyProofTargetCode = "H_WEB_FLANGE_PROOF_TARGET",
                TopologyRewriteFamilyProofTargetLabelZh = ResolveTopologyRewriteFamilyProofTargetLabelZh("H_WEB_FLANGE_PROOF_TARGET"),
                TopologyRewriteDefinitionClauseCode = "H_WEB_FLANGE_CONTINUITY_CLAUSE",
                TopologyRewriteDefinitionClauseLabelZh = ResolveTopologyRewriteDefinitionClauseLabelZh("H_WEB_FLANGE_CONTINUITY_CLAUSE"),
                Reasons = reasons
            };
        }

        return item;
    }

    private static bool HasTopologyRewriteProofChain(CoreBodyProofPartResult item)
    {
        return !string.Equals(item.TopologyRewriteProofTypeCode, "NONE", StringComparison.Ordinal) ||
               !string.Equals(item.TopologyRewriteFamilyProofTargetCode, "NONE", StringComparison.Ordinal) ||
               !string.Equals(item.TopologyRewriteDefinitionClauseCode, "NONE", StringComparison.Ordinal);
    }

    private static string ResolveTopologyRewriteControllerRoleLabelZh(string roleCode)
    {
        return roleCode switch
        {
            "MIXED_CONTROLLER_ROLE" => "既有自身控制改写，也有同组控制切换",
            "DIRECT_ENVELOPE_CONTROLLER" => "被移除零件本身就是主包络控制件",
            "INDIRECT_ENVELOPE_CONTROLLER" => "主包络改写主要由同组控制件切换触发",
            _ => "无控制件角色改写"
        };
    }

    private static string ResolveTopologyRewritePatternLabelZh(string patternCode)
    {
        return patternCode switch
        {
            "SPAN_YZ_PLUS_CONTROLLER" => "Y/Z 跨度同时改写 + 包络控制件切换",
            "SPAN_Y_PLUS_CONTROLLER" => "Y 向跨度改写 + 包络控制件切换",
            "SPAN_Z_PLUS_CONTROLLER" => "Z 向跨度改写 + 包络控制件切换",
            "SPAN_YZ_ONLY" => "Y/Z 跨度同时改写",
            "SPAN_Y_ONLY" => "Y 向跨度改写",
            "SPAN_Z_ONLY" => "Z 向跨度改写",
            "CONTROLLER_SWITCH_ONLY" => "仅包络控制件切换",
            _ => "无主截面改写"
        };
    }

    private static string ResolveTopologyRewriteShapeRoleCode(int cohortPartCount, bool isInputMainPart)
    {
        if (cohortPartCount <= 0)
        {
            return "NONE";
        }

        if (cohortPartCount == 1)
        {
            return isInputMainPart
                ? "SINGLE_PRIMARY_BODY_PLATE_REWRITE"
                : "SINGLE_BODY_PLATE_REWRITE";
        }

        if (cohortPartCount == 2)
        {
            return "PAIRED_BODY_PLATE_REWRITE";
        }

        return "MULTI_BODY_PLATE_CLUSTER_REWRITE";
    }

    private static string ResolveTopologyRewriteShapeRoleLabelZh(string shapeRoleCode)
    {
        return shapeRoleCode switch
        {
            "SINGLE_PRIMARY_BODY_PLATE_REWRITE" => "单主板改写",
            "SINGLE_BODY_PLATE_REWRITE" => "单主体板改写",
            "PAIRED_BODY_PLATE_REWRITE" => "成对主体板改写",
            "CLOSED_BOX_WALL_SHELL_REWRITE" => "闭合箱壁壳改写",
            "H_BENT_FLANGE_SECTION_REWRITE" => "折型翼缘 H 截面改写",
            "MULTI_BODY_PLATE_CLUSTER_REWRITE" => "多主体板簇改写",
            _ => "无主体板组角色"
        };
    }

    private static string ResolveTopologyRewriteFamilyRiskCode(
        string patternCode,
        string controllerRoleCode,
        string shapeRoleCode)
    {
        if (string.Equals(patternCode, "NONE", StringComparison.Ordinal) ||
            string.Equals(shapeRoleCode, "NONE", StringComparison.Ordinal))
        {
            return "NONE";
        }

        if (string.Equals(shapeRoleCode, "PAIRED_BODY_PLATE_REWRITE", StringComparison.Ordinal) &&
            (string.Equals(patternCode, "SPAN_Y_PLUS_CONTROLLER", StringComparison.Ordinal) ||
             string.Equals(patternCode, "SPAN_Z_PLUS_CONTROLLER", StringComparison.Ordinal) ||
             string.Equals(patternCode, "SPAN_YZ_PLUS_CONTROLLER", StringComparison.Ordinal) ||
             string.Equals(patternCode, "SPAN_Y_ONLY", StringComparison.Ordinal) ||
             string.Equals(patternCode, "SPAN_Z_ONLY", StringComparison.Ordinal) ||
             string.Equals(patternCode, "SPAN_YZ_ONLY", StringComparison.Ordinal)))
        {
            return "PAIRED_PRIMARY_WALL_SYSTEM_RISK";
        }

        if (string.Equals(shapeRoleCode, "SINGLE_PRIMARY_BODY_PLATE_REWRITE", StringComparison.Ordinal))
        {
            return "SINGLE_PRIMARY_PLATE_SYSTEM_RISK";
        }

        if (string.Equals(shapeRoleCode, "SINGLE_BODY_PLATE_REWRITE", StringComparison.Ordinal))
        {
            return "SINGLE_BODY_PLATE_SYSTEM_RISK";
        }

        if (string.Equals(shapeRoleCode, "MULTI_BODY_PLATE_CLUSTER_REWRITE", StringComparison.Ordinal))
        {
            return string.Equals(controllerRoleCode, "DIRECT_ENVELOPE_CONTROLLER", StringComparison.Ordinal)
                ? "MULTI_PLATE_DIRECT_CONTROLLER_RISK"
                : "MULTI_PLATE_CLUSTER_RISK";
        }

        if (string.Equals(patternCode, "CONTROLLER_SWITCH_ONLY", StringComparison.Ordinal))
        {
            return "CONTROLLER_REASSIGNMENT_RISK";
        }

        return "GENERAL_TOPOLOGY_REWRITE_RISK";
    }

    private static string RefineTopologyRewriteFamilyRiskCode(
        CoreBodyProofPartResult item,
        IReadOnlyList<CoreBodyProofPartResult> candidates,
        string familyRiskCode)
    {
        if (!string.Equals(familyRiskCode, "PAIRED_PRIMARY_WALL_SYSTEM_RISK", StringComparison.Ordinal))
        {
            return familyRiskCode;
        }

        if (HasPersistentPairedWallEvidence(item, candidates))
        {
            return familyRiskCode;
        }

        return string.Equals(item.TopologyRewritePatternCode, "CONTROLLER_SWITCH_ONLY", StringComparison.Ordinal) ||
               string.Equals(item.TopologyRewriteControllerRoleCode, "DIRECT_ENVELOPE_CONTROLLER", StringComparison.Ordinal)
            ? "CONTROLLER_REASSIGNMENT_RISK"
            : "GENERAL_TOPOLOGY_REWRITE_RISK";
    }

    private static bool HasPersistentPairedWallEvidence(
        CoreBodyProofPartResult item,
        IReadOnlyList<CoreBodyProofPartResult> candidates)
    {
        if (!string.Equals(item.TopologyRewriteShapeRoleCode, "PAIRED_BODY_PLATE_REWRITE", StringComparison.Ordinal))
        {
            return false;
        }

        var cohortPartIds = item.TopologyRewriteCohortPartIds
            .Distinct()
            .OrderBy(partId => partId)
            .ToArray();
        if (cohortPartIds.Length != 2)
        {
            return false;
        }

        var cohort = candidates
            .Where(candidate => cohortPartIds.Contains(candidate.PartId))
            .ToArray();
        if (cohort.Length != 2)
        {
            return false;
        }

        return cohort.All(candidate => candidate.PartitionClass == BodyCandidatePartitionClass.BodyCandidate) &&
               cohort.All(candidate => candidate.PriorityStationPresenceRatio >= 0.95) &&
               cohort.All(candidate => candidate.EnvelopeSupportStationRatio >= 0.95) &&
               cohort.All(HasPersistentClosedLoopEvidence);
    }

    private static bool HasPersistentClosedLoopEvidence(CoreBodyProofPartResult item)
    {
        var requiredClosedLoopStations = RequiredPersistentClosedLoopStationCount(item);
        return item.ClosedLoopStationCountBeforeRemoval >= requiredClosedLoopStations &&
               item.LostClosedLoopStationIds.Count >= requiredClosedLoopStations;
    }

    private static int RequiredPersistentClosedLoopStationCount(CoreBodyProofPartResult item)
    {
        var priorityStationCount = ResolvePriorityStationCount(item);
        if (priorityStationCount <= 0)
        {
            return int.MaxValue;
        }

        return Math.Max(2, (int)Math.Ceiling(priorityStationCount / 2.0));
    }

    private static int ResolvePriorityStationCount(CoreBodyProofPartResult item)
    {
        if (item.PriorityStationPresenceRatio <= 0.0)
        {
            return item.PriorityStationPresenceCount;
        }

        return (int)Math.Round(item.PriorityStationPresenceCount / item.PriorityStationPresenceRatio, MidpointRounding.AwayFromZero);
    }

    private static string ResolveTopologyRewriteFamilyRiskLabelZh(string familyRiskCode)
    {
        return familyRiskCode switch
        {
            "PAIRED_PRIMARY_WALL_SYSTEM_RISK" => "成对主壁板系统改写风险",
            "SINGLE_PRIMARY_PLATE_SYSTEM_RISK" => "单主板系统改写风险",
            "SINGLE_BODY_PLATE_SYSTEM_RISK" => "单主体板系统改写风险",
            "H_WEB_FLANGE_SYSTEM_RISK" => "H型腹板/翼缘系统改写风险",
            "MULTI_PLATE_DIRECT_CONTROLLER_RISK" => "多主体板簇直接控制风险",
            "MULTI_PLATE_CLUSTER_RISK" => "多主体板簇改写风险",
            "CONTROLLER_REASSIGNMENT_RISK" => "包络控制件重分配风险",
            "GENERAL_TOPOLOGY_REWRITE_RISK" => "一般主截面改写风险",
            _ => "无家族证明风险"
        };
    }

    private static string ResolveTopologyRewriteProofTypeCode(
        string familyRiskCode,
        string patternCode,
        string shapeRoleCode)
    {
        if (string.Equals(familyRiskCode, "NONE", StringComparison.Ordinal))
        {
            return "NONE";
        }

        if (string.Equals(shapeRoleCode, "PAIRED_BODY_PLATE_REWRITE", StringComparison.Ordinal))
        {
            return "PAIRED_WALL_MAIN_CONTOUR_PROOF";
        }

        if (string.Equals(shapeRoleCode, "SINGLE_PRIMARY_BODY_PLATE_REWRITE", StringComparison.Ordinal))
        {
            return "SINGLE_PRIMARY_PLATE_PROOF";
        }

        if (string.Equals(shapeRoleCode, "SINGLE_BODY_PLATE_REWRITE", StringComparison.Ordinal))
        {
            return "SINGLE_BODY_PLATE_PROOF";
        }

        if (string.Equals(shapeRoleCode, "H_BENT_FLANGE_SECTION_REWRITE", StringComparison.Ordinal))
        {
            return "H_MAIN_CONTOUR_PROOF";
        }

        if (string.Equals(shapeRoleCode, "MULTI_BODY_PLATE_CLUSTER_REWRITE", StringComparison.Ordinal))
        {
            return "MULTI_PLATE_CLUSTER_PROOF";
        }

        if (string.Equals(patternCode, "CONTROLLER_SWITCH_ONLY", StringComparison.Ordinal))
        {
            return "ENVELOPE_CONTROLLER_PROOF";
        }

        return "GENERAL_MAIN_CONTOUR_PROOF";
    }

    private static string ResolveTopologyRewriteProofTypeLabelZh(string proofTypeCode)
    {
        return proofTypeCode switch
        {
            "PAIRED_WALL_MAIN_CONTOUR_PROOF" => "成对主壁板主轮廓证明",
            "SINGLE_PRIMARY_PLATE_PROOF" => "单主板主轮廓证明",
            "SINGLE_BODY_PLATE_PROOF" => "单主体板主轮廓证明",
            "H_MAIN_CONTOUR_PROOF" => "H型主轮廓证明",
            "MULTI_PLATE_CLUSTER_PROOF" => "多主体板簇主轮廓证明",
            "ENVELOPE_CONTROLLER_PROOF" => "包络控制主轮廓证明",
            "GENERAL_MAIN_CONTOUR_PROOF" => "一般主轮廓证明",
            _ => "无主截面证明对象"
        };
    }

    private static string ResolveTopologyRewriteFamilyProofTargetCode(
        string proofTypeCode,
        string familyRiskCode,
        string patternCode)
    {
        if (string.Equals(proofTypeCode, "NONE", StringComparison.Ordinal))
        {
            return "NONE";
        }

        if (string.Equals(proofTypeCode, "PAIRED_WALL_MAIN_CONTOUR_PROOF", StringComparison.Ordinal) &&
            string.Equals(familyRiskCode, "PAIRED_PRIMARY_WALL_SYSTEM_RISK", StringComparison.Ordinal))
        {
            return "BOX_WALL_PAIR_PROOF_TARGET";
        }

        if (string.Equals(proofTypeCode, "SINGLE_PRIMARY_PLATE_PROOF", StringComparison.Ordinal))
        {
            return "SINGLE_PRIMARY_PLATE_SYSTEM_TARGET";
        }

        if (string.Equals(proofTypeCode, "SINGLE_BODY_PLATE_PROOF", StringComparison.Ordinal))
        {
            return "SINGLE_BODY_PLATE_SYSTEM_TARGET";
        }

        if (string.Equals(proofTypeCode, "H_MAIN_CONTOUR_PROOF", StringComparison.Ordinal))
        {
            return "H_WEB_FLANGE_PROOF_TARGET";
        }

        if (string.Equals(proofTypeCode, "MULTI_PLATE_CLUSTER_PROOF", StringComparison.Ordinal))
        {
            return "MULTI_PLATE_CLUSTER_SYSTEM_TARGET";
        }

        if (string.Equals(proofTypeCode, "ENVELOPE_CONTROLLER_PROOF", StringComparison.Ordinal) ||
            string.Equals(patternCode, "CONTROLLER_SWITCH_ONLY", StringComparison.Ordinal))
        {
            return "ENVELOPE_CONTROLLER_REASSIGNMENT_TARGET";
        }

        return "GENERAL_FAMILY_PROOF_TARGET";
    }

    private static string ResolveTopologyRewriteFamilyProofTargetLabelZh(string familyProofTargetCode)
    {
        return familyProofTargetCode switch
        {
            "BOX_WALL_PAIR_PROOF_TARGET" => "箱型对壁闭合证明目标",
            "SINGLE_PRIMARY_PLATE_SYSTEM_TARGET" => "单主板体系待定目标",
            "SINGLE_BODY_PLATE_SYSTEM_TARGET" => "单主体板体系待定目标",
            "H_WEB_FLANGE_PROOF_TARGET" => "H型腹板/翼缘证明目标",
            "MULTI_PLATE_CLUSTER_SYSTEM_TARGET" => "多主体板簇待定目标",
            "ENVELOPE_CONTROLLER_REASSIGNMENT_TARGET" => "包络控制重分配待定目标",
            "GENERAL_FAMILY_PROOF_TARGET" => "一般家族证明目标",
            _ => "无家族证明目标"
        };
    }

    private static string ResolveTopologyRewriteDefinitionClauseCode(
        string familyProofTargetCode,
        string patternCode,
        string controllerRoleCode)
    {
        if (string.Equals(familyProofTargetCode, "NONE", StringComparison.Ordinal))
        {
            return "NONE";
        }

        if (string.Equals(familyProofTargetCode, "BOX_WALL_PAIR_PROOF_TARGET", StringComparison.Ordinal))
        {
            return "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE";
        }

        if (string.Equals(familyProofTargetCode, "SINGLE_PRIMARY_PLATE_SYSTEM_TARGET", StringComparison.Ordinal))
        {
            return "PRIMARY_PLATE_CONTINUITY_CLAUSE";
        }

        if (string.Equals(familyProofTargetCode, "SINGLE_BODY_PLATE_SYSTEM_TARGET", StringComparison.Ordinal))
        {
            return "BODY_PLATE_CONTINUITY_CLAUSE";
        }

        if (string.Equals(familyProofTargetCode, "H_WEB_FLANGE_PROOF_TARGET", StringComparison.Ordinal))
        {
            return "H_WEB_FLANGE_CONTINUITY_CLAUSE";
        }

        if (string.Equals(familyProofTargetCode, "MULTI_PLATE_CLUSTER_SYSTEM_TARGET", StringComparison.Ordinal))
        {
            return string.Equals(controllerRoleCode, "DIRECT_ENVELOPE_CONTROLLER", StringComparison.Ordinal)
                ? "CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE"
                : "CLUSTER_ACCESSORY_EXCLUSION_CLAUSE";
        }

        if (string.Equals(familyProofTargetCode, "ENVELOPE_CONTROLLER_REASSIGNMENT_TARGET", StringComparison.Ordinal) ||
            string.Equals(patternCode, "CONTROLLER_SWITCH_ONLY", StringComparison.Ordinal))
        {
            return "CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE";
        }

        return "GENERAL_DEFINITION_REVIEW_CLAUSE";
    }

    private static string ResolveTopologyRewriteDefinitionClauseLabelZh(string definitionClauseCode)
    {
        return definitionClauseCode switch
        {
            "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE" => "箱型：闭合/对边稳定条款",
            "PRIMARY_PLATE_CONTINUITY_CLAUSE" => "主板：多数站位持续性条款",
            "BODY_PLATE_CONTINUITY_CLAUSE" => "主体板：多数站位持续性条款",
            "H_WEB_FLANGE_CONTINUITY_CLAUSE" => "H型：腹板/翼缘连续性条款",
            "CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE" => "多板簇：直接控制件排除条款",
            "CLUSTER_ACCESSORY_EXCLUSION_CLAUSE" => "多板簇：附属板排除条款",
            "CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE" => "包络控制重分配复核条款",
            "GENERAL_DEFINITION_REVIEW_CLAUSE" => "一般定义复核条款",
            _ => "无定义条款提示"
        };
    }

    private static string ResolveTopologyRewriteDefinitionClauseEffectCode(CoreBodyProofPartResult item)
    {
        if (string.Equals(item.TopologyRewriteDefinitionClauseCode, "NONE", StringComparison.Ordinal))
        {
            return "NONE";
        }

        return item.TopologyRewriteDefinitionClauseCode switch
        {
            "BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE" => item.LostClosedLoopStationIds.Count > 0
                ? "BOX_CLOSED_LOOP_DIRECT_BREAK_EFFECT"
                : "BOX_OPPOSITE_WALL_STABILITY_REWRITE_EFFECT",
            "PRIMARY_PLATE_CONTINUITY_CLAUSE" => item.LostBodyCoverageStationIds.Count > 0 ||
                                                 item.PriorityBodyCoverageAfterRemovalRatio < 0.999
                ? "PRIMARY_PLATE_CONTINUITY_BREAK_EFFECT"
                : "PRIMARY_PLATE_CONTINUITY_REWRITE_EFFECT",
            "BODY_PLATE_CONTINUITY_CLAUSE" => item.LostBodyCoverageStationIds.Count > 0 ||
                                              item.PriorityBodyCoverageAfterRemovalRatio < 0.999
                ? "BODY_PLATE_CONTINUITY_BREAK_EFFECT"
                : "BODY_PLATE_CONTINUITY_REWRITE_EFFECT",
            "H_WEB_FLANGE_CONTINUITY_CLAUSE" => item.LostBodyCoverageStationIds.Count > 0 ||
                                                item.PriorityBodyCoverageAfterRemovalRatio < 0.999 ||
                                                item.BodyWidthRetentionRatio < 0.60 ||
                                                ShouldEscalateHContinuityToBreak(item)
                ? "H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT"
                : "H_WEB_FLANGE_CONTINUITY_REWRITE_EFFECT",
            "CLUSTER_DIRECT_CONTROL_EXCLUSION_CLAUSE" => string.Equals(
                    item.TopologyRewriteControllerRoleCode,
                    "DIRECT_ENVELOPE_CONTROLLER",
                    StringComparison.Ordinal)
                ? "CLUSTER_DIRECT_CONTROLLER_NOT_EXCLUDABLE_EFFECT"
                : "CLUSTER_DIRECT_CONTROL_REVIEW_EFFECT",
            "CLUSTER_ACCESSORY_EXCLUSION_CLAUSE" => item.LostClosedLoopStationIds.Count == 0 &&
                                                    item.LostEnvelopeSupportStationIds.Count == 0 &&
                                                    item.EnvelopeBodyCoverageAfterRemovalRatio >= 0.999
                ? "CLUSTER_ACCESSORY_EXCLUSION_SUPPORTED_EFFECT"
                : "CLUSTER_ACCESSORY_EXCLUSION_REVIEW_EFFECT",
            "CONTROLLER_REASSIGNMENT_REVIEW_CLAUSE" => "CONTROLLER_REASSIGNMENT_TRIGGERED_EFFECT",
            "GENERAL_DEFINITION_REVIEW_CLAUSE" => "GENERAL_DEFINITION_REVIEW_TRIGGERED_EFFECT",
            _ => "GENERAL_DEFINITION_REVIEW_TRIGGERED_EFFECT"
        };
    }

    private static string ResolveTopologyRewriteDefinitionClauseEffectLabelZh(string definitionClauseEffectCode)
    {
        return definitionClauseEffectCode switch
        {
            "BOX_CLOSED_LOOP_DIRECT_BREAK_EFFECT" => "箱型：移除此件会直接破坏闭合",
            "BOX_OPPOSITE_WALL_STABILITY_REWRITE_EFFECT" => "箱型：移除此件会改写对边稳定控制",
            "PRIMARY_PLATE_CONTINUITY_BREAK_EFFECT" => "主板：移除此件会破坏多数站位持续性",
            "PRIMARY_PLATE_CONTINUITY_REWRITE_EFFECT" => "主板：移除此件会改写多数站位持续性",
            "BODY_PLATE_CONTINUITY_BREAK_EFFECT" => "主体板：移除此件会破坏多数站位持续性",
            "BODY_PLATE_CONTINUITY_REWRITE_EFFECT" => "主体板：移除此件会改写多数站位持续性",
            "H_WEB_FLANGE_CONTINUITY_BREAK_EFFECT" => "H型：移除此件会破坏腹板/翼缘连续性",
            "H_WEB_FLANGE_CONTINUITY_REWRITE_EFFECT" => "H型：移除此件会改写腹板/翼缘连续性",
            "CLUSTER_DIRECT_CONTROLLER_NOT_EXCLUDABLE_EFFECT" => "多板簇：此件更像直接控制件，不能直接排除",
            "CLUSTER_DIRECT_CONTROL_REVIEW_EFFECT" => "多板簇：此件仍需按直接控制路径复核",
            "CLUSTER_ACCESSORY_EXCLUSION_SUPPORTED_EFFECT" => "多板簇：此件更像附属/边界件，可优先排除",
            "CLUSTER_ACCESSORY_EXCLUSION_REVIEW_EFFECT" => "多板簇：此件排除后仍会改写主截面，需复核",
            "CONTROLLER_REASSIGNMENT_TRIGGERED_EFFECT" => "包络控制：移除此件会触发控制重分配",
            "GENERAL_DEFINITION_REVIEW_TRIGGERED_EFFECT" => "一般定义：移除此件会触发定义级复核",
            _ => "无定义条款效果"
        };
    }

    private static bool ShouldEscalateHContinuityToBreak(CoreBodyProofPartResult item)
    {
        return string.Equals(item.TopologyRewriteShapeRoleCode, "H_BENT_FLANGE_SECTION_REWRITE", StringComparison.Ordinal) &&
               !string.Equals(item.TopologyRewritePatternCode, "NONE", StringComparison.Ordinal) &&
               item.EnvelopeSupportStationRatio >= 0.95 &&
               item.BodyWidthRetentionRatio < 0.70;
    }

    private static string BuildTopologyRewriteCohortKey(string patternCode, string controllerRoleCode)
    {
        return $"{patternCode}::{controllerRoleCode}";
    }

    private sealed record StationRecomputeSnapshot
    {
        public required string StationId { get; init; }
        public required int BodyCandidateTraceCount { get; init; }
        public required int BodyCandidateEnvelopeTraceCount { get; init; }
        public required bool ClosedLoopCandidate { get; init; }
        public required double BodyWidth { get; init; }
        public required double EnvelopeSpanY { get; init; }
        public required double EnvelopeSpanZ { get; init; }
        public required IReadOnlyList<int> DominantBodyPartIds { get; init; }
        public required IReadOnlyList<int> DominantEnvelopeBodyPartIds { get; init; }
    }
}
