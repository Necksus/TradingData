using HtmlAgilityPack;
using IPTMGrabber.InvestorWebsite;
using IPTMGrabber.Utils;
using PuppeteerSharp;

namespace IPTMGrabber.Utils.Browser;

internal class SelectPager : Pager
{
    private readonly string[] _values;

    private int _currentIndex;
    private HtmlNode? _selectNode;

    public SelectPager(BrowserService browser, PagerDefinition? pagerInfo, HtmlDocument doc)
        : base(browser, pagerInfo)
    {
        _selectNode = GetSelectNode(doc);
        _values = _selectNode
            ?.SelectNodes(".//option")
            ?.Select(o => o.GetUnescapedAttribute("value"))
            ?.ToArray() ?? Array.Empty<string>();

        _currentIndex = 0;
    }

    public override bool LastPage => _selectNode == null || _currentIndex >= _values.Length;

    public override async Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken)
    {
        _currentIndex++;

        if (!LastPage)
        {
            var script =
                $"var select = document.querySelector(\"{_selectNode.GetQuerySelector()}\");" +
                $" select.value = '{_values[_currentIndex]}';" +
                "select.dispatchEvent(new Event('change'));";
            var doc = await Browser.ExecuteJavascriptAsync(script, cancellationToken);
            if (PagerInfo?.MoveNextScript != null)
            {
                doc = await Browser.ExecuteJavascriptAsync(PagerInfo.MoveNextScript, cancellationToken);
            }

            _selectNode = GetSelectNode(doc);

            return doc;
        }

        return await base.MoveNextAsync(cancellationToken);
    }

    private HtmlNode? GetSelectNode(HtmlDocument doc)
        => doc.DocumentNode.SelectSingleNode($"//select[option[text()='{DateTime.UtcNow.Year - 1}']]");

    public static bool FoundPager(BrowserService browser, PagerDefinition? pagerInfo, HtmlDocument doc, out SelectPager? pager)
    {
        var nextPager = new SelectPager(browser, pagerInfo, doc);
        pager = !nextPager.LastPage ? nextPager : null;
        return pager != null;
        /*
        var selectNode = doc.DocumentNode.SelectSingleNode($"//select[option[text()='{DateTime.UtcNow.Year - 1}']]");

        pager = selectNode != null ? new SelectPager(browser, pagerInfo, selectNode) : null;

        return pager != null;

        */
    }
}