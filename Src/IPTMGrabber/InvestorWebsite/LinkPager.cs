using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.OffScreen;
using HtmlAgilityPack;

namespace IPTMGrabber.InvestorWebsite
{
    internal class LinkPager : Pager
    {
        private readonly ChromiumWebBrowser _browser;

        public LinkPager(ChromiumWebBrowser browser)
        {
            _browser = browser;
        }

        public static bool FoundPager(ChromiumWebBrowser browser, HtmlDocument doc, out LinkPager pager)
        {
            throw new NotImplementedException();
        }
    }
}
