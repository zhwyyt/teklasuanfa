using TeklaBodyBracketRecognition.Core.Algorithms;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class PipelineSummarySupport
{
    public static string BuildBodyCandidatePartitionSummaryMarkdown(
        IReadOnlyList<BodyCandidatePartitionViewOutput> recognitionInputViews,
        IReadOnlyList<BodyCandidatePartitionViewOutput> realInputViews)
    {
        var lines = new List<string>
        {
            "# 主体候选分层双视图摘要",
            string.Empty,
            "本摘要同时展示两套分层视图：",
            "- `recognition_input`：当前旧识别器真正吃到的输入（可能含虚拟主体）。",
            "- `real_input`：只看真实零件的分层视图。",
            string.Empty,
            $" - recognition_input 样本数: `{recognitionInputViews.Count}`",
            $" - real_input 样本数: `{realInputViews.Count}`",
            $" - SynthesizedBody 样本数: `{recognitionInputViews.Count(item => item.SynthesizedBody)}`",
            string.Empty,
            "## 关键差异样本",
            string.Empty
        };

        var differingRows = recognitionInputViews
            .Join(
                realInputViews,
                left => left.AssemblyId,
                right => right.AssemblyId,
                (left, right) => new { Recognition = left, Real = right })
            .Where(item => item.Recognition.SynthesizedBody)
            .Select(
                item =>
                {
                    var recognitionIds = item.Recognition.Partition.Items.Select(part => part.PartId).ToHashSet();
                    var realBodyCandidates = item.Real.Partition.Items
                        .Where(part => part.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
                        .Select(part => part.PartId)
                        .ToArray();
                    var missingRealBodyCandidates = realBodyCandidates
                        .Where(partId => !recognitionIds.Contains(partId))
                        .ToArray();
                    return new
                    {
                        item.Recognition.MemberId,
                        item.Recognition.AssemblyId,
                        item.Recognition.ImportSynthesisKind,
                        RealBodyCandidates = realBodyCandidates,
                        MissingRealBodyCandidates = missingRealBodyCandidates
                    };
                })
            .Where(item => item.MissingRealBodyCandidates.Length > 0)
            .OrderBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (differingRows.Length == 0)
        {
            lines.Add("当前没有发现“real_input 里的主体候选在 recognition_input 中完全缺失”的样本。");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("| MemberId | AssemblyId | ImportSynthesisKind | real_input body_candidate | recognition_input 缺失的真实主体候选 |");
        lines.Add("| --- | --- | --- | --- | --- |");
        foreach (var row in differingRows)
        {
            lines.Add(
                $"| {row.MemberId} | {row.AssemblyId} | {row.ImportSynthesisKind ?? string.Empty} | {string.Join("<br>", row.RealBodyCandidates)} | {string.Join("<br>", row.MissingRealBodyCandidates)} |");
        }

        lines.Add(string.Empty);
        lines.Add("## 说明");
        lines.Add(string.Empty);
        lines.Add("- 若 `recognition_input` 缺失了 `real_input` 中的真实主体候选，说明当前导入层仍把“主体分层”与“启发式识别输入”混在一起。");
        lines.Add("- 这类样本应优先作为后续阶段 2 收敛对象，例如 `T3-1HXZ-1`。");
        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildStableBodyZoneSummaryMarkdown(
        IReadOnlyList<StableBodyZoneViewOutput> recognitionInputViews,
        IReadOnlyList<StableBodyZoneViewOutput> realInputViews,
        IReadOnlyList<SectionStationViewOutput> realInputStations)
    {
        var lines = new List<string>
        {
            "# 稳定区与站位摘要",
            string.Empty,
            $" - recognition_input 稳定区结果数: `{recognitionInputViews.Count}`",
            $" - real_input 稳定区结果数: `{realInputViews.Count}`",
            $" - real_input 站位结果数: `{realInputStations.Count}`",
            string.Empty,
            "## 代表样本",
            string.Empty
        };

        var representatives = realInputViews
            .Where(item => item.Result.Zones.Count > 0)
            .OrderByDescending(item => item.Result.Zones.Count(zone => zone.ZoneKind == StableBodyZoneKind.Stable))
            .ThenByDescending(item => item.Result.Zones.Count(zone => zone.ZoneKind is StableBodyZoneKind.EndComplex or StableBodyZoneKind.LocalComplex))
            .ThenByDescending(item => realInputStations.FirstOrDefault(station => station.AssemblyId == item.AssemblyId && station.ViewKind == item.ViewKind)?.Result.Stations.Count ?? 0)
            .ThenBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();

        if (representatives.Length == 0)
        {
            lines.Add("当前没有可展示的稳定区结果。");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("| MemberId | AssemblyId | View | StableZones | End/Local Complex Zones | StationCount |");
        lines.Add("| --- | --- | --- | --- | --- | --- |");

        foreach (var item in representatives)
        {
            var stableCount = item.Result.Zones.Count(zone => zone.ZoneKind == StableBodyZoneKind.Stable);
            var complexCount = item.Result.Zones.Count(zone => zone.ZoneKind is StableBodyZoneKind.EndComplex or StableBodyZoneKind.LocalComplex);
            var stationCount = realInputStations.FirstOrDefault(station => station.AssemblyId == item.AssemblyId && station.ViewKind == item.ViewKind)?.Result.Stations.Count ?? 0;
            lines.Add($"| {item.MemberId} | {item.AssemblyId} | {item.ViewKind} | {stableCount} | {complexCount} | {stationCount} |");
        }

        lines.Add(string.Empty);
        lines.Add("## 说明");
        lines.Add(string.Empty);
        lines.Add("- 这一版的稳定区/站位还是阶段 3 首版原型，主要目标是把端部复杂区与主体稳定区拆开。");
        lines.Add("- 后续阶段 4 的横截面迹线重建，将默认优先消费 `real_input` 视图下的稳定区与站位。");
        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildSectionTraceSummaryMarkdown(
        IReadOnlyList<SectionTraceViewOutput> recognitionInputViews,
        IReadOnlyList<SectionTraceViewOutput> realInputViews,
        IReadOnlyList<SectionTraceTopologySummaryRow> topologySummaryRows)
    {
        var lines = new List<string>
        {
            "# 横截面迹线摘要",
            string.Empty,
            $" - recognition_input 迹线结果数: `{recognitionInputViews.Count}`",
            $" - real_input 迹线结果数: `{realInputViews.Count}`",
            $" - real_input 非空迹线样本数: `{topologySummaryRows.Count(item => item.ViewKind == "real_input" && item.NonEmptyStationCount > 0)}`",
            string.Empty,
            "## 代表样本",
            string.Empty
        };

        var representatives = topologySummaryRows
            .Where(item => item.ViewKind == "real_input")
            .Where(item => item.NonEmptyStationCount > 0)
            .OrderByDescending(item => item.PriorityStationBodyCoverage)
            .ThenByDescending(item => item.NonEmptyStationCount)
            .ThenByDescending(item => item.DistinctTracedPartCount)
            .ThenByDescending(item => item.SynthesizedBody)
            .ThenBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();

        if (representatives.Length == 0)
        {
            lines.Add("当前没有可展示的横截面迹线结果。");
        }
        else
        {
            lines.Add("| MemberId | AssemblyId | View | NonEmpty/Stations | Priority Body Coverage | DominantPartIds | Flags |");
            lines.Add("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (var item in representatives)
            {
                lines.Add(
                    $"| {item.MemberId} | {item.AssemblyId} | {item.ViewKind} | {item.NonEmptyStationCount}/{item.StationCount} | {item.PriorityStationBodyCoverage:0.00} | {string.Join("<br>", item.DominantPartIds)} | {string.Join("<br>", item.Flags)} |");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## 合成主体差异样本");
        lines.Add(string.Empty);

        var synthesizedDiffs = recognitionInputViews
            .Join(
                realInputViews,
                left => left.AssemblyId,
                right => right.AssemblyId,
                (left, right) => new { Recognition = left, Real = right })
            .Where(item => item.Real.SynthesizedBody)
            .Select(
                item =>
                {
                    var recognitionParts = item.Recognition.Result.Stations
                        .SelectMany(station => station.Segments)
                        .Select(segment => segment.PartId)
                        .ToHashSet();
                    var realDominant = item.Real.Result.Stations
                        .SelectMany(station => station.Segments)
                        .GroupBy(segment => segment.PartId)
                        .Where(group => group.Count() >= Math.Max(1, (int)Math.Ceiling(Math.Max(item.Real.Result.Stations.Count(station => station.Segments.Count > 0), 1) * 0.6)))
                        .OrderByDescending(group => group.Count())
                        .ThenBy(group => group.Key)
                        .Select(group => group.Key)
                        .ToArray();
                    var missingRealDominant = realDominant
                        .Where(partId => !recognitionParts.Contains(partId))
                        .ToArray();
                    return new
                    {
                        item.Real.MemberId,
                        item.Real.AssemblyId,
                        item.Real.ImportSynthesisKind,
                        RealDominantPartIds = realDominant,
                        MissingRealDominantPartIds = missingRealDominant
                    };
                })
            .Where(item => item.MissingRealDominantPartIds.Length > 0)
            .OrderBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (synthesizedDiffs.Length == 0)
        {
            lines.Add("当前没有发现“real_input 主导迹线零件在 recognition_input 中完全缺失”的样本。");
        }
        else
        {
            lines.Add("| MemberId | AssemblyId | ImportSynthesisKind | real_input 主导迹线零件 | recognition_input 缺失的主导迹线零件 |");
            lines.Add("| --- | --- | --- | --- | --- |");
            foreach (var item in synthesizedDiffs)
            {
                lines.Add(
                    $"| {item.MemberId} | {item.AssemblyId} | {item.ImportSynthesisKind ?? string.Empty} | {string.Join("<br>", item.RealDominantPartIds)} | {string.Join("<br>", item.MissingRealDominantPartIds)} |");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## 说明");
        lines.Add(string.Empty);
        lines.Add("- 这一版迹线仍是“按站位投影 OBB 宽向”的最小原型，还不是“实体与截面平面精确求交”。");
        lines.Add("- `section-topology-summary.json` 用于快速看哪些真实零件在多数站位持续出现，可作为后续主体核心证明器的候选证据。");
        lines.Add("- 阶段 5 前，仍需要继续补 `trace cleaning / envelope / closed-loop` 这三步。");
        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildSectionTopologyAnalysisSummaryMarkdown(
        IReadOnlyList<SectionTopologyViewOutput> recognitionInputViews,
        IReadOnlyList<SectionTopologyViewOutput> realInputViews)
    {
        var allViews = recognitionInputViews.Concat(realInputViews).ToArray();
        var lines = new List<string>
        {
            "# 截面拓扑分析摘要",
            string.Empty,
            $" - recognition_input 拓扑结果数: `{recognitionInputViews.Count}`",
            $" - real_input 拓扑结果数: `{realInputViews.Count}`",
            $" - real_input 含闭合候选站位的样本数: `{realInputViews.Count(item => item.Result.Stations.Any(station => station.ClosedLoopCandidate))}`",
            $" - real_input 含内部迹线站位的样本数: `{realInputViews.Count(item => item.Result.Stations.Any(station => station.InternalTraceIds.Count > 0))}`",
            string.Empty,
            "## 代表样本",
            string.Empty
        };

        var representatives = allViews
            .Where(item => item.ViewKind == "real_input")
            .OrderByDescending(item => item.Result.Stations.Count(station => station.ClosedLoopCandidate))
            .ThenByDescending(item => item.Result.Stations.Count(station => station.InternalTraceIds.Count > 0))
            .ThenByDescending(item => item.Result.Stations.Count(station => station.RetainedTraceIds.Count > 0))
            .ThenByDescending(item => item.SynthesizedBody)
            .ThenBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();

        if (representatives.Length == 0)
        {
            lines.Add("当前没有可展示的截面拓扑分析结果。");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("| MemberId | AssemblyId | View | Retained/Stations | Internal Stations | ClosedLoop Stations | MissingBody Stations |");
        lines.Add("| --- | --- | --- | --- | --- | --- | --- |");

        foreach (var item in representatives)
        {
            var retainedStations = item.Result.Stations.Count(station => station.RetainedTraceIds.Count > 0);
            var internalStations = item.Result.Stations.Count(station => station.InternalTraceIds.Count > 0);
            var closedLoopStations = item.Result.Stations.Count(station => station.ClosedLoopCandidate);
            var missingBodyStations = item.Result.Stations.Count(station => station.StationFlags.Contains("BODY_CANDIDATE_MISSING_AFTER_CLEANING"));
            lines.Add(
                $"| {item.MemberId} | {item.AssemblyId} | {item.ViewKind} | {retainedStations}/{item.Result.Stations.Count} | {internalStations} | {closedLoopStations} | {missingBodyStations} |");
        }

        lines.Add(string.Empty);
        lines.Add("## 说明");
        lines.Add(string.Empty);
        lines.Add("- `section-topology-*.json` 是阶段 4.5 的旁路产物，先在原始迹线之上做最小 `trace cleaning + envelope`。");
        lines.Add("- `ClosedLoopCandidate` 目前只表示“站位上出现了四边包络证据”，不是最终箱型判定。");
        lines.Add("- 阶段 5 的主体核心证明器，将优先消费这里的 `retained / internal / envelope` 三类证据。");
        return string.Join(Environment.NewLine, lines);
    }
}
