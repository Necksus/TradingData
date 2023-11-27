using Newtonsoft.Json;

namespace IPTMGrabber.YahooFinance
{
    public class FinancialModelingItem
    {
        [JsonProperty("cik")]
        public string Cik { get; set; }

        [JsonProperty("isin")]
        public string Isin { get; set; }

        [JsonProperty("cusip")]
        public string Cusip { get; set; }

        [JsonProperty("symbol")]
        public string Ticker { get; set; }
    }
}
