using System.Text.Json;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `excel` command.
    /// </summary>
    internal sealed class ExcelCommand : ICommand
    {
        public string Name => "excel";

        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var action = (options.Action ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine(
                    "excel requires an action: list | schema | row | cell | query | set | insert | delete | validate.");
                return 1;
            }

            var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/excel", new
            {
                action = action,
                count = options.Count,
                value = options.Value,
                dryRun = options.DryRun,
                excelFilePath = options.ExcelFilePath,
                excelSheet = options.ExcelSheet,
                excelTable = options.ExcelTable,
                excelRoot = options.ExcelRoot,
                excelRow = options.ExcelRow,
                excelRowSet = options.ExcelRowSet,
                excelIndex = options.ExcelIndex,
                excelIndexSet = options.ExcelIndexSet,
                excelColumn = options.ExcelColumn,
                excelField = options.ExcelField,
                excelKey = options.ExcelKey,
                excelKeyField = options.ExcelKeyField,
                excelWhere = options.ExcelWhere,
                excelValuesJson = options.ExcelValuesJson,
                excelHeaderRow = options.ExcelHeaderRow,
                excelHeaderRowSet = options.ExcelHeaderRowSet
            }, "POST");

            return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
        }

        static void WriteHuman(JsonElement root)
        {
            var ok = root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
            if (!ok)
            {
                ResponseFormatter.WriteError(root);
                return;
            }

            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : "excel command completed";
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            WriteString(data, "action", "  action: ");
            WriteString(data, "filePath", "  file: ");
            WriteString(data, "sheet", "  sheet: ");
            WriteInt(data, "headerRow", "  header row: ");
            WriteInt(data, "totalMatched", "  matched: ");
            WriteBool(data, "dryRun", "  dry-run: ");

            if (data.TryGetProperty("tables", out var tables) && tables.ValueKind == JsonValueKind.Array)
            {
                var tableList = tables.EnumerateArray().ToArray();
                if (tableList.Length > 0) Console.WriteLine("  tables:");
                foreach (var table in tableList)
                {
                    Console.WriteLine("    " + GetString(table, "name") + "  " + GetString(table, "filePath"));
                }
            }

            if (data.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
            {
                var fieldList = fields.EnumerateArray().ToArray();
                if (fieldList.Length > 0) Console.WriteLine("  fields:");
                foreach (var field in fieldList)
                {
                    Console.WriteLine(
                        "    " + GetString(field, "name") +
                        "  col=" + GetString(field, "column") +
                        "  index=" + GetInt(field, "index"));
                }
            }

            if (data.TryGetProperty("rows", out var rows) && rows.ValueKind == JsonValueKind.Array)
            {
                var rowList = rows.EnumerateArray().ToArray();
                if (rowList.Length > 0) Console.WriteLine("  rows:");
                foreach (var row in rowList)
                {
                    var excelRow = GetInt(row, "excelRow");
                    var dataIndex = GetInt(row, "dataIndex");
                    Console.Write("    #" + dataIndex + " excelRow=" + excelRow);
                    if (row.TryGetProperty("cells", out var cells) && cells.ValueKind == JsonValueKind.Array)
                    {
                        var parts = new List<string>();
                        foreach (var cell in cells.EnumerateArray())
                        {
                            var key = GetString(cell, "field");
                            var value = GetString(cell, "value");
                            if (key.Length > 0 && value.Length > 0)
                            {
                                parts.Add(key + "=" + value);
                            }
                        }

                        if (parts.Count > 0)
                        {
                            Console.Write("  " + string.Join(", ", parts.Take(8)));
                            if (parts.Count > 8) Console.Write(" ...");
                        }
                    }

                    Console.WriteLine();
                }
            }

            if (data.TryGetProperty("cell", out var selectedCell) && selectedCell.ValueKind == JsonValueKind.Object)
            {
                var address = GetString(selectedCell, "address");
                if (address.Length > 0)
                {
                    Console.WriteLine(
                        "  cell: " + address +
                        " " + GetString(selectedCell, "field") +
                        "=" + GetString(selectedCell, "value"));
                }
            }

            if (data.TryGetProperty("issues", out var issues) && issues.ValueKind == JsonValueKind.Array)
            {
                var issueList = issues.EnumerateArray().ToArray();
                if (issueList.Length > 0) Console.WriteLine("  issues:");
                foreach (var issue in issueList)
                {
                    Console.WriteLine("    " + GetString(issue, "severity") + ": " + GetString(issue, "message"));
                }
            }
        }

        static void WriteString(JsonElement element, string name, string prefix)
        {
            var value = GetString(element, name);
            if (value.Length > 0) Console.WriteLine(prefix + value);
        }

        static void WriteInt(JsonElement element, string name, string prefix)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number)
            {
                Console.WriteLine(prefix + value.GetInt32());
            }
        }

        static void WriteBool(JsonElement element, string name, string prefix)
        {
            if (element.TryGetProperty(name, out var value)
                && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
            {
                Console.WriteLine(prefix + value.GetBoolean());
            }
        }

        static string GetString(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }

        static int GetInt(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
                ? value.GetInt32()
                : 0;
        }
    }
}
