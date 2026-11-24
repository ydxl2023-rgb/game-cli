using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles screenshot requests from the UnityCLI executable.
    /// </summary>
    internal sealed class ScreenshotUnityCLICommand : IUnityCLICommand
    {
        /// <summary>
        /// Gets the bridge route path.
        /// </summary>
        public string Path => "/screenshot";

        /// <summary>
        /// Gets the bridge method.
        /// </summary>
        public string Method => "POST";

        /// <summary>
        /// Executes the screenshot command asynchronously.
        /// </summary>
        /// <param name="request">The bridge request.</param>
        /// <param name="context">The command context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that completes with a serializable bridge response DTO.</returns>
        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                var data = await context.RunOnMainThreadAsync(() => UnityCliScreenshotService.Execute(request));
                return ScreenshotResult.Ok(data);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("screenshot_failed", exception.Message);
            }
        }
    }
}
