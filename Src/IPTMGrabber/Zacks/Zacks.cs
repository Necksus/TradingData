using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace IPTMGrabber.Zacks
{
    internal class Zacks
    {
        public static IEnumerable<string> GetTickers(string dataRoot)
        {
            // Prepare reader
            using var reader = new StreamReader(Path.Combine(dataRoot, "Zacks", "Screener.csv"));
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
    }
}
