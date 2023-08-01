using HtmlAgilityPack;
using IPTMGrabber.Utils;
using IPTMGrabber.Zacks;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using CefSharp;
using CefSharp.OffScreen;

namespace IPTMGrabber.DNB
{
    internal class DNBGrabber
    {
        private ChromiumWebBrowser _browser;
        private const string SearchUrlFormat = "https://www.dnb.com/apps/dnb/servlets/TypeaheadSearchServlet?includeAddress=true&includeCompanyInfoCookie=true&languageCode=en&resourcePath=%2Fcontent%2Fdnb-us%2Fen%2Fhome%2Fsite-search-results%2Fjcr:content%2Fcontent-ipar-cta%2Fsinglepagesearch&searchTerm={0}&token=eyJwNCI6Ik96aUxNalNJcXpTeXJZYTlkU0NhMWZITVBHT0dKR0dMTklLIiwicDIiOjYsInAzIjozNSwicDEiOjE2OTA4MDMwMDU3MjR9";
        private const string UrlFormat =
//            "https://www.dnb.com/apps/dnb/servlets/TypeaheadSearchServlet?includeAddress=true&includeCompanyInfoCookie=true&languageCode=en&resourcePath=%2Fcontent%2Fdnb-us%2Fen%2Fhome%2Fsite-search-results%2Fjcr:content%2Fcontent-ipar-cta%2Fsinglepagesearch&searchTerm=hewlet&token=eyJwNCI6Ik96aUxNalNJcXpTeXJZYTlkU0NhMWZITVBHT0dKR0dMTklLIiwicDIiOjYsInAzIjozNSwicDEiOjE2OTA4MDMwMDU3MjR9";
            "https://www.dnb.com/site-search-results.html#CompanyProfilesPageNumber=1&CompanyProfilesSearch={0}&ContactProfilesPageNumber=1&DAndBMarketplacePageNumber=1&IndustryPageNumber=1&SiteContentPageNumber=1&tab=Company%20Profiles";


        public DNBGrabber()
        {
        }

        public async Task ExecuteAsync(string dataroot)
        {
            var dnbFilename = Path.Combine(dataroot, "DNB", "stocks.csv");
            var dnbStocks = File.Exists(dnbFilename) ? Enumerators.EnumerateFromCsv<DNBStock>(dnbFilename).ToList() : new List<DNBStock>();
            
            var writer = await FileHelper.CreateCsvWriterAsync<DNBStock>(dnbFilename);
            await writer.WriteRecordsAsync(dnbStocks);
            await writer.FlushAsync();

            var web = new HtmlWeb
            {
                CaptureRedirect = true,
                UserAgent = "PostmanRuntime/7.29.0",
                UseCookies = true,
                PreRequest = r =>
                {
                    r.AllowAutoRedirect = true;
                    return true;
                }
            };

            var success = await Cef.InitializeAsync(new CefSettings
            {
                LogSeverity = LogSeverity.Disable
            });

            _browser = new ChromiumWebBrowser();

            var count = 0;
            foreach (var stock in Enumerators.EnumerateFromCsv<ZacksStock>(Path.Combine(dataroot, "Zacks", "Screener.csv")).Where(s => !string.IsNullOrEmpty(s.CompanyName)))
            {
                if (dnbStocks.Any(s => s.Ticker == stock.Ticker))
                {
                    Console.WriteLine($"Already exists {stock.CompanyName}");
                    continue;
                }

                var client = new HttpClient(new HttpClientHandler
                {
                    CookieContainer = new CookieContainer(),
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                });
                var request = new HttpRequestMessage(HttpMethod.Get, string.Format(SearchUrlFormat, stock.CompanyName.Replace(" ","+").Replace("&", "%26")));

                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PostmanRuntime", "7.29.0"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                var searchResponse = await client.SendAsync(request);
                if (searchResponse.IsSuccessStatusCode)
                {
                    var searchResult = JsonConvert.DeserializeObject<SearchResult>(await searchResponse.Content.ReadAsStringAsync());
//                    foreach (var company in searchResult.companies)
                    {
                        if (searchResult.companies.Count == 0)
                        {
                            Console.WriteLine($"Not found for {stock.CompanyName}!");
                            continue;
                        }

                        // Clear cache to prevent blacklist from dnb
                       if (count % 20 == 0)
                        {
                            Cef.GetGlobalCookieManager().DeleteCookies(string.Empty, string.Empty);
                            Console.WriteLine("===> Clear cache!");
                        }


                        var detailDoc = await GetCompanyDetailAsync($"https://www.dnb.com/{searchResult.companies.First().companyProfileLink}");

                        if (detailDoc != null)
                        {
                            var dnbName = detailDoc.DocumentNode
                                .SelectSingleNode("//div[@class='company-profile-header-title']").InnerText?.Trim();
                            var companyWebsite = detailDoc.DocumentNode
                                .SelectSingleNode("//a[@id='hero-company-link']")
                                ?.InnerText?.Trim();
                            var industries = detailDoc.DocumentNode
                                .SelectSingleNode("//span[@name='industry_links']")
                                ?.SelectNodes(".//a[@class='company_profile_overview_underline_links']")
                                ?.Select(n => n.InnerText.Trim())
                                ?.ToArray() ?? Array.Empty<string>();
                            var dnbStock = new DNBStock(stock.Ticker, dnbName, companyWebsite, industries);

                            Console.WriteLine(
                                $"Search {stock.CompanyName}, found {dnbStock.Name} ({dnbStock.Website})");
                            foreach (var industry in industries)
                            {
                                Console.WriteLine($"  - {industry}");
                            }

                            writer.WriteRecord(dnbStock);
                            await writer.NextRecordAsync();
                            await writer.FlushAsync();
                        }
                        else
                            Console.WriteLine($"Detail not found for {stock.CompanyName}");
                    }
                }

                count++;
            }
        }

        private async Task<HtmlDocument> GetCompanyDetailAsync(string url)
        {
            var tcs = new TaskCompletionSource<HtmlDocument>();

            async void OnLoadingStateChanged(object? s, LoadingStateChangedEventArgs e)
            {
                if (!e.IsLoading && s is ChromiumWebBrowser browser)
                {
                    var script = "document.documentElement.outerHTML;";
                    var html = (await browser.EvaluateScriptAsync(script)).Result as string;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var industryNode = doc.DocumentNode.SelectSingleNode("//div[@class='company_profile_overview_header_title']");
                    if (industryNode != null)
                        tcs.TrySetResult(doc);

                    var businessDirectory = doc.DocumentNode.SelectSingleNode("//div[@class='html basecomp RootBaseComponent section']");
                    if (businessDirectory != null)
                        tcs.TrySetResult(null);
                }
            }

            _browser.LoadingStateChanged += OnLoadingStateChanged;
            _browser.Load(url);

            var industryList = await tcs.Task;

            _browser.LoadingStateChanged -= OnLoadingStateChanged;

            return industryList;
        }
    }
}
