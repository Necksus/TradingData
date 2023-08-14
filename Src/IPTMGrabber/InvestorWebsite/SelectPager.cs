﻿using CefSharp;
using CefSharp.DevTools.Page;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using IPTMGrabber.Utils;

namespace IPTMGrabber.InvestorWebsite;

internal class SelectPager : Pager
{
    private readonly ChromiumWebBrowser _browser;
    private readonly HtmlNode _selectNode;
    private readonly string[] _values;

    private int _currentIndex;

    public SelectPager(ChromiumWebBrowser browser, HtmlNode selectNode)
    {
        _browser = browser;
        _selectNode = selectNode;
        _values = selectNode
            .SelectNodes(".//option")
            .Select(o => o.GetUnescapedAttribute("value"))
            .ToArray();

        _currentIndex = 0;
    }

    public override bool LastPage => _currentIndex >= _values.Length;

    public override async Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken)
    {
        _currentIndex++;

        if (!LastPage)
        {
            var script =
                $"var select = document.querySelector('{_selectNode.GetQuerySelector()}');" +
                $" select.value = '{_values[_currentIndex]}';" +
                "select.dispatchEvent(new Event('change'));";
            await _browser.EvaluateScriptAsPromiseAsync(script);

            //File.WriteAllBytes(@"C:\Data\Downloads\ConsoleApp2\screeshot.png", await _browser.CaptureScreenshotAsync(CaptureScreenshotFormat.Png));
            //await _browser.WaitForRenderIdleAsync(cancellationToken: cancellationToken);
            await Task.Delay(300, cancellationToken);
            return await _browser.GetHtmlDocumentAsync(cancellationToken);
        }

        return await base.MoveNextAsync(cancellationToken);
    }

    public static bool FoundPager(ChromiumWebBrowser browser, HtmlDocument doc, out SelectPager? pager)
    {
        //var selectNode = doc.DocumentNode.SelectSingleNode($"//select[option/@value='{DateTime.UtcNow.Year - 1}']");
        var selectNode = doc.DocumentNode.SelectSingleNode($"//select[option[text()='{DateTime.UtcNow.Year - 1}']]");

        pager = selectNode != null ? new SelectPager(browser, selectNode) : null;

        return pager != null;
    }
}