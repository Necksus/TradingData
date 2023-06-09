using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IPTMGrabber.YahooFinance
{
    public class QuoteDetail
    {
        public string Ticker { get; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.industry")]
        public string Industry { get; private set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.industryDisp")]
        public string IndustryDisp { get; private set; }

        [JsonPath("$.quoteSummary.result[0].assetProfile.sector")]
        public string Sector { get; private set; }

        [JsonPath("$.quoteSummary.result[0].defaultKeyStatistics.enterpriseValue.raw")]
        public long EnterpriseValue { get; private set; }

        [JsonPath("$.quoteSummary.result[0].defaultKeyStatistics.shortRatio.raw")]
        public double ShortRatio { get; private set; }

        [JsonPath("$.quoteSummary.result[0].defaultKeyStatistics.beta.raw")]
        public double Beta { get; private set; }

        [JsonPath("$.quoteSummary.result[0].defaultKeyStatistics.bookValue.raw")]
        public double BookValue { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.totalCash.raw")]
        public long TotalCash { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.ebitda.raw")]
        public long Ebitda { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.totalDebt.raw")]
        public long TotalDebt { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.quickRatio.raw")]
        public double QuickRatio { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.currentRatio.raw")]
        public double CurrentRatio { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.totalRevenue.raw")]
        public long TotalRevenue { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.debtToEquity.raw")]
        public double DebtToEquity { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.returnOnAssets.raw")]
        public double ReturnOnAssets { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.returnOnEquity.raw")]
        public double ReturnOnEquity { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.freeCashflow.raw")]
        public double FreeCashflow { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.grossMargins.raw")]
        public double GrossMargins { get; private set; }

        [JsonPath("$.quoteSummary.result[0].financialData.grossProfits.raw")]
        public long GrossProfits { get; private set; }


        #region Recommendation 1
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[0].period")]
        public string TrendPeriod1 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[0].strongBuy")]
        public int TrendStrongBuy1 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[0].buy")]
        public int TrendBuy1 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[0].hold")]
        public int TrendHold1 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[0].sell")]
        public int TrendSell1 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[0].strongSell")]
        public int TrendStrongSell1 { get; private set; }
        #endregion

        #region Recommendation 2
        [JsonPath("$.quoteSummary.result[1].recommendationTrend.trend[1].period")]
        public string TrendPeriod2 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[1].strongBuy")]
        public int TrendStrongBuy2 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[1].buy")]
        public int TrendBuy2 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[1].hold")]
        public int TrendHold2 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[1].sell")]
        public int TrendSell2 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[1].strongSell")]
        public int TrendStrongSell2 { get; private set; }
        #endregion

        #region Recommendation 3
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[2].period")]
        public string TrendPeriod3 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[2].strongBuy")]
        public int TrendStrongBuy3 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[2].buy")]
        public int TrendBuy3 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[2].hold")]
        public int TrendHold3 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[2].sell")]
        public int TrendSell3 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[2].strongSell")]
        public int TrendStrongSell3 { get; private set; }
        #endregion

        #region Recommendation 4
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[3].period")]
        public string TrendPeriod4 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[3].strongBuy")]
        public int TrendStrongBuy4 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[3].buy")]
        public int TrendBuy4 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[3].hold")]
        public int TrendHold4 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[3].sell")]
        public int TrendSell4 { get; private set; }
        [JsonPath("$.quoteSummary.result[0].recommendationTrend.trend[3].strongSell")]
        public int TrendStrongSell4 { get; private set; }
        #endregion

        public QuoteDetail(string ticker)
        {
            Ticker = ticker;
        }

        public static QuoteDetail FromJson(string ticker, string json)
        {
            var stocks = new QuoteDetail(ticker);
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

            return stocks;
        }
    }
}
