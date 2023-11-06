﻿using HtmlAgilityPack;
using System.Globalization;
using IPTMGrabber.Utils;
using PuppeteerSharp;
using Microsoft.Extensions.Logging;
using IPTMGrabber.Utils.Browser;

namespace IPTMGrabber.InvestorWebsite
{
    public class NewsAndEventsGrabber
    {
        private readonly ILogger<NewsAndEventsGrabber> _logger;
        private readonly BrowserService _browserService;
        private readonly EarningPredictionModel _earningPrediction;
        private IBrowser _browser;

        public NewsAndEventsGrabber(ILogger<NewsAndEventsGrabber> logger, BrowserService browserService, EarningPredictionModel earningPrediction)
        {
            _logger = logger;
            _browserService = browserService;
            _earningPrediction = earningPrediction;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            foreach (var dataSource in Config.NewsEventsDataSource)
            {
                if (!string.IsNullOrEmpty(dataSource.Ticker))
                {
                    _logger?.LogInformation($"=== Start grabbing data for {dataSource.Ticker}");

                    try
                    {
                        await using var newsOutputFile = File.OpenWrite(Config.GetPressReleasesFilename(dataSource.Ticker));
                        await DownloadAsync(dataSource.NewsUrls, newsOutputFile, cancellationToken);

                        await using var eventsOutputFile = File.OpenWrite(Config.GetEventsFilename(dataSource.Ticker));
                        await DownloadAsync(dataSource.EventsUrls, eventsOutputFile, cancellationToken);
                    }
                    catch(Exception ex)
                    {
                        _logger?.LogError(ex, $"Cannot get data for {dataSource.Ticker}: {ex.Message}");
                    }
                }
            }
        }

        public async Task GrabPressReleasesAsync(string ticker, Stream csvStream, CancellationToken cancellationToken)
        {
            var source = Config.NewsEventsDataSource.FirstOrDefault(s => s.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));
            if (source != null) 
            {
                await DownloadAsync(source.NewsUrls, csvStream, cancellationToken);
            }
        }

        private async Task DownloadAsync(UrlDefinition urlsInfo, Stream csvStream, CancellationToken cancellationToken)
        {
            if (urlsInfo.Urls.Length > 0)
            {
                var allEvents = new List<EventInfo>();
                foreach (var url in urlsInfo.Urls)
                {
                    var doc = await _browserService.OpenUrlAsync(url, cancellationToken);
                    var pager = _browserService.FindPager(urlsInfo.PagerDefinition, doc);
                    var counter = 1;
                    bool newItems;

                    do
                    {
                        doc = await pager.LoadMoreAsync(doc, cancellationToken);

                        var publicationDates = FindPublicationDate(doc.DocumentNode, urlsInfo.DateFormat, urlsInfo.Culture).ToArray();
                        newItems = false;

                        if (publicationDates.Length > 0)
                        {
                            Console.WriteLine($"   - {_browserService.Url} ({counter++})");
                            var events = FindDescriptions(publicationDates);

                            foreach (var eventInfo in events)
                            {
                                if (!allEvents.Contains(eventInfo))
                                {
                                    allEvents.Add(eventInfo);
                                    newItems = true;
                                    Console.WriteLine(eventInfo);
                                }
                            }
                        }
                        else
                        {
                            _logger?.LogWarning("Publication date not found, please check for custum date time format.");
                        }

                        doc = await pager.MoveNextAsync(cancellationToken);

                        await Task.Delay(urlsInfo.Delay ?? 1000, cancellationToken);
                    } while (doc != null && newItems);

                    Console.WriteLine();
                }

                if (allEvents.Count > 0)
                {
                    await using var writer = await FileHelper.CreateCsvWriterAsync<EventInfo>(csvStream);
                    await writer.WriteRecordsAsync(allEvents.OrderByDescending(e => e.Date).ThenBy(e => e.Description), cancellationToken);
                }
            }
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
                if (!string.IsNullOrEmpty(description))
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
            string RemoveTimezone(string value)
            {
                var index = value.LastIndexOf(' ');
                return index > 0 ? value.Substring(0, index) : value;
            }

            if (string.IsNullOrEmpty(datetimeFormat))
                return DateTime.TryParse(text, new CultureInfo(culture ?? "en-US"), out result);

            return DateTime.TryParseExact(text, datetimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ||
                   DateTime.TryParseExact(RemoveTimezone(text), datetimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

            result = default;
            return false;
        }

        private bool TryParseDescription(string text)
            => text.Split(' ').Length > 3; // At least 4 words!
    }
}
