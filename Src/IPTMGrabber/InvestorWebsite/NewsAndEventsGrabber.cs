using System.Drawing;
using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using System.Globalization;
using IPTMGrabber.Utils;

namespace IPTMGrabber.InvestorWebsite
{
    internal class NewsAndEventsGrabber
    {
        public readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

        public async Task ExecuteAsync(string url, CancellationToken cancellationToken)
        {
            using var browser = await CreateBrowserAsync(url);
            var doc = await browser.GetHtmlDocumentAsync(cancellationToken);
            var pager = FindPager(browser, doc);

            Console.WriteLine($"=== {url}");
            do
            {
                var publicationDates = FindPublicationDate(doc.DocumentNode);
                var events = FindDescriptions(publicationDates);

                foreach (var eventInfo in events)
                {
                    Console.WriteLine(eventInfo);
                }

                doc = await pager.MoveNextAsync(cancellationToken);
                
            } while (!pager.LastPage && doc != null);
            Console.WriteLine();
        }

        private Pager FindPager(ChromiumWebBrowser browser, HtmlDocument doc)
        {
            if (LinkPager.FoundPager(browser, doc, out var linkPager))
                return linkPager!;

            if (SelectPager.FoundPager(browser, doc, out var selectPager))
                return selectPager!;
            /*
            var selectNode = doc.DocumentNode.SelectSingleNode($"//select[option/@value='{DateTime.UtcNow.Year - 1}']");

            if (selectNode != null)
            {
                return new SelectPager(browser, selectNode);
            }*/

            return new Pager();
        }

        private IEnumerable<EventInfo> FindDescriptions(IEnumerable<TargetNode<DateTime>> publicationDates)
        {
            string? FindDescription(HtmlNode node)
            {
                if (node.ChildNodes.Count == 0 && TryParseDescription(node.InnerText.Trim()))
                    return node.InnerText.Trim();
                foreach (var childNode in node.ChildNodes)
                {
                    var result = FindDescription(childNode);
                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
                return null;
            }

            var ancestors = publicationDates.Select(d => (HighestParent: d.Node.ParentNode, DateNode: d)).ToArray();
            while (ancestors.All(a => FindDescription(a.HighestParent) == null))
            {
                ancestors = ancestors.Select(a => (CurrentParent: a.HighestParent.ParentNode, DateNode: a.DateNode)).ToArray();
            }

            var descriptions = ancestors
                .Select(a => new EventInfo(a.DateNode.Value, FindDescription(a.HighestParent), ""))
                .Where(e => !string.IsNullOrEmpty(e.Description))
                .ToArray();
            return descriptions;
        }

        private IEnumerable<TargetNode<DateTime>> FindPublicationDate(HtmlNode node)
        {
            IEnumerable<TargetNode<DateTime>> FindAllDates(HtmlNode node, int level)
            {
                if (node.ChildNodes.Count == 0 && TryParseDate(node.InnerText, out var foundDate))
                {
                    yield return new TargetNode<DateTime>(foundDate, node, level);
                }

                foreach (var childNode in node.ChildNodes)
                {
                    foreach (var childDate in FindAllDates(childNode, level + 1))
                    {
                        yield return childDate;
                    }
                }
            }

            var allDates = FindAllDates(node, 0).ToArray();

            var max = allDates
                .GroupBy(node => node.Level)
                .Select(group => new { Level = group.Key, Count = group.Count() })
                .MaxBy(group => group.Count);

            return allDates.Where(d => d.Level == max.Level);
        }


        private bool TryParseDate(string text, out DateTime result)
            => DateTime.TryParse(text, new CultureInfo("en-US"), out result) ||
               DateTime.TryParse(text, new CultureInfo("fr-FR"), out result);

        private bool TryParseDescription(string text)
            => text.Split(' ').Length > 3; // At least 4 words!

        private async Task<ChromiumWebBrowser> CreateBrowserAsync(string url)
        {
            var settings = new CefSettings
            {
                LogSeverity = LogSeverity.Disable,
                BackgroundColor = Cef.ColorSetARGB(255, 255, 255, 255), MultiThreadedMessageLoop = true
            };
            await Cef.InitializeAsync(settings);


            var browser = new ChromiumWebBrowser
            {
                Size = new Size(1920, 4096)
            };

            //browser.LoadingStateChanged += (s, e) =>
            //    Console.WriteLine($"{e.IsLoading} {e.Browser.MainFrame.Url}");

            Console.WriteLine($"Loading {url}...");
            await Task.Delay(500);
            await browser.LoadUrlAsync(url);
            return browser;
        }
    }
}
