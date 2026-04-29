using TeklaBodyBracketRecognition.Core.Algorithms;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class PipelineArtifactSupport
{
    public static BodyCandidatePartitionViewOutput BuildBodyCandidatePartitionViewOutput(
        ImportedAssemblyJob job,
        string viewKind,
        BodyCandidatePartitionResult partition)
    {
        return new BodyCandidatePartitionViewOutput
        {
            SourceFile = job.SourceFile,
            MemberId = job.MemberId,
            AssemblyId = partition.AssemblyId,
            ViewKind = viewKind,
            InputMainPartId = partition.InputMainPartId,
            SourceMainPartId = job.SourceMainPartId,
            SourceMainPartName = job.SourceMainPartName,
            SourceMainPartProfileString = job.SourceMainPartProfileString,
            SynthesizedBody = job.SynthesizedBody,
            ImportSynthesisKind = job.SynthesisKind,
            SourceBodySeedPartIds = job.SourceBodySeedParts.Select(part => part.PartId).ToArray(),
            Partition = partition
        };
    }

    public static StableBodyZoneViewOutput BuildStableBodyZoneViewOutput(
        ImportedAssemblyJob job,
        string viewKind,
        StableBodyZoneResult stableZones)
    {
        return new StableBodyZoneViewOutput
        {
            SourceFile = job.SourceFile,
            MemberId = job.MemberId,
            AssemblyId = stableZones.AssemblyId,
            ViewKind = viewKind,
            SourceMainPartId = job.SourceMainPartId,
            ImportSynthesisKind = job.SynthesisKind,
            SynthesizedBody = job.SynthesizedBody,
            Result = stableZones
        };
    }

    public static SectionStationViewOutput BuildSectionStationViewOutput(
        ImportedAssemblyJob job,
        string viewKind,
        SectionStationPlanResult stationPlan)
    {
        return new SectionStationViewOutput
        {
            SourceFile = job.SourceFile,
            MemberId = job.MemberId,
            AssemblyId = stationPlan.AssemblyId,
            ViewKind = viewKind,
            SourceMainPartId = job.SourceMainPartId,
            ImportSynthesisKind = job.SynthesisKind,
            SynthesizedBody = job.SynthesizedBody,
            Result = stationPlan
        };
    }

    public static SectionTraceViewOutput BuildSectionTraceViewOutput(
        ImportedAssemblyJob job,
        string viewKind,
        SectionTraceExtractionResult traceResult)
    {
        return new SectionTraceViewOutput
        {
            SourceFile = job.SourceFile,
            MemberId = job.MemberId,
            AssemblyId = traceResult.AssemblyId,
            ViewKind = viewKind,
            SourceMainPartId = job.SourceMainPartId,
            ImportSynthesisKind = job.SynthesisKind,
            SynthesizedBody = job.SynthesizedBody,
            Result = traceResult
        };
    }

    public static SectionTraceCleaningViewOutput BuildSectionTraceCleaningViewOutput(
        ImportedAssemblyJob job,
        string viewKind,
        SectionTraceCleaningResult cleaningResult)
    {
        return new SectionTraceCleaningViewOutput
        {
            SourceFile = job.SourceFile,
            MemberId = job.MemberId,
            AssemblyId = cleaningResult.AssemblyId,
            ViewKind = viewKind,
            SourceMainPartId = job.SourceMainPartId,
            ImportSynthesisKind = job.SynthesisKind,
            SynthesizedBody = job.SynthesizedBody,
            Result = cleaningResult
        };
    }

    public static SectionTopologyViewOutput BuildSectionTopologyViewOutput(
        ImportedAssemblyJob job,
        string viewKind,
        SectionTopologyAnalysisResult topologyResult)
    {
        return new SectionTopologyViewOutput
        {
            SourceFile = job.SourceFile,
            MemberId = job.MemberId,
            AssemblyId = topologyResult.AssemblyId,
            ViewKind = viewKind,
            SourceMainPartId = job.SourceMainPartId,
            ImportSynthesisKind = job.SynthesisKind,
            SynthesizedBody = job.SynthesizedBody,
            Result = topologyResult
        };
    }

    public static CoreBodyProofViewOutput BuildCoreBodyProofViewOutput(
        ImportedAssemblyJob job,
        string viewKind,
        CoreBodyProofResult proofResult)
    {
        return new CoreBodyProofViewOutput
        {
            SourceFile = job.SourceFile,
            MemberId = job.MemberId,
            AssemblyId = proofResult.AssemblyId,
            ViewKind = viewKind,
            SourceMainPartId = job.SourceMainPartId,
            ImportSynthesisKind = job.SynthesisKind,
            SynthesizedBody = job.SynthesizedBody,
            Result = proofResult
        };
    }

    public static IReadOnlyList<SectionTraceTopologySummaryRow> BuildSectionTraceTopologySummaryRows(
        IReadOnlyList<SectionTraceViewOutput> recognitionInputViews,
        IReadOnlyList<SectionTraceViewOutput> realInputViews)
    {
        return recognitionInputViews
            .Concat(realInputViews)
            .Select(BuildSectionTraceTopologySummaryRow)
            .OrderBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.ViewKind, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static SectionTraceTopologySummaryRow BuildSectionTraceTopologySummaryRow(SectionTraceViewOutput view)
    {
        var stations = view.Result.Stations.ToArray();
        var allSegments = stations.SelectMany(station => station.Segments).ToArray();
        var nonEmptyStations = stations.Where(station => station.Segments.Count > 0).ToArray();
        var priorityStations = stations.Where(station => station.StationKind != SectionStationKind.LowPriority).ToArray();
        var tracedPartIds = allSegments
            .Select(segment => segment.PartId)
            .Distinct()
            .OrderBy(partId => partId)
            .ToArray();
        var dominantThreshold = Math.Max(1, (int)Math.Ceiling(Math.Max(nonEmptyStations.Length, 1) * 0.6));
        var dominantPartIds = allSegments
            .GroupBy(segment => segment.PartId)
            .Where(group => group.Count() >= dominantThreshold)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .ToArray();

        var bodyCandidateSegmentCount = allSegments.Count(segment => segment.PartitionClass == BodyCandidatePartitionClass.BodyCandidate);
        var bodyAccessorySegmentCount = allSegments.Count(segment => segment.PartitionClass == BodyCandidatePartitionClass.BodyAccessoryCandidate);
        var endConnectionSegmentCount = allSegments.Count(segment => segment.PartitionClass == BodyCandidatePartitionClass.EndConnectionCandidate);
        var localStiffenerSegmentCount = allSegments.Count(segment => segment.PartitionClass == BodyCandidatePartitionClass.LocalStiffenerCandidate);
        var tinyPartSegmentCount = allSegments.Count(segment => segment.PartitionClass == BodyCandidatePartitionClass.TinyPart);
        var specialShapeSegmentCount = allSegments.Count(segment => segment.PartitionClass == BodyCandidatePartitionClass.SpecialShape);
        var priorityStationsWithBodyCandidate = priorityStations.Count(
            station => station.Segments.Any(segment => segment.PartitionClass == BodyCandidatePartitionClass.BodyCandidate));

        var flags = new List<string>();
        if (stations.Length == 0)
        {
            flags.Add("NO_STATIONS");
        }

        if (nonEmptyStations.Length == 0)
        {
            flags.Add("ALL_STATIONS_EMPTY");
        }

        if (allSegments.Length > 0 && bodyCandidateSegmentCount == 0)
        {
            flags.Add("NO_BODY_CANDIDATE_SEGMENTS");
        }

        if (priorityStations.Length > 0 && priorityStationsWithBodyCandidate < priorityStations.Length)
        {
            flags.Add("BODY_CANDIDATE_GAPS_ON_PRIORITY_STATIONS");
        }

        if (view.SynthesizedBody &&
            string.Equals(view.ViewKind, "recognition_input", StringComparison.OrdinalIgnoreCase) &&
            tracedPartIds.Any(partId => partId < 0))
        {
            flags.Add("CONTAINS_SYNTHETIC_TRACE_PARTS");
        }

        return new SectionTraceTopologySummaryRow
        {
            SourceFile = view.SourceFile,
            MemberId = view.MemberId,
            AssemblyId = view.AssemblyId,
            ViewKind = view.ViewKind,
            SourceMainPartId = view.SourceMainPartId,
            SynthesizedBody = view.SynthesizedBody,
            ImportSynthesisKind = view.ImportSynthesisKind,
            StationCount = stations.Length,
            NonEmptyStationCount = nonEmptyStations.Length,
            PriorityStationCount = priorityStations.Length,
            PriorityStationsWithBodyCandidate = priorityStationsWithBodyCandidate,
            PriorityStationBodyCoverage = priorityStations.Length == 0
                ? 0.0
                : Math.Round(priorityStationsWithBodyCandidate / (double)priorityStations.Length, 4),
            DistinctTracedPartCount = tracedPartIds.Length,
            TracedPartIds = tracedPartIds,
            DominantPartIds = dominantPartIds,
            BodyCandidateSegmentCount = bodyCandidateSegmentCount,
            BodyAccessorySegmentCount = bodyAccessorySegmentCount,
            EndConnectionSegmentCount = endConnectionSegmentCount,
            LocalStiffenerSegmentCount = localStiffenerSegmentCount,
            TinyPartSegmentCount = tinyPartSegmentCount,
            SpecialShapeSegmentCount = specialShapeSegmentCount,
            AverageSegmentsPerNonEmptyStation = nonEmptyStations.Length == 0
                ? 0.0
                : Math.Round(allSegments.Length / (double)nonEmptyStations.Length, 4),
            AverageBodyCandidateSegmentsPerStation = stations.Length == 0
                ? 0.0
                : Math.Round(bodyCandidateSegmentCount / (double)stations.Length, 4),
            Flags = flags
        };
    }
}
