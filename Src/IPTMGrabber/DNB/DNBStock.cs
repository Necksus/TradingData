namespace IPTMGrabber.DNB
{
    internal class DNBStock
    {

        public string Ticker { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public string NaicsIndusties1 { get; set; }
        public string NaicsIndusties2 { get; set; }

        public DNBStock(string ticker, string name, string website, string[] naicsIndusties)
        {
            Ticker = ticker;
            Name = name;
            Website = website;
            NaicsIndusties1 = naicsIndusties.Length > 0 ? naicsIndusties[0] : "";
            NaicsIndusties2 = naicsIndusties.Length > 1 ? naicsIndusties[1] : "";
        }

        public DNBStock()
        {
        }
    }
}
