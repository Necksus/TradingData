﻿using HtmlAgilityPack;
using IPTMGrabber.Utils;
using PuppeteerSharp;

namespace IPTMGrabber.InvestorWebsite
{
    internal class Pager
    {
        public virtual bool LastPage => false;
        public BrowserService Browser { get; }
        public PagerDefinition? PagerInfo { get; }

        public virtual Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken) => Task.FromResult<HtmlDocument?>(null);

        public Pager(BrowserService browser, PagerDefinition? pagerInfo)
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
                    doc = await Browser.ExecuteJavascriptAsync($"document.querySelector(\"{selector}\")?.click()", cancellationToken);
                }
            }

            return doc;
        }
    }
}
