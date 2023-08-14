using System.Drawing;
using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using System.Globalization;
using IPTMGrabber.Utils;
using Newtonsoft.Json;
using System;
using IPTMGrabber.DNB;

namespace IPTMGrabber.InvestorWebsite
{
    internal class NewsAndEventsGrabber
    {
        private EarningPredictionModel _earningPrediction;

        public async Task ExecuteAsync(string dataroot, CancellationToken cancellationToken)
        {
            var dataSourceFilename = Path.Combine(dataroot, "NewsEvents", "DataSources.json");
            _earningPrediction = new EarningPredictionModel(dataroot);

            foreach (var dataSource in JsonConvert.DeserializeObject<DataSource[]>(File.ReadAllText(dataSourceFilename))!)
            {
                if (!string.IsNullOrEmpty(dataSource.Ticker))
                {
                    Console.WriteLine($"=== Start grabbing data for {dataSource.Ticker}");

                    await DownloadAsync(dataSource.NewsUrls, Path.Combine(dataroot, "NewsEvents", "News", $"{dataSource.Ticker}.csv"), cancellationToken);
                    await DownloadAsync(dataSource.EventsUrls, Path.Combine(dataroot, "NewsEvents", "Events", $"{dataSource.Ticker}.csv"), cancellationToken);
                }
            }
        }


        private async Task DownloadAsync(UrlDefinition urlsInfo, string csvFilename, CancellationToken cancellationToken)
        {
            if (urlsInfo.Urls.Length > 0)
            {
                var allEvents = new List<EventInfo>();
                foreach (var url in urlsInfo.Urls)
                {
                    using var browser = await CreateBrowserAsync(url);
                    var doc = await browser.GetHtmlDocumentAsync(cancellationToken);
                    var pager = FindPager(browser, doc, urlsInfo);
                    var counter = 1;
                    bool newItems;

                    do
                    {
                        var publicationDates = FindPublicationDate(doc.DocumentNode, urlsInfo.DateFormat, urlsInfo.Culture).ToArray();
                        newItems = false;

                        if (publicationDates.Length > 0)
                        {
                            Console.WriteLine($"   - {browser.Address} ({counter++})");
                            var events = FindDescriptions(publicationDates);

                            foreach (var eventInfo in events)
                            {
                                if (!allEvents.Contains(eventInfo))
                                {
                                    allEvents.Add(eventInfo);
                                    newItems = true;
                                    //Console.WriteLine(eventInfo);
                                }
                            }
                        }

                        doc = await pager.MoveNextAsync(cancellationToken);

                        await Task.Delay(urlsInfo.Delay ?? 1000, cancellationToken);
                    } while (doc != null && newItems);

                    Console.WriteLine();
                }

                if (allEvents.Count > 0)
                {
                    await using var writer = await FileHelper.CreateCsvWriterAsync<EventInfo>(csvFilename);
                    await writer.WriteRecordsAsync(allEvents.OrderByDescending(e => e.Date), cancellationToken);
                }
            }
        }

        private Pager FindPager(ChromiumWebBrowser browser, HtmlDocument doc, UrlDefinition urlInfo)
        {
            if (NextPager.FoundPager(browser, doc, urlInfo.NextButton, out var nextPager))
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

            foreach (var ancestor in ancestors)
            {
                var description = FindDescription(ancestor.HighestParent, ancestor.DateNode.Node);
                yield return new EventInfo(ancestor.DateNode.Value, description, "", _earningPrediction.PredictEarning(description!));
            }
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

            return (DateTime.TryParseExact(text, datetimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ||
                    (text.Length>4 && DateTime.TryParseExact(text.Substring(0, text.Length-4), datetimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result)));

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

            await Task.Delay(500);
            await browser.LoadUrlAsync(url);
            return browser;
        }
    }
}
