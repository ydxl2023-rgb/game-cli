using System.Text.Json;
using UnityCLI.Abstractions;

namespace UnityCLI;

internal interface ICommand
{
    string Name { get; }
    Task<int> ExecuteAsync(CliOptions options);
}

internal sealed class CliOptions
{
    public string? ProjectPath { get; set; }
    public bool JsonOutput { get; set; }
    public bool CompileWait { get; set; }
    public int Count { get; set; } = 20;
    public string Type { get; set; } = "all";
    public string Search { get; set; } = "";
}

public sealed class UnityPlugin : IPlugin
{
    public string Name => "UnityPlugin";
    public IReadOnlyCollection<string> Commands => new[] { "ping", "console", "logs", "compile" };
    public string Help => "ping | console (alias: logs) | compile [--wait]; common: [--project path] [--json]; console: [--count 1..200] [--type all|log|warning|error] [--search text]";
    public async Task<int> ExecuteAsync(string commandName, string[] arguments)
    {
        var args = new[] { commandName }.Concat(arguments).ToArray();
        var options = new CliOptions { JsonOutput = args.Contains("--json") };
        try
        {
            if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
            {
                Console.WriteLine("""
                    unity-cli <ping|console|logs|compile> [options]
                      --project <path>  Unity project root (default: search current directory upward)
                      --json            Print machine-readable JSON
                      --count <1..200>   Latest matching console entries (default: 20)
                      --type <type>     all, log, warning, error (default: all)
                      --search <text>   Console text / wildcard filter
                      --wait            compile only: wait up to 5 minutes, retry during domain reload
                    compile refreshes assets and requests script compilation. Use console --type error for diagnostics.
                    Requires the com.oathx.unitycli Editor bridge in the open Unity project.
                    Request timeout: UNITYCLI_REQUEST_TIMEOUT_SECONDS (5..300, default: 5).
                    """);
                return 0;
            }
            string? command = null;
            bool hasLogOptions = false;
            for (int i = 0; i < args.Length; i++)
            {
                string Value()
                {
                    if (++i >= args.Length) throw new ArgumentException("Missing value for " + args[i - 1]);
                    return args[i];
                }
                switch (args[i])
                {
                    case "--project": options.ProjectPath = Value(); break;
                    case "--json": break;
                    case "--wait": options.CompileWait = true; break;
                    case "--count":
                        hasLogOptions = true;
                        if (!int.TryParse(Value(), out var count) || count < 1 || count > 200)
                            throw new ArgumentException("--count must be between 1 and 200.");
                        options.Count = count; break;
                    case "--type":
                        hasLogOptions = true;
                        options.Type = Value().ToLowerInvariant();
                        if (options.Type is not ("all" or "log" or "warning" or "error"))
                            throw new ArgumentException("--type must be all, log, warning, or error.");
                        break;
                    case "--search": hasLogOptions = true; options.Search = Value(); break;
                    case "ping": case "console": case "logs": case "compile":
                        if (command != null) throw new ArgumentException("Specify only one command.");
                        command = args[i]; break;
                    default: throw new ArgumentException("Unknown argument: " + args[i]);
                }
            }
            if (command == null) throw new ArgumentException("Specify ping, console, logs, or compile.");
            if (command is not ("console" or "logs") && hasLogOptions) throw new ArgumentException("Log filters require console or logs.");
            if (command != "compile" && options.CompileWait) throw new ArgumentException("--wait requires compile.");
            ICommand handler = command switch { "ping" => new PingCommand(), "compile" => new CompileCommand(), _ => new ConsoleCommand() };
            return await handler.ExecuteAsync(options);
        }
        catch (Exception exception)
        {
            var message = exception is OperationCanceledException
                ? "Unity request timed out. Check that the Editor is open and its bridge is running."
                : exception.Message;
            if (options.JsonOutput)
                Console.WriteLine(JsonSerializer.Serialize(new { ok = false, error = new { message } }));
            else Console.Error.WriteLine(message);
            return exception is ArgumentException ? 2 : 1;
        }
    }
}

