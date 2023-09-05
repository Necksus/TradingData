using HtmlAgilityPack;
using IPTMGrabber.Utils;
using PuppeteerSharp;

namespace IPTMGrabber.InvestorWebsite
{
    internal class NextPager : Pager
    {
        private HtmlNode? _nextNode;

        public override bool LastPage => _nextNode == null;

        public NextPager(BrowserService browser, PagerDefinition? pagerInfo, HtmlDocument doc) : base(browser, pagerInfo)
        {
            FindNextLink(doc);
        }

        public override async Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken)
        {
            if (!LastPage)
            {
                var doc = await TryNavigateHrefAsync(cancellationToken) ?? await TryClickAsync(cancellationToken);
                if (doc != null)
                {
                    FindNextLink(doc);
                    return doc;
                }
            }

            return await base.MoveNextAsync(cancellationToken);
        }

        private async Task<HtmlDocument?> TryClickAsync(CancellationToken cancellationToken)
        {
            var selector = _nextNode?.GetQuerySelector();

            if (!string.IsNullOrEmpty(selector))
            {
                return await Browser.ExecuteJavascriptAsync($"document.querySelector(\"{selector}\").click()", cancellationToken);
            }

            return null;
        }

        private async Task<HtmlDocument?> TryNavigateHrefAsync(CancellationToken cancellationToken)
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
                            url = new Uri(new Uri(Browser.Url), url);

                        if (!url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) && !url.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                            return null;

                        return await Browser.OpenUrlAsync(url.AbsoluteUri, cancellationToken);
                    }
                }

                ancestor = ancestor.ParentNode;
                level++;
            }
            return null;
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

        public static bool FoundPager(BrowserService browser, PagerDefinition? pagerInfo, HtmlDocument doc, out NextPager? pager)
        {
            var nextPager = new NextPager(browser, pagerInfo, doc);
            pager = !nextPager.LastPage ? nextPager : null;
            return pager != null;
        }
    }
}
