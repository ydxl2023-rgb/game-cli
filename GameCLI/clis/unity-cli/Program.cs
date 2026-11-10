using System.Text.Json;
using UnityCLI;
using UnityCLI.Abstractions;

try
{
    var arguments = args.ToList();
    var plugins = PluginRegistry.Create();
    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var commands = new Dictionary<string, IPlugin>(StringComparer.OrdinalIgnoreCase);
    foreach (var plugin in plugins)
    {
        if (string.IsNullOrWhiteSpace(plugin.Name) || !names.Add(plugin.Name))
            throw new InvalidOperationException($"Invalid or duplicate plugin name: {plugin.Name}");
        foreach (var command in plugin.Commands)
            if (string.IsNullOrWhiteSpace(command) || !commands.TryAdd(command, plugin))
                throw new InvalidOperationException($"Invalid or duplicate command '{command}' in {plugin.Name}.");
    }
    if (arguments.Count == 0 || arguments[0] is "--help" or "-h")
    {
        Console.WriteLine("unity-cli <command> [options]");
        foreach (var plugin in plugins) Console.WriteLine($"{plugin.Name}: {plugin.Help}");
        return 0;
    }
    if (!commands.TryGetValue(arguments[0], out var owner))
        throw new ArgumentException($"Unknown command '{arguments[0]}'. Check plugins or --help.");
    return await owner.ExecuteAsync(arguments[0].ToLowerInvariant(), arguments.Skip(1).ToArray());
}
catch (Exception exception)
{
    if (args.Contains("--json")) Console.WriteLine(JsonSerializer.Serialize(new { ok = false, error = new { message = exception.Message } }));
    else Console.Error.WriteLine(exception.Message);
    return exception is ArgumentException ? 2 : 1;
}
