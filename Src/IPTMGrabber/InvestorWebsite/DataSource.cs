namespace IPTMGrabber.InvestorWebsite
{

    internal class UrlDefinition
    {
        public string[] Urls { get; set; }

        public string? DateFormat { get; set; }

        public string? Culture { get; set; }

        public int? Delay { get; set; }

        public string? NextButton { get; set; }
    }

    internal class DataSource
    {
        public string Ticker { get; set; }

        public UrlDefinition NewsUrls { get; set; }
        
        public UrlDefinition EventsUrls { get; set; }
        
        public override string ToString()
            => $"{Ticker}";
    }
}
