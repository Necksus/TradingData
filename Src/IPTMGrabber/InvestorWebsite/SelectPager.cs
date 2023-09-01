using HtmlAgilityPack;
using IPTMGrabber.Utils;
using PuppeteerSharp;

namespace IPTMGrabber.InvestorWebsite;

internal class SelectPager : Pager
{
    private readonly string[] _values;

    private int _currentIndex;
    private HtmlNode? _selectNode;

    public SelectPager(IPage browser, PagerDefinition? pagerInfo, HtmlDocument doc)
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
            await Browser.ExecuteJavascriptAsync(script);
            if (PagerInfo?.MoveNextScript != null)
            {
                await Browser.ExecuteJavascriptAsync(PagerInfo.MoveNextScript);
            }

            var doc = await Browser.GetHtmlDocumentAsync(cancellationToken);
            _selectNode = GetSelectNode(doc);

            return doc;
        }

        return await base.MoveNextAsync(cancellationToken);
    }

    private HtmlNode? GetSelectNode(HtmlDocument doc)
        => doc.DocumentNode.SelectSingleNode($"//select[option[text()='{DateTime.UtcNow.Year - 1}']]");

    public static bool FoundPager(IPage browser, PagerDefinition? pagerInfo, HtmlDocument doc, out SelectPager? pager)
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