using CsvHelper.Configuration;
using CsvHelper;
using HtmlAgilityPack;
using IPTMGrabber.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using IPTMGrabber.YahooFinance;

namespace IPTMGrabber.Edgar
{
    internal class EdgarGrabber
    {
        private const string InsiderUrlFormat = "https://www.sec.gov/cgi-bin/own-disp?action=getissuer&CIK={0}&type=&dateb=&owner=include&start={1}";

        public async Task ExecuteAsync(string dataroot, bool force)
        {
            var web = new HtmlWeb
            {
                CaptureRedirect = true,
                UseCookies = true,
                PreRequest = r =>
                {
                    r.AllowAutoRedirect = true;
                    return true;
                }
            };

            foreach (var quote in Enumerators.EnumerateFromCsv<QuoteDetail>(FileHelper.GetYahooScreenerFilename(dataroot)))
            {
                var end = false;
                var index = 0;
                var filename = Path.Combine(dataroot, "Edgar", "Transactions", $"{quote.Ticker}.csv");


                if ((force || !File.Exists(filename)) && !string.IsNullOrEmpty(quote.Cik))
                {
                    await using var csvWriter = await FileHelper.CreateCsvWriterAsync<InsiderMove>(filename);

                    while (!end)
                    {
                        try
                        {
                            var doc = web.Load(string.Format(InsiderUrlFormat, quote.Cik, index));
                            var moves = doc
                                .ParseTable<InsiderMove>("//table[@id='transaction-report']", preprocess: FixEdgarTable)
                                .Where(m => m.MoveType != MoveType.Unknown)
                                .ToArray();

                            await csvWriter.WriteRecordsAsync(moves);
                            end = moves.Length == 0 || moves.Last().Date < DateTime.Now - TimeSpan.FromDays(365 * 6);
                            index += 80;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                if (File.Exists(filename))
                {
                    var aggregateFilename = Path.Combine(dataroot, "Edgar", "TransactionsPerMonth", $"{quote.Ticker}.csv");
                    var insiderMoves = Enumerators.EnumerateFromCsv<InsiderMove>(filename);
                    await using var aggregateWriter = await FileHelper.CreateCsvWriterAsync<MonthlyTransaction>(aggregateFilename);
                    await aggregateWriter.WriteRecordsAsync(AggregateData(insiderMoves).ToArray());
                }

                Console.WriteLine($"Get Edgar data for {quote.Ticker}");
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

        private IEnumerable<MonthlyTransaction> AggregateData(IEnumerable<InsiderMove> insiderMoves)
        {
            return insiderMoves
                .GroupBy(move => new {move.Date.Year, move.Date.Month})
                .Select(group => new MonthlyTransaction
                {
                    Date = new DateTime(group.Key.Year, group.Key.Month, 1),
                    TotalBuy = group
                        .Where(move => move.MoveType == MoveType.A)
                        .Sum(move => move.NumberOfSecuritiesTransacted),
                    TotalSell = group
                        .Where(move => move.MoveType == MoveType.D)
                        .Sum(move => move.NumberOfSecuritiesTransacted)
                });
        }
    }
}
