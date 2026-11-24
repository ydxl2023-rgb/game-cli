using System;
using System.Collections.Generic;
using System.Linq;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Owns the editor-side route table exposed through the UnityCLI named-pipe bridge.
    /// </summary>
    internal static class UnityCliCommandRegistry
    {
        /// <summary>
        /// Maps bridge routes to command handlers.
        /// </summary>
        static readonly Dictionary<string, IUnityCLICommand> Commands = new Dictionary<string, IUnityCLICommand>(StringComparer.Ordinal)
        {
            { "/ping", new PingUnityCLICommand() },
            { "/console", new ConsoleUnityCLICommand() },
            { "/search", new SearchUnityCLICommand() },
            { "/measure", new MeasureUnityCLICommand() },
            { "/modify", new ModifyUnityCLICommand() },
            { "/run", new RunUnityCLICommand() },
            { "/call", new CallUnityCLICommand() },
            { "/compile", new CompileUnityCLICommand() },
            { "/save", new SaveUnityCLICommand() },
            { "/components", new ComponentsUnityCLICommand() },
            { "/asset", new AssetUnityCLICommand() },
            { "/ugui", new UguiUnityCLICommand() },
            { "/widget-metadata", new WidgetMetadataUnityCLICommand() },
            { "/particle", new ParticleUnityCLICommand() },
            { "/prefab", new PrefabUnityCLICommand() },
            { "/animator", new AnimatorUnityCLICommand() },
            { "/excel", new ExcelUnityCLICommand() },
            { "/terrain", new TerrainUnityCLICommand() },
            { "/screenshot", new ScreenshotUnityCLICommand() }
        };

        /// <summary>
        /// Looks up the handler registered for a normalized bridge path.
        /// </summary>
        /// <param name="path">The route path, including the leading slash.</param>
        /// <param name="command">The matching command handler when one exists.</param>
        /// <returns>True when the route has a registered command.</returns>
        internal static bool TryGetCommand(string path, out IUnityCLICommand command)
        {
            return Commands.TryGetValue(path, out command);
        }

        /// <summary>
        /// Formats registered routes for ping responses and unknown-route diagnostics.
        /// </summary>
        /// <returns>A comma-separated list of method/path pairs.</returns>
        internal static string FormatImplementedRoutes()
        {
            return string.Join(", ", Commands.Values
                .Select(command => command.Method + " " + command.Path)
                .OrderBy(line => line, StringComparer.Ordinal));
        }

        /// <summary>
        /// Enumerates registered route paths for the editor command toggle cache.
        /// </summary>
        /// <returns>The registered route paths.</returns>
        internal static IEnumerable<string> RegisteredPaths()
        {
            return Commands.Keys;
        }
    }
}
