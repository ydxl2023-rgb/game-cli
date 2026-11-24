using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    internal sealed class AnimatorUnityCLICommand : IUnityCLICommand
    {
        public string Path => "/animator";

        public string Method => "POST";

        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                var result = await context.RunOnMainThreadAsync(() => UnityCliAnimatorService.Execute(request));
                return AnimatorResult.Ok(result);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("animator_failed", exception.Message);
            }
        }
    }
}
