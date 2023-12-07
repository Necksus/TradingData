using BlazorBootstrap;
using IPTMGrabber.Zacks;
using IPTMGrabber.Utils;


namespace SharpITPM.Pages
{
    public partial class Screener
    {
        BlazorBootstrap.Grid<ScreenerStock> grid = default!;
        /*
        public Screener(ZacksService zacksService)
        {
            _zacksService = zacksService;
        }*/

        private async Task<GridDataProviderResult<ScreenerStock>> ScreenerDataProvider(GridDataProviderRequest<ScreenerStock> request)
        {
            using var memoryStream = new MemoryStream();
            await ZacksService.GetScreenerAsync(memoryStream, default);
            var stocks = Enumerators.EnumerateFromCsv< ScreenerStock>(memoryStream).ToList();

            return await Task.FromResult(request.ApplyTo(stocks));
        }

        private string FormatToPercent(double? number)
            => number.HasValue ? number.Value.ToString("P2") : "";

        private object FormatToDecimal(double? number)
            => number.HasValue ? number.Value.ToString("0.00") : "";
    }
}
