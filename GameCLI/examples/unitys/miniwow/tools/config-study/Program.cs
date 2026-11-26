using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Luban;

string root = Path.GetFullPath(args[0]);
string output = Path.Combine(root, "research", "config-study");
Directory.CreateDirectory(Path.Combine(output, "tables"));
var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
object Project(object value)
{
    if (value == null || value is string || value.GetType().IsPrimitive) return value;
    if (value is Enum) return new { value = Convert.ToInt32(value), name = value.ToString() };
    if (value is IDictionary dictionary)
    {
        var map = new Dictionary<string, object>();
        foreach (DictionaryEntry pair in dictionary) map.Add(pair.Key.ToString(), Project(pair.Value));
        return map;
    }
    if (value is IEnumerable list) return list.Cast<object>().Select(Project).ToArray();
    var fields = new Dictionary<string, object> { ["$type"] = value.GetType().FullName };
    foreach (var field in value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        if (!field.Name.EndsWith("_Ref")) fields.Add(field.Name, Project(field.GetValue(value)));
    return fields;
}
var assembly = Assembly.GetExecutingAssembly();
byte[] packed = File.ReadAllBytes(Path.Combine(root, "extracted-resources/assets/data/config/configs.bytes"));
var records = new List<object>();
int failed = 0;
foreach (string line in File.ReadLines(Path.Combine(root, "extracted-resources/assets/data/config/confighead.txt")))
{
    if (string.IsNullOrWhiteSpace(line)) continue;
    string[] entry = line.Trim().Split(',');
    string name = entry[0];
    int offset = int.Parse(entry[1]);
    int size = int.Parse(entry[2]);
    byte[] bytes = packed.AsSpan(offset, size).ToArray();
    string hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    var buffer = new ByteBuf(bytes);
    try
    {
        var type = assembly.GetTypes().Single(t => t.Namespace == "Zombie.Table" && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        var table = Activator.CreateInstance(type, buffer);
        if (buffer.Size != 0) throw new InvalidDataException($"Unconsumed bytes: {buffer.Size}");
        var rows = ((IEnumerable)type.GetProperty("DataList").GetValue(table)).Cast<object>().ToArray();
        File.WriteAllText(Path.Combine(output, "tables", name + ".json"), JsonSerializer.Serialize(rows.Select(Project), options));
        var rowType = type.GetProperty("DataList").PropertyType.GenericTypeArguments[0];
        var schema = rowType.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => new { name = f.Name, type = f.FieldType.ToString(), reference = f.Name.EndsWith("_Ref") });
        records.Add(new { name, offset, size, sha256 = hash, rows = rows.Length, consumed = buffer.Position, status = "decoded", rowType = rowType.FullName, fields = schema });
    }
    catch (Exception exception)
    {
        failed++;
        records.Add(new { name, offset, size, sha256 = hash, consumed = buffer.Position, status = "failed", error = exception.GetBaseException().Message });
        Console.WriteLine($"FAILED {name}: {exception.GetBaseException().Message}");
    }
}
File.WriteAllText(Path.Combine(output, "inventory.json"), JsonSerializer.Serialize(new { packedSha256 = Convert.ToHexString(SHA256.HashData(packed)).ToLowerInvariant(), tables = records }, options));
var schemaTypes = assembly.GetTypes().Where(t => t.Namespace?.StartsWith("Zombie.Table") == true)
    .OrderBy(t => t.FullName).Select(t => new {
        name = t.FullName,
        kind = t.IsEnum ? "enum" : "class",
        baseType = t.BaseType?.FullName,
        members = t.IsEnum ? Enum.GetNames(t).Select(n => new { name = n, value = Convert.ToInt64(Enum.Parse(t, n)) }).ToArray() : null,
        fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => new { name = f.Name, type = f.FieldType.ToString() })
    });
File.WriteAllText(Path.Combine(output, "schema.json"), JsonSerializer.Serialize(schemaTypes, options));
if (failed == 0)
{
    var resolved = new Zombie.Table.Tables(name => new ByteBuf(File.ReadAllBytes(
        Path.Combine(root, "extracted-resources/config-tables", name + ".bytes"))));
    Console.WriteLine("Original Tables constructor and ResolveRef completed.");
}
Console.WriteLine($"Tables {records.Count}, decoded {records.Count - failed}, failed {failed}");
Environment.ExitCode = failed == 0 ? 0 : 1;
