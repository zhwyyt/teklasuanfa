using System.Globalization;
using TeklaBodyBracketRecognition.Core.Algorithms;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class CoreBodyProofSummarySupport
{
    public static string BuildCoreBodyProofSummaryMarkdown(
        IReadOnlyList<CoreBodyProofViewOutput> recognitionInputViews,
        IReadOnlyList<CoreBodyProofViewOutput> realInputViews)
    {
        var lines = new List<string>
        {
            "# 主体核心证明 bootstrap 摘要",
            string.Empty,
            $" - recognition_input 证明结果数: `{recognitionInputViews.Count}`",
            $" - real_input 证明结果数: `{realInputViews.Count}`",
            $" - real_input 含核心件候选的样本数: `{realInputViews.Count(item => item.Result.CoreBodyPartIds.Count > 0)}`",
            $" - real_input 仍需复核零件的样本数: `{realInputViews.Count(item => item.Result.ReviewPartIds.Count > 0)}`",
            string.Empty,
            "## 代表样本",
            string.Empty
        };

        var representatives = realInputViews
            .OrderByDescending(item => item.Result.ReviewPartIds.Count)
            .ThenByDescending(item => item.Result.CoreBodyPartIds.Count)
            .ThenByDescending(item => item.Result.BodyAccessoryPartIds.Count)
            .ThenByDescending(item => item.SynthesizedBody)
            .ThenBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();

        if (representatives.Length == 0)
        {
            lines.Add("当前没有可展示的主体核心 bootstrap 结果。");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("| MemberId | AssemblyId | View | CoreBodyPartIds | AccessoryPartIds | ReviewPartIds |");
        lines.Add("| --- | --- | --- | --- | --- | --- |");
        foreach (var item in representatives)
        {
            lines.Add(
                $"| {item.MemberId} | {item.AssemblyId} | {item.ViewKind} | {string.Join("<br>", item.Result.CoreBodyPartIds)} | {string.Join("<br>", item.Result.BodyAccessoryPartIds)} | {string.Join("<br>", item.Result.ReviewPartIds)} |");
        }

        lines.Add(string.Empty);
        lines.Add("## 主件扰动命中样本");
        lines.Add(string.Empty);

        var mainPartImpactRows = realInputViews
            .Select(
                item => new
                {
                    View = item,
                    MainPart = item.Result.Parts.FirstOrDefault(part => part.IsInputMainPart),
                    BodyCandidatePartCount = item.Result.Parts.Count(part => part.PartitionClass == BodyCandidatePartitionClass.BodyCandidate)
                })
            .Where(item => item.MainPart is not null)
            .Where(item => item.MainPart!.ProofClass == CoreBodyProofClass.CoreBodyPart)
            .Where(item => item.BodyCandidatePartCount >= 2 || item.MainPart!.ClosedLoopStationCountBeforeRemoval > 0)
            .OrderByDescending(item => item.MainPart!.RemovalImpactScore)
            .ThenBy(item => item.View.MemberId, StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();

        if (mainPartImpactRows.Length == 0)
        {
            lines.Add("当前没有命中“输入主件因扰动证据进入核心”的样本。");
        }
        else
        {
            lines.Add("| MemberId | AssemblyId | InputMainPartId | RemovalImpactScore | BodyWidthRetentionRatio | PriorityBodyCoverageAfterRemovalRatio | EnvelopeBodyCoverageAfterRemovalRatio | ClosedLoopAfterRemoval |");
            lines.Add("| --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (var row in mainPartImpactRows)
            {
                var mainPart = row.MainPart!;
                lines.Add(
                    $"| {row.View.MemberId} | {row.View.AssemblyId} | {mainPart.PartId} | {mainPart.RemovalImpactScore:0.000} | {mainPart.BodyWidthRetentionRatio:0.000} | {mainPart.PriorityBodyCoverageAfterRemovalRatio:0.000} | {mainPart.EnvelopeBodyCoverageAfterRemovalRatio:0.000} | {mainPart.ClosedLoopStationCountAfterRemoval}/{mainPart.ClosedLoopStationCountBeforeRemoval} |");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## 移除后站位退化样本");
        lines.Add(string.Empty);

        var degradedRows = realInputViews
            .SelectMany(
                item =>
                {
                    var bodyCandidatePartCount = item.Result.Parts.Count(part => part.PartitionClass == BodyCandidatePartitionClass.BodyCandidate);
                    return item.Result.Parts
                        .Where(part =>
                            part.LostClosedLoopStationIds.Count > 0 ||
                            part.LostEnvelopeSupportStationIds.Count > 0 ||
                            part.LostBodyCoverageStationIds.Count > 0 ||
                            part.TopologyRewriteStationIds.Count > 0 ||
                            part.DominantDimensionSwitchStationIds.Count > 0)
                        .Where(part => bodyCandidatePartCount >= 2 || part.LostClosedLoopStationIds.Count > 0 || part.TopologyRewriteStationIds.Count > 0)
                        .Select(part => new { View = item, Part = part });
                })
            .OrderByDescending(item => item.Part.LostClosedLoopStationIds.Count)
            .ThenByDescending(item => item.Part.TopologyRewriteStationIds.Count)
            .ThenByDescending(item => item.Part.LostEnvelopeSupportStationIds.Count)
            .ThenByDescending(item => item.Part.LostBodyCoverageStationIds.Count)
            .ThenByDescending(item => item.Part.RemovalImpactScore)
            .ThenBy(item => item.View.MemberId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Part.PartId)
            .Take(20)
            .ToArray();

        if (degradedRows.Length == 0)
        {
            lines.Add("当前没有检测到明显的“移除后站位退化”样本。");
        }
        else
        {
            lines.Add("| MemberId | AssemblyId | PartId | LostBodyCoverage | LostEnvelopeSupport | LostClosedLoop | TopologyRewrite | DominantSwitch |");
            lines.Add("| --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (var row in degradedRows)
            {
                lines.Add(
                    $"| {row.View.MemberId} | {row.View.AssemblyId} | {row.Part.PartId} | {string.Join("<br>", row.Part.LostBodyCoverageStationIds)} | {string.Join("<br>", row.Part.LostEnvelopeSupportStationIds)} | {string.Join("<br>", row.Part.LostClosedLoopStationIds)} | {string.Join("<br>", row.Part.TopologyRewriteStationIds)} | {string.Join("<br>", row.Part.DominantDimensionSwitchStationIds)} |");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## 主截面改写样本");
        lines.Add(string.Empty);

        var topologyRewriteRows = CollectTopologyRewriteSummaryRows(realInputViews);
        var topologyMemberRows = BuildTopologyRewriteMemberAggregateRows(topologyRewriteRows, 20);
        var topologyRows = topologyRewriteRows
            .OrderByDescending(item => item.Part.TopologyRewriteStationIds.Count)
            .ThenByDescending(item => item.Part.LostClosedLoopStationIds.Count)
            .ThenByDescending(item => item.Part.LostEnvelopeSupportStationIds.Count)
            .ThenByDescending(item => item.Part.LostBodyCoverageStationIds.Count)
            .ThenByDescending(item => item.Part.RemovalImpactScore)
            .ThenBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Part.PartId)
            .Take(20)
            .ToArray();

        if (topologyRows.Length == 0)
        {
            lines.Add("当前没有命中“工程意义门槛后的主截面改写”样本。");
        }
        else
        {
            lines.Add("### 构件级聚合");
            lines.Add(string.Empty);
            lines.Add("| MemberId | AssemblyId | PartCount | TopologyCount | 主条款 | 主条款占比 | 条款混合度 | 主条款效果 | 主条款效果占比 | 条款效果混合度 | DefinitionClauses | DefinitionClauseEffects | Patterns |");
            lines.Add("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (var row in topologyMemberRows)
            {
                lines.Add(
                    $"| {row.MemberId} | {row.AssemblyId} | {row.PartCount} | {row.TotalTopologyCount} | {row.LeadDefinitionClause} | {row.LeadDefinitionClauseShare} | {row.ClauseMixStatus} | {row.LeadDefinitionClauseEffect} | {row.LeadDefinitionClauseEffectShare} | {row.ClauseEffectMixStatus} | {row.DefinitionClauses} | {row.DefinitionClauseEffects} | {row.Patterns} |");
            }

            lines.Add(string.Empty);
            lines.Add("### 代表零件");
            lines.Add(string.Empty);
            lines.Add("| MemberId | AssemblyId | PartId | Pattern | ControllerRole | ShapeRole | FamilyRisk | ProofType | FamilyProofTarget | DefinitionClause | DefinitionClauseEffect | CohortParts | TopologyRewrite | SpanYRewrite | SpanZRewrite | ControllerSwitch | LostClosedLoop |");
            lines.Add("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (var row in topologyRows)
            {
                lines.Add(
                    $"| {row.MemberId} | {row.AssemblyId} | {row.Part.PartId} | {row.Part.TopologyRewritePatternLabelZh} | {row.Part.TopologyRewriteControllerRoleLabelZh} | {row.Part.TopologyRewriteShapeRoleLabelZh} | {row.Part.TopologyRewriteFamilyRiskLabelZh} | {row.Part.TopologyRewriteProofTypeLabelZh} | {row.Part.TopologyRewriteFamilyProofTargetLabelZh} | {row.Part.TopologyRewriteDefinitionClauseLabelZh} | {row.Part.TopologyRewriteDefinitionClauseEffectLabelZh} | {string.Join("<br>", row.Part.TopologyRewriteCohortPartIds)} | {string.Join("<br>", row.Part.TopologyRewriteStationIds)} | {string.Join("<br>", row.Part.TopologyRewriteSpanYStationIds)} | {string.Join("<br>", row.Part.TopologyRewriteSpanZStationIds)} | {string.Join("<br>", row.Part.TopologyRewriteEnvelopeControllerSwitchStationIds)} | {string.Join("<br>", row.Part.LostClosedLoopStationIds)} |");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## 说明");
        lines.Add(string.Empty);
        lines.Add("- 这一版仍是阶段 5 的 `bootstrap+remove-and-recompute` 过渡版，还不是最终的定义驱动主体证明器。");
        lines.Add("- 当前核心件判据已经开始消费四类证据：`priority station 持续性`、`外包络支撑`、`移除后重算的站位退化`、`移除后的扰动指标`。");
        lines.Add("- 当前的 `remove-and-recompute` 已能回答：去掉某零件后，哪些优先站位会失去 `body coverage / envelope support / closed-loop`，以及哪些站位会出现“工程意义门槛后的主截面改写”。");
        lines.Add("- 当前 `TopologyRewrite` 已经过一轮收紧：只有基线站位本身具备 `closed-loop` 或至少 `3` 条主体控制迹线时，才会把“包络主导关系/关键跨度改写”记成主截面改写。");
        lines.Add("- 当前还新增了 `ControllerRole`，用来区分“被删零件本身就是主包络控制件”还是“只是把同组控制件切成了主导”；这更接近工程上的主截面证明风险。");
        lines.Add("- 当前还新增了 `ShapeRole`，用来区分“单主板改写 / 成对主体板改写 / 多主体板簇改写”；这一步还不是家族标签，但已经更接近工程上的主体角色解释。");
        lines.Add("- 当前还新增了 `FamilyRisk`，把 `Pattern + ControllerRole + ShapeRole` 压成更接近工程语义的“家族证明风险”，但仍不是最终家族标签。");
        lines.Add("- 当前还新增了 `ProofType`，进一步回答“更像哪一种主轮廓/主截面证明对象被改写”；它比 `FamilyRisk` 更接近工程定义，但仍不是最终家族标签。");
        lines.Add("- 当前还新增了 `FamilyProofTarget`，进一步回答“下一步最该拿哪一类家族定义去验证这条改写证据”；它仍不是最终家族标签。");
        lines.Add("- 当前还新增了 `DefinitionClause`，进一步把改写证据压到“更像哪一条定义条款正在被触发/破坏”的提示层；它仍不是最终家族标签。");
        lines.Add("- 当前还新增了 `DefinitionClauseEffect`，进一步回答“移除此件更像会破坏哪条条款、触发哪类重分配，还是仅停留在复核提示”；它比 `DefinitionClause` 更接近条款满足/破坏语义，但仍不是最终家族标签。");
        lines.Add("- 当前还新增了 `主条款 / 主条款占比 / 条款混合度`，用来区分“单条款已落稳”与“多条款/未定零件仍混在一起”的构件级样本。");
        lines.Add("- 当前还新增了 `主条款效果 / 主条款效果占比 / 条款效果混合度`，用来区分“单一条款效果已落稳”与“多种破坏/复核效果仍混在一起”的构件级样本。");
        lines.Add("- 后续真正进入阶段 5 完整版时，还需要继续把 `TopologyRewrite + ControllerRole + ShapeRole + FamilyRisk + ProofType + FamilyProofTarget + DefinitionClause + DefinitionClauseEffect` 从“控制结构变化”收紧到更接近“家族证明条款会被满足 / 破坏”的级别。");
        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildTopologyRewriteSummaryMarkdown(
        IReadOnlyList<CoreBodyProofViewOutput> realInputViews)
    {
        var lines = new List<string>
        {
            "# Topology Rewrite 摘要",
            string.Empty
        };

        var rewriteRows = CollectTopologyRewriteSummaryRows(realInputViews);

        lines.Add($"- 命中 `TopologyRewrite` 的零件数: `{rewriteRows.Length}`");
        lines.Add($"- 命中样本数: `{rewriteRows.Select(item => item.AssemblyId).Distinct(StringComparer.Ordinal).Count()}`");
        lines.Add(string.Empty);

        var memberRows = BuildTopologyRewriteMemberAggregateRows(rewriteRows, 20);

        lines.Add("## 构件级聚合");
        lines.Add(string.Empty);
        if (memberRows.Length == 0)
        {
            lines.Add("当前没有命中 `TopologyRewrite` 的样本。");
        }
        else
        {
            lines.Add("| MemberId | AssemblyId | PartCount | TopologyCount | SpanYCount | SpanZCount | ControllerSwitchCount | ControllerRoles | ShapeRoles | FamilyRisks | ProofTypes | FamilyProofTargets | DefinitionClauses | DefinitionClauseEffects | Patterns | 主条款 | 主条款占比 | 条款混合度 | 主条款效果 | 主条款效果占比 | 条款效果混合度 |");
            lines.Add("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (var row in memberRows)
            {
                lines.Add(
                    $"| {row.MemberId} | {row.AssemblyId} | {row.PartCount} | {row.TotalTopologyCount} | {row.TotalSpanYCount} | {row.TotalSpanZCount} | {row.TotalControllerSwitchCount} | {row.ControllerRoles} | {row.ShapeRoles} | {row.FamilyRisks} | {row.ProofTypes} | {row.FamilyProofTargets} | {row.DefinitionClauses} | {row.DefinitionClauseEffects} | {row.Patterns} | {row.LeadDefinitionClause} | {row.LeadDefinitionClauseShare} | {row.ClauseMixStatus} | {row.LeadDefinitionClauseEffect} | {row.LeadDefinitionClauseEffectShare} | {row.ClauseEffectMixStatus} |");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## 代表零件");
        lines.Add(string.Empty);

        var topPartRows = rewriteRows
            .OrderByDescending(item => item.Part.TopologyRewriteStationIds.Count)
            .ThenByDescending(item => item.Part.TopologyRewriteSpanYStationIds.Count)
            .ThenByDescending(item => item.Part.TopologyRewriteEnvelopeControllerSwitchStationIds.Count)
            .ThenBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Part.PartId)
            .Take(20)
            .ToArray();

        if (topPartRows.Length == 0)
        {
            lines.Add("当前没有可展示的代表零件。");
        }
        else
        {
            lines.Add("| MemberId | AssemblyId | PartId | Pattern | ControllerRole | ShapeRole | FamilyRisk | ProofType | FamilyProofTarget | DefinitionClause | DefinitionClauseEffect | CohortParts | TopologyRewrite | SpanYRewrite | SpanZRewrite | ControllerSwitch | LostClosedLoop |");
            lines.Add("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (var row in topPartRows)
            {
                lines.Add(
                    $"| {row.MemberId} | {row.AssemblyId} | {row.Part.PartId} | {row.Part.TopologyRewritePatternLabelZh} | {row.Part.TopologyRewriteControllerRoleLabelZh} | {row.Part.TopologyRewriteShapeRoleLabelZh} | {row.Part.TopologyRewriteFamilyRiskLabelZh} | {row.Part.TopologyRewriteProofTypeLabelZh} | {row.Part.TopologyRewriteFamilyProofTargetLabelZh} | {row.Part.TopologyRewriteDefinitionClauseLabelZh} | {row.Part.TopologyRewriteDefinitionClauseEffectLabelZh} | {string.Join("<br>", row.Part.TopologyRewriteCohortPartIds)} | {string.Join("<br>", row.Part.TopologyRewriteStationIds)} | {string.Join("<br>", row.Part.TopologyRewriteSpanYStationIds)} | {string.Join("<br>", row.Part.TopologyRewriteSpanZStationIds)} | {string.Join("<br>", row.Part.TopologyRewriteEnvelopeControllerSwitchStationIds)} | {string.Join("<br>", row.Part.LostClosedLoopStationIds)} |");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## 说明");
        lines.Add(string.Empty);
        lines.Add("- 这份摘要只看阶段 5 当前已命中的 `TopologyRewrite`，不含纯 `LostClosedLoop / LostBodyCoverage / LostEnvelopeSupport` 样本。");
        lines.Add("- 当前 `SpanYRewrite / SpanZRewrite / ControllerSwitch` 是结构化原因拆分，`ControllerRole` 则回答“被删零件本身是不是主包络控制件”。");
        lines.Add("- 当前 `ShapeRole` 进一步回答“这个改写更像单主板、成对主体板，还是多主体板簇”。");
        lines.Add("- 当前 `FamilyRisk` 进一步把这些证据压成“更像哪一类主截面证明风险”，但仍不等于最终家族判定。");
        lines.Add("- 当前 `ProofType` 进一步回答“更像哪一类主轮廓/主截面证明对象被改写”，比 `FamilyRisk` 更接近工程语义。");
        lines.Add("- 当前 `FamilyProofTarget` 进一步回答“下一步最该拿哪一类家族定义去验证这条改写证据”，但仍不等于最终家族标签。");
        lines.Add("- 当前 `DefinitionClause` 进一步回答“更像哪一条定义条款正在被触发 / 破坏”，这是进入阶段 6 前最直接的定义级提示。");
        lines.Add("- 当前 `DefinitionClauseEffect` 进一步回答“移除此件更像会破坏哪条条款、触发哪类重分配，还是只停留在复核提示”。");
        lines.Add("- 当前 `主条款 / 主条款占比 / 条款混合度` 会把构件级样本继续拆成“单条款稳定 / 同条款多形态混合 / 主条款占优但仍混合 / 多条款并列”几类。");
        lines.Add("- 当前 `主条款效果 / 主条款效果占比 / 条款效果混合度` 会把构件级样本继续拆成“单条款效果稳定 / 同条款效果多形态混合 / 主条款效果占优但仍混合 / 多条款效果并列”几类。");
        lines.Add("- 这仍不等于最终家族判定，但已经比单纯模式标签更接近“主截面证明是否被这块板直接改写”。");
        return string.Join(Environment.NewLine, lines);
    }

    private sealed record TopologyRewriteSummaryRow
    {
        public required string MemberId { get; init; }
        public required string AssemblyId { get; init; }
        public required CoreBodyProofPartResult Part { get; init; }
    }

    private sealed record TopologyRewriteMemberAggregateRow
    {
        public required string MemberId { get; init; }
        public required string AssemblyId { get; init; }
        public required int PartCount { get; init; }
        public required int TotalTopologyCount { get; init; }
        public required int TotalSpanYCount { get; init; }
        public required int TotalSpanZCount { get; init; }
        public required int TotalControllerSwitchCount { get; init; }
        public required string ControllerRoles { get; init; }
        public required string ShapeRoles { get; init; }
        public required string FamilyRisks { get; init; }
        public required string ProofTypes { get; init; }
        public required string FamilyProofTargets { get; init; }
        public required string DefinitionClauses { get; init; }
        public required string DefinitionClauseEffects { get; init; }
        public required string Patterns { get; init; }
        public required string LeadDefinitionClause { get; init; }
        public required string LeadDefinitionClauseShare { get; init; }
        public required string ClauseMixStatus { get; init; }
        public required string LeadDefinitionClauseEffect { get; init; }
        public required string LeadDefinitionClauseEffectShare { get; init; }
        public required string ClauseEffectMixStatus { get; init; }
    }

    private sealed record TopologyRewriteClauseMixSummary
    {
        public required string LeadDefinitionClause { get; init; }
        public required string LeadDefinitionClauseShare { get; init; }
        public required string ClauseMixStatus { get; init; }
    }

    private sealed record TopologyRewriteClauseEffectMixSummary
    {
        public required string LeadDefinitionClauseEffect { get; init; }
        public required string LeadDefinitionClauseEffectShare { get; init; }
        public required string ClauseEffectMixStatus { get; init; }
    }

    private static TopologyRewriteSummaryRow[] CollectTopologyRewriteSummaryRows(
        IReadOnlyList<CoreBodyProofViewOutput> realInputViews)
    {
        return realInputViews
            .SelectMany(
                view => view.Result.Parts
                    .Where(part => part.TopologyRewriteStationIds.Count > 0)
                    .Select(
                        part => new TopologyRewriteSummaryRow
                        {
                            MemberId = view.MemberId,
                            AssemblyId = view.AssemblyId,
                            Part = part
                        }))
            .ToArray();
    }

    private static TopologyRewriteMemberAggregateRow[] BuildTopologyRewriteMemberAggregateRows(
        IReadOnlyList<TopologyRewriteSummaryRow> rewriteRows,
        int takeCount)
    {
        return rewriteRows
            .GroupBy(item => new { item.MemberId, item.AssemblyId })
            .Select(
                group =>
                {
                    var groupRows = group.ToArray();
                    var clauseSummary = BuildTopologyRewriteClauseMixSummary(groupRows);
                    var clauseEffectSummary = BuildTopologyRewriteClauseEffectMixSummary(groupRows);
                    return new TopologyRewriteMemberAggregateRow
                    {
                        MemberId = group.Key.MemberId,
                        AssemblyId = group.Key.AssemblyId,
                        PartCount = groupRows.Length,
                        TotalTopologyCount = groupRows.Sum(item => item.Part.TopologyRewriteStationIds.Count),
                        TotalSpanYCount = groupRows.Sum(item => item.Part.TopologyRewriteSpanYStationIds.Count),
                        TotalSpanZCount = groupRows.Sum(item => item.Part.TopologyRewriteSpanZStationIds.Count),
                        TotalControllerSwitchCount = groupRows.Sum(item => item.Part.TopologyRewriteEnvelopeControllerSwitchStationIds.Count),
                        ControllerRoles = JoinDistinctLabels(groupRows.Select(item => item.Part.TopologyRewriteControllerRoleLabelZh)),
                        ShapeRoles = JoinDistinctLabels(groupRows.Select(item => item.Part.TopologyRewriteShapeRoleLabelZh)),
                        FamilyRisks = JoinDistinctLabels(groupRows.Select(item => item.Part.TopologyRewriteFamilyRiskLabelZh)),
                        ProofTypes = JoinDistinctLabels(groupRows.Select(item => item.Part.TopologyRewriteProofTypeLabelZh)),
                        FamilyProofTargets = JoinDistinctLabels(groupRows.Select(item => item.Part.TopologyRewriteFamilyProofTargetLabelZh)),
                        DefinitionClauses = JoinDistinctLabels(groupRows.Select(item => item.Part.TopologyRewriteDefinitionClauseLabelZh)),
                        DefinitionClauseEffects = JoinDistinctLabels(groupRows.Select(item => item.Part.TopologyRewriteDefinitionClauseEffectLabelZh)),
                        Patterns = JoinDistinctLabels(groupRows.Select(item => item.Part.TopologyRewritePatternLabelZh)),
                        LeadDefinitionClause = clauseSummary.LeadDefinitionClause,
                        LeadDefinitionClauseShare = clauseSummary.LeadDefinitionClauseShare,
                        ClauseMixStatus = clauseSummary.ClauseMixStatus,
                        LeadDefinitionClauseEffect = clauseEffectSummary.LeadDefinitionClauseEffect,
                        LeadDefinitionClauseEffectShare = clauseEffectSummary.LeadDefinitionClauseEffectShare,
                        ClauseEffectMixStatus = clauseEffectSummary.ClauseEffectMixStatus
                    };
                })
            .OrderByDescending(item => item.TotalTopologyCount)
            .ThenBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .Take(takeCount)
            .ToArray();
    }

    private static TopologyRewriteClauseMixSummary BuildTopologyRewriteClauseMixSummary(
        IReadOnlyList<TopologyRewriteSummaryRow> groupRows)
    {
        var totalTopologyCount = groupRows.Sum(item => item.Part.TopologyRewriteStationIds.Count);
        var nonNoneClauseRows = groupRows
            .Where(item => !string.Equals(item.Part.TopologyRewriteDefinitionClauseCode, "NONE", StringComparison.Ordinal))
            .ToArray();
        if (nonNoneClauseRows.Length == 0)
        {
            return new TopologyRewriteClauseMixSummary
            {
                LeadDefinitionClause = "无主条款",
                LeadDefinitionClauseShare = $"0/{totalTopologyCount} (0%)",
                ClauseMixStatus = "无条款提示"
            };
        }

        var leadClause = nonNoneClauseRows
            .GroupBy(
                item => new
                {
                    item.Part.TopologyRewriteDefinitionClauseCode,
                    item.Part.TopologyRewriteDefinitionClauseLabelZh
                })
            .Select(
                group => new
                {
                    group.Key.TopologyRewriteDefinitionClauseCode,
                    group.Key.TopologyRewriteDefinitionClauseLabelZh,
                    Weight = group.Sum(item => item.Part.TopologyRewriteStationIds.Count)
                })
            .OrderByDescending(item => item.Weight)
            .ThenBy(item => item.TopologyRewriteDefinitionClauseCode, StringComparer.Ordinal)
            .First();

        var leadShare = totalTopologyCount <= 0
            ? 0.0
            : leadClause.Weight / (double)totalTopologyCount;
        var hasNoClauseRows = nonNoneClauseRows.Length != groupRows.Count;
        var nonNoneClauseCount = nonNoneClauseRows
            .Select(item => item.Part.TopologyRewriteDefinitionClauseCode)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var nonNoneTargetCount = nonNoneClauseRows
            .Select(item => item.Part.TopologyRewriteFamilyProofTargetCode)
            .Where(code => !string.Equals(code, "NONE", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var nonNoneShapeRoleCount = nonNoneClauseRows
            .Select(item => item.Part.TopologyRewriteShapeRoleCode)
            .Where(code => !string.Equals(code, "NONE", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var nonNonePatternCount = nonNoneClauseRows
            .Select(item => item.Part.TopologyRewritePatternCode)
            .Where(code => !string.Equals(code, "NONE", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var nonNoneControllerRoleCount = nonNoneClauseRows
            .Select(item => item.Part.TopologyRewriteControllerRoleCode)
            .Where(code => !string.Equals(code, "NONE", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Count();

        var clauseMixStatus = hasNoClauseRows
            ? "主条款占优，但仍有未定零件"
            : nonNoneClauseCount == 1 &&
              nonNoneTargetCount <= 1 &&
              nonNoneShapeRoleCount <= 1 &&
              nonNonePatternCount <= 1 &&
              nonNoneControllerRoleCount <= 1
                ? "单条款稳定"
                : nonNoneClauseCount == 1
                    ? "同条款多形态混合"
                    : leadShare >= 0.6
                        ? "主条款占优，但仍属混合改写"
                        : "多条款并列，暂不直判";

        return new TopologyRewriteClauseMixSummary
        {
            LeadDefinitionClause = leadClause.TopologyRewriteDefinitionClauseLabelZh,
            LeadDefinitionClauseShare = $"{leadClause.Weight}/{totalTopologyCount} ({leadShare.ToString("0%", CultureInfo.InvariantCulture)})",
            ClauseMixStatus = clauseMixStatus
        };
    }

    private static TopologyRewriteClauseEffectMixSummary BuildTopologyRewriteClauseEffectMixSummary(
        IReadOnlyList<TopologyRewriteSummaryRow> groupRows)
    {
        var totalTopologyCount = groupRows.Sum(item => item.Part.TopologyRewriteStationIds.Count);
        var nonNoneEffectRows = groupRows
            .Where(item => !string.Equals(item.Part.TopologyRewriteDefinitionClauseEffectCode, "NONE", StringComparison.Ordinal))
            .ToArray();
        if (nonNoneEffectRows.Length == 0)
        {
            return new TopologyRewriteClauseEffectMixSummary
            {
                LeadDefinitionClauseEffect = "无主条款效果",
                LeadDefinitionClauseEffectShare = $"0/{totalTopologyCount} (0%)",
                ClauseEffectMixStatus = "无条款效果提示"
            };
        }

        var leadEffect = nonNoneEffectRows
            .GroupBy(
                item => new
                {
                    item.Part.TopologyRewriteDefinitionClauseEffectCode,
                    item.Part.TopologyRewriteDefinitionClauseEffectLabelZh
                })
            .Select(
                group => new
                {
                    group.Key.TopologyRewriteDefinitionClauseEffectCode,
                    group.Key.TopologyRewriteDefinitionClauseEffectLabelZh,
                    Weight = group.Sum(item => item.Part.TopologyRewriteStationIds.Count)
                })
            .OrderByDescending(item => item.Weight)
            .ThenBy(item => item.TopologyRewriteDefinitionClauseEffectCode, StringComparer.Ordinal)
            .First();

        var leadShare = totalTopologyCount <= 0
            ? 0.0
            : leadEffect.Weight / (double)totalTopologyCount;
        var hasNoEffectRows = nonNoneEffectRows.Length != groupRows.Count;
        var nonNoneEffectCount = nonNoneEffectRows
            .Select(item => item.Part.TopologyRewriteDefinitionClauseEffectCode)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var nonNoneClauseCount = nonNoneEffectRows
            .Select(item => item.Part.TopologyRewriteDefinitionClauseCode)
            .Where(code => !string.Equals(code, "NONE", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Count();

        var effectMixStatus = hasNoEffectRows
            ? "主条款效果占优，但仍有未定零件"
            : nonNoneEffectCount == 1 && nonNoneClauseCount <= 1
                ? "单条款效果稳定"
                : nonNoneEffectCount == 1
                    ? "同条款效果多形态混合"
                    : leadShare >= 0.6
                        ? "主条款效果占优，但仍属混合改写"
                        : "多条款效果并列，暂不直判";

        return new TopologyRewriteClauseEffectMixSummary
        {
            LeadDefinitionClauseEffect = leadEffect.TopologyRewriteDefinitionClauseEffectLabelZh,
            LeadDefinitionClauseEffectShare = $"{leadEffect.Weight}/{totalTopologyCount} ({leadShare.ToString("0%", CultureInfo.InvariantCulture)})",
            ClauseEffectMixStatus = effectMixStatus
        };
    }

    private static string JoinDistinctLabels(IEnumerable<string> values)
    {
        return string.Join(
            " / ",
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal));
    }
}
