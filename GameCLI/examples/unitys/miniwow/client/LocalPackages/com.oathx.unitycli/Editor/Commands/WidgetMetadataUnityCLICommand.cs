using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the optional widget package route `/widget-metadata`.
    /// </summary>
    internal sealed class WidgetMetadataUnityCLICommand : IUnityCLICommand
    {
        const string GeneratorTypeName = "Oathx.Widget.Editor.WidgetMetadataPrefabGenerator";

        /// <summary>
        /// Gets the bridge route path.
        /// </summary>
        public string Path => "/widget-metadata";

        /// <summary>
        /// Gets the bridge method required by this route.
        /// </summary>
        public string Method => "POST";

        /// <summary>
        /// Executes the route handler asynchronously.
        /// </summary>
        /// <param name="request">The bridge request payload.</param>
        /// <param name="context">Bridge context and editor-thread dispatcher.</param>
        /// <param name="cancellationToken">The token used to cancel command execution.</param>
        /// <returns>A task that completes with a serializable bridge response DTO.</returns>
        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                return await context.RunOnMainThreadAsync(() => ExecuteOnMainThread(request));
            }
            catch (Exception exception)
            {
                return PingResult.Fail("widget_metadata_failed", exception.Message);
            }
        }

        static object ExecuteOnMainThread(UnityCliBridgeRequest request)
        {
            if (request.validateExisting)
            {
                return UnityCliWidgetMetadataValidationService.Execute(request);
            }

            var generatorType = ResolveType(GeneratorTypeName);
            if (generatorType == null)
            {
                return WidgetMetadataSkippedResult.Skip(request);
            }

            var method = generatorType.GetMethod(
                "Generate",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(string), typeof(string), typeof(bool) },
                null);
            if (method == null)
            {
                return PingResult.Fail(
                    "widget_metadata_unavailable",
                    "Widget metadata generator does not expose Generate(string prefabPath, string uiName, string vmName, bool dryRun).");
            }

            try
            {
                return method.Invoke(null, new object[] { request.prefabPath, request.uiName, request.vmName, request.dryRun });
            }
            catch (TargetInvocationException exception)
            {
                return PingResult.Fail("widget_metadata_failed", exception.InnerException?.Message ?? exception.Message);
            }
        }

        static Type ResolveType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                Type type = null;
                try
                {
                    type = assembly.GetType(typeName, false, false)
                        ?? assembly.GetTypes().FirstOrDefault(t => string.Equals(t.FullName, typeName, StringComparison.Ordinal));
                }
                catch
                {
                    type = null;
                }

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        [Serializable]
        sealed class WidgetMetadataSkippedResult
        {
            public bool ok;
            public string status;
            public string message;
            public WidgetMetadataSkippedData data;
            public PingError error;

            public static WidgetMetadataSkippedResult Skip(UnityCliBridgeRequest request)
            {
                return new WidgetMetadataSkippedResult
                {
                    ok = true,
                    status = "skipped",
                    message = "Widget metadata generator was not found. com.oathx.widget is optional; skipped widget-metadata for pure UGUI prefab generation.",
                    data = new WidgetMetadataSkippedData
                    {
                        uiName = request.uiName ?? string.Empty,
                        vmName = request.vmName ?? string.Empty,
                        prefabPath = request.prefabPath ?? string.Empty,
                        dryRun = request.dryRun,
                        skippedReason = "widget_metadata_unavailable"
                    }
                };
            }
        }

        [Serializable]
        sealed class WidgetMetadataSkippedData
        {
            public string uiName;
            public string vmName;
            public string prefabPath;
            public bool dryRun;
            public string skippedReason;
        }
    }
}
