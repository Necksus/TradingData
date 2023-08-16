using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using IPTMGrabber.Utils;

namespace IPTMGrabber.InvestorWebsite
{
    internal class Pager
    {
        public virtual bool LastPage => false;
        public ChromiumWebBrowser Browser { get; }
        public PagerDefinition? PagerInfo { get; }

        public virtual Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken) => Task.FromResult<HtmlDocument?>(null);

        public Pager(ChromiumWebBrowser browser, PagerDefinition? pagerInfo)
        {
            Browser = browser;
            PagerInfo = pagerInfo;
        }

        public async Task<HtmlDocument> LoadMoreAsync(HtmlDocument doc, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(PagerInfo?.MoreButton))
            {
                while (doc.DocumentNode.SelectSingleNode(PagerInfo.MoreButton) != null)
                {
                    // Convert "classic" xpath to querySelector
                    var selector = PagerInfo.MoreButton.Trim('/').Replace("@", "");

                    await Browser.EvaluateScriptAsPromiseAsync($"document.querySelector(\"{selector}\").click()");
                    await Browser.WaitForRenderIdleAsync(cancellationToken: cancellationToken);

                    doc = await Browser.GetHtmlDocumentAsync(cancellationToken);
                }
            }

            return doc;
        }
    }
}
