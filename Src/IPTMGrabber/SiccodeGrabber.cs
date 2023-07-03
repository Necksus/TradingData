using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPTMGrabber
{
    internal class SiccodeGrabber
    {
        // https://siccode.com/search-business/


        public async Task ExecuteAsync(string dataroot)
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
            var count = 0;
            foreach (var file in Directory.GetFiles(Path.Combine(dataroot, "FinancialModeling")))
            {
                var array = JsonConvert.DeserializeObject<JArray>(File.ReadAllText(file));
                if (array!.Count != 1)
                    continue;

                var company = array[0] as JObject;
                var companyName = company?.GetValue("companyName")?.Value<string>();
                var city = company?.GetValue("city")?.Value<string>();
                var ticker = company?.GetValue("symbol")?.Value<string>();
                var cleanedCompany = GetCompanyCleanedName(companyName);

                var doc = web.Load(GetUrl(companyName));
                var resultDiv = doc.DocumentNode.SelectNodes($"//div[@class='result-lists section with-country']//a[contains(., '{city}')]");
                if (resultDiv != null)
                {
                    var filteredNode = resultDiv
                        .Where(n =>
                        {
                            var divCompany =
                                GetCompanyCleanedName(n.SelectSingleNode("./div[@class='bold']").InnerText);
                            return divCompany.Name.Equals(cleanedCompany.Name) &&
                                   divCompany.Status == cleanedCompany.Status;
                        })
                        .Select(n =>
                        {
                            var detailUrl = n.GetAttributeValue("href", "");
                            doc = web.Load(detailUrl);
                            var sicCode = doc.DocumentNode.SelectSingleNode("//a[@class='sic']/span").InnerText.Replace("SIC CODE", "").Trim();
                            var naicsCode = doc.DocumentNode.SelectSingleNode("//a[@class='naics']/span").InnerText.Replace("NAICS CODE", "").Trim();
                            return (sicCode, naicsCode);
                        })
                        .ToArray<(string SicCode, string NaicsCode)>();

                    if (filteredNode.Select(c => c.NaicsCode).Distinct().Count() == 1)
                    {
                        Console.WriteLine($"{ticker} : {filteredNode.First().NaicsCode} ({filteredNode.First().SicCode})");
                        count++;
                    }
                    else
                    {
                        Console.WriteLine($"{ticker} : ERROR found multiple matching for {companyName}");
                    }
                }
                else
                    Console.WriteLine($"{ticker} : NOT FOUND {companyName}");
            }
        }

        private (string Name, string Status) GetCompanyCleanedName(string companyName)
        {
            foreach (var pattern in new Dictionary<string,string>
                     {
                         { @",? Inc\.?$", "Inc" },
                         { @",? Plc\.?$", "Plc" },
                         { @",? Corp\.?$", "Corp" },
                         { @",? Corporation\.?$", "Corp" }
                     })
            {
                var regex = new Regex(pattern.Key, RegexOptions.IgnoreCase);
                if (regex.Match(companyName).Success)
                    return (new Regex(pattern.Key, RegexOptions.IgnoreCase).Replace(companyName, ""), pattern.Value);
            }

            return (companyName.Replace("-", " "), "");
        }

        private string GetUrl(string companyName)
        {
            string cleanedName = companyName
                .Split(',').First()
                .Replace("&", "and")
                .Replace(" ", "+")
                .TrimEnd('s');  // <= Fix site search bug!

            return $"https://siccode.com/search-business/{cleanedName}";
        }
    }
}
