using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Text.Json;

namespace TeklaBodyBracketRecognition.App;

internal static class CoarseClassificationExcelExporter
{
    private const string CoarseObservationFileName = "coarse-main-class-observation.json";
    private const string NewInputLayerDraftFileName = "new-input-layer-draft.json";
    private const string OutputFileName = "粗分类结果.xlsx";

    public static string Generate(string outputDirectory)
    {
        var coarsePath = Path.Combine(outputDirectory, CoarseObservationFileName);
        var draftPath = Path.Combine(outputDirectory, NewInputLayerDraftFileName);
        var outputPath = Path.Combine(outputDirectory, OutputFileName);

        if (!File.Exists(coarsePath) || !File.Exists(draftPath))
        {
            throw new FileNotFoundException("生成粗分类 Excel 时缺少必要 JSON 输出。");
        }

        using var coarseDocument = JsonDocument.Parse(File.ReadAllText(coarsePath, Encoding.UTF8));
        using var draftDocument = JsonDocument.Parse(File.ReadAllText(draftPath, Encoding.UTF8));

        var boundaryByMember = BuildBoundaryGapByMember(draftDocument.RootElement);
        var representationByMember = BuildRepresentationByMember(draftDocument.RootElement);
        var workbook = new XlsxWorkbook();

        workbook.AddWorksheet(
            "粗分类结果表",
            BuildCoarseResultSheet(coarseDocument.RootElement, boundaryByMember, representationByMember));
        workbook.AddWorksheet(
            "表达等级汇总",
            BuildRepresentationSummarySheet(draftDocument.RootElement));
        workbook.AddWorksheet(
            "边界缺口汇总",
            BuildBoundarySummarySheet(draftDocument.RootElement));
        workbook.AddWorksheet(
            "边界缺口明细",
            BuildBoundaryDetailSheet(draftDocument.RootElement));

        workbook.Save(outputPath);
        return outputPath;
    }

    private static IReadOnlyDictionary<string, BoundaryGapSummary> BuildBoundaryGapByMember(JsonElement root)
    {
        var result = new Dictionary<string, BoundaryGapSummary>(StringComparer.Ordinal);
        foreach (var member in root.GetProperty("Members").EnumerateArray())
        {
            var memberId = member.GetProperty("MemberId").GetString() ?? string.Empty;
            var boundaryGapCount = 0;
            var warningCounts = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var part in member.GetProperty("NormalizedParts").EnumerateArray())
            {
                if (!part.TryGetProperty("NormalizationWarnings", out var warningArray) ||
                    warningArray.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var warning in warningArray.EnumerateArray())
                {
                    var code = warning.GetString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        continue;
                    }

                    if (string.Equals(code, "BOUNDARY_UNRESOLVED", StringComparison.Ordinal))
                    {
                        boundaryGapCount++;
                    }

                    warningCounts[code] = warningCounts.TryGetValue(code, out var count) ? count + 1 : 1;
                }
            }

            var topWarning = warningCounts
                .OrderByDescending(static item => item.Value)
                .ThenBy(static item => item.Key, StringComparer.Ordinal)
                .Select(static item => item.Key)
                .FirstOrDefault() ?? string.Empty;

            result[memberId] = new BoundaryGapSummary(boundaryGapCount, topWarning);
        }

        return result;
    }

    private static XlsxSheetData BuildCoarseResultSheet(
        JsonElement root,
        IReadOnlyDictionary<string, BoundaryGapSummary> boundaryByMember,
        IReadOnlyDictionary<string, RepresentationSummary> representationByMember)
    {
        var headers = new[]
        {
            "构件编号", "AssemblyId", "来源主类", "来源主类中文", "当前粗分类", "当前粗分类中文", "是否与来源一致",
            "置信度", "判定子类", "判定子类中文", "判定原因", "判定原因中文", "BodyFamily", "SectionType",
            "导入合成类型", "主线类型中文", "优先站位数", "有效站位数", "候选零件数", "排除零件数",
            "箱形站位数", "H型站位数", "主板站位数", "闭环站位数", "箱形占比", "H型占比", "主板占比", "闭环占比",
            "精确/近精确数", "近似/退化数", "高风险数", "边界缺口数", "主要缺口类型", "候选零件编号", "来源文件"
        };

        var rows = new List<List<XlsxCell>>
        {
            headers.Select(static item => XlsxCell.Header(item)).ToList()
        };

        foreach (var row in root.GetProperty("Rows").EnumerateArray())
        {
            var memberId = ReadString(row, "MemberId");
            var sourceCode = ReadString(row, "SourceMemberMainClassCode");
            var coarseCode = ReadString(row, "CoarseMainClassCode");
            var same = !string.IsNullOrWhiteSpace(sourceCode) && !string.IsNullOrWhiteSpace(coarseCode)
                ? (string.Equals(sourceCode, coarseCode, StringComparison.Ordinal) ? "是" : "否")
                : string.Empty;

            boundaryByMember.TryGetValue(memberId, out var boundary);
            representationByMember.TryGetValue(memberId, out var representationSummary);
            var rowStyle = string.Equals(same, "否", StringComparison.Ordinal) ? XlsxCellStyle.Highlight : XlsxCellStyle.Normal;

            rows.Add(new List<XlsxCell>
            {
                new(memberId, rowStyle),
                new(ReadString(row, "AssemblyId"), rowStyle),
                new(sourceCode, rowStyle),
                new(ReadString(row, "SourceMemberMainClassLabelZh"), rowStyle),
                new(coarseCode, rowStyle),
                new(ReadString(row, "CoarseMainClassLabelZh"), rowStyle),
                new(same, rowStyle),
                new(ReadNumber(row, "CoarseMainClassConfidence"), rowStyle),
                new(ReadString(row, "CoarseMainClassSubtypeCode"), rowStyle),
                new(ReadString(row, "CoarseMainClassSubtypeLabelZh"), rowStyle),
                new(ReadString(row, "CoarseMainClassReasonCode"), rowStyle),
                new(ReadString(row, "CoarseMainClassReasonLabelZh"), rowStyle),
                new(ReadString(row, "BodyDescriptorFamily"), rowStyle),
                new(ReadString(row, "BodyDescriptorSectionType"), rowStyle),
                new(ReadString(row, "ImportSynthesisKind"), rowStyle),
                new(Fallback(ReadString(row, "LongitudinalTypeLabelZh"), ReadString(row, "LongitudinalTypeCode")), rowStyle),
                new(ReadNumber(row, "PriorityStationCount"), rowStyle),
                new(ReadNumber(row, "EligibleStationCount"), rowStyle),
                new(ReadNumber(row, "CandidatePartCount"), rowStyle),
                new(ReadNumber(row, "ExcludedPartCount"), rowStyle),
                new(ReadNumber(row, "BoxStationCount"), rowStyle),
                new(ReadNumber(row, "HStationCount"), rowStyle),
                new(ReadNumber(row, "PrimaryPlateStationCount"), rowStyle),
                new(ReadNumber(row, "ClosedLoopStationCount"), rowStyle),
                new(ReadNumber(row, "BoxStationRatio"), rowStyle),
                new(ReadNumber(row, "HStationRatio"), rowStyle),
                new(ReadNumber(row, "PrimaryPlateStationRatio"), rowStyle),
                new(ReadNumber(row, "ClosedLoopStationRatio"), rowStyle),
                new(representationSummary?.ExactLikeCount ?? 0, (representationSummary?.ExactLikeCount ?? 0) > 0 ? rowStyle : XlsxCellStyle.Warn),
                new(representationSummary?.ApproximateCount ?? 0, (representationSummary?.ApproximateCount ?? 0) > 0 ? XlsxCellStyle.Warn : rowStyle),
                new(representationSummary?.HighRiskCount ?? 0, (representationSummary?.HighRiskCount ?? 0) > 0 ? XlsxCellStyle.Warn : rowStyle),
                new(boundary?.BoundaryGapCount ?? 0, (boundary?.BoundaryGapCount ?? 0) > 0 ? XlsxCellStyle.Warn : rowStyle),
                new(boundary?.TopWarning ?? string.Empty, (boundary?.BoundaryGapCount ?? 0) > 0 ? XlsxCellStyle.Warn : rowStyle),
                new(ReadString(row, "CandidatePartIds"), rowStyle),
                new(ReadString(row, "SourceFile"), rowStyle)
            });
        }

        return new XlsxSheetData(rows);
    }

    private static XlsxSheetData BuildBoundarySummarySheet(JsonElement root)
    {
        var byPartType = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var member in root.GetProperty("Members").EnumerateArray())
        {
            foreach (var part in member.GetProperty("NormalizedParts").EnumerateArray())
            {
                if (!part.TryGetProperty("NormalizationWarnings", out var warningsElement) ||
                    warningsElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var warnings = warningsElement
                    .EnumerateArray()
                    .Select(static item => item.GetString() ?? string.Empty)
                    .Where(static item => !string.IsNullOrWhiteSpace(item))
                    .ToArray();

                if (!warnings.Contains("BOUNDARY_UNRESOLVED", StringComparer.Ordinal))
                {
                    continue;
                }

                var partType = ReadString(part, "PartType");
                byPartType[partType] = byPartType.TryGetValue(partType, out var count) ? count + 1 : 1;
            }
        }

        var rows = new List<List<XlsxCell>>
        {
            new() { XlsxCell.Header("零件类型"), XlsxCell.Header("边界未解数量") }
        };

        foreach (var item in byPartType.OrderByDescending(static pair => pair.Value).ThenBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            rows.Add(new List<XlsxCell>
            {
                new(item.Key, XlsxCellStyle.Normal),
                new(item.Value, XlsxCellStyle.Warn)
            });
        }

        return new XlsxSheetData(rows);
    }

    private static XlsxSheetData BuildRepresentationSummarySheet(JsonElement root)
    {
        var rows = new List<List<XlsxCell>>
        {
            new()
            {
                XlsxCell.Header("构件编号"),
                XlsxCell.Header("AssemblyId"),
                XlsxCell.Header("零件总数"),
                XlsxCell.Header("精确/近精确"),
                XlsxCell.Header("近似/退化"),
                XlsxCell.Header("高风险"),
                XlsxCell.Header("主要表达类型"),
                XlsxCell.Header("主要风险原因")
            }
        };

        foreach (var member in root.GetProperty("Members").EnumerateArray())
        {
            var memberId = ReadString(member, "MemberId");
            var assemblyId = ReadString(member, "AssemblyId");
            var parts = member.GetProperty("NormalizedParts").EnumerateArray().ToArray();
            var exactLikeCount = 0;
            var approximateCount = 0;
            var highRiskCount = 0;
            var kindCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var riskReasonCounts = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var part in parts)
            {
                var representation = part.TryGetProperty("Representation", out var rep) ? rep : default;
                var level = ReadString(representation, "RepresentationLevel");
                var kind = ReadString(representation, "RepresentationKindLabelZh");
                var risk = ReadString(representation, "DistortionRiskCode");

                if (string.Equals(level, "EXACT", StringComparison.Ordinal) ||
                    string.Equals(level, "NEAR_EXACT", StringComparison.Ordinal))
                {
                    exactLikeCount++;
                }

                if (string.Equals(level, "APPROXIMATE", StringComparison.Ordinal) ||
                    string.Equals(level, "FALLBACK", StringComparison.Ordinal))
                {
                    approximateCount++;
                }

                if (string.Equals(risk, "HIGH", StringComparison.Ordinal))
                {
                    highRiskCount++;
                }

                if (!string.IsNullOrWhiteSpace(kind))
                {
                    kindCounts[kind] = kindCounts.TryGetValue(kind, out var kindCount) ? kindCount + 1 : 1;
                }

                if (representation.ValueKind == JsonValueKind.Object &&
                    representation.TryGetProperty("DistortionRiskReasons", out var reasons) &&
                    reasons.ValueKind == JsonValueKind.Array)
                {
                    foreach (var reason in reasons.EnumerateArray())
                    {
                        var code = reason.GetString() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(code))
                        {
                            continue;
                        }

                        riskReasonCounts[code] = riskReasonCounts.TryGetValue(code, out var reasonCount) ? reasonCount + 1 : 1;
                    }
                }
            }

            var primaryKind = kindCounts.OrderByDescending(static item => item.Value).ThenBy(static item => item.Key, StringComparer.Ordinal).FirstOrDefault().Key ?? string.Empty;
            var primaryRiskReason = riskReasonCounts.OrderByDescending(static item => item.Value).ThenBy(static item => item.Key, StringComparer.Ordinal).FirstOrDefault().Key ?? string.Empty;

            rows.Add(new List<XlsxCell>
            {
                new(memberId, XlsxCellStyle.Normal),
                new(assemblyId, XlsxCellStyle.Normal),
                new(parts.Length, XlsxCellStyle.Normal),
                new(exactLikeCount, XlsxCellStyle.Normal),
                new(approximateCount, approximateCount > 0 ? XlsxCellStyle.Warn : XlsxCellStyle.Normal),
                new(highRiskCount, highRiskCount > 0 ? XlsxCellStyle.Warn : XlsxCellStyle.Normal),
                new(primaryKind, XlsxCellStyle.Normal),
                new(primaryRiskReason, highRiskCount > 0 ? XlsxCellStyle.Warn : XlsxCellStyle.Normal)
            });
        }

        return new XlsxSheetData(rows);
    }

    private static IReadOnlyDictionary<string, RepresentationSummary> BuildRepresentationByMember(JsonElement root)
    {
        var result = new Dictionary<string, RepresentationSummary>(StringComparer.Ordinal);
        foreach (var member in root.GetProperty("Members").EnumerateArray())
        {
            var memberId = ReadString(member, "MemberId");
            var exactLikeCount = 0;
            var approximateCount = 0;
            var highRiskCount = 0;

            foreach (var part in member.GetProperty("NormalizedParts").EnumerateArray())
            {
                if (!part.TryGetProperty("Representation", out var representation))
                {
                    continue;
                }

                var level = ReadString(representation, "RepresentationLevel");
                var risk = ReadString(representation, "DistortionRiskCode");
                if (string.Equals(level, "EXACT", StringComparison.Ordinal) ||
                    string.Equals(level, "NEAR_EXACT", StringComparison.Ordinal))
                {
                    exactLikeCount++;
                }

                if (string.Equals(level, "APPROXIMATE", StringComparison.Ordinal) ||
                    string.Equals(level, "FALLBACK", StringComparison.Ordinal))
                {
                    approximateCount++;
                }

                if (string.Equals(risk, "HIGH", StringComparison.Ordinal))
                {
                    highRiskCount++;
                }
            }

            result[memberId] = new RepresentationSummary(exactLikeCount, approximateCount, highRiskCount);
        }

        return result;
    }

    private static XlsxSheetData BuildBoundaryDetailSheet(JsonElement root)
    {
        var rows = new List<List<XlsxCell>>
        {
            new()
            {
                XlsxCell.Header("构件编号"),
                XlsxCell.Header("AssemblyId"),
                XlsxCell.Header("零件编号"),
                XlsxCell.Header("零件类型"),
                XlsxCell.Header("截面字符串"),
                XlsxCell.Header("警告")
            }
        };

        foreach (var member in root.GetProperty("Members").EnumerateArray())
        {
            var memberId = member.GetProperty("MemberId").GetString() ?? string.Empty;
            var assemblyId = member.GetProperty("AssemblyId").GetString() ?? string.Empty;

            foreach (var part in member.GetProperty("NormalizedParts").EnumerateArray())
            {
                if (!part.TryGetProperty("NormalizationWarnings", out var warningsElement) ||
                    warningsElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var warnings = warningsElement
                    .EnumerateArray()
                    .Select(static item => item.GetString() ?? string.Empty)
                    .Where(static item => !string.IsNullOrWhiteSpace(item))
                    .ToArray();

                if (!warnings.Contains("BOUNDARY_UNRESOLVED", StringComparer.Ordinal))
                {
                    continue;
                }

                rows.Add(new List<XlsxCell>
                {
                    new(memberId, XlsxCellStyle.Normal),
                    new(assemblyId, XlsxCellStyle.Normal),
                    new(ReadNumber(part, "PartId"), XlsxCellStyle.Normal),
                    new(ReadString(part, "PartType"), XlsxCellStyle.Normal),
                    new(ReadString(part, "ProfileString"), XlsxCellStyle.Normal),
                    new(string.Join(",", warnings), XlsxCellStyle.Warn)
                });
            }
        }

        return new XlsxSheetData(rows);
    }

    private static string Fallback(string first, string second) =>
        !string.IsNullOrWhiteSpace(first) ? first : second;

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return string.Empty;
        }

        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString() ?? string.Empty,
            JsonValueKind.Null => string.Empty,
            _ => property.ToString()
        };
    }

    private static double? ReadNumber(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.GetDouble(),
            JsonValueKind.String when double.TryParse(property.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }

    private sealed record BoundaryGapSummary(int BoundaryGapCount, string TopWarning);

    private sealed record RepresentationSummary(int ExactLikeCount, int ApproximateCount, int HighRiskCount);
}

internal sealed class XlsxWorkbook
{
    private readonly List<(string Name, XlsxSheetData Data)> _worksheets = new();

    public void AddWorksheet(string name, XlsxSheetData data) => _worksheets.Add((name, data));

    public void Save(string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        using var stream = File.Create(outputPath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        WriteEntry(archive, "[Content_Types].xml", BuildContentTypes());
        WriteEntry(archive, "_rels/.rels", BuildRootRelationships());
        WriteEntry(archive, "xl/workbook.xml", BuildWorkbook());
        WriteEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationships());
        WriteEntry(archive, "xl/styles.xml", BuildStyles());

        for (var index = 0; index < _worksheets.Count; index++)
        {
            WriteEntry(
                archive,
                $"xl/worksheets/sheet{index + 1}.xml",
                BuildWorksheetXml(_worksheets[index].Data));
        }
    }

    private string BuildContentTypes()
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        builder.Append("""<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">""");
        builder.Append("""<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>""");
        builder.Append("""<Default Extension="xml" ContentType="application/xml"/>""");
        builder.Append("""<Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>""");
        builder.Append("""<Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>""");
        for (var index = 0; index < _worksheets.Count; index++)
        {
            builder.Append($"""<Override PartName="/xl/worksheets/sheet{index + 1}.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>""");
        }

        builder.Append("</Types>");
        return builder.ToString();
    }

    private static string BuildRootRelationships() =>
        """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""";

    private string BuildWorkbook()
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        builder.Append("""<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets>""");
        for (var index = 0; index < _worksheets.Count; index++)
        {
            var name = SecurityElement.Escape(_worksheets[index].Name) ?? string.Empty;
            builder.Append($"""<sheet name="{name}" sheetId="{index + 1}" r:id="rId{index + 1}"/>""");
        }

        builder.Append("</sheets></workbook>");
        return builder.ToString();
    }

    private string BuildWorkbookRelationships()
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        builder.Append("""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">""");
        for (var index = 0; index < _worksheets.Count; index++)
        {
            builder.Append($"""<Relationship Id="rId{index + 1}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet{index + 1}.xml"/>""");
        }

        builder.Append($"""<Relationship Id="rId{_worksheets.Count + 1}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>""");
        builder.Append("</Relationships>");
        return builder.ToString();
    }

    private static string BuildStyles() =>
        """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><color rgb="FFFFFFFF"/><name val="Calibri"/></font></fonts><fills count="4"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FF1F4E78"/><bgColor indexed="64"/></patternFill></fill><fill><patternFill patternType="solid"><fgColor rgb="FFFCE4D6"/><bgColor indexed="64"/></patternFill></fill></fills><borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders><cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="4"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="2" borderId="0" xfId="0" applyFont="1" applyFill="1"/><xf numFmtId="0" fontId="0" fillId="3" borderId="0" xfId="0" applyFill="1"/><xf numFmtId="0" fontId="0" fillId="3" borderId="0" xfId="0" applyFill="1"/></cellXfs></styleSheet>""";

    private static string BuildWorksheetXml(XlsxSheetData sheet)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        builder.Append("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>""");

        for (var rowIndex = 0; rowIndex < sheet.Rows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 1;
            builder.Append($"""<row r="{rowNumber}">""");
            var row = sheet.Rows[rowIndex];

            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                var cellRef = ToCellReference(columnIndex + 1, rowNumber);
                AppendCell(builder, row[columnIndex], cellRef);
            }

            builder.Append("</row>");
        }

        builder.Append("</sheetData></worksheet>");
        return builder.ToString();
    }

    private static void AppendCell(StringBuilder builder, XlsxCell cell, string cellReference)
    {
        var styleIndex = cell.Style switch
        {
            XlsxCellStyle.Header => 1,
            XlsxCellStyle.Highlight => 2,
            XlsxCellStyle.Warn => 3,
            _ => 0
        };

        if (cell.Value is null)
        {
            builder.Append($"""<c r="{cellReference}" s="{styleIndex}"/>""");
            return;
        }

        if (cell.Value is string text)
        {
            var escaped = SecurityElement.Escape(text) ?? string.Empty;
            builder.Append($"""<c r="{cellReference}" s="{styleIndex}" t="inlineStr"><is><t>{escaped}</t></is></c>""");
            return;
        }

        if (cell.Value is bool flag)
        {
            builder.Append($"""<c r="{cellReference}" s="{styleIndex}" t="b"><v>{(flag ? 1 : 0)}</v></c>""");
            return;
        }

        var number = Convert.ToString(cell.Value, CultureInfo.InvariantCulture) ?? string.Empty;
        builder.Append($"""<c r="{cellReference}" s="{styleIndex}"><v>{number}</v></c>""");
    }

    private static string ToCellReference(int columnNumber, int rowNumber)
    {
        var dividend = columnNumber;
        var columnName = string.Empty;
        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName + rowNumber.ToString(CultureInfo.InvariantCulture);
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }
}

internal sealed record XlsxSheetData(List<List<XlsxCell>> Rows);

internal sealed record XlsxCell(object? Value, XlsxCellStyle Style)
{
    public static XlsxCell Header(string value) => new(value, XlsxCellStyle.Header);
}

internal enum XlsxCellStyle
{
    Normal,
    Header,
    Highlight,
    Warn
}
