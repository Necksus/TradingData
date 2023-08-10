using CefSharp.DevTools.Page;
using HtmlAgilityPack;

namespace IPTMGrabber.InvestorWebsite
{
    internal class Pager
    {
        public virtual bool LastPage => false;

        public virtual Task<HtmlDocument?> MoveNextAsync(CancellationToken cancellationToken) => Task.FromResult<HtmlDocument?>(null);
    }
}
