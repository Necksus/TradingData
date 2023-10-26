using CsvHelper;
using CsvHelper.Configuration;
using IPTMGrabber.Edgar;
using System.Globalization;

namespace IPTMGrabber.Utils
{
    internal static class FileHelper
    {
        public static async Task<CsvWriter> CreateCsvWriterAsync<T>(string filename)
        {
            await using var csvStream = File.OpenRead(filename);
            return await CreateCsvWriterAsync<T>(csvStream);
        }

        public static async Task<CsvWriter> CreateCsvWriterAsync<T>(Stream csvStream)
        {
            var csvWriter = new CsvWriter(
                new StreamWriter(csvStream, System.Text.Encoding.UTF8),
                new CsvConfiguration(new CultureInfo("fr-FR"))
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                },
                true);
            csvWriter.WriteHeader<T>();
            await csvWriter.NextRecordAsync();

            return csvWriter;
        }
    }
}
