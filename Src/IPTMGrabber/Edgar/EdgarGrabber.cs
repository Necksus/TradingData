using HtmlAgilityPack;
using IPTMGrabber.InvestorWebsite;
using IPTMGrabber.Utils;
using IPTMGrabber.Utils.Browser;
using IPTMGrabber.YahooFinance;
using Microsoft.Extensions.Logging;

namespace IPTMGrabber.Edgar
{
    public class EdgarGrabber
    {
        private readonly ILogger<EdgarGrabber> _logger;
        private readonly BrowserService _browserService;
        private const string InsiderUrlFormat = "https://www.sec.gov/cgi-bin/own-disp?action=getissuer&CIK={0}&type=&dateb=&owner=include&start=0";

        public EdgarGrabber(ILogger<EdgarGrabber> logger, BrowserService browserService)
        {
            _logger = logger;
            _browserService = browserService;
        }

        public async Task GragInsidersAsync(string ticker, Stream csvStream, CancellationToken cancellationToken)
        {
            var pagerDefinition = new PagerDefinition
            {
                NextButton = "//input[@value='Next 80']",
                NextQuerySelector = "input[value='Next 80']"
            };
            var doc = await _browserService.OpenUrlAsync(string.Format(InsiderUrlFormat, ticker), cancellationToken);
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
