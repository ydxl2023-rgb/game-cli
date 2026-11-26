using System.Globalization;
using System.Text.Json;
using Oathx.Expression;

string root = Path.GetFullPath(args[0]);
string output = Path.GetFullPath(args[1]);
using JsonDocument source = JsonDocument.Parse(File.ReadAllText(
    Path.Combine(root, "research/config-study/tables/tbguidetask.json")));
using JsonDocument generated = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "json/TaskGuideConfig.bytes")));
using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tools/task-config/manifest.json")));
JsonElement definitions = generated.RootElement.GetProperty("data");
int tipId = manifest.RootElement.GetProperty("tipId").GetInt32();
var engine = new ExpressionEngine();
ExpressionContext context = engine.CreateContext();
JsonElement expected = default;
int calls = 0;
int parameters = 0;
int conditions = 0;
int allowedConditions = 0;
foreach (JsonProperty signature in manifest.RootElement.GetProperty("signatures").EnumerateObject())
{
    string command = signature.Name;
    ExpressionValueKind[] kinds = signature.Value.EnumerateArray()
        .Select(value => Enum.Parse<ExpressionValueKind>(value.GetString())).ToArray();
    engine.Register(command, ExpressionFunctionKind.Command, ExpressionValueKind.Void, arguments =>
    {
        string originalCommand = expected.GetProperty("Action").GetString();
        string expectedCommand = originalCommand switch
        {
            "Navigation" => "Navigate",
            "Conversation" => "ShowDialogue",
            _ => originalCommand
        };
        if (command != expectedCommand)
            throw new Exception("Converted action differs from source");
        JsonElement original = expected.GetProperty("ActionParam");
        if (arguments.count != original.GetArrayLength()) throw new Exception("Parameter count changed");
        for (int index = 0; index < arguments.count; index++)
        {
            string value = original[index].GetString();
            if (command == "Tips")
            {
                if (arguments[index].AsNumber() != tipId) throw new Exception("Incorrect common text ID");
            }
            else if (command == "Navigate")
            {
                int buildingId = int.Parse(value.Split('/')[0].Split('_')[1], CultureInfo.InvariantCulture);
                if (arguments[index].AsNumber() != buildingId
                    || manifest.RootElement.GetProperty("navigationTargets")
                        .GetProperty(buildingId.ToString(CultureInfo.InvariantCulture)).GetString() != value)
                    throw new Exception("Navigation target changed");
            }
            else if (kinds[index] == ExpressionValueKind.Number)
            {
                if (arguments[index].AsNumber() != double.Parse(value, CultureInfo.InvariantCulture))
                    throw new Exception("Numeric parameter changed");
            }
            else if (arguments[index].AsString() != value)
                throw new Exception("String parameter changed");
            parameters++;
        }
        calls++;
        return default;
    }, kinds);
}

foreach (JsonElement row in source.RootElement.EnumerateArray())
{
    expected = row;
    string id = row.GetProperty("ID").GetInt32().ToString(CultureInfo.InvariantCulture);
    string expression = definitions.GetProperty(id).GetProperty("ActionExpression").GetString();
    string condition = definitions.GetProperty(id).GetProperty("ConditionExpression").GetString();
    if (string.IsNullOrWhiteSpace(condition) || string.IsNullOrWhiteSpace(expression))
        throw new Exception("Both expression columns require explicit values");
    CompiledExpression predicate = engine.Compile(condition, ExpressionMode.Evaluate);
    if (!ReferenceEquals(predicate, engine.Compile(condition, ExpressionMode.Evaluate)))
        throw new Exception("Condition cache was not reused");
    bool allowed = engine.Run(predicate, context).AsBoolean();
    conditions++;
    if (allowed) allowedConditions++;
    if (!System.Text.RegularExpressions.Regex.IsMatch(expression, @"^[A-Za-z_]\w*\("))
        throw new Exception("Action must use function call format");
    CompiledExpression program = engine.Compile(expression, ExpressionMode.Execute);
    if (!ReferenceEquals(program, engine.Compile(expression, ExpressionMode.Execute)))
        throw new Exception("Compilation cache was not reused");
    // Validate action parameters even when its condition is false; callbacks have no game side effects.
    engine.Run(program, context);
}
if (calls != definitions.EnumerateObject().Count()) throw new Exception("Expression coverage mismatch");

int uniqueCompiled = engine.cacheCount;
int rejected = 0;
foreach (string invalid in new[] { "{\"UnknownAction\"}", "{\"Wait\"}", "{\"Wait\", 0.1", "{\"Wait\", \"wrong\"}" })
{
    try
    {
        engine.Execute(invalid, context);
    }
    catch (ExpressionException)
    {
        rejected++;
    }
}
if (rejected != 4) throw new Exception("Invalid configuration was accepted");
int rejectedConditions = 0;
foreach (string invalid in new[] { "", "1", "Wait(0.6)", "unknown > 1", "true &&" })
{
    try { engine.Evaluate(invalid, context).AsBoolean(); }
    catch (ExpressionException) { rejectedConditions++; }
    catch (InvalidOperationException) { rejectedConditions++; }
}
if (rejectedConditions != 5) throw new Exception("Invalid condition was accepted");

int triggered = 0;
double hp = 80;
engine.Register("ATTR", ExpressionFunctionKind.Query, ExpressionValueKind.Number,
    values => new ExpressionValue(hp), ExpressionValueKind.Number, ExpressionValueKind.Number);
engine.Register("Record", ExpressionFunctionKind.Command, ExpressionValueKind.Void, values =>
{
    triggered++;
    return default;
});
context.Set("SELF", new ExpressionValue(1.0));
context.Set("HP", new ExpressionValue(2.0));
CompiledExpression attributeCondition = engine.Compile("ATTR(SELF, HP) > 100");
CompiledExpression record = engine.Compile("{\"Record\"}", ExpressionMode.Execute);
long compilations = engine.compilationCount;
if (engine.Run(attributeCondition, context).AsBoolean()) engine.Run(record, context);
if (triggered != 0) throw new Exception("False condition executed an action");
hp = 150;
if (engine.Run(attributeCondition, context).AsBoolean()) engine.Run(record, context);
if (triggered != 1 || engine.compilationCount != compilations)
    throw new Exception("Changed attribute was not evaluated using cached programs");
string report = JsonSerializer.Serialize(new
{
    status = "passed",
    validatedNodes = calls,
    validatedConditions = conditions,
    trueConditionsInValidationContext = allowedConditions,
    comparedParameters = parameters,
    uniqueCompiledExpressions = uniqueCompiled,
    rejectedInvalidFixtures = rejected,
    rejectedInvalidConditions = rejectedConditions,
    dynamicConditionGate = "passed: hp 80 skips; hp 150 executes; neither recompiles",
    execution = "Validation callbacks only; no game navigation or UI actions are executed."
}, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(Path.Combine(root, "tools/task-config/expression-validation.json"), report + "\n");
Console.WriteLine(report);
