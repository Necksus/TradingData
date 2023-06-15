using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IPTMGrabber.YahooFinance
{

    // BE CAREFUL HERE: get only minimal information from Yahoo finance, because all numerical values are not reliable (the same
    // data can be different from an API to another, and different from the website). Use Zacks for all financial data!

    public class QuoteDetail
    {
        public string Ticker { get; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.industry")]
        public string Industry { get; private set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.industryDisp")]
        public string IndustryDisp { get; private set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.sector")]
        public string Sector { get; private set; }

        [Ignore]
        [JsonPath("$.quoteSummary.result[0].assetProfile.website")]
        public string YahooWebsite { get; private set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.address1")]
        public string Address1 { get; set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.address2")]
        public string Address2 { get; set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.city")]
        public string City { get; set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.zip")]
        public string Zip { get; set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.state")]
        public string State { get; set; }
        

        public string WebSite
        {
            get
            {
                if (!string.IsNullOrEmpty(YahooWebsite))
                {
                    var uri = new Uri(YahooWebsite);
                    return uri.Host.StartsWith("www.") ? uri.Host.Substring(4) : uri.Host;
                }

                return "";
            }
        }

        public QuoteDetail(string ticker)
        {
            Ticker = ticker;
        }

        public static QuoteDetail FromJson(string ticker, string json)
        {
            var stocks = new QuoteDetail(ticker);
            FillObjectWithJson(stocks, json);

            return stocks;
        }

        private static void FillObjectWithJson(QuoteDetail stocks, string json)
        {
            var jsonObject = JsonConvert.DeserializeObject<JObject>(json);

            // Parcourir toutes les propriétés de la classe Stocks
            foreach (var propertyInfo in typeof(QuoteDetail).GetProperties())
            {
                // Vérifier si la propriété a l'attribut JsonPath
                var jsonPathAttribute = propertyInfo.GetCustomAttributes(typeof(JsonPathAttribute), true)
                    .FirstOrDefault() as JsonPathAttribute;

                if (jsonPathAttribute != null)
                {
                    // Utiliser le chemin JSON pour extraire la valeur correspondante
                    var jsonPath = jsonPathAttribute.Path;
                    var jsonValue = jsonObject.SelectToken(jsonPath);

                    // Vérifier si la valeur est de type compatible avec la propriété
                    if (jsonValue != null && jsonValue.Type != JTokenType.Null)
                    {
                        var propertyType = propertyInfo.PropertyType;
                        var propertyValue = jsonValue.ToObject(propertyType);

                        propertyInfo.SetValue(stocks, propertyValue);
                    }
                }
            }
        }
    }
}
