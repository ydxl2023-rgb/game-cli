namespace UnityCLI.Abstractions;

// Each plugin owns its command parsing and implementation; the host knows no domain options.
public interface IPlugin
{
    string Name { get; }
    IReadOnlyCollection<string> Commands { get; }
    string Help { get; }
    Task<int> ExecuteAsync(string command, string[] args);
}
