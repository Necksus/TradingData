using IPTMGrabber.InvestorWebsite;
using IPTMGrabber.Nasdaq;
using Newtonsoft.Json;
using System.Data;

namespace IPTMGrabber.Utils
{
    internal static class Data
    {
        // Thanks to Scott https://www.hanselman.com/blog/detecting-that-a-net-core-app-is-running-in-a-docker-container-and-skippablefacts-in-xunit
        private static readonly string DataRoot = 
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ? "/data" : Environment.GetCommandLineArgs().Skip(1).Single();

        public static string GetInvestingFolder()
               => Path.Combine(DataRoot, "Investing");

        public static string GetYahooScreenerFilename()
            => Path.Combine(DataRoot, "YahooFinance", "ScreenerDetails.csv");

        public static string GetZacksScreener()
            => Path.Combine(DataRoot, "Zacks", "Screener.csv");

        public static string GetFinancialModeling(string ticker)
            => Path.Combine(DataRoot, "FinancialModeling", $"{ticker}.json");

        public static DataSource[] NewsEventsDataSource
           => JsonConvert.DeserializeObject<DataSource[]>(File.ReadAllText(Path.Combine(DataRoot, "NewsEvents", "DataSources.json")))!;

        public static string GetPressReleasesFilename(string ticker)
                => Path.Combine(DataRoot, "NewsEvents", "News", $"{ticker}.csv");

        public static string GetEventsFilename(string ticker)
            => Path.Combine(DataRoot, "NewsEvents", "Events", $"{ticker}.csv");


        public static string GetSECFillings(string ticker)
            => Path.Combine(DataRoot, "SEC", "Fillings", $"{ticker}.csv");

        public static string GetSECDetailPathFromUrl(string fillingType, string ticker, string url)
        {
            var uri = new Uri(url);
            return Path.Combine(DataRoot, "SEC", "Fillings", "Details", fillingType.Replace("/", "-"), ticker, uri.Segments.Last());
        }

        public static string GetISMManufacturingFilename(NasdaqType type) => Path.Combine(DataRoot, "ISM", "Manufacturing ROB", $"{type}.csv");
        public static string GetISMServiceFilename(NasdaqType type) => Path.Combine(DataRoot, "ISM", "Service ROB", $"{type}.csv");
        public static string GetISMSectorFilename(bool isManufacturing) => Path.Combine(DataRoot, "ISM", isManufacturing ? "Manufacturing ROB" : "Service ROB", "Sectors.csv");

    }
}
