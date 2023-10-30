// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System.Globalization;
using CsvHelper;
using IPTMGrabber.Utils;
using Newtonsoft.Json;

namespace IPTMGrabber.Investing
{

    public enum InvestingType
    {
        Migigan_ConsumerExpectations,
        Michigan_UMCSI,
        Michigan_CurrentCondition
    }

    class InvestingGrabber
    {
        private Dictionary<InvestingType, string> _datasets = new Dictionary<InvestingType, string>()
        {
            {InvestingType.Migigan_ConsumerExpectations, "https://sbcharts.investing.com/events_charts/us/900.json"},
            {InvestingType.Michigan_UMCSI, "https://sbcharts.investing.com/events_charts/us/320.json"},
            {InvestingType.Michigan_CurrentCondition, "https://sbcharts.investing.com/events_charts/us/901.json"}
        };

        public async Task DownloadAllAsync()
        {
            foreach (var dataset in _datasets)
            {
                var client = new HttpClient();
                var json = await client.GetStringAsync(dataset.Value);

                var investingData = JsonConvert.DeserializeObject<InvestingData>(json);

                await using var writer = new StreamWriter(GetFilename(dataset.Key));
                await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap(new RecordMap(GetColumnName(dataset.Key)));

                await csv.WriteRecordsAsync(investingData!.Records);
            }
        }

        private string GetColumnName(InvestingType type)
        {
            switch (type)
            {
                case InvestingType.Migigan_ConsumerExpectations:
                    return "Consumer expectations";
                case InvestingType.Michigan_UMCSI:
                    return "UMCSI";
                case InvestingType.Michigan_CurrentCondition:
                    return "Current conditions";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private string GetFilename(InvestingType type)
        {
            switch (type)
            {
                case InvestingType.Migigan_ConsumerExpectations:
                case InvestingType.Michigan_UMCSI:
                case InvestingType.Michigan_CurrentCondition:
                    return Path.Combine(Config.GetInvestingFolder(), $"{type}.csv");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}