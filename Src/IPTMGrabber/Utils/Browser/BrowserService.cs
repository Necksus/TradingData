using HtmlAgilityPack;
using IPTMGrabber.InvestorWebsite;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace IPTMGrabber.Utils.Browser
{
    public class BrowserService
    {
        private readonly ILogger<BrowserService> _logger;
        private IBrowser _browser;
        private IPage _currentPage;

        public BrowserService(ILogger<BrowserService> logger)
        {
            _logger = logger;
        }

        public async Task<HtmlDocument> OpenUrlAsync(string url, CancellationToken cancellationToken)
        {
            // Trick for AMAT: keep it?
            _browser?.Dispose();
            _browser = null;

            if (_browser == null)
                await CreateBrowserAsync();
            if (_currentPage != null) await _currentPage.DisposeAsync();

            _currentPage = await _browser.NewPageAsync();

            var userAgent = (await _currentPage.Browser.GetUserAgentAsync()).Replace("Headless", "");
            await _currentPage.SetUserAgentAsync(userAgent);
            _logger?.LogInformation($"Using the user agent: {userAgent}");

            await _currentPage.SetCacheEnabledAsync(false);
            await _currentPage.SetViewportAsync(new ViewPortOptions { Width = 1280, Height = 4000 });
            await _currentPage.GoToAsync(url, new NavigationOptions { Timeout = 2 * 60 * 1000 });

            //            await _currentPage.ScreenshotAsync(@"C:\Data\Downloads\screenshot.png", new ScreenshotOptions { Type = ScreenshotType.Png });
            return await GetHtmlDocumentAsync(cancellationToken);
        }

        public async Task<HtmlDocument> ExecuteJavascriptAsync(string script, CancellationToken cancellationToken)
        {
            await _currentPage.EvaluateExpressionAsync(script);
            await _currentPage.WaitForNetworkIdleAsync();

            return await GetHtmlDocumentAsync(cancellationToken);
        }

        public string Url => _currentPage?.Url;

        public Pager FindPager(PagerDefinition? pagerInfo, HtmlDocument doc)
        {
            if (NextPager.FoundPager(this, pagerInfo, doc, out var nextPager))
            {
                _logger?.LogInformation("Found a 'next button' pager.");
                return nextPager!;
            }

            if (SelectPager.FoundPager(this, pagerInfo, doc, out var selectPager))
            {
                _logger?.LogInformation("Found a 'combo box' pager.");
                return selectPager!;
            }

            _logger?.LogWarning("No pager found");
            return new Pager(this, pagerInfo);
        }

        private async Task CreateBrowserAsync()
        {
            // see https://stackoverflow.com/questions/70752901/how-to-get-puppeteer-sharp-working-on-an-aws-elastic-beanstalk-running-docker
            if (_browser == null)
            {
                using var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                _browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions
                    {
                        Headless = true,
                        Args = new[] {
                            "--disable-gpu",
                            "--disable-dev-shm-usage",
                            "--disable-setuid-sandbox",
                            "--no-sandbox"}
                    });
            }
        }

        private async Task<HtmlDocument> GetHtmlDocumentAsync(CancellationToken cancellationToken)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(await _currentPage.GetContentAsync());

            return doc;
        }

    }
}
