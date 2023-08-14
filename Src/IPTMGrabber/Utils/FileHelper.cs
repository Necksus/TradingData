using CsvHelper;
using CsvHelper.Configuration;
using IPTMGrabber.Edgar;
using System.Globalization;

namespace IPTMGrabber.Utils
{
    internal static class FileHelper
    {
        public static string GetYahooScreenerFilename(string dataRoot)
            => Path.Combine(dataRoot, "YahooFinance", "ScreenerDetails.csv");

        public static async Task<CsvWriter> CreateCsvWriterAsync<T>(string filename)
        {
            var writer = new StreamWriter(filename);
            var csvWriter = new CsvWriter(writer, new CsvConfiguration(new CultureInfo("fr-FR"))
            {
                Delimiter = ",",
                HasHeaderRecord = true,
            });
            csvWriter.WriteHeader<T>();
            await csvWriter.NextRecordAsync();
            
            return csvWriter;
        }
    }
}
