using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the editor bridge route `/particle`.
    /// </summary>
    internal sealed class ParticleUnityCLICommand : IUnityCLICommand
    {
        public string Path => "/particle";

        public string Method => "POST";

        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                var result = await context.RunOnMainThreadAsync(() => UnityCliParticleService.Execute(request));
                return ParticleResult.Ok(result);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("particle_failed", exception.Message);
            }
        }
    }
}
