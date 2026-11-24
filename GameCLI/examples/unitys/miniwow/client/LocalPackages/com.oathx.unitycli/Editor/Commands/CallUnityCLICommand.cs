using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the editor bridge route `/call`.
    /// </summary>
    internal sealed class CallUnityCLICommand : IUnityCLICommand
    {
        /// <summary>
        /// Gets the bridge route path.
        /// </summary>
        public string Path => "/call";

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
                var data = await context.RunOnMainThreadAsync(() =>
                {
                    var invocation = ReflectionCallInvoker.Invoke(
                        request.invokeType,
                        request.invokeMember,
                        request.invokeArgsJson,
                        request.invokeArgs ?? Array.Empty<string>());
                    return new CallData
                    {
                        invokeType = invocation.invokeType,
                        invokeMember = invocation.invokeMember,
                        resolvedMember = invocation.resolvedMember,
                        returnIsVoid = invocation.returnIsVoid,
                        returnType = invocation.returnType,
                        returnSummary = invocation.returnSummary,
                        respondedAtUtc = invocation.respondedAtUtc
                    };
                });
                return CallResult.Ok(data);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("call_failed", exception.Message);
            }
        }
    }
}
