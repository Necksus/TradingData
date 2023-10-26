using HtmlAgilityPack;
using IPTMGrabber.InvestorWebsite;
using IPTMGrabber.Utils;
using IPTMGrabber.Utils.Browser;
using IPTMGrabber.YahooFinance;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace IPTMGrabber.Edgar
{
    public class EdgarService
    {
        private readonly string[] _filingsTypes =
        {
            "8-K", "8-K/A", "6-K", "6-K/A",
            "10-K", "10-K/A", "NT 10-K", "NT 10-K/A",
            "10-Q", "10-Q/A", "NT 10-Q", "NT 10-Q/A",
            "10-Q", "10-Q/A", "NT 10-Q", "NT 10-Q/A",
            "20-F", "20-F/A", "NT 20-F", "NT 20-F/A",
            "DEF 14A"
        };


        private readonly ILogger<EdgarService> _logger;
        private readonly BrowserService _browserService;
        private readonly YahooService _yahooService;

        private const string InsiderUrlFormat = "https://www.sec.gov/cgi-bin/own-disp?action=getissuer&CIK={0}&type=&dateb=&owner=include&start=0";

        // http://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&output=xml&start=10&type=8-K&datea=20081005&dateb=20231002&ownership=include&CIK=0001739940
        private const string FillingUrlFormat = "https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&output=xml&start={0}&type={1}&datea{2}&dateb={3}&ownership=include&CIK={4}";

        public EdgarService(ILogger<EdgarService> logger, BrowserService browserService, YahooService yahooService)
        {
            _logger = logger;
            _browserService = browserService;
            _yahooService = yahooService;
        }

        public async Task GrabInsidersAsync(string ticker, Stream csvStream, CancellationToken cancellationToken)
        {
            var cikCode = _yahooService.GetCIK(ticker);
            if (string.IsNullOrEmpty(cikCode))
                throw new ArgumentException($"CIK code not found for ticker {ticker}");

            var pagerDefinition = new PagerDefinition
            {
                NextButton = "//input[@value='Next 80']",
                NextQuerySelector = "input[value='Next 80']"
            };
            var doc = await _browserService.OpenUrlAsync(string.Format(InsiderUrlFormat, cikCode), cancellationToken);
            var pager = _browserService.FindPager(pagerDefinition, doc);
            await using var writer = await FileHelper.CreateCsvWriterAsync<InsiderMove>(csvStream);

            while (!pager.LastPage)
            {
                _logger?.LogInformation($"Grabbing from {_browserService.Url}");

                var moves = doc
                    .ParseTable<InsiderMove>("//table[@id='transaction-report']", preprocess: FixEdgarTable)
                    .Where(m => m.MoveType != MoveType.Unknown)
                    .ToArray();

                await writer.WriteRecordsAsync(moves, cancellationToken);
                doc = await pager.MoveNextAsync(cancellationToken);
            }
        }

        public async Task GrabFillings(string ticker, Stream csvStream, CancellationToken cancellationToken)
        {
            var cikCode = _yahooService.GetCIK(ticker);
            if (string.IsNullOrEmpty(cikCode))
                throw new ArgumentException($"CIK code not found for ticker {ticker}");

            var endDate = DateTime.Now.Date;
            var startDate = endDate - TimeSpan.FromDays(365 * 15);
            using var client = new HttpClient();
            var fillings = new List<Filling>();

            void UpdateUserAgent()
            {
                client.DefaultRequestHeaders.Remove("User-Agent");
                client.DefaultRequestHeaders.Add("User-Agent", RandomUserAgentGenerator.Next());
            }

            UpdateUserAgent();
            foreach (var fillingType in _filingsTypes)
            {
                var index = 0;
                var oldIndex = 0; ;
                do
                {
                    oldIndex = index;
                    var url = string.Format(FillingUrlFormat, index, fillingType.Replace(" ", "%20"), startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"), cikCode);
                    using var request = await client.GetAsync(url);
                    if (request.IsSuccessStatusCode)
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(await request.Content.ReadAsStringAsync(cancellationToken));
                        var fillingNodes = doc.SelectNodes("//filing/filingHREF");

                        foreach (XmlNode hrefNode in fillingNodes)
                        {
                            var detailUrl = hrefNode.InnerText.Replace("-index.html", ".txt").Replace("-index.htm", ".txt");
                            var filePath = Data.GetSECDetailPathFromUrl(fillingType, ticker, detailUrl);

                            _logger.LogDebug($"Downloading {detailUrl}");
                            var detail = await client.ReadOrDownloadAsync(detailUrl, filePath, cancellationToken);
                            var acceptanceDate = FindDate(detail, "<ACCEPTANCE-DATETIME>", "yyyyMMddHHmmss");
                            var filledDate = FindDate(detail, "FILED AS OF DATE:\\s*", "yyyyMMdd");
                            string pattern = @"ITEM INFORMATION:(.*?)\r?\n";
                            var allInformation = Regex.Matches(detail, pattern).Select(match => match.Groups[1].Value.Trim()).ToList();

                            //if (!allInformation.Any() && detail.Contains("ITEM INFORMATION:"))
                            if (!allInformation.Any())
                            {
                                allInformation.Add("");
                            }

                            foreach (var itemInformation in allInformation)
                            {
                                fillings.Add(new Filling(acceptanceDate, Path.GetFileName(filePath), fillingType, filledDate, itemInformation));
                            }
                            UpdateUserAgent();
                        }
                        index += fillingNodes.Count;
                    }
                } while (index > oldIndex);
            }

            await using var writer = await FileHelper.CreateCsvWriterAsync<Filling>(csvStream);
            await writer.WriteRecordsAsync(fillings.OrderByDescending(e => e.FiledAsOfDate).ThenBy(e => e.Type), cancellationToken);
        }

        private DateTime? FindDate(string detail, string name, string pattern)
        {
            Match match = Regex.Match(detail, $@"{name}(\d{{{pattern.Length}}})");
            if (match.Success && DateTime.TryParseExact(match.Groups[1].Value, pattern, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                return result;

            return null;
        }

        private string FixEdgarTable(int column, string value)
        {
            switch (column)
            {
                case 0:
                    return value == "-" ? MoveType.Unknown.ToString() : value;
                case 1:
                    return value == "-" ? "" : value;
                case 5:
                    return value.Substring(value.IndexOf("-"));
                case 8:
                    return value.TrimStart('$');
                default:
                    return value;
            }
        }
    }
}
