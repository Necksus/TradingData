using HtmlAgilityPack;
using PuppeteerSharp;

namespace IPTMGrabber.Utils
{
    internal static class PageExtensions
    {
        public static async Task<HtmlDocument> GetHtmlDocumentAsync(this IPage page, CancellationToken cancellationToken)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(await page.GetContentAsync());

            return doc;
        }

        public static async Task ExecuteJavascriptAsync(this IPage page, string script)
        {
            //page.ScreenshotAsync(@"C:\Data\Downloads\screenshot.png", new ScreenshotOptions { Type = ScreenshotType.Png });
            await page.EvaluateExpressionAsync(script);
            await page.WaitForNetworkIdleAsync();
        }

        public static async Task NavigateAsync(this IPage page, string url)
        {
            await page.GoToAsync(url, new NavigationOptions { Timeout = 2*60*1000 });
            //await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)");
            //await page.ScreenshotAsync(@"C:\Data\Downloads\screenshot.png", new ScreenshotOptions { Type = ScreenshotType.Png });
        }
    }
}
