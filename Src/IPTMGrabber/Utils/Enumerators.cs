using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using IPTMGrabber.YahooFinance;

namespace IPTMGrabber.Utils
{
    internal class Enumerators
    {
        public static IEnumerable<string> GetTickers()
        {
            // Prepare reader
            using var reader = new StreamReader(Config.GetZacksScreener());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ","
            });
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                yield return csv.GetField<string>("Ticker")!;
            }
        }

        public static IEnumerable<T> EnumerateFromCsv<T>(string filename)
        {
            using var reader = new StreamReader(filename);
            using var csv = new CsvReader(reader, new CsvConfiguration(new CultureInfo("fr-FR"))
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ","
            });
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                yield return csv.GetRecord<T>()!;
            }
        }
    }
}
