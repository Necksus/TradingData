using Newtonsoft.Json;

namespace IPTMGrabber.DNB
{

    public class Company
    {
        [JsonProperty("companyAddress")]
        public string CompanyAddress { get; set; }

        [JsonProperty("companyCity")]
        public string CompanyCity { get; set; }

        [JsonProperty("companyCountry")]
        public string CompanyCountry { get; set; }

        [JsonProperty("companyProfileLink")]
        public string CompanyProfileLink { get; set; }

        [JsonProperty("companyRegion")]
        public string CompanyRegion { get; set; }

        [JsonProperty("companyZipCode")]
        public string CompanyZipCode { get; set; }

        [JsonProperty("countryRegion")]
        public string CountryRegion { get; set; }

        [JsonProperty("duns")]
        public string Duns { get; set; }

        [JsonProperty("industryName")]
        public string IndustryName { get; set; }

        [JsonProperty("locationType")]
        public string LocationType { get; set; }

        [JsonProperty("primaryName")]
        public string PrimaryName { get; set; }

        [JsonProperty("tradeStyleNames")]
        public string TradeStyleNames { get; set; }

        [JsonProperty("urlSelector")]
        public string UrlSelector { get; set; }
    }

    public class SearchResult2
    {
        [JsonProperty("companies")]
        public List<Company> Companies { get; set; }
    }

}
