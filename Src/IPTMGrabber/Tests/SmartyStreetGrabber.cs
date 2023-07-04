using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json;
using SmartyStreets;
using SmartyStreets.USStreetApi;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using IPTMGrabber.YahooFinance;

namespace IPTMGrabber.Tests
{
    class StreetRecord
    {
        public string Id { get; }
        public string DeliveryPointBarcode { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public string DeliveryLine1 { get; }
        public string DeliveryLine2 { get; }
        public string LastLine { get; }

        public StreetRecord(
            string id,
            string deliveryPointBarcode,
            double latitude,
            double longitude,
            string deliveryLine1,
            string deliveryLine2,
            string lastLine)
        {
            Id = id;
            DeliveryPointBarcode = deliveryPointBarcode;
            Latitude = latitude;
            Longitude = longitude;
            DeliveryLine1 = deliveryLine1;
            DeliveryLine2 = deliveryLine2;
            LastLine = lastLine;
        }
    }

    internal class SmartyStreetGrabber
    {
        private Client _clientBuilder;
        private readonly SHA256 _sha256;
        private readonly Dictionary<string, StreetRecord> _existingStreets;
        private readonly string _cacheFilename;

        public const string _authId = "277dfacc-ecd0-07c1-c8e5-d93c142973e9";
        public const string _authToken = "Q9VgNQGn4N27fgYrlHuM";

        public SmartyStreetGrabber(string dataroot)
        {
            _clientBuilder = new ClientBuilder(_authId, _authToken).BuildUsStreetApiClient();
            _sha256 = SHA256.Create();
            _cacheFilename = Path.Combine(dataroot, "SmartyStreet", "SmartyStreet.json");
            _existingStreets =
                JsonConvert.DeserializeObject<Dictionary<string, StreetRecord>>(File.ReadAllText(_cacheFilename))!;
        }

        public StreetRecord Lookup(string address)
        {
            var addressId = Convert.ToHexString(_sha256.ComputeHash(Encoding.UTF8.GetBytes(address))).Replace("-", "");

            if (_existingStreets.TryGetValue(addressId, out var existingAddress))
            {
                return existingAddress;
            }

            var lookup = new Lookup(address);

            try
            {
                _clientBuilder.Send(lookup);
                Thread.Sleep(300);
            }
            catch (Exception)
            {
                _clientBuilder = new ClientBuilder(_authId, _authToken).BuildUsStreetApiClient();
                _clientBuilder.Send(lookup);
            }

            var result = lookup.Result.SingleOrDefault();
            var newStreet = result != null
                ? new StreetRecord(
                    addressId,
                    result.DeliveryPointBarcode,
                    result.Metadata.Latitude,
                    result.Metadata.Longitude,
                    result.DeliveryLine1,
                    result.DeliveryLine2,
                    result.LastLine)
                : null;
            _existingStreets.Add(addressId, newStreet);
            File.WriteAllText(_cacheFilename, JsonConvert.SerializeObject(_existingStreets, Formatting.Indented));

            return newStreet;
        }

        public List<(string code, string website, StreetRecord street)> GetNAICSAddresses(string dataRoot)
        {
            var mapping = new List<(string code, string website, StreetRecord street)>();

            // Prepare reader
            using var reader = new StreamReader(Path.Combine(dataRoot, "NAICS", "NAICS_Companies.csv"));
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ","
            });
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var code = csv.GetField<string>("Code")!;
                var website = csv.GetField<string>("Website")!;
                var address = csv.GetField<string>("Address")!;
                var result = Lookup(address);

                if (result != null)
                {
                    //Console.WriteLine($"{result.DeliveryLine1 ?? ""} {result.DeliveryLine2 ?? ""}{result.LastLine ?? ""}");
                    mapping.Add((code, website, result));
                }
                else
                    Console.WriteLine($"NOT FOUND ===>> {address}");
            }

            return mapping;
        }

        public List<(string code, string website, StreetRecord street)> GetYahooAddresses(string dataRoot)
        {
            var mapping = new List<(string code, string website, StreetRecord street)>();

            using var reader = new StreamReader(Path.Combine(dataRoot, "YahooFinance", "ScreenerDetails.csv"));
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ","
            });
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var quote = csv.GetRecord<QuoteDetail>();

                if (string.IsNullOrEmpty(quote.Zip))
                    continue;

                var address = $"{quote.Address1 ?? ""} {quote.Address2 ?? ""} {quote.City ?? ""} {quote.State ?? ""} {quote.Zip ?? ""}";
                var result = Lookup(address);
                if (result != null)
                {
                    //Console.WriteLine($"{result.DeliveryLine1 ?? ""} {result.DeliveryLine2 ?? ""}{result.LastLine ?? ""}");
                    mapping.Add((quote.Ticker, quote.WebSite, result));
                }
                else
                    Console.WriteLine($"NOT FOUND ===>> {address}");
            }

            return mapping;
        }
    }
}