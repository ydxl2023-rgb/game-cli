using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the editor bridge route `/compile`.
    /// </summary>
    internal sealed class CompileUnityCLICommand : IUnityCLICommand
    {
        /// <summary>
        /// Gets the bridge route path.
        /// </summary>
        public string Path => "/compile";

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
                if (!request.compileCheckOnly)
                {
                    await context.RunOnMainThreadAsync(() =>
                    {
                        AssetDatabase.Refresh(ImportAssetOptions.Default);
                        CompilationPipeline.RequestScriptCompilation();
                        return true;
                    });
                }

                if (request.compileWait && !request.compileCheckOnly)
                {
                    await Task.Delay(100, cancellationToken);
                    var deadline = DateTime.UtcNow.AddMinutes(5);
                    while (DateTime.UtcNow < deadline)
                    {
                        var compiling = await context.RunOnMainThreadAsync(() => EditorApplication.isCompiling);
                        if (!compiling)
                        {
                            break;
                        }

                        await Task.Delay(50, cancellationToken);
                    }

                    if (DateTime.UtcNow >= deadline)
                    {
                        return PingResult.Fail("compile_failed", "Compilation did not finish within 5 minutes.");
                    }
                }

                var data = await context.RunOnMainThreadAsync(() => new CompileData
                {
                    waited = request.compileWait,
                    isCompiling = EditorApplication.isCompiling,
                    respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
                });
                return CompileResult.Ok(data);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("compile_failed", exception.Message);
            }
        }
    }
}
