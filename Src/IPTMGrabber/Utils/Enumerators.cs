using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using IPTMGrabber.YahooFinance;

namespace IPTMGrabber.Utils
{
    public class Enumerators
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
            => EnumerateFromCsv<T>(File.OpenRead(filename));


        public static IEnumerable<T> EnumerateFromCsv<T>(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(new CultureInfo("fr-FR"))
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ",",
                MissingFieldFound=null
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
