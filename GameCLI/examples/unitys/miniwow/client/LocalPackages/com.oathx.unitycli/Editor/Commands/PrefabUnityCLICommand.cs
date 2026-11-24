using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the editor bridge route `/prefab`.
    /// </summary>
    internal sealed class PrefabUnityCLICommand : IUnityCLICommand
    {
        public string Path => "/prefab";

        public string Method => "POST";

        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                if (request.action == "verify")
                {
                    var evidence = await context.RunOnMainThreadAsync(() => UnityCliVerificationService.Execute(request));
                    return new VerificationResult
                    {
                        ok = (bool)evidence["passed"],
                        status = (bool)evidence["passed"] ? "verified" : "failed",
                        evidenceJson = evidence.ToString()
                    };
                }
                var result = await context.RunOnMainThreadAsync(() => UnityCliPrefabService.Execute(request));
                return PrefabResult.Ok(result);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("prefab_failed", exception.Message);
            }
        }
    }

    [Serializable]
    internal sealed class VerificationResult
    {
        public bool ok;
        public string status;
        public string evidenceJson;
    }
}
