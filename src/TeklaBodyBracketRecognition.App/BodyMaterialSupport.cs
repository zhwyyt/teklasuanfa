using System.Globalization;
using TeklaBodyBracketRecognition.Core.Domain;

namespace TeklaBodyBracketRecognition.App;

internal static class BodyMaterialSupport
{
    public static IEnumerable<BodyMaterialPartRow> BuildBodyMaterialPartRows(BodyMaterialSummary summary)
    {
        return summary.BodyDescriptorCoreParts.Select(
            corePart => new BodyMaterialPartRow
            {
                SourceFile = summary.SourceFile,
                MemberId = summary.MemberId,
                MemberProfileString = summary.MemberProfileString,
                AssemblyId = summary.AssemblyId,
                InputMainPartId = summary.InputMainPartId,
                SourceMainPartId = summary.SourceMainPartId,
                BodyDescriptorFamily = summary.BodyDescriptorFamily,
                BodyDescriptorSectionType = summary.BodyDescriptorSectionType,
                SynthesizedBody = summary.SynthesizedBody,
                ImportSynthesisKind = summary.ImportSynthesisKind,
                CorePartOrder = corePart.CorePartOrder,
                PartId = corePart.PartId,
                PartName = corePart.PartName,
                ProfileString = corePart.ProfileString,
                Material = corePart.Material,
                RuntimeType = corePart.RuntimeType,
                Thickness = corePart.Thickness,
                SemanticRole = corePart.SemanticRole,
                IsMainPart = corePart.IsMainPart,
                IsSyntheticPart = corePart.IsSyntheticPart
            });
    }

    public static IEnumerable<BodyMaterialSourceSeedRow> BuildBodyMaterialSourceSeedRows(BodyMaterialSummary summary)
    {
        return summary.SourceBodySeedParts.Select(
            seedPart => new BodyMaterialSourceSeedRow
            {
                SourceFile = summary.SourceFile,
                MemberId = summary.MemberId,
                MemberProfileString = summary.MemberProfileString,
                AssemblyId = summary.AssemblyId,
                BodyDescriptorFamily = summary.BodyDescriptorFamily,
                BodyDescriptorSectionType = summary.BodyDescriptorSectionType,
                SynthesizedBody = summary.SynthesizedBody,
                ImportSynthesisKind = summary.ImportSynthesisKind,
                SourceMainPartId = summary.SourceMainPartId,
                SourceMainPartName = summary.SourceMainPartName,
                SourceMainPartProfileString = summary.SourceMainPartProfileString,
                SeedOrder = seedPart.SeedOrder,
                PartId = seedPart.PartId,
                PartName = seedPart.PartName,
                ProfileString = seedPart.ProfileString,
                Material = seedPart.Material,
                Thickness = seedPart.Thickness,
                SemanticRole = seedPart.SemanticRole,
                SemanticRoleScore = seedPart.SemanticRoleScore,
                IsSourceMainPart = seedPart.IsSourceMainPart
            });
    }

    public static BodyMaterialSummary BuildBodyMaterialSummary(
        ImportedAssemblyJob job,
        CoreBodyProofViewOutput proofView)
    {
        var partLookup = job.RealInput.Parts
            .Concat(job.Input.Parts)
            .GroupBy(part => part.PartId)
            .ToDictionary(group => group.Key, group => group.First());
        var corePartIds = proofView.Result.CoreBodyPartIds;
        var reviewPartIds = proofView.Result.ReviewPartIds.ToHashSet();
        var coreParts = new List<CoreBodyPartSummary>(corePartIds.Count);
        for (var index = 0; index < corePartIds.Count; index++)
        {
            var partId = corePartIds[index];
            if (!partLookup.TryGetValue(partId, out var part))
            {
                coreParts.Add(
                    new CoreBodyPartSummary
                    {
                        CorePartOrder = index + 1,
                        PartId = partId,
                        PartName = "<missing>",
                        ProfileString = string.Empty,
                        Material = string.Empty,
                        RuntimeType = "Missing",
                        Thickness = null,
                        SemanticRole = "Missing",
                        IsMainPart = partId == job.Input.MainPartId,
                        IsSyntheticPart = partId < 0
                    });
                continue;
            }

            coreParts.Add(
                new CoreBodyPartSummary
                {
                    CorePartOrder = index + 1,
                    PartId = part.PartId,
                    PartName = string.IsNullOrWhiteSpace(part.Name) ? "<unnamed>" : part.Name,
                    ProfileString = part.ProfileString,
                    Material = part.Material,
                    RuntimeType = part.RuntimeType.ToString(),
                    Thickness = part.Thickness,
                    SemanticRole = part.SemanticRole.ToString(),
                    IsMainPart = part.PartId == job.Input.MainPartId,
                    IsSyntheticPart = part.PartId < 0 || part.Name.StartsWith("Synthetic", StringComparison.Ordinal)
                });
        }

        var bodyDescriptorFamily = "UNRESOLVED";
        var bodyDescriptorSectionType = "UNRESOLVED";
        var bodyDescriptorDerivationType = "CORE_BODY_PROOF_ONLY";

        if (TryResolveSourceStandardSectionSummary(job, out var sourceStandardSectionType))
        {
            bodyDescriptorFamily = "StandardSection";
            bodyDescriptorSectionType = sourceStandardSectionType;
            bodyDescriptorDerivationType = "SOURCE_STANDARD_SECTION";
        }
        else if (!string.IsNullOrWhiteSpace(job.SynthesisKind))
        {
            bodyDescriptorDerivationType = "IMPORT_SYNTHESIS_DESCRIPTOR";
            switch (job.SynthesisKind.Trim().ToUpperInvariant())
            {
                case "H":
                    bodyDescriptorFamily = "BuiltUpH";
                    bodyDescriptorSectionType = corePartIds.Count == 3 ? "BUILTUP_H" : "BUILTUP_H_VARIANT";
                    break;
                case "BOX":
                    bodyDescriptorFamily = "BuiltUpBox";
                    bodyDescriptorSectionType = corePartIds.Count >= 4 ? "BUILTUP_BOX" : "BUILTUP_BOX_VARIANT";
                    break;
                case "T":
                    bodyDescriptorFamily = "BuiltUpT";
                    bodyDescriptorSectionType = "BUILTUP_T";
                    break;
                case "CROSS":
                    bodyDescriptorFamily = "BuiltUpCross";
                    bodyDescriptorSectionType = "BUILTUP_CROSS";
                    break;
            }
        }

        var longitudinalTypeCode = ResolveLongitudinalTypeCode(job);
        var longitudinalTypeLabelZh = ToLongitudinalTypeLabelZh(longitudinalTypeCode);
        var longitudinalSubtypeCode = ResolveLongitudinalSubtypeCode(job, longitudinalTypeCode);
        var longitudinalSubtypeLabelZh = ToLongitudinalSubtypeLabelZh(longitudinalSubtypeCode);
        var bodyDescriptorReviewReasons = proofView.Result.Parts
            .Where(part => reviewPartIds.Contains(part.PartId))
            .SelectMany(part => part.Reasons)
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new BodyMaterialSummary
        {
            SourceFile = job.SourceFile,
            MemberId = job.MemberId,
            MemberProfileString = job.MemberProfileString,
            AssemblyId = job.Input.AssemblyId,
            InputMainPartId = job.Input.MainPartId,
            SourceMainPartId = job.SourceMainPartId,
            SourceMainPartName = job.SourceMainPartName,
            SourceMainPartProfileString = job.SourceMainPartProfileString,
            SynthesizedBody = job.SynthesizedBody,
            ImportSynthesisKind = job.SynthesisKind,
            SourceBodySeedParts = job.SourceBodySeedParts,
            BodyDescriptorFamily = bodyDescriptorFamily,
            BodyDescriptorSectionType = bodyDescriptorSectionType,
            BodyDescriptorDerivationType = bodyDescriptorDerivationType,
            BodyDescriptorConfidence = corePartIds.Count > 0 ? 1.0 : 0.0,
            LongitudinalTypeCode = longitudinalTypeCode,
            LongitudinalTypeLabelZh = longitudinalTypeLabelZh,
            LongitudinalSubtypeCode = longitudinalSubtypeCode,
            LongitudinalSubtypeLabelZh = longitudinalSubtypeLabelZh,
            BodyDescriptorReviewRequired = proofView.Result.ReviewPartIds.Count > 0,
            BodyDescriptorReviewReasons = bodyDescriptorReviewReasons,
            BodyDescriptorCorePartIds = corePartIds.ToArray(),
            BodyDescriptorCoreParts = coreParts,
            DefinitionBodyFamily = null,
            DefinitionSatisfied = null,
            DefinitionFailReasons = null,
            DefinitionCoreBodyPartIds = null,
            DefinitionAccessoryPartIds = null,
            DefinitionStableStations = null,
            DefinitionReviewRequired = null,
            DefinitionReviewReasons = null
        };
    }

    public static BodyMaterialExplanation BuildBodyMaterialExplanation(
        ImportedAssemblyJob job,
        BodyMaterialSummary summary,
        CoreBodyProofViewOutput proofView)
    {
        var importDescriptorMismatch = !string.IsNullOrWhiteSpace(summary.ImportSynthesisKind) &&
                                       !string.Equals(summary.ImportSynthesisKind, summary.BodyDescriptorFamily, StringComparison.OrdinalIgnoreCase) &&
                                       !string.Equals(summary.ImportSynthesisKind, summary.BodyDescriptorSectionType, StringComparison.OrdinalIgnoreCase);

        return new BodyMaterialExplanation
        {
            SourceFile = summary.SourceFile,
            MemberId = summary.MemberId,
            MemberProfileString = summary.MemberProfileString,
            AssemblyId = summary.AssemblyId,
            InputMainPartId = summary.InputMainPartId,
            SourceMainPartId = summary.SourceMainPartId,
            SourceMainPartName = summary.SourceMainPartName,
            SourceMainPartProfileString = summary.SourceMainPartProfileString,
            SynthesizedBody = summary.SynthesizedBody,
            ImportSynthesisKind = summary.ImportSynthesisKind,
            DependsOnImportSynthesis = summary.SynthesizedBody,
            SourceBodySeedPartIds = summary.SourceBodySeedParts.Select(part => part.PartId).ToArray(),
            SourceBodySeedParts = summary.SourceBodySeedParts,
            BodyDescriptorFamily = summary.BodyDescriptorFamily,
            BodyDescriptorSectionType = summary.BodyDescriptorSectionType,
            BodyDescriptorDerivationType = summary.BodyDescriptorDerivationType,
            BodyDescriptorConfidence = summary.BodyDescriptorConfidence,
            LongitudinalTypeCode = summary.LongitudinalTypeCode,
            LongitudinalTypeLabelZh = summary.LongitudinalTypeLabelZh,
            LongitudinalSubtypeCode = summary.LongitudinalSubtypeCode,
            LongitudinalSubtypeLabelZh = summary.LongitudinalSubtypeLabelZh,
            BodyDescriptorReviewRequired = summary.BodyDescriptorReviewRequired,
            BodyDescriptorReviewReasons = summary.BodyDescriptorReviewReasons,
            BodyDescriptorCorePartIds = summary.BodyDescriptorCorePartIds,
            BodyDescriptorCoreParts = summary.BodyDescriptorCoreParts,
            BodyDescriptorAccessoryPartIds = proofView.Result.BodyAccessoryPartIds.ToArray(),
            BodyDescriptorSectionFamilyVotes = new Dictionary<string, double>(),
            ImportDescriptorMismatch = importDescriptorMismatch,
            DefinitionBodyFamily = summary.DefinitionBodyFamily,
            DefinitionSatisfied = summary.DefinitionSatisfied,
            DefinitionFailReasons = summary.DefinitionFailReasons,
            ExplanationNotes = BuildExplanationNotes(job, summary, proofView, importDescriptorMismatch)
        };
    }

    public static string BuildBodyMaterialSummaryCsv(IReadOnlyList<BodyMaterialSummary> summaries)
    {
        var rows = new List<string>(summaries.Count + 1)
        {
            string.Join(
                ",",
                new[]
                {
                    "SourceFile",
                    "MemberId",
                    "MemberProfileString",
                    "AssemblyId",
                    "InputMainPartId",
                    "SourceMainPartId",
                    "SourceMainPartName",
                    "SourceMainPartProfileString",
                    "SynthesizedBody",
                    "ImportSynthesisKind",
                    "BodyDescriptorFamily",
                    "BodyDescriptorSectionType",
                    "BodyDescriptorDerivationType",
                    "BodyDescriptorConfidence",
                    "LongitudinalTypeCode",
                    "LongitudinalTypeLabelZh",
                    "LongitudinalSubtypeCode",
                    "LongitudinalSubtypeLabelZh",
                    "BodyDescriptorReviewRequired",
                    "BodyDescriptorReviewReasons",
                    "DefinitionBodyFamily",
                    "DefinitionSatisfied",
                    "SourceBodySeedPartIds",
                    "SourceBodySeedParts",
                    "BodyDescriptorCorePartCount",
                    "BodyDescriptorCorePartIds",
                    "BodyDescriptorCorePartNames",
                    "BodyDescriptorCorePartProfiles"
                })
        };

        rows.AddRange(
            summaries.Select(
                summary => string.Join(
                    ",",
                    new[]
                    {
                        Csv(summary.SourceFile),
                        Csv(summary.MemberId),
                        Csv(summary.MemberProfileString),
                        Csv(summary.AssemblyId),
                        Csv(summary.InputMainPartId),
                        Csv(summary.SourceMainPartId),
                        Csv(summary.SourceMainPartName),
                        Csv(summary.SourceMainPartProfileString),
                        Csv(summary.SynthesizedBody),
                        Csv(summary.ImportSynthesisKind ?? string.Empty),
                        Csv(summary.BodyDescriptorFamily),
                        Csv(summary.BodyDescriptorSectionType),
                        Csv(summary.BodyDescriptorDerivationType),
                        Csv(summary.BodyDescriptorConfidence),
                        Csv(summary.LongitudinalTypeCode),
                        Csv(summary.LongitudinalTypeLabelZh),
                        Csv(summary.LongitudinalSubtypeCode),
                        Csv(summary.LongitudinalSubtypeLabelZh),
                        Csv(summary.BodyDescriptorReviewRequired),
                        Csv(string.Join("|", summary.BodyDescriptorReviewReasons)),
                        Csv(summary.DefinitionBodyFamily ?? string.Empty),
                        Csv(summary.DefinitionSatisfied),
                        Csv(string.Join("|", summary.SourceBodySeedParts.Select(part => part.PartId))),
                        Csv(string.Join("|", summary.SourceBodySeedParts.Select(part => $"{part.PartId}:{part.PartName}:{part.ProfileString}"))),
                        Csv(summary.BodyDescriptorCoreParts.Count),
                        Csv(string.Join("|", summary.BodyDescriptorCorePartIds)),
                        Csv(string.Join("|", summary.BodyDescriptorCoreParts.Select(part => $"{part.PartId}:{part.PartName}"))),
                        Csv(string.Join("|", summary.BodyDescriptorCoreParts.Select(part => $"{part.PartId}:{part.ProfileString}")))
                    })));

        return string.Join(Environment.NewLine, rows);
    }

    public static string BuildBodyMaterialPartCsv(IReadOnlyList<BodyMaterialPartRow> rows)
    {
        var lines = new List<string>(rows.Count + 1)
        {
            string.Join(
                ",",
                new[]
                {
                    "SourceFile",
                    "MemberId",
                    "MemberProfileString",
                    "AssemblyId",
                    "InputMainPartId",
                    "SourceMainPartId",
                    "BodyDescriptorFamily",
                    "BodyDescriptorSectionType",
                    "SynthesizedBody",
                    "ImportSynthesisKind",
                    "CorePartOrder",
                    "PartId",
                    "PartName",
                    "ProfileString",
                    "Material",
                    "RuntimeType",
                    "Thickness",
                    "SemanticRole",
                    "IsMainPart",
                    "IsSyntheticPart"
                })
        };

        lines.AddRange(
            rows.Select(
                row => string.Join(
                    ",",
                    new[]
                    {
                        Csv(row.SourceFile),
                        Csv(row.MemberId),
                        Csv(row.MemberProfileString),
                        Csv(row.AssemblyId),
                        Csv(row.InputMainPartId),
                        Csv(row.SourceMainPartId),
                        Csv(row.BodyDescriptorFamily),
                        Csv(row.BodyDescriptorSectionType),
                        Csv(row.SynthesizedBody),
                        Csv(row.ImportSynthesisKind ?? string.Empty),
                        Csv(row.CorePartOrder),
                        Csv(row.PartId),
                        Csv(row.PartName),
                        Csv(row.ProfileString),
                        Csv(row.Material),
                        Csv(row.RuntimeType),
                        Csv(row.Thickness),
                        Csv(row.SemanticRole),
                        Csv(row.IsMainPart),
                        Csv(row.IsSyntheticPart)
                    })));

        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildBodyMaterialSourceSeedCsv(IReadOnlyList<BodyMaterialSourceSeedRow> rows)
    {
        var lines = new List<string>(rows.Count + 1)
        {
            string.Join(
                ",",
                new[]
                {
                    "SourceFile",
                    "MemberId",
                    "MemberProfileString",
                    "AssemblyId",
                    "BodyDescriptorFamily",
                    "BodyDescriptorSectionType",
                    "SynthesizedBody",
                    "ImportSynthesisKind",
                    "SourceMainPartId",
                    "SourceMainPartName",
                    "SourceMainPartProfileString",
                    "SeedOrder",
                    "PartId",
                    "PartName",
                    "ProfileString",
                    "Material",
                    "Thickness",
                    "SemanticRole",
                    "SemanticRoleScore",
                    "IsSourceMainPart"
                })
        };

        lines.AddRange(
            rows.Select(
                row => string.Join(
                    ",",
                    new[]
                    {
                        Csv(row.SourceFile),
                        Csv(row.MemberId),
                        Csv(row.MemberProfileString),
                        Csv(row.AssemblyId),
                        Csv(row.BodyDescriptorFamily),
                        Csv(row.BodyDescriptorSectionType),
                        Csv(row.SynthesizedBody),
                        Csv(row.ImportSynthesisKind ?? string.Empty),
                        Csv(row.SourceMainPartId),
                        Csv(row.SourceMainPartName),
                        Csv(row.SourceMainPartProfileString),
                        Csv(row.SeedOrder),
                        Csv(row.PartId),
                        Csv(row.PartName),
                        Csv(row.ProfileString),
                        Csv(row.Material),
                        Csv(row.Thickness),
                        Csv(row.SemanticRole),
                        Csv(row.SemanticRoleScore),
                        Csv(row.IsSourceMainPart)
                    })));

        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildBodyMaterialReviewSummaryMarkdown(IReadOnlyList<BodyMaterialExplanation> explanations)
    {
        var mismatchRows = explanations
            .Where(item => item.ImportDescriptorMismatch || item.BodyDescriptorReviewRequired)
            .OrderBy(item => item.MemberId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var lines = new List<string>
        {
            "# 主材识别复核摘要",
            string.Empty,
            $" - 样本数: `{explanations.Count}`",
            $" - 导入阶段使用虚拟主体重建的样本数: `{explanations.Count(item => item.SynthesizedBody)}`",
            $" - 导入重建类型与当前主体摘要描述不一致的样本数: `{explanations.Count(item => item.ImportDescriptorMismatch)}`",
            $" - 当前阶段 5 proof 已触发复核的样本数: `{explanations.Count(item => item.BodyDescriptorReviewRequired)}`",
            string.Empty,
            "## 重点复核样本",
            string.Empty
        };

        if (mismatchRows.Length == 0)
        {
            lines.Add("当前没有命中“导入解释与主体摘要描述不一致 / 已触发主体 proof 复核”的样本。");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("| MemberId | AssemblyId | SourceMainPartId | ImportSynthesisKind | BodyDescriptorFamily | BodyDescriptorSectionType | SourceBodySeedPartIds | CoreBodyPartIds | ReviewReasons |");
        lines.Add("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");

        foreach (var item in mismatchRows)
        {
            lines.Add(
                $"| {item.MemberId} | {item.AssemblyId} | {item.SourceMainPartId} | {item.ImportSynthesisKind ?? string.Empty} | {item.BodyDescriptorFamily} | {item.BodyDescriptorSectionType} | {string.Join("<br>", item.SourceBodySeedPartIds)} | {string.Join("<br>", item.BodyDescriptorCorePartIds)} | {string.Join("<br>", item.BodyDescriptorReviewReasons)} |");
        }

        lines.Add(string.Empty);
        lines.Add("## 说明");
        lines.Add(string.Empty);
        lines.Add("- `ImportSynthesisKind` 只表示离线导入阶段怎样重建虚拟主体，不是最终工程结论。");
        lines.Add("- `BodyDescriptorFamily / BodyDescriptorSectionType` 来自当前阶段 5 proof 主链与 source semantic 摘要，不再来自旧启发式识别器。");
        lines.Add("- 若核心件里出现负号编号，复核时必须同时看 `SourceBodySeedPartIds`。");
        return string.Join(Environment.NewLine, lines);
    }

    private static string ResolveLongitudinalTypeCode(ImportedAssemblyJob job)
    {
        var sourceMainPart = FindSourceMainPart(job);
        if (sourceMainPart is not null)
        {
            return ResolveLongitudinalTypeCode(sourceMainPart);
        }

        foreach (var seed in job.SourceBodySeedParts)
        {
            var seedPart = job.RealInput.Parts.FirstOrDefault(part => part.PartId == seed.PartId) ??
                           job.Input.Parts.FirstOrDefault(part => part.PartId == seed.PartId);
            if (seedPart is null)
            {
                continue;
            }

            var seedTypeCode = ResolveLongitudinalTypeCode(seedPart);
            if (!string.Equals(seedTypeCode, "STRAIGHT", StringComparison.Ordinal))
            {
                return seedTypeCode;
            }
        }

        return "STRAIGHT";
    }

    private static PartInput? FindSourceMainPart(ImportedAssemblyJob job)
    {
        return job.RealInput.Parts.FirstOrDefault(part => part.PartId == job.SourceMainPartId) ??
               job.Input.Parts.FirstOrDefault(part => part.PartId == job.SourceMainPartId) ??
               job.RealInput.Parts.FirstOrDefault(part => part.PartId == job.Input.MainPartId) ??
               job.Input.Parts.FirstOrDefault(part => part.PartId == job.Input.MainPartId);
    }

    private static string ResolveLongitudinalTypeCode(PartInput part)
    {
        if (part.RuntimeType == RuntimePartType.PolyBeam ||
            part.RuntimeType == RuntimePartType.BentPlate ||
            part.IsSpecialShape)
        {
            return "POLYLINE";
        }

        return "STRAIGHT";
    }

    private static string ToLongitudinalTypeLabelZh(string? code)
    {
        return string.IsNullOrWhiteSpace(code) ? "直线主线" : code.Trim() switch
        {
            "ARC" => "弧线主线",
            "POLYLINE" => "折线主线",
            _ => "直线主线"
        };
    }

    private static string ResolveLongitudinalSubtypeCode(ImportedAssemblyJob job, string longitudinalTypeCode)
    {
        if (!string.Equals(longitudinalTypeCode, "STRAIGHT", StringComparison.Ordinal))
        {
            return "NONE";
        }

        return LooksLikeAxialKinkedStraight(job) ? "AXIAL_KINKED_STRAIGHT" : "GENERAL_STRAIGHT";
    }

    private static bool LooksLikeAxialKinkedStraight(ImportedAssemblyJob job)
    {
        var sourceMainPart = FindSourceMainPart(job);
        if (sourceMainPart is null)
        {
            return false;
        }

        var axis = sourceMainPart.PlateLongDirection.Normalize();
        if (axis.Length <= 1e-9)
        {
            return false;
        }

        var candidateParts = job.SourceBodySeedParts
            .Select(seed => job.RealInput.Parts.FirstOrDefault(part => part.PartId == seed.PartId) ??
                            job.Input.Parts.FirstOrDefault(part => part.PartId == seed.PartId))
            .Where(static part => part is not null)
            .Cast<PartInput>()
            .Where(
                part => string.Equals(part.Name, sourceMainPart.Name, StringComparison.OrdinalIgnoreCase) &&
                        (part.RuntimeType == RuntimePartType.Beam || part.RuntimeType == RuntimePartType.PolyBeam))
            .ToList();

        if (candidateParts.Count < 4)
        {
            return false;
        }

        if (!candidateParts.Any(static part => part.RuntimeType == RuntimePartType.PolyBeam))
        {
            return false;
        }

        var reference = sourceMainPart.Centroid;
        var projected = candidateParts
            .Select(
                part =>
                {
                    var delta = part.Centroid - reference;
                    var station = delta.Dot(axis);
                    var lateral = delta - (axis * station);
                    return new
                    {
                        Station = station,
                        Lateral = lateral
                    };
                })
            .OrderBy(item => item.Station)
            .ToList();

        var stationSpan = projected[^1].Station - projected[0].Station;
        if (stationSpan < 500.0)
        {
            return false;
        }

        var splitIndex = projected.Count / 2;
        var lower = projected.Take(splitIndex).ToList();
        var upper = projected.Skip(splitIndex).ToList();
        if (lower.Count < 2 || upper.Count < 2)
        {
            return false;
        }

        var lowerCenter = AverageVector(lower.Select(item => item.Lateral));
        var upperCenter = AverageVector(upper.Select(item => item.Lateral));
        var lateralOffset = (upperCenter - lowerCenter).Length;
        return lateralOffset >= 80.0;
    }

    private static Vector3 AverageVector(IEnumerable<Vector3> vectors)
    {
        var sum = Vector3.Zero;
        var count = 0;
        foreach (var vector in vectors)
        {
            sum += vector;
            count++;
        }

        return count == 0 ? Vector3.Zero : sum / count;
    }

    private static string ToLongitudinalSubtypeLabelZh(string? code)
    {
        return string.IsNullOrWhiteSpace(code) ? "无长度方向细分类" : code.Trim() switch
        {
            "AXIAL_KINKED_STRAIGHT" => "主轴折向直线",
            "GENERAL_STRAIGHT" => "一般直线主线",
            _ => "无长度方向细分类"
        };
    }

    private static IReadOnlyList<string> BuildExplanationNotes(
        ImportedAssemblyJob job,
        BodyMaterialSummary summary,
        CoreBodyProofViewOutput proofView,
        bool importDescriptorMismatch)
    {
        var notes = new List<string>();

        if (summary.SynthesizedBody)
        {
            notes.Add($"导入阶段按 {summary.ImportSynthesisKind ?? "UNKNOWN"} 重建了虚拟主体。");
        }
        else
        {
            notes.Add("当前样本未启用导入阶段虚拟主体重建。");
        }

        if (summary.SourceBodySeedParts.Count > 0)
        {
            notes.Add(
                "真实主体种子零件为: " +
                string.Join(
                    " | ",
                    summary.SourceBodySeedParts.Select(
                        part => $"{part.PartId}:{part.PartName}:{part.ProfileString}:{part.SemanticRole}")));
        }

        notes.Add(
            $"当前主体摘要来自阶段 5 proof 主链，当前描述为 {summary.BodyDescriptorFamily} / {summary.BodyDescriptorSectionType}。");

        if (TryResolveSourceStandardSectionSummary(job, out var sourceStandardSectionType))
        {
            notes.Add(
                $"源主件 {job.SourceMainPartProfileString} 已明确属于标准截面，当前主体家族口径优先按 StandardSection / {sourceStandardSectionType} 解释；导入阶段的 {summary.ImportSynthesisKind ?? "UNKNOWN"} 板链仅作为 proof 表示。");
        }

        if (importDescriptorMismatch)
        {
            notes.Add(
                $"导入阶段重建类型 {summary.ImportSynthesisKind} 与当前主体摘要描述 {summary.BodyDescriptorFamily} 不一致，需要避免把导入类型误当最终结论。");
        }

        if (summary.BodyDescriptorCoreParts.Any(part => part.IsSyntheticPart))
        {
            notes.Add("当前主体核心件包含虚拟件编号，复核时必须结合真实主体种子零件一起看。");
        }

        if (summary.BodyDescriptorReviewRequired)
        {
            notes.Add("当前阶段 5 proof 结果已触发复核。");
        }

        if (summary.BodyDescriptorReviewReasons.Count > 0)
        {
            notes.Add("主体复核原因: " + string.Join(" | ", summary.BodyDescriptorReviewReasons));
        }

        if (proofView.Result.PriorityStationCount > 0)
        {
            notes.Add(
                $"priority stations = {proofView.Result.PriorityStationCount}，core/accessory/review = {proofView.Result.CoreBodyPartIds.Count}/{proofView.Result.BodyAccessoryPartIds.Count}/{proofView.Result.ReviewPartIds.Count}。");
        }

        return notes;
    }

    private static bool TryResolveSourceStandardSectionSummary(
        ImportedAssemblyJob job,
        out string standardSectionType)
    {
        standardSectionType = string.Empty;

        var profile = NormalizeStandardProfileString(job.SourceMainPartProfileString);
        if (string.IsNullOrWhiteSpace(profile) || profile.StartsWith("PL", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IsAngleProfile(profile))
        {
            standardSectionType = "STANDARD_ANGLE";
            return true;
        }

        if (IsChannelProfile(profile))
        {
            standardSectionType = "STANDARD_CHANNEL";
            return true;
        }

        if (IsIHProfile(profile))
        {
            standardSectionType = "STANDARD_IH";
            return true;
        }

        if (IsTeeProfile(profile))
        {
            standardSectionType = "STANDARD_TEE";
            return true;
        }

        if (IsBoxProfile(profile))
        {
            standardSectionType = "STANDARD_BOX";
            return true;
        }

        if (IsPipeProfile(profile))
        {
            standardSectionType = "STANDARD_PIPE";
            return true;
        }

        if (IsRodProfile(profile))
        {
            standardSectionType = "STANDARD_ROD";
            return true;
        }

        return false;
    }

    private static string NormalizeStandardProfileString(string profileString)
    {
        return string.IsNullOrWhiteSpace(profileString)
            ? string.Empty
            : profileString.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
    }

    private static bool IsAngleProfile(string profile) =>
        profile.StartsWith("L", StringComparison.OrdinalIgnoreCase);

    private static bool IsChannelProfile(string profile) =>
        profile.StartsWith("C", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("U", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("[", StringComparison.OrdinalIgnoreCase);

    private static bool IsIHProfile(string profile) =>
        profile.StartsWith("BH", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("H", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("I", StringComparison.OrdinalIgnoreCase);

    private static bool IsTeeProfile(string profile) =>
        profile.StartsWith("T", StringComparison.OrdinalIgnoreCase);

    private static bool IsBoxProfile(string profile) =>
        profile.StartsWith("BK", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("BOX", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("RHS", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("SHS", StringComparison.OrdinalIgnoreCase);

    private static bool IsPipeProfile(string profile) =>
        profile.StartsWith("PIPE", StringComparison.OrdinalIgnoreCase) ||
        profile.StartsWith("CHS", StringComparison.OrdinalIgnoreCase);

    private static bool IsRodProfile(string profile) =>
        profile.StartsWith("ROD", StringComparison.OrdinalIgnoreCase) ||
        (profile.StartsWith("D", StringComparison.OrdinalIgnoreCase) &&
         profile.Length > 1 &&
         char.IsDigit(profile[1]));

    private static string Csv(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return value switch
        {
            bool boolValue => boolValue ? "true" : "false",
            double doubleValue => doubleValue.ToString("0.###", CultureInfo.InvariantCulture),
            float floatValue => floatValue.ToString("0.###", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        } is var text && (text.Contains(',') || text.Contains('"') || text.Contains('\n') || text.Contains('\r'))
            ? "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
            : text;
    }
}
