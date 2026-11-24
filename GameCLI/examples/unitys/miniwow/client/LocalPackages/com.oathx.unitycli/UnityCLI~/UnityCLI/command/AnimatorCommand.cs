using System.Text.Json;

namespace UnityCLI;

internal sealed class AnimatorCommand : ICommand
{
    public string Name => "animator";

    public async Task<int> ExecuteAsync(CliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Action))
        {
            Console.Error.WriteLine("animator requires an action: controller, parameter, clip, state, transition, bind, inspect.");
            return 1;
        }

        var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/animator", new
        {
            action = options.Action,
            jsonFile = string.IsNullOrWhiteSpace(options.JsonFile) ? null : Path.GetFullPath(options.JsonFile),
            animatorOperation = options.AnimatorOperation,
            animatorControllerPath = options.AnimatorControllerPath,
            animationClipPath = options.AnimationClipPath,
            objectPath = options.ObjectPath,
            component = options.Component,
            property = options.Property,
            animatorLayer = options.AnimatorLayer,
            animatorParameterName = options.AnimatorParameterName,
            animatorParameterType = string.IsNullOrWhiteSpace(options.AnimatorParameterType) ? options.Type : options.AnimatorParameterType,
            animatorDefaultValue = options.AnimatorDefaultValue,
            animatorStateName = options.AnimatorStateName,
            animatorFromState = options.AnimatorFromState,
            animatorToState = options.AnimatorToState,
            animatorCondition = options.AnimatorCondition,
            animatorKeys = options.AnimatorKeys,
            animatorDuration = options.AnimatorDuration,
            animatorDurationSet = options.AnimatorDurationSet,
            animatorSampleTime = options.AnimatorSampleTime,
            animatorSampleTimeSet = options.AnimatorSampleTimeSet,
            animatorLoop = options.AnimatorLoop,
            animatorLoopSet = options.AnimatorLoopSet,
            dryRun = options.DryRun
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

        Console.WriteLine(root.TryGetProperty("message", out var message) ? message.GetString() : "animator command completed");
        if (!root.TryGetProperty("data", out var data)) return;

        WriteString(data, "action");
        WriteString(data, "operation");
        WriteString(data, "controllerPath");
        WriteString(data, "clipPath");
        WriteString(data, "objectPath");
        WriteString(data, "layer");
        WriteString(data, "stateName");
        WriteString(data, "parameterName");
        WriteString(data, "parameterType");
        WriteString(data, "fromState");
        WriteString(data, "toState");
        WriteInt(data, "parameterCount");
        WriteInt(data, "layerCount");
        WriteInt(data, "stateCount");
        WriteInt(data, "transitionCount");
        WriteInt(data, "curveCount");
        WriteBool(data, "dryRun");
        WriteBool(data, "assetModified");
        WriteBool(data, "sceneModified");

        if (data.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in details.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String) Console.WriteLine("  - " + item.GetString());
            }
        }
    }

    static void WriteString(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (!string.IsNullOrEmpty(text)) Console.WriteLine("  " + name + ": " + text);
        }
    }

    static void WriteInt(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.TryGetInt32(out var number))
        {
            Console.WriteLine("  " + name + ": " + number);
        }
    }

    static void WriteBool(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value)
            && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
        {
            Console.WriteLine("  " + name + ": " + value.GetBoolean());
        }
    }
}
