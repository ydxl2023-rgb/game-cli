using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the editor bridge route `/run`.
    /// </summary>
    internal sealed class RunUnityCLICommand : IUnityCLICommand
    {
        /// <summary>
        /// Gets the bridge route path.
        /// </summary>
        public string Path => "/run";

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
                var data = await context.RunOnMainThreadAsync(() => UnityCliEditorRunController.Run(request.action));
                return RunResult.Ok(data);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("run_failed", exception.Message);
            }
        }
    }
}
