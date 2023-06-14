using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using HtmlAgilityPack;
using System;
using System.Globalization;
using System.Net;

namespace IPTMGrabber.NAICS
{
    internal class NAICSGrabber
    {
        private const string CompanyProfilePage = "https://www.naics.com/company-profile-page/?co=";

        class NAICSCompany
        {
            public int Code { get; }
            public string Name { get; }
            public string Naics1 { get; }
            public string Nacis2 { get; }
            public string Duns { get; }
            public string Website { get; }
            public string Address { get; }

            public NAICSCompany(int code, string name, string naics1, string nacis2, string duns, string website, string address)
            {
                Code = code;
                Name = name;
                Naics1 = naics1;
                Nacis2 = nacis2;
                Duns = duns;
                Website = website;
                Address = address;
            }
        }

        // NAICS Structure can be found at: https://www.census.gov/naics/?48967
        public async Task DownloadAsync(string dataRoot)
        {
            var web = new HtmlWeb
            {
                CaptureRedirect = true,
                UseCookies = true,
                PreRequest = r =>
                {
                    r.AllowAutoRedirect = true;
                    return true;
                }
            };

            await using var writer = new StreamWriter(Path.Combine(dataRoot, "NAICS", "NAICS_Companies.csv"));
            await using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
            });
            csvWriter.WriteHeader<NAICSCompany>();
            await csvWriter.NextRecordAsync();

            var code = 1;
            var errors = 0;
            while (errors < 20)
            {
                HtmlDocument doc;
                try
                {
                    doc = web.Load(CompanyProfilePage + code);
                }
                catch (Exception)
                {
                    continue;
                }

                var companyDetail = doc.DocumentNode.SelectSingleNode("//table[@class='companyDetail topCompanyDetail']");
                if (companyDetail == null) 
                    break;

                var companyNameNode = doc.DocumentNode.SelectSingleNode("//td[contains(., 'Company Name: ')]");
                var dunsNode = doc.DocumentNode.SelectSingleNode("//strong[contains(., 'DUNS#:')]");
                var naics1Node = doc.DocumentNode.SelectSingleNode("//td[starts-with(., 'NAICS 1: ')]/a");
                var naics2Node = doc.DocumentNode.SelectSingleNode("//td[starts-with(., 'NAICS 2: ')]/a");
                var addressNode = doc.DocumentNode.SelectSingleNode("//td[contains(., 'Street Address: ')]");

                var href = doc.DocumentNode.SelectSingleNode("//td[contains(., 'URL: ')]/a")?.GetAttributeValue("href", "");
                var webSite = string.Empty;
                if (href != null)
                {
                    var uri = new Uri(href);
                    webSite = uri.Host.StartsWith("www.") ? uri.Host.Substring(4) : uri.Host;
                }

                if (companyNameNode == null || dunsNode == null || naics1Node == null || naics2Node == null || addressNode == null)
                {
                    Console.WriteLine($"Error for code: {code}");
                }

                var company = new NAICSCompany(
                    code,
                    companyNameNode.InnerText.Trim().Replace("Company Name: ", "").Replace("&amp;", "&"),
                    naics1Node.InnerText,
                    naics2Node.InnerText,
                    dunsNode.InnerText.Trim().Replace("DUNS#: ", ""),
                    webSite,
                    addressNode.InnerText.Replace("Street Address: ", "").Trim());

                if (company.Name != "Company Name:")
                {
                    csvWriter.WriteRecord(company);
                    await csvWriter.NextRecordAsync();
                    errors = 0;
                }
                else
                    errors++;

                Console.WriteLine($"{code} : {company.Name} => {company.Address}");

                code++;
            }
        }
    }
}
