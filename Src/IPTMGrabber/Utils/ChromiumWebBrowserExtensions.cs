using CefSharp;
using CefSharp.DevTools.Page;
using CefSharp.OffScreen;
using HtmlAgilityPack;

namespace IPTMGrabber.Utils
{
    internal static class ChromiumWebBrowserExtensions
    {
        public static async Task<HtmlDocument> GetHtmlDocumentAsync(this ChromiumWebBrowser browser, CancellationToken cancellationToken)
        {
            var doc = new HtmlDocument();
            if (browser.IsLoading)
                await browser.WaitForNavigationAsync(cancellationToken: cancellationToken);
            doc.LoadHtml(await browser.GetSourceAsync());

            return doc;
        }

        public static async Task SaveAsPNGAsync(this ChromiumWebBrowser browser, string filename, CancellationToken cancellationToken)
            => await File.WriteAllBytesAsync(filename, await browser.CaptureScreenshotAsync(CaptureScreenshotFormat.Png), cancellationToken);
    }
}
