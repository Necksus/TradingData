using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using IPTMGrabber.Utils;

namespace IPTMGrabber.InvestorWebsite
{
    internal class NextPager : Pager
    {
        private readonly ChromiumWebBrowser _browser;
        private HtmlNode? _nextNode;

        public override bool LastPage => _nextNode == null;

        public NextPager(ChromiumWebBrowser browser, HtmlDocument doc)
        {
            _browser = browser;
            FindNextLink(doc);
        }

        public override async Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken)
        {
            if (!LastPage && (await TryNavigateHrefAsync() || await TryClickAsync()))
            {
                var doc = await _browser.GetHtmlDocumentAsync(cancellationToken);
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
                await _browser.EvaluateScriptAsPromiseAsync($"document.querySelector(\"{selector}\").click()");
                await _browser.WaitForRenderIdleAsync();
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
                            url = new Uri(new Uri(_browser.Address), url);

                        if (!url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) && !url.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                            return false;

                        await _browser.LoadUrlAsync(url.AbsoluteUri);
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
            _nextNode = doc.DocumentNode
                .Descendants()
                .FirstOrDefault(node => node.ChildNodes.Count == 0 &&
                                        (node.GetUnescapedText()?.Equals("next", StringComparison.OrdinalIgnoreCase) == true ||
                                         node.GetUnescapedText()?.Equals("next page", StringComparison.OrdinalIgnoreCase) == true) ||
                                         node.GetUnescapedAttribute("title")?.Equals("next", StringComparison.OrdinalIgnoreCase) == true ||
                                         node.GetUnescapedAttribute("title")?.Equals("next page", StringComparison.OrdinalIgnoreCase) == true);

            int level = 0;
            while (level < 3 && _nextNode != null && _nextNode.GetQuerySelector() == null)
                _nextNode = _nextNode.ParentNode;

            if (_nextNode?.Attributes?.Contains("disabled") == true || _nextNode?.GetQuerySelector() == null)
                _nextNode = null;
        }

        public static bool FoundPager(ChromiumWebBrowser browser, HtmlDocument doc, out NextPager? pager)
        {
            var nextPager = new NextPager(browser, doc);
            pager = !nextPager.LastPage ? nextPager : null;
            return pager != null;
        }
    }
}
