using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IPTMGrabber.YahooFinance
{

    // BE CAREFUL HERE: get only minimal information from Yahoo finance, because all numerical values are not reliable (the same
    // data can be different from an API to another, and different from the website). Use Zacks for all financial data!

    public enum SensibilityType
    {
        Cyclical,
        Defensive,
        Both
    }

    public class QuoteDetail
    {
        private static Dictionary<string, SensibilityType> _sensibility = new()
        {
            { "Energy", SensibilityType.Cyclical},

            { "Basic Materials", SensibilityType.Cyclical},
            { "Packaging & Containers", SensibilityType.Defensive },
            { "Paper & Paper Products", SensibilityType.Defensive },

            { "Industrials", SensibilityType.Cyclical},
            { "Aerospace & Defense", SensibilityType.Defensive},
            { "Engineering & Construction", SensibilityType.Both },
            { "Electrical Equipment & Parts", SensibilityType.Both },
            { "Conglomerates", SensibilityType.Both },
            { "Railroads", SensibilityType.Both },
            // ??? "Transportation Infrastructure"


            { "Consumer Cyclical", SensibilityType.Cyclical},

            { "Consumer Defensive", SensibilityType.Defensive},
            { "Beverages—Brewers", SensibilityType.Both },
            { "Household & Personal Products", SensibilityType.Cyclical },

            { "Healthcare", SensibilityType.Defensive},

            { "Financial Services", SensibilityType.Cyclical},
            { "Insurance—Life", SensibilityType.Both },

            { "Technology", SensibilityType.Cyclical},
            { "Communication Equipment", SensibilityType.Both },

            { "Communication Services", SensibilityType.Cyclical},
            // ??? "Diversified Telecommunication Services"
            // ??? "Wireless Telecommunication Services"


            { "Utilities", SensibilityType.Defensive},

            { "Real Estate", SensibilityType.Cyclical}
        };

        public string Ticker { get; set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.industry")]
        public string Industry { get; set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.sector")]
        public string Sector { get; set; }

        [Ignore]
        [JsonPath("$.quoteSummary.result[0].assetProfile.website")]
        public string YahooWebsite { get; set; }

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

        [JsonPath("$.quoteSummary.result[0].calendarEvents.earnings.earningsDate[0].fmt")]
        public string EarningDate1 { get; set; }

        [JsonPath("$.quoteSummary.result[0].calendarEvents.earnings.earningsDate[1].fmt")]
        public string EarningDate2 { get; set; }


        #region From Financial Modeling

        [JsonPath("$.cik")]
        public string Cik { get; set; }

        [JsonPath("$.isin")]
        public string Isin { get; set; }

        [JsonPath("$.cusip")]
        public string Cusip { get; set; }

        #endregion

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

        public SensibilityType? Sensibility
        {
            get
            {
                SensibilityType result;

                if (string.IsNullOrEmpty(Sector) && string.IsNullOrEmpty(Industry))
                    return null;

                if (!_sensibility.TryGetValue(Industry, out result) && !_sensibility.TryGetValue(Sector, out result))
                    throw new ArgumentException("Sensibility not found!");

                return result;
            }
        }


        public QuoteDetail()
        {
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

        public static void FillObjectWithJson(QuoteDetail stocks, string json)
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
