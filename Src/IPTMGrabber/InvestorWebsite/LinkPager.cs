using CefSharp.OffScreen;
using HtmlAgilityPack;
using IPTMGrabber.Utils;

namespace IPTMGrabber.InvestorWebsite
{
    internal class LinkPager : Pager
    {
        private readonly ChromiumWebBrowser _browser;
        
        private TargetNode<int>[] _pages;
        public int CurrentPage { get; private set; }

        public override bool LastPage => _pages.Length == 0 || CurrentPage >= _pages.Max(p => p.Value);

        public LinkPager(ChromiumWebBrowser browser, PagerDefinition? pagerInfo, HtmlDocument doc) : base(browser, pagerInfo)
        {
            _browser = browser;
            _pages = FindAllPages(doc);
            CurrentPage = _pages.Any() ? _pages.Min(p => p.Value) : 0;
        }

        public override async Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken)
        {
            CurrentPage++;
            if (!LastPage)
            {
                var ancestor = _pages.Single(p => p.Value == CurrentPage).Node;
                while (ancestor.Attributes["href"] == null)
                {
                    ancestor = ancestor.ParentNode;
                }
                var href = ancestor.GetUnescapedAttribute("href");
                if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out var url))
                {
                    if (!url.IsAbsoluteUri)
                        url = new Uri(new Uri(_browser.Address), url);

                    await _browser.LoadUrlAsync(url.AbsoluteUri);
                    var doc = await _browser.GetHtmlDocumentAsync(cancellationToken);
                    _pages = FindAllPages(doc);

                    return doc;
                }
            }

            return await base.MoveNextAsync(cancellationToken);
        }

        public static bool FoundPager(ChromiumWebBrowser browser, PagerDefinition pagerInfo, HtmlDocument doc, out LinkPager? pager)
        {
            var linkPager = new LinkPager(browser, pagerInfo, doc);

            pager = linkPager.CurrentPage == 1 ? linkPager : null;

            return pager != null;
        }

        private TargetNode<int>[] FindAllPages(HtmlDocument doc)
        {
            var allNumbers = FindAllNumbers(doc.DocumentNode, 0).ToArray();
            if (allNumbers.Length > 1)
            {
                var pageLevel = allNumbers
                    .GroupBy(node => node.Level)
                    .Select(group => new { Level = group.Key, Count = group.Count() })
                    .MaxBy(group => group.Count)!
                .Level;

                return allNumbers.Where(n => n.Level == pageLevel).ToArray();
            }

            return Array.Empty<TargetNode<int>>();
        }

        private IEnumerable<TargetNode<int>> FindAllNumbers(HtmlNode node, int level)
        {
            if (node.Name.Equals("select", StringComparison.OrdinalIgnoreCase))
                yield break;

            if (node.ChildNodes.Count == 0 && int.TryParse(node.InnerText.Trim(), out var foundNumber))
            {
                yield return new TargetNode<int>(foundNumber, node, level);
            }

            foreach (var childNode in node.ChildNodes)
            {
                foreach (var childDate in FindAllNumbers(childNode, level + 1))
                {
                    yield return childDate;
                }
            }
        }
    }
}
