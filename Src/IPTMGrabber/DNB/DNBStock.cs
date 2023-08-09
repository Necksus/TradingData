using CsvHelper.Configuration.Attributes;

namespace IPTMGrabber.DNB
{
    internal class DNBStock
    {

        public string Ticker { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }

        public string DNBExchange { get; set; }
        public string DNBTicker { get; set; }

        public string NaicsSector { get; set; }
        public string NaicsIndustry { get; set; }

        public string KeyPrincipal { get; set; }
        public string DetailLink { get; set; }
        public string Description { get; set; }

        public DNBStock(string ticker,
            string name,
            string website,
            string naicsSector,
            string naicsIndustry,
            string stockExchange,
            string keyPrincipal,
            string detailLink,
            string description)
        {
            var exchangeParts = stockExchange?.Split(':', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
            Ticker = ticker;
            Name = name;
            Website = website;
            NaicsSector = naicsSector;
            NaicsIndustry = naicsIndustry;
            KeyPrincipal = keyPrincipal;
            Description = description;
            DetailLink = detailLink;
            DNBExchange = exchangeParts?.Length == 2 ? exchangeParts[0] : "";
            DNBTicker = exchangeParts?.Length == 2 ? exchangeParts[1] : "";
        }

        public DNBStock()
        {
        }
    }
}
