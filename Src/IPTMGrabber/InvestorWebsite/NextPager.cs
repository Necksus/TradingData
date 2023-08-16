using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using IPTMGrabber.Utils;

namespace IPTMGrabber.InvestorWebsite
{
    internal class NextPager : Pager
    {
        private HtmlNode? _nextNode;

        public override bool LastPage => _nextNode == null;

        public NextPager(ChromiumWebBrowser browser, PagerDefinition? pagerInfo, HtmlDocument doc) : base(browser, pagerInfo)
        {
            FindNextLink(doc);
        }

        public override async Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken)
        {
            if (!LastPage && (await TryNavigateHrefAsync() || await TryClickAsync()))
            {
                var doc = await Browser.GetHtmlDocumentAsync(cancellationToken);
                FindNextLink(doc);

                return doc;
            }

            return await base.MoveNextAsync(cancellationToken);
        }

        private async Task<bool> TryClickAsync()
        {
            var selector = _nextNode?.GetQuerySelector();

            if (!string.IsNullOrEmpty(selector))
            {
                await Browser.EvaluateScriptAsPromiseAsync($"document.querySelector(\"{selector}\").click()");
                await Browser.WaitForRenderIdleAsync();
                return true;
            }

            return false;
        }

        private async Task<bool> TryNavigateHrefAsync()
        {
            var ancestor = _nextNode;
            var level = 0;
            while (ancestor != null && level < 5)
            {
                if (ancestor.Attributes["href"] != null)
                {
                    var href = ancestor.GetUnescapedAttribute("href");
                    if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out var url))
                    {
                        if (!url.IsAbsoluteUri)
                            url = new Uri(new Uri(Browser.Address), url);

                        if (!url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) && !url.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                            return false;

                        await Browser.LoadUrlAsync(url.AbsoluteUri);
                        return true;
                    }
                }

                ancestor = ancestor.ParentNode;
                level++;
            }
            return false;
        }

        private void FindNextLink(HtmlDocument doc)
        {
            _nextNode = doc.DocumentNode.SelectSingleNode(PagerInfo?.NextButton ?? "//*[not(*) and (normalize-space(text()) = 'Next' or normalize-space(text()) = 'Next page')]");

            int level = 0;
            while (level < 3 && _nextNode != null && string.IsNullOrEmpty(_nextNode.GetUnescapedAttribute("href")) && _nextNode.GetQuerySelector() == null)
            {
                _nextNode = _nextNode.ParentNode;
                level++;
            }

            if (_nextNode?.Attributes?.Contains("disabled") == true)
                _nextNode = null;
        }

        public static bool FoundPager(ChromiumWebBrowser browser, PagerDefinition? pagerInfo, HtmlDocument doc, out NextPager? pager)
        {
            var nextPager = new NextPager(browser, pagerInfo, doc);
            pager = !nextPager.LastPage ? nextPager : null;
            return pager != null;
        }
    }
}
