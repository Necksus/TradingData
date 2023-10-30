using IPTMGrabber.InvestorWebsite;
using IPTMGrabber.Nasdaq;
using Newtonsoft.Json;
using System.Data;

namespace IPTMGrabber.Utils
{
    internal static class Config
    {

        internal class ConfigParameters
        {
            public string DataRoot { get; set; }
            public string ZacksScreenerUrl { get; set; }
        }

        private static ConfigParameters _parameters;

        static Config()
        {
        // Thanks to Scott https://www.hanselman.com/blog/detecting-that-a-net-core-app-is-running-in-a-docker-container-and-skippablefacts-in-xunit
            var configPath = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ?
                "/data/config.json" :
                Environment.GetCommandLineArgs().Skip(1).Single();

            Console.WriteLine($"Using config file: {configPath}");
            _parameters = JsonConvert.DeserializeObject<ConfigParameters>(File.ReadAllText(configPath))!;
            Console.WriteLine("Config file is loaded");
        }

        private static string DataRoot => _parameters.DataRoot;

        public static string GetInvestingFolder()
               => Path.Combine(DataRoot, "Investing");

        public static string GetYahooScreenerFilename()
            => Path.Combine(DataRoot, "YahooFinance", "ScreenerDetails.csv");

        public static string GetZacksScreener()
            => Path.Combine(DataRoot, "Zacks", "Screener.csv");

        public static string ZacksScreenerUrl
            => _parameters.ZacksScreenerUrl;

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
