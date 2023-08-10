﻿using System.Drawing;
using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using System.Globalization;
using IPTMGrabber.Utils;
using Newtonsoft.Json;
using System;

namespace IPTMGrabber.InvestorWebsite
{
    internal class NewsAndEventsGrabber
    {
        public async Task ExecuteAsync(string dataroot, CancellationToken cancellationToken)
        {
            var dataSourceFilename = Path.Combine(dataroot, "NewsEvents", "DataSources.json");
            foreach (var dataSource in JsonConvert.DeserializeObject<DataSource[]>(File.ReadAllText(dataSourceFilename))!)
            {
                if (!string.IsNullOrEmpty(dataSource.Ticker))
                {
                    await DownloadAsync(dataSource.EventsUrls, cancellationToken);
                    await DownloadAsync(dataSource.NewsUrls, cancellationToken);
                }
            }
        }


        private async Task DownloadAsync(UrlDefinition urlsInfo, CancellationToken cancellationToken)
        {
            foreach (var url in urlsInfo.Urls)
            {
                using var browser = await CreateBrowserAsync(url);
                var doc = await browser.GetHtmlDocumentAsync(cancellationToken);
                var pager = FindPager(browser, doc);

                Console.WriteLine($"=== {url}");
                do
                {
                    var publicationDates =
                        FindPublicationDate(doc.DocumentNode, urlsInfo.DateFormat, urlsInfo.Culture).ToArray();
                    if (publicationDates.Length > 0)
                    {
                        var events = FindDescriptions(publicationDates);

                        foreach (var eventInfo in events)
                        {
                            Console.WriteLine(eventInfo);
                        }
                    }

                    doc = await pager.MoveNextAsync(cancellationToken);

                    if (urlsInfo.Delay.HasValue)
                        await Task.Delay(urlsInfo.Delay.Value, cancellationToken);
                } while (!pager.LastPage && doc != null);

                Console.WriteLine();
            }
        }

        private Pager FindPager(ChromiumWebBrowser browser, HtmlDocument doc)
        {
            if (NextPager.FoundPager(browser, doc, out var nextPager))
                return nextPager!;

            if (SelectPager.FoundPager(browser, doc, out var selectPager))
                return selectPager!;

            return new Pager();
        }

        private IEnumerable<EventInfo> FindDescriptions(IEnumerable<TargetNode<DateTime>> publicationDates)
        {
            string? FindDescription(HtmlNode candidateNode, HtmlNode dateNode)
            {
                if (candidateNode != dateNode && candidateNode.ChildNodes.Count == 0 && TryParseDescription(candidateNode.GetUnescapedText()))
                    return candidateNode.GetUnescapedText();
                foreach (var childNode in candidateNode.ChildNodes)
                {
                    var result = FindDescription(childNode, dateNode);
                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
                return null;
            }

            var ancestors = publicationDates.Select(d => (HighestParent: d.Node.ParentNode, DateNode: d)).ToArray();
            while (ancestors.All(a => FindDescription(a.HighestParent, a.DateNode.Node) == null))
            {
                ancestors = ancestors.Select(a => (CurrentParent: a.HighestParent.ParentNode, DateNode: a.DateNode)).ToArray();
            }

            var descriptions = ancestors
                .Select(a => new EventInfo(a.DateNode.Value, FindDescription(a.HighestParent, a.DateNode.Node), ""))
                .Where(e => !string.IsNullOrEmpty(e.Description))
                .ToArray();
            return descriptions;
        }

        private IEnumerable<TargetNode<DateTime>> FindPublicationDate(HtmlNode node, string? datetimeFormat, string? culture)
        {
            IEnumerable<TargetNode<DateTime>> FindAllDates(HtmlNode node, int level)
            {
                if (node.ChildNodes.Count == 0 && TryParseDate(node.GetUnescapedText(), datetimeFormat, culture, out var foundDate))
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


        private bool TryParseDate(string text, string? datetimeFormat, string? culture, out DateTime result)
        {
            if (string.IsNullOrEmpty(datetimeFormat))
                return DateTime.TryParse(text, new CultureInfo(culture ?? "en-US"), out result);

            if (text.Length >= datetimeFormat.Length)
            {
                return DateTime.TryParseExact(
                    text.Substring(0, datetimeFormat.Length),
                    datetimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out result);
            }

            result = default;
            return false;
        }

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
