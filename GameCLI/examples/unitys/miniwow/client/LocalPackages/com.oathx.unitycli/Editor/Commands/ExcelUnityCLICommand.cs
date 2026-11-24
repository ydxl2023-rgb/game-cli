using System;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Handles the editor bridge route `/excel`.
    /// </summary>
    internal sealed class ExcelUnityCLICommand : IUnityCLICommand
    {
        public string Path => "/excel";

        public string Method => "POST";

        public async Task<object> ExecuteAsync(UnityCliBridgeRequest request, UnityCLICommandContext context, CancellationToken cancellationToken)
        {
            try
            {
                var result = await context.RunOnMainThreadAsync(() => UnityCliExcelService.Execute(request));
                return ExcelResult.Ok(result.data, result.message);
            }
            catch (Exception exception)
            {
                return PingResult.Fail("excel_failed", exception.Message);
            }
        }
    }
}
