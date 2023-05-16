using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace IPTMGrabber.Investing
{

    public sealed class RecordMap : ClassMap<Record>
    {
        private readonly string _valueColumName;

        public RecordMap(string valueColumName)
        {
            _valueColumName = valueColumName;

            Map(p => p.Date)
                .Name("Date")
                .TypeConverterOption.Format("yyyy-MM-dd");
            Map(p => p.Actual).Name(_valueColumName);
        }
    }

    public class Record
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("actual_state")]
        public string ActualState { get; set; }

        [JsonProperty("actual")]
        public double Actual { get; set; }

        [JsonProperty("actual_formatted")]
        public string ActualFormatted { get; set; }

        [JsonProperty("forecast")]
        public double? Forecast { get; set; }

        [JsonProperty("forecast_formatted")]
        public string ForecastFormatted { get; set; }

        [JsonProperty("revised")]
        public double? Revised { get; set; }

        [JsonProperty("revised_formatted")]
        public string RevisedFormatted { get; set; }

        [JsonIgnore]
        public DateTime Date
            => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;

        public override string ToString()
            => $"{Date} : {Actual}";
    }

    public class InvestingData
    {
        public InvestingData(List<Record> attr)
        {
            this.Records = attr;
        }

        [JsonProperty("attr")]
        public List<Record> Records { get; set; }
    }
}
