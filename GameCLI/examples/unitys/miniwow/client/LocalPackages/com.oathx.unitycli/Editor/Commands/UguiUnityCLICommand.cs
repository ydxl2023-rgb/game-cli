using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the editor bridge route `/ugui`.
    /// </summary>
    internal sealed class UguiUnityCLICommand : IUnityCLICommand
    {
        /// <summary>
        /// Gets the bridge route path.
        /// </summary>
        public string Path => "/ugui";

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
                var contract = UnityCliContractBuildService.ReadContract(request.jsonFile);
                if (contract != null)
                {
                    var contractResult = await context.RunOnMainThreadAsync(() => UnityCliContractBuildService.Execute(request, contract));
                    return PrefabResult.Ok(contractResult);
                }
                var result = await context.RunOnMainThreadAsync(() => UnityCliWebVisualUguiPrefabService.Execute(request));
                return UguiResult.Ok(result);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("ugui_failed", exception.Message);
            }
        }
    }
}
