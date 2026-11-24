using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Backend for <c>POST /excel</c>. Reads, queries, validates, and conservatively edits .xlsx files.
    /// </summary>
    internal static class UnityCliExcelService
    {
        static readonly XNamespace SpreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        static readonly XNamespace RelationshipsNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        static readonly XNamespace PackageRelationshipsNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static (ExcelData data, string message) Execute(UnityCliBridgeRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Empty request.");
            }

            var action = (request.action ?? string.Empty).Trim().ToLowerInvariant();
            switch (action)
            {
                case "list":     return DoList(request);
                case "schema":   return DoSchema(request);
                case "row":      return DoRow(request);
                case "cell":     return DoCell(request);
                case "query":    return DoQuery(request);
                case "set":      return DoSet(request);
                case "insert":   return DoInsert(request);
                case "delete":   return DoDelete(request);
                case "validate": return DoValidate(request);
                default:
                    throw new InvalidOperationException(
                        "excel requires --action list|schema|row|cell|query|set|insert|delete|validate " +
                        "(got: \"" + (request.action ?? string.Empty) + "\").");
            }
        }

        static (ExcelData, string) DoList(UnityCliBridgeRequest request)
        {
            var roots = ResolveSearchRoots(request.excelRoot);
            var entries = new List<ExcelTableEntry>();
            foreach (var root in roots)
            {
                if (!Directory.Exists(root)) continue;
                foreach (var path in Directory.GetFiles(root, "*.xlsx", SearchOption.AllDirectories)
                             .Where(path => !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal)))
                {
                    if (entries.Count >= Math.Max(1, request.count == 0 ? 100 : request.count)) break;
                    entries.Add(new ExcelTableEntry
                    {
                        name = Path.GetFileNameWithoutExtension(path),
                        filePath = ToProjectRelative(path),
                        sheet = string.Empty
                    });
                }
            }

            var data = BaseData("list", string.Empty, string.Empty, 0, request);
            data.tables = entries.ToArray();
            data.totalMatched = entries.Count;
            data.returnedCount = entries.Count;
            return (data, "excel list: " + entries.Count + " table file(s)");
        }

        static (ExcelData, string) DoSchema(UnityCliBridgeRequest request)
        {
            var workbook = LoadWorkbook(request);
            var table = BuildTable(workbook, request);
            var data = BaseData("schema", workbook.FilePath, workbook.SheetName, table.HeaderRow, request);
            data.fields = table.Fields;
            data.totalMatched = table.Fields.Length;
            data.returnedCount = table.Fields.Length;
            return (data, "excel schema: " + table.Fields.Length + " field(s)");
        }

        static (ExcelData, string) DoRow(UnityCliBridgeRequest request)
        {
            var workbook = LoadWorkbook(request);
            var table = BuildTable(workbook, request);
            var rows = SelectRows(table, request).Take(1).ToArray();
            var data = BaseData("row", workbook.FilePath, workbook.SheetName, table.HeaderRow, request);
            data.fields = table.Fields;
            data.rows = rows.Select(row => BuildRowEntry(table, row)).ToArray();
            data.totalMatched = rows.Length;
            data.returnedCount = data.rows.Length;
            return (data, "excel row: " + data.returnedCount + " row(s)");
        }

        static (ExcelData, string) DoCell(UnityCliBridgeRequest request)
        {
            var workbook = LoadWorkbook(request);
            var table = BuildTable(workbook, request);
            var row = SelectRows(table, request).FirstOrDefault();
            if (row == null)
            {
                throw new InvalidOperationException("No row matched the supplied selector.");
            }

            var field = ResolveField(table, request);
            var data = BaseData("cell", workbook.FilePath, workbook.SheetName, table.HeaderRow, request);
            data.fields = table.Fields;
            data.cell = BuildCellEntry(field, row.ExcelRow, row.Get(field.index));
            data.totalMatched = 1;
            data.returnedCount = 1;
            return (data, "excel cell: " + data.cell.address);
        }

        static (ExcelData, string) DoQuery(UnityCliBridgeRequest request)
        {
            var workbook = LoadWorkbook(request);
            var table = BuildTable(workbook, request);
            var matched = FilterRows(table, request.excelWhere).ToArray();
            var limit = request.count > 0 ? Math.Min(request.count, 500) : 50;
            var data = BaseData("query", workbook.FilePath, workbook.SheetName, table.HeaderRow, request);
            data.fields = table.Fields;
            data.rows = matched.Take(limit).Select(row => BuildRowEntry(table, row)).ToArray();
            data.totalMatched = matched.Length;
            data.returnedCount = data.rows.Length;
            return (data, "excel query: " + data.returnedCount + " of " + matched.Length + " row(s)");
        }

        static (ExcelData, string) DoSet(UnityCliBridgeRequest request)
        {
            var workbook = LoadWorkbook(request);
            var table = BuildTable(workbook, request);
            var row = SelectRows(table, request).FirstOrDefault();
            if (row == null)
            {
                throw new InvalidOperationException("No row matched the supplied selector.");
            }

            var field = ResolveField(table, request);
            var oldValue = row.Get(field.index);
            var newValue = request.value ?? string.Empty;
            var data = BaseData("set", workbook.FilePath, workbook.SheetName, table.HeaderRow, request);
            data.fields = table.Fields;
            data.cell = BuildCellEntry(field, row.ExcelRow, oldValue);
            data.rows = new[]
            {
                new ExcelRowEntry
                {
                    excelRow = row.ExcelRow,
                    dataIndex = row.DataIndex,
                    cells = new[]
                    {
                        BuildCellEntry(field, row.ExcelRow, oldValue),
                        new ExcelCellEntry
                        {
                            address = field.column + row.ExcelRow.ToString(CultureInfo.InvariantCulture),
                            column = field.column,
                            field = field.name,
                            value = newValue
                        }
                    }
                }
            };
            data.totalMatched = 1;
            data.returnedCount = 1;

            if (!request.dryRun)
            {
                SetCellValue(workbook.SheetDocument, row.ExcelRow, field.column, newValue);
                SaveSheet(workbook);
                data.modified = true;
            }

            return (data, request.dryRun ? "excel set: dry-run" : "excel set: updated " + field.column + row.ExcelRow);
        }

        static (ExcelData, string) DoInsert(UnityCliBridgeRequest request)
        {
            var workbook = LoadWorkbook(request);
            var table = BuildTable(workbook, request);
            var values = ParseValuesJson(request.excelValuesJson);
            var nextRow = Math.Max(table.HeaderRow + 1, table.Rows.Select(row => row.ExcelRow).DefaultIfEmpty(table.HeaderRow).Max() + 1);
            var data = BaseData("insert", workbook.FilePath, workbook.SheetName, table.HeaderRow, request);
            data.fields = table.Fields;
            data.rows = new[]
            {
                new ExcelRowEntry
                {
                    excelRow = nextRow,
                    dataIndex = table.Rows.Length,
                    cells = table.Fields
                        .Where(field => values.ContainsKey(field.name))
                        .Select(field => BuildCellEntry(field, nextRow, values[field.name]))
                        .ToArray()
                }
            };
            data.totalMatched = 1;
            data.returnedCount = 1;

            if (!request.dryRun)
            {
                EnsureRow(workbook.SheetDocument, nextRow);
                foreach (var field in table.Fields)
                {
                    if (values.TryGetValue(field.name, out var value))
                    {
                        SetCellValue(workbook.SheetDocument, nextRow, field.column, value);
                    }
                }

                SaveSheet(workbook);
                data.modified = true;
            }

            return (data, request.dryRun ? "excel insert: dry-run" : "excel insert: appended row " + nextRow);
        }

        static (ExcelData, string) DoDelete(UnityCliBridgeRequest request)
        {
            var workbook = LoadWorkbook(request);
            var table = BuildTable(workbook, request);
            var rows = SelectRows(table, request).ToArray();
            if (rows.Length == 0)
            {
                throw new InvalidOperationException("No row matched the supplied selector.");
            }

            var data = BaseData("delete", workbook.FilePath, workbook.SheetName, table.HeaderRow, request);
            data.fields = table.Fields;
            data.rows = rows.Select(row => BuildRowEntry(table, row)).ToArray();
            data.totalMatched = rows.Length;
            data.returnedCount = data.rows.Length;

            if (!request.dryRun)
            {
                foreach (var row in rows)
                {
                    RemoveRow(workbook.SheetDocument, row.ExcelRow);
                }

                SaveSheet(workbook);
                data.modified = true;
            }

            return (data, request.dryRun ? "excel delete: dry-run" : "excel delete: removed " + rows.Length + " row(s)");
        }

        static (ExcelData, string) DoValidate(UnityCliBridgeRequest request)
        {
            var workbook = LoadWorkbook(request);
            var table = BuildTable(workbook, request);
            var issues = new List<ExcelIssueEntry>();
            var seenFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in table.Fields)
            {
                if (string.IsNullOrWhiteSpace(field.name))
                {
                    issues.Add(new ExcelIssueEntry { severity = "error", message = "Empty header field.", field = field.column });
                }
                else if (!seenFields.Add(field.name))
                {
                    issues.Add(new ExcelIssueEntry { severity = "error", message = "Duplicate header field: " + field.name, field = field.name });
                }
            }

            if (!string.IsNullOrEmpty(request.excelKeyField))
            {
                var keyField = ResolveField(table, request.excelKeyField);
                var seenKeys = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var row in table.Rows)
                {
                    var key = row.Get(keyField.index);
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        issues.Add(new ExcelIssueEntry { severity = "warning", message = "Empty key value.", row = row.ExcelRow, field = keyField.name });
                        continue;
                    }

                    if (seenKeys.TryGetValue(key, out var firstRow))
                    {
                        issues.Add(new ExcelIssueEntry
                        {
                            severity = "error",
                            message = "Duplicate key \"" + key + "\" first seen at row " + firstRow + ".",
                            row = row.ExcelRow,
                            field = keyField.name
                        });
                    }
                    else
                    {
                        seenKeys[key] = row.ExcelRow;
                    }
                }
            }

            var data = BaseData("validate", workbook.FilePath, workbook.SheetName, table.HeaderRow, request);
            data.fields = table.Fields;
            data.issues = issues.ToArray();
            data.totalMatched = issues.Count;
            data.returnedCount = issues.Count;
            return (data, issues.Count == 0 ? "excel validate: ok" : "excel validate: " + issues.Count + " issue(s)");
        }

        static LoadedWorkbook LoadWorkbook(UnityCliBridgeRequest request)
        {
            var filePath = ResolveFilePath(request);
            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException("Excel file not found: " + filePath);
            }

            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only .xlsx files are supported by unity-cli excel.");
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false))
            {
                var workbookEntry = archive.GetEntry("xl/workbook.xml")
                    ?? throw new InvalidOperationException("Invalid .xlsx: missing xl/workbook.xml.");
                var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels")
                    ?? throw new InvalidOperationException("Invalid .xlsx: missing xl/_rels/workbook.xml.rels.");

                var workbookDocument = LoadXml(workbookEntry);
                var relsDocument = LoadXml(relsEntry);
                var sheets = workbookDocument.Root
                    .Descendants(SpreadsheetNs + "sheet")
                    .Select(sheet => new
                    {
                        Name = (string)sheet.Attribute("name") ?? string.Empty,
                        RelationId = (string)sheet.Attribute(RelationshipsNs + "id") ?? string.Empty
                    })
                    .Where(sheet => sheet.Name.Length > 0 && sheet.RelationId.Length > 0)
                    .ToArray();
                if (sheets.Length == 0)
                {
                    throw new InvalidOperationException("Invalid .xlsx: no worksheet found.");
                }

                var selectedSheet = string.IsNullOrEmpty(request.excelSheet)
                    ? sheets[0]
                    : sheets.FirstOrDefault(sheet => string.Equals(sheet.Name, request.excelSheet, StringComparison.OrdinalIgnoreCase));
                if (selectedSheet == null)
                {
                    throw new InvalidOperationException("Worksheet not found: " + request.excelSheet);
                }

                var rel = relsDocument.Root
                    .Elements(PackageRelationshipsNs + "Relationship")
                    .FirstOrDefault(element => string.Equals((string)element.Attribute("Id"), selectedSheet.RelationId, StringComparison.Ordinal));
                if (rel == null)
                {
                    throw new InvalidOperationException("Invalid .xlsx: worksheet relationship not found for " + selectedSheet.Name + ".");
                }

                var sheetEntryPath = NormalizeSheetEntryPath((string)rel.Attribute("Target") ?? string.Empty);
                var sheetEntry = archive.GetEntry(sheetEntryPath)
                    ?? throw new InvalidOperationException("Invalid .xlsx: worksheet entry not found: " + sheetEntryPath);

                var sharedStrings = ReadSharedStrings(archive);
                return new LoadedWorkbook
                {
                    FilePath = filePath,
                    SheetName = selectedSheet.Name,
                    SheetEntryPath = sheetEntryPath,
                    SharedStrings = sharedStrings,
                    SheetDocument = LoadXml(sheetEntry)
                };
            }
        }

        static TableModel BuildTable(LoadedWorkbook workbook, UnityCliBridgeRequest request)
        {
            var rawRows = ReadRows(workbook.SheetDocument, workbook.SharedStrings);
            var headerRow = request.excelHeaderRowSet ? request.excelHeaderRow : DetectHeaderRow(rawRows);
            var header = rawRows.FirstOrDefault(row => row.ExcelRow == headerRow)
                ?? throw new InvalidOperationException("Header row not found: " + headerRow);

            var maxColumn = header.Cells.Keys.DefaultIfEmpty(0).Max();
            var fields = new List<ExcelFieldEntry>();
            for (var col = 1; col <= maxColumn; col++)
            {
                var name = header.GetByColumn(col).Trim();
                if (string.IsNullOrEmpty(name)) continue;
                fields.Add(new ExcelFieldEntry
                {
                    name = name,
                    column = ColumnName(col),
                    index = fields.Count
                });
            }

            if (fields.Count == 0)
            {
                throw new InvalidOperationException("No header fields were found at row " + headerRow + ".");
            }

            var dataStartRow = headerRow + 1;
            var possibleTypeRow = rawRows.FirstOrDefault(row => row.ExcelRow == dataStartRow);
            if (possibleTypeRow != null && LooksLikeTypeDeclarationRow(possibleTypeRow, fields))
            {
                dataStartRow++;
            }

            var rows = rawRows
                .Where(row => row.ExcelRow >= dataStartRow)
                .Where(row => row.Cells.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
                .Select((row, index) => new RowModel
                {
                    ExcelRow = row.ExcelRow,
                    DataIndex = index,
                    Values = fields.Select(field => row.GetByColumn(ColumnIndex(field.column))).ToArray()
                })
                .ToArray();

            return new TableModel { HeaderRow = headerRow, Fields = fields.ToArray(), Rows = rows };
        }

        static bool LooksLikeTypeDeclarationRow(RawRow row, List<ExcelFieldEntry> fields)
        {
            if (fields.Count == 0) return false;
            var matches = 0;
            var checkedCount = 0;
            foreach (var field in fields)
            {
                var value = row.GetByColumn(ColumnIndex(field.column)).Trim();
                if (string.IsNullOrEmpty(value)) continue;
                checkedCount++;
                if (Regex.IsMatch(value, "^(int|long|float|double|string|bool|boolean|enum|date|datetime)$", RegexOptions.IgnoreCase)
                    || Regex.IsMatch(value, "^(List|Array|Dictionary)<.+>$", RegexOptions.IgnoreCase))
                {
                    matches++;
                }
            }

            return checkedCount > 0 && matches >= Math.Max(1, checkedCount / 2);
        }

        static IEnumerable<RowModel> SelectRows(TableModel table, UnityCliBridgeRequest request)
        {
            if (request.excelRowSet)
            {
                return table.Rows.Where(row => row.ExcelRow == request.excelRow);
            }

            if (request.excelIndexSet)
            {
                return table.Rows.Where(row => row.DataIndex == request.excelIndex);
            }

            if (!string.IsNullOrEmpty(request.excelKey))
            {
                var keyFieldName = string.IsNullOrEmpty(request.excelKeyField) ? table.Fields[0].name : request.excelKeyField;
                var keyField = ResolveField(table, keyFieldName);
                return table.Rows.Where(row => string.Equals(row.Get(keyField.index), request.excelKey, StringComparison.Ordinal));
            }

            var queryRows = FilterRows(table, request.excelWhere);
            return queryRows.Any() ? queryRows : table.Rows.Take(1);
        }

        static IEnumerable<RowModel> FilterRows(TableModel table, string where)
        {
            if (string.IsNullOrWhiteSpace(where))
            {
                return table.Rows;
            }

            var orParts = Regex.Split(where, "\\s+or\\s+", RegexOptions.IgnoreCase);
            return table.Rows.Where(row => orParts.Any(part => MatchesAndExpression(table, row, part)));
        }

        static bool MatchesAndExpression(TableModel table, RowModel row, string expression)
        {
            var andParts = Regex.Split(expression, "\\s+and\\s+", RegexOptions.IgnoreCase);
            foreach (var part in andParts)
            {
                if (!MatchesPredicate(table, row, part.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        static bool MatchesPredicate(TableModel table, RowModel row, string predicate)
        {
            var match = Regex.Match(predicate, "^(.+?)\\s*(contains|startsWith|endsWith|!=|>=|<=|=|>|<)\\s*(.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new InvalidOperationException("Unsupported --where predicate: " + predicate);
            }

            var field = ResolveField(table, TrimQuotes(match.Groups[1].Value.Trim()));
            var op = match.Groups[2].Value.Trim().ToLowerInvariant();
            var expected = TrimQuotes(match.Groups[3].Value.Trim());
            var actual = row.Get(field.index);

            if (decimal.TryParse(actual, NumberStyles.Float, CultureInfo.InvariantCulture, out var actualNumber)
                && decimal.TryParse(expected, NumberStyles.Float, CultureInfo.InvariantCulture, out var expectedNumber))
            {
                switch (op)
                {
                    case "=":  return actualNumber == expectedNumber;
                    case "!=": return actualNumber != expectedNumber;
                    case ">":  return actualNumber > expectedNumber;
                    case "<":  return actualNumber < expectedNumber;
                    case ">=": return actualNumber >= expectedNumber;
                    case "<=": return actualNumber <= expectedNumber;
                }
            }

            switch (op)
            {
                case "=":          return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
                case "!=":         return !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
                case "contains":   return actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
                case "startswith": return actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase);
                case "endswith":   return actual.EndsWith(expected, StringComparison.OrdinalIgnoreCase);
                default:
                    throw new InvalidOperationException("Operator requires numeric values: " + op);
            }
        }

        static ExcelFieldEntry ResolveField(TableModel table, UnityCliBridgeRequest request)
        {
            if (!string.IsNullOrEmpty(request.excelField))
            {
                return ResolveField(table, request.excelField);
            }

            if (!string.IsNullOrEmpty(request.excelColumn))
            {
                var col = request.excelColumn.Trim();
                var match = table.Fields.FirstOrDefault(field =>
                    string.Equals(field.column, col, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(field.name, col, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }

            throw new InvalidOperationException("A cell write/read requires --field <name> or --col <letter>.");
        }

        static ExcelFieldEntry ResolveField(TableModel table, string name)
        {
            var field = table.Fields.FirstOrDefault(entry =>
                string.Equals(entry.name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entry.column, name, StringComparison.OrdinalIgnoreCase));
            if (field == null)
            {
                throw new InvalidOperationException("Field not found: " + name);
            }

            return field;
        }

        static ExcelRowEntry BuildRowEntry(TableModel table, RowModel row)
        {
            return new ExcelRowEntry
            {
                excelRow = row.ExcelRow,
                dataIndex = row.DataIndex,
                cells = table.Fields
                    .Select(field => BuildCellEntry(field, row.ExcelRow, row.Get(field.index)))
                    .ToArray()
            };
        }

        static ExcelCellEntry BuildCellEntry(ExcelFieldEntry field, int row, string value)
        {
            return new ExcelCellEntry
            {
                address = field.column + row.ToString(CultureInfo.InvariantCulture),
                column = field.column,
                field = field.name,
                value = value ?? string.Empty
            };
        }

        static ExcelData BaseData(string action, string filePath, string sheet, int headerRow, UnityCliBridgeRequest request)
        {
            return new ExcelData
            {
                action = action,
                filePath = ToProjectRelative(filePath),
                sheet = sheet ?? string.Empty,
                headerRow = headerRow,
                dryRun = request.dryRun,
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }

        static string ResolveFilePath(UnityCliBridgeRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.excelFilePath))
            {
                return ResolvePath(request.excelFilePath);
            }

            if (string.IsNullOrWhiteSpace(request.excelTable))
            {
                throw new InvalidOperationException("excel requires --file <xlsx> or --table <name> for this action.");
            }

            var table = request.excelTable.Trim();
            var candidates = BuildTableNameCandidates(table);
            foreach (var root in ResolveSearchRoots(request.excelRoot))
            {
                if (!Directory.Exists(root)) continue;
                foreach (var path in Directory.GetFiles(root, "*.xlsx", SearchOption.AllDirectories)
                             .Where(path => !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal)))
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (candidates.Any(candidate => string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return path;
                    }
                }
            }

            throw new InvalidOperationException("Could not resolve Excel table: " + table);
        }

        static string[] BuildTableNameCandidates(string table)
        {
            var values = new List<string> { table };
            if (!table.StartsWith("prop", StringComparison.OrdinalIgnoreCase))
            {
                values.Add("prop" + table);
            }

            if (table.EndsWith("Config", StringComparison.OrdinalIgnoreCase))
            {
                var withoutConfig = table.Substring(0, table.Length - "Config".Length);
                values.Add(withoutConfig);
                values.Add("prop" + withoutConfig);
            }

            return values.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        static string[] ResolveSearchRoots(string rawRoot)
        {
            if (!string.IsNullOrWhiteSpace(rawRoot))
            {
                return rawRoot.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ResolvePath)
                    .ToArray();
            }

            var root = ProjectRoot;
            return new[]
            {
                Path.GetFullPath(Path.Combine(root, "../config")),
                Path.Combine(root, "Assets")
            };
        }

        static string ResolvePath(string path)
        {
            var trimmed = path.Trim().Trim('"');
            if (Path.IsPathRooted(trimmed))
            {
                return Path.GetFullPath(trimmed);
            }

            return Path.GetFullPath(Path.Combine(ProjectRoot, trimmed));
        }

        static string ToProjectRelative(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            var root = ProjectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var full = Path.GetFullPath(path);
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                ? full.Substring(root.Length).Replace('\\', '/')
                : full.Replace('\\', '/');
        }

        static XDocument LoadXml(ZipArchiveEntry entry)
        {
            using (var stream = entry.Open())
            {
                return XDocument.Load(stream);
            }
        }

        static string NormalizeSheetEntryPath(string target)
        {
            target = (target ?? string.Empty).Replace('\\', '/').TrimStart('/');
            if (target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase)) return target;
            return "xl/" + target;
        }

        static string[] ReadSharedStrings(ZipArchive archive)
        {
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return Array.Empty<string>();
            var document = LoadXml(entry);
            return document.Root
                .Elements(SpreadsheetNs + "si")
                .Select(si => string.Concat(si.Descendants(SpreadsheetNs + "t").Select(t => t.Value)))
                .ToArray();
        }

        static RawRow[] ReadRows(XDocument sheetDocument, string[] sharedStrings)
        {
            return sheetDocument.Root
                .Descendants(SpreadsheetNs + "row")
                .Select(row =>
                {
                    var rowNumber = ReadRowNumber(row);
                    var cells = new Dictionary<int, string>();
                    foreach (var cell in row.Elements(SpreadsheetNs + "c"))
                    {
                        var address = ((string)cell.Attribute("r")) ?? string.Empty;
                        var col = string.IsNullOrEmpty(address) ? cells.Count + 1 : ColumnIndex(Regex.Match(address, "^[A-Za-z]+").Value);
                        cells[col] = ReadCellValue(cell, sharedStrings);
                    }

                    return new RawRow { ExcelRow = rowNumber, Cells = cells };
                })
                .Where(row => row.ExcelRow > 0)
                .OrderBy(row => row.ExcelRow)
                .ToArray();
        }

        static int DetectHeaderRow(RawRow[] rows)
        {
            if (rows.Length == 0)
            {
                throw new InvalidOperationException("Worksheet is empty.");
            }

            var bestRow = rows[0].ExcelRow;
            var bestScore = int.MinValue;
            foreach (var row in rows.Take(20))
            {
                var values = row.Cells.Values.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
                if (values.Length == 0) continue;
                var identifierCount = values.Count(value => Regex.IsMatch(value, "^[A-Za-z_][A-Za-z0-9_]*$"));
                var duplicatePenalty = values.Length - values.Distinct(StringComparer.OrdinalIgnoreCase).Count();
                var score = identifierCount * 3 + values.Length - duplicatePenalty * 2;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRow = row.ExcelRow;
                }
            }

            return bestRow;
        }

        static string ReadCellValue(XElement cell, string[] sharedStrings)
        {
            var type = ((string)cell.Attribute("t")) ?? string.Empty;
            if (type == "s")
            {
                var text = cell.Element(SpreadsheetNs + "v")?.Value ?? string.Empty;
                return int.TryParse(text, out var index) && index >= 0 && index < sharedStrings.Length
                    ? sharedStrings[index]
                    : string.Empty;
            }

            if (type == "inlineStr")
            {
                return string.Concat(cell.Descendants(SpreadsheetNs + "t").Select(t => t.Value));
            }

            return cell.Element(SpreadsheetNs + "v")?.Value ?? string.Empty;
        }

        static void SetCellValue(XDocument document, int rowNumber, string column, string value)
        {
            var row = EnsureRow(document, rowNumber);
            var address = column.ToUpperInvariant() + rowNumber.ToString(CultureInfo.InvariantCulture);
            var cell = row.Elements(SpreadsheetNs + "c")
                .FirstOrDefault(element => string.Equals((string)element.Attribute("r"), address, StringComparison.OrdinalIgnoreCase));
            if (cell == null)
            {
                cell = new XElement(SpreadsheetNs + "c", new XAttribute("r", address));
                row.Add(cell);
                SortCells(row);
            }

            cell.RemoveNodes();
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                cell.Attribute("t")?.Remove();
                cell.Add(new XElement(SpreadsheetNs + "v", value));
            }
            else
            {
                cell.SetAttributeValue("t", "inlineStr");
                cell.Add(new XElement(SpreadsheetNs + "is", new XElement(SpreadsheetNs + "t", value ?? string.Empty)));
            }
        }

        static XElement EnsureRow(XDocument document, int rowNumber)
        {
            var sheetData = document.Root.Descendants(SpreadsheetNs + "sheetData").FirstOrDefault()
                ?? throw new InvalidOperationException("Invalid worksheet: missing sheetData.");
            var row = sheetData.Elements(SpreadsheetNs + "row")
                .FirstOrDefault(element => ReadRowNumber(element) == rowNumber);
            if (row != null) return row;

            row = new XElement(SpreadsheetNs + "row", new XAttribute("r", rowNumber));
            sheetData.Add(row);
            var sortedRows = sheetData.Elements(SpreadsheetNs + "row")
                .OrderBy(ReadRowNumber)
                .ToArray();
            sheetData.ReplaceNodes(sortedRows);
            return sheetData.Elements(SpreadsheetNs + "row").First(element => ReadRowNumber(element) == rowNumber);
        }

        static void RemoveRow(XDocument document, int rowNumber)
        {
            var row = document.Root
                .Descendants(SpreadsheetNs + "row")
                .FirstOrDefault(element => ReadRowNumber(element) == rowNumber);
            row?.Remove();
        }

        static void SortCells(XElement row)
        {
            row.ReplaceNodes(row.Elements(SpreadsheetNs + "c")
                .OrderBy(cell => ColumnIndex(Regex.Match(((string)cell.Attribute("r")) ?? string.Empty, "^[A-Za-z]+").Value))
                .ToArray());
        }

        static void SaveSheet(LoadedWorkbook workbook)
        {
            using (var archive = ZipFile.Open(workbook.FilePath, ZipArchiveMode.Update))
            {
                var entry = archive.GetEntry(workbook.SheetEntryPath)
                    ?? throw new InvalidOperationException("Worksheet entry disappeared: " + workbook.SheetEntryPath);
                entry.Delete();
                var newEntry = archive.CreateEntry(workbook.SheetEntryPath);
                using (var stream = newEntry.Open())
                {
                    workbook.SheetDocument.Save(stream);
                }
            }
        }

        static Dictionary<string, string> ParseValuesJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("insert requires --values-json <object>.");
            }

            var obj = JObject.Parse(json);
            return obj.Properties().ToDictionary(
                property => property.Name,
                property => property.Value.Type == JTokenType.Null ? string.Empty : property.Value.ToString(),
                StringComparer.OrdinalIgnoreCase);
        }

        static int ReadRowNumber(XElement row)
        {
            return int.TryParse((string)row.Attribute("r"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                ? number
                : 0;
        }

        static string TrimQuotes(string value)
        {
            if (value.Length >= 2
                && ((value[0] == '"' && value[value.Length - 1] == '"')
                    || (value[0] == '\'' && value[value.Length - 1] == '\'')))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        static string ColumnName(int index)
        {
            var name = string.Empty;
            while (index > 0)
            {
                index--;
                name = (char)('A' + (index % 26)) + name;
                index /= 26;
            }

            return name;
        }

        static int ColumnIndex(string name)
        {
            var value = 0;
            foreach (var ch in name.ToUpperInvariant())
            {
                if (ch < 'A' || ch > 'Z') continue;
                value = value * 26 + (ch - 'A' + 1);
            }

            return value;
        }

        static string ProjectRoot
        {
            get { return Path.GetDirectoryName(Application.dataPath) ?? Directory.GetCurrentDirectory(); }
        }

        sealed class LoadedWorkbook
        {
            public string FilePath;
            public string SheetName;
            public string SheetEntryPath;
            public string[] SharedStrings;
            public XDocument SheetDocument;
        }

        sealed class TableModel
        {
            public int HeaderRow;
            public ExcelFieldEntry[] Fields;
            public RowModel[] Rows;
        }

        sealed class RowModel
        {
            public int ExcelRow;
            public int DataIndex;
            public string[] Values;

            public string Get(int index)
            {
                return index >= 0 && index < Values.Length ? Values[index] ?? string.Empty : string.Empty;
            }
        }

        sealed class RawRow
        {
            public int ExcelRow;
            public Dictionary<int, string> Cells;

            public string GetByColumn(int col)
            {
                return Cells.TryGetValue(col, out var value) ? value ?? string.Empty : string.Empty;
            }
        }
    }
}
