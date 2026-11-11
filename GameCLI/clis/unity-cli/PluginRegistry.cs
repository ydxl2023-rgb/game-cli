using UnityCLI.Abstractions;

namespace UnityCLI;

// Source plugins are compiled with the host. Add/remove registrations here.
internal static class PluginRegistry
{
    public static IReadOnlyList<IPlugin> Create() => new IPlugin[]
    {
        new UnityPlugin(),
        new UguiPlugin(),
        new AnimatorPlaugin(),
        new ExcelPlugin(),
    };
}
