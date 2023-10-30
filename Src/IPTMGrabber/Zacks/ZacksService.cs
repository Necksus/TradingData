using IPTMGrabber.Utils;
using IPTMGrabber.Utils.Browser;
using Microsoft.Extensions.Logging;

namespace IPTMGrabber.Zacks
{
    public class ZacksService
    {
        private const string ScreenerScriptTemplate =
            // Select saved screeners
            "document.querySelector(\"#my-screen-tab\").click();\n" +

            // Run and download first saved screener
            "document.querySelector(\"table[id='screenlist']\").querySelector(\"a[onclick^='runsavedscreen']\").click();\n" +
            "document.querySelector(\"a[class='dt-button buttons-csv buttons-html5']\").click();";


        private readonly ILogger<ZacksService> _logger;
        private readonly BrowserService _browserService;

        public ZacksService(ILogger<ZacksService> logger, BrowserService browserService)
        {
            _logger = logger;
            _browserService = browserService;
        }

        public async Task GetScreenerAsync(Stream csvStream, CancellationToken cancellationToken)
        {
            var downloadFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(downloadFolder);
            try
            {
                await _browserService.OpenUrlAsync(Config.ZacksScreenerUrl, cancellationToken);
                await _browserService.SetDownloadFolder(downloadFolder);
                foreach (var scriptLine in string.Format(ScreenerScriptTemplate).Split('\n'))
                {
                    await _browserService.ExecuteJavascriptAsync(scriptLine, cancellationToken);
                }

                var csvFile = Directory.GetFiles(downloadFolder).Single();

                await using var downloadedStream = File.OpenRead(csvFile);
                await downloadedStream.CopyToAsync(csvStream, cancellationToken);
                await _browserService.CloseCurrentPage();
            }
            finally
            {
                try
                {
                    Directory.Delete(downloadFolder, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Cannot delete {downloadFolder}: {ex.Message}");
                }
            }
        }
    }
}
