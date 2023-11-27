using System.Net;
using IPTMGrabber.Utils;
using Newtonsoft.Json;

namespace IPTMGrabber.YahooFinance
{
    public class FinancialModelingService
    {
        private readonly string[] _apiKeys =
        {
            "04ce754a5242d76f10bbc3b7f28c0020",
            "af3a57890a997cf2fc4ed4011579157e",
            "8eaece1b1980fcaf044f8a157bb3b45c",
            "8119a67d1e46b1c83b60ec6143dfeae3",
            "86481dde90882ae897b2771a0e22c049",
            "13691dde3d2fb6ef1eb93449468a1af5",
            "53c1c6462560371b3d20e5b2b8e67cb9",
            "920196c0623364f6bc75eddc63be3456",
            "30689f4e3c08795b0cfb6f0c2d7aa59b",
            "5be13c41d65855b9f03652fd66033c59"
        };
        private int currentApiKey;

        private string GetUrl(string ticker, string apiKey)
            => $"https://financialmodelingprep.com/api/v3/profile/{ticker}?apikey={apiKey}";

        public async Task<FinancialModelingItem> GetFinancialInfoAsync(string ticker)
        {
            var filename = Config.GetFinancialModeling(ticker);
            string json;
            if (File.Exists(filename))
            {
                json = await File.ReadAllTextAsync(filename);
            }
            else
            {
                json = await GetJsonResponseAsync(ticker);
                if (!string.IsNullOrEmpty(json))
                    await File.WriteAllTextAsync(filename, json);
            }
            return JsonConvert.DeserializeObject<FinancialModelingItem[]>(json).SingleOrDefault();
        }

        private async Task<string> GetJsonResponseAsync(string ticker)
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)

            };
            HttpResponseMessage response = null;
            bool error;
            do
            {
                try
                {
                    response = await client.GetAsync(GetUrl(ticker, _apiKeys[currentApiKey]));
                    error = false;
                }
                catch (Exception ex)
                {
                    error = true;
                }

                if (!response!.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.TooManyRequests:
                            currentApiKey = (currentApiKey + 1) % _apiKeys.Length;
                            if (currentApiKey >= _apiKeys.Length)
                            {
                                Console.WriteLine("No more api keys");
                                return "";
                            }
                            error = true;
                            break;
                        case HttpStatusCode.Forbidden:
                            return "";
                    }
                }
            } while (error);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
