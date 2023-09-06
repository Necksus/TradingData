namespace IPTMGrabber.InvestorWebsite
{

    public class PagerDefinition
    {
        public string? NextButton { get; set; }
        public string? NextQuerySelector { get; set; }
        public string? MoreButton { get; set; }
        public string? MoveNextScript { get; set; }
    }

    internal class UrlDefinition
    {
        public string[] Urls { get; set; }

        public string? DateFormat { get; set; }

        public string? Culture { get; set; }

        public int? Delay { get; set; }

        public PagerDefinition? PagerDefinition { get; set; }
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
