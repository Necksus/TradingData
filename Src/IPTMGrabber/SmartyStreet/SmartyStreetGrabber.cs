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

namespace IPTMGrabber.SmartyStreet
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

        public const string _authId = "93193f78-ee8a-199a-1fd8-7f1abe9f4563";
        public const string _authToken = "YWTfcIFl14YCyDug5XDk";

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
            catch (WebException)
            {
                _clientBuilder = new ClientBuilder(_authId, _authToken).BuildUsStreetApiClient();
                _clientBuilder.Send(lookup);
            }

            var result = lookup.Result.SingleOrDefault();
            var newStreet = result != null ? new StreetRecord(
                addressId,
                result.DeliveryPointBarcode,
                result.Metadata.Latitude,
                result.Metadata.Longitude,
                result.DeliveryLine1,
                result.DeliveryLine2,
                result.LastLine) : null;
            _existingStreets.Add(addressId, newStreet);
            File.WriteAllText(_cacheFilename, JsonConvert.SerializeObject(_existingStreets, Formatting.Indented));

            return newStreet;
        }

        public void GetNAICSAddresses(string dataRoot)
        {
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
                var address = csv.GetField<string>("Address")!;
                var result = Lookup(address);

                if (result != null)
                    Console.WriteLine($"{result.DeliveryLine1 ?? ""} {result.DeliveryLine2 ?? ""}{result.LastLine ?? ""}");
                else
                    Console.WriteLine($"NOT FOUND ===>> {address}");
            }
        }
    }
}