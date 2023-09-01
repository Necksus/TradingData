using IPTMGrabber.InvestorWebsite;
using Newtonsoft.Json;

namespace IPTMGrabber.Utils
{
    internal static class Data
    {
        // Thanks to Scott https://www.hanselman.com/blog/detecting-that-a-net-core-app-is-running-in-a-docker-container-and-skippablefacts-in-xunit
        private static readonly string DataRoot = 
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ? "/data" : Environment.GetCommandLineArgs().Skip(1).Single();

        public static DataSource[] NewsEventsDataSource
           => JsonConvert.DeserializeObject<DataSource[]>(File.ReadAllText(Path.Combine(DataRoot, "NewsEvents", "DataSources.json")))!;
    }
}
