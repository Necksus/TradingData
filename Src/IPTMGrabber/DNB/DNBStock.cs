using CsvHelper.Configuration.Attributes;

namespace IPTMGrabber.DNB
{
    internal class DNBStock
    {

        public string Ticker { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }

        [Ignore]
        public string DNBExchange { get; set; }
        [Ignore]
        public string DNBTicker { get; set; }

        public string NaicsIndusties1 { get; set; }
        public string NaicsIndusties2 { get; set; }

        public DNBStock(string ticker, string name, string website, string[] naicsIndusties, string stockExchange)
        {
            var exchangeParts = stockExchange?.Split(':', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
            Ticker = ticker;
            Name = name;
            Website = website;
            NaicsIndusties1 = naicsIndusties.Length > 0 ? naicsIndusties[0] : "";
            NaicsIndusties2 = naicsIndusties.Length > 1 ? naicsIndusties[1] : "";
            DNBExchange = exchangeParts?.Length == 2 ? exchangeParts[0] : "";
            DNBTicker = exchangeParts?.Length == 2 ? exchangeParts[1] : "";
        }

        public DNBStock()
        {
        }
    }
}
