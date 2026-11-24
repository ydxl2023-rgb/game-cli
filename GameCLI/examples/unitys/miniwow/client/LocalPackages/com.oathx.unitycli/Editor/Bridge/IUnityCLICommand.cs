using System;
using System.Threading;
using System.Threading.Tasks;
using Oathx.UnityCLI.Protocol;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Defines the contract for editor-side handlers that process UnityCLI bridge requests.
    /// </summary>
    internal interface IUnityCLICommand
    {
        /// <summary>
        /// Gets the route path handled by the command.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the HTTP-like method handled by the command.
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Executes the command asynchronously in response to a bridge request.
        /// </summary>
        /// <param name="request">The bridge request payload.</param>
        /// <param name="context">The command execution context.</param>
        /// <param name="cancellationToken">The token used to cancel command execution.</param>
        /// <returns>A task that completes with the serialized command response.</returns>
        Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides endpoint information and main-thread dispatch helpers for editor-side commands.
    /// </summary>
    internal sealed class UnityCLICommandContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityCLICommandContext"/> class.
        /// </summary>
        /// <param name="currentEndpoint">The current CLI endpoint advertised by the editor bridge.</param>
        public UnityCLICommandContext(CLIEndpoint currentEndpoint)
        {
            CurrentEndpoint = currentEndpoint;
        }

        /// <summary>
        /// Gets the current CLI endpoint information.
        /// </summary>
        public CLIEndpoint CurrentEndpoint { get; private set; }

        /// <summary>
        /// Runs the specified work on the Unity main thread.
        /// </summary>
        /// <typeparam name="T">The result type returned by the work delegate.</typeparam>
        /// <param name="work">The work delegate to execute on the main thread.</param>
        /// <returns>A task that completes with the delegate result.</returns>
        public Task<T> RunOnMainThreadAsync<T>(Func<T> work)
        {
            return UnityCliMainThread.RunOnMainThreadAsync(work);
        }
    }
}
