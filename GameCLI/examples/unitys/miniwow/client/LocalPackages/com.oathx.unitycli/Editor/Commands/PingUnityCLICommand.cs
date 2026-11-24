using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the editor bridge route `/ping`.
    /// </summary>
    internal sealed class PingUnityCLICommand : IUnityCLICommand
    {
        /// <summary>
        /// Gets the bridge route path.
        /// </summary>
        public string Path => "/ping";

        /// <summary>
        /// Gets the bridge method required by this route.
        /// </summary>
        public string Method => "GET";

        /// <summary>
        /// Executes the route handler asynchronously.
        /// </summary>
        /// <param name="request">The bridge request payload.</param>
        /// <param name="context">Bridge context and editor-thread dispatcher.</param>
        /// <param name="cancellationToken">The token used to cancel command execution.</param>
        /// <returns>A task that completes with a serializable bridge response DTO.</returns>
        public Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            var routes = UnityCliCommandRegistry.FormatImplementedRoutes();
            return Task.FromResult<object>(PingResult.Pong(context.CurrentEndpoint, routes));
        }
    }
}
