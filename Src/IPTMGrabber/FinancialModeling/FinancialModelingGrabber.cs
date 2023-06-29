using System.Net;

namespace IPTMGrabber.FinancialModeling
{
    internal class FinancialModelingGrabber
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

        public static string GetFilename(string dataroot, string ticker)
            => Path.Combine(dataroot, "FinancialModeling", $"{ticker}.json");

        private string GetUrl(string ticker, string apiKey)
            => $"https://financialmodelingprep.com/api/v3/profile/{ticker}?apikey={apiKey}";

        public async Task ExecuteAsync(string dataroot)
        {
            var currentApiKey = 0;
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)

            };
            foreach (var ticker in Zacks.Zacks.GetTickers(dataroot))
            {
                if (File.Exists(GetFilename(dataroot, ticker)))
                    continue;

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
                                currentApiKey++;
                                if (currentApiKey >= _apiKeys.Length)
                                {
                                    Console.WriteLine("No more api keys");
                                    return;
                                }
                                error = true;
                                break;
                            case HttpStatusCode.Forbidden:
                                File.WriteAllText(GetFilename(dataroot, ticker), "[]");
                                continue;
                            default:
                                break;
                        }
                    }
                } while (error);

                await File.WriteAllTextAsync(GetFilename(dataroot, ticker), await response.Content.ReadAsStringAsync());
                Console.WriteLine($"Get Financial Modeling profile for {ticker}.");
            }
        }
    }
}
