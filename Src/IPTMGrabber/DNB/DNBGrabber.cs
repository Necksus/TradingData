using HtmlAgilityPack;
using IPTMGrabber.Utils;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using CefSharp;
using CefSharp.OffScreen;
using IPTMGrabber.YahooFinance;

namespace IPTMGrabber.DNB
{
    internal class DNBGrabber
    {
        private const int Timeout = 40000;

        private ChromiumWebBrowser _browser;
        private const string SearchUrlFormat1 = "https://www.dnb.com/apps/dnb/servlets/CompanySearchServlet?familyTreeRolesPlayed=9141&pageNumber=1&pageSize=25&resourcePath=%2Fcontent%2Fdnb-us%2Fen%2Fhome%2Fsite-search-results%2Fjcr:content%2Fcontent-ipar-cta%2Fsinglepagesearch&returnNav=true&searchTerm={0}&token=eyJwNCI6IlQxaWc4QmVzZ3RDenJzRjVCR0pBSkFJQUdKQUhHIiwicDIiOjAsInAzIjoyOSwicDEiOjE2OTA5MDgwNjkwNzZ9";
        private const string SearchUrlFormat2 = "https://www.dnb.com/apps/dnb/servlets/CompanySearchServlet?pageNumber=1&pageSize=25&resourcePath=%2Fcontent%2Fdnb-us%2Fen%2Fhome%2Fsite-search-results%2Fjcr:content%2Fcontent-ipar-cta%2Fsinglepagesearch&returnNav=true&searchTerm={0}&token=eyJwNCI6IlQxaWc4QmVzZ3RDenJzRjVCR0pBSkFJQUdKQUhHIiwicDIiOjAsInAzIjoyOSwicDEiOjE2OTA5MDgwNjkwNzZ9";

        public DNBGrabber()
        {
        }

        public async Task ExecuteAsync(string dataroot)
        {
            var dnbFilename = Path.Combine(dataroot, "DNB", "stocks.csv");
            var dnbStocks = File.Exists(dnbFilename) ? Enumerators.EnumerateFromCsv<DNBStock>(dnbFilename).ToList() : new List<DNBStock>();
            var naicsItems = Enumerators.EnumerateFromCsv<NAICSItem>(Path.Combine(dataroot, "NAICS", "NAICS_Structure.csv")).ToList();
            var mappings = Enumerators.EnumerateFromCsv<ManualMapping>(Path.Combine(dataroot, "DNB", "ManualMapping.csv")).ToList();

            var different = 0;
            var yahooStocks = Enumerators.EnumerateFromCsv<QuoteDetail>(FileHelper.GetYahooScreenerFilename(dataroot)).ToArray();

            var writer = await FileHelper.CreateCsvWriterAsync<DNBStock>(dnbFilename);
            await writer.WriteRecordsAsync(dnbStocks.OrderBy(s => s.Ticker));
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

            Console.WriteLine("Start scrapping...");
            var count = 0;
            foreach (var zacksStock in Enumerators.EnumerateFromCsv<ZacksStock>(Path.Combine(dataroot, "Zacks", "Screener.csv")).Where(s => !string.IsNullOrEmpty(s.CompanyName)))
            {
                if (zacksStock.Exchange == "OTC" || dnbStocks.Any(s => s.Ticker == zacksStock.Ticker))
                {
                    //Console.WriteLine($"Already exists {stock.CompanyName}");
                    continue;
                }
                var mapping = mappings.FirstOrDefault(m => m.ZacksTicker == zacksStock.Ticker);
                var searchResult = (mapping != null) ?
                    new SearchResult2 { Companies = new List<Company>{ new() { CompanyProfileLink = mapping.DNBLink } } } :
                    await GetRequestAsync<SearchResult2>(string.Format(SearchUrlFormat2, zacksStock.CompanyName.Replace(" ", "+").Replace("&", "%26")));
                if (searchResult?.Companies.Count == 0)
                    searchResult = await GetRequestAsync<SearchResult2>(string.Format(SearchUrlFormat2, zacksStock.CleanedCompanyName.Replace(" ", "+").Replace("&", "%26")));
                if (searchResult?.Companies.Count > 0)
                {
                    var found = false;
                    foreach (var company in searchResult.Companies)
                    {
                        // Clear cache to prevent blacklist from dnb
                        count++;
                        if (count % 20 == 0)
                        {
                            await ResetBrowser();
                        }

                        var detailDoc = await GetCompanyDetailAsync($"https://www.dnb.com/{company.CompanyProfileLink}");

                        if (detailDoc != null)
                        {
                            var dnbName = detailDoc.DocumentNode
                                .SelectSingleNode("//div[@class='company-profile-header-title']").InnerText?.Trim();
                            var companyWebsite = detailDoc.DocumentNode
                                .SelectSingleNode("//a[@id='hero-company-link']")
                                ?.InnerText?.Trim();
                            var stockExchange = detailDoc.DocumentNode
                                .SelectSingleNode("//span[@name='stock_exchange']/span")
                                ?.InnerText?.Trim();
                            var industries = detailDoc.DocumentNode
                                .SelectSingleNode("//span[@name='industry_links']")
                                ?.SelectNodes(".//a[@class='company_profile_overview_underline_links']")
                                ?.Select(n => n.InnerText.Trim())
                                ?.ToArray() ?? Array.Empty<string>();
                            var keyPrincipal = detailDoc.DocumentNode
                                .SelectSingleNode("//span[@name='key_principal']")
                                ?.SelectSingleNode(".//span[1]")
                                ?.InnerText
                                ?.Trim()
                                ?.Split('\n')
                                ?.First();
                            var description = detailDoc.DocumentNode
                                .SelectSingleNode("//span[@data-tracking-name='Company Description:']")
                                ?.InnerText
                                ?.Trim();

                            if (industries.Length == 0)
                                continue;

                            var naicsItem = GetNAISCItem(naicsItems, industries);
                            var dnbStock = new DNBStock(
                                zacksStock.Ticker, 
                                dnbName, 
                                companyWebsite,
                                naicsItem.Sector,
                                naicsItem.Industry,
                                stockExchange, 
                                keyPrincipal,
                                company.CompanyProfileLink,
                                description);

                            var yahooStock = yahooStocks.SingleOrDefault(y => y.Ticker == zacksStock.Ticker);
                            if (dnbStock.NaicsIndustry != "Management of Companies and Enterprises" &&                // We don't want holdings
                                (mapping != null || SameTicker(dnbStock, zacksStock) || SameUrl(dnbStock, yahooStock?.YahooWebsite)))
                            {
                                Console.WriteLine($"Search {zacksStock.CompanyName}, found {dnbStock.Name} ({dnbStock.Website})");
                                Console.WriteLine($"  - {dnbStock.NaicsSector}");
                                Console.WriteLine($"  - {dnbStock.NaicsIndustry}");

                                writer.WriteRecord(dnbStock);
                                await writer.NextRecordAsync();
                                await writer.FlushAsync();

                                found = true;
                                break;
                            }
                            //Console.WriteLine($"Can be: {zacksStock.Ticker},{yahooStock?.YahooWebsite},\"{zacksStock.CompanyName}\",{dnbStock.DNBTicker},{dnbStock.Website},\"{dnbStock.Name}\"");
                        }
                        await Task.Delay(100);
                    }

                    if (!found)
                        Console.WriteLine($"Cannot find data for {zacksStock.Ticker} ({zacksStock.CompanyName})");
                }
                else
                    Console.WriteLine($"Not found for {zacksStock.Ticker} ({zacksStock.CompanyName})");
                await Task.Delay(100);
            }
        }

        private (string Sector, string Industry) GetNAISCItem(List<NAICSItem> naicsItems, string[] industries)
        {
            var industry = industries
                .Select(i => naicsItems.LastOrDefault(n => n.Name.Equals(i, StringComparison.OrdinalIgnoreCase)))
                ?.FirstOrDefault(n => n != null && n.Code.Length > 3);
            
            var sectorCode = industry?.Code.Substring(0, industry.Code.StartsWith("3") ? 3 : 2);
            var sector = naicsItems.SingleOrDefault(i => i.Code.Equals(sectorCode));

            return (sector?.Name, industry?.Name);
        }

        private async Task ResetBrowser()
        {
            var settings = new CefSettings
            {
                LogSeverity = LogSeverity.Disable
            };
            settings.CefCommandLineArgs.Add("disable-application-cache", "1");
            settings.CefCommandLineArgs.Add("disable-session-storage", "1"); 
            _browser.Dispose();
            Cef.GetGlobalCookieManager().DeleteCookies(string.Empty, string.Empty);
            await Cef.InitializeAsync(settings);
            _browser = new ChromiumWebBrowser();
            await Task.Delay(5000);
        }

        private bool SameTicker(DNBStock dnbStock, ZacksStock zacksStock)
        {
            return dnbStock.DNBTicker.Equals(zacksStock.Ticker);
        }

        private bool SameUrl(DNBStock dnbStock, string yahooUrl)
        {
            var cleanUrl = Uri.IsWellFormedUriString(dnbStock.Website, UriKind.Absolute)
                ? new Uri(dnbStock.Website).Host
                : dnbStock.Website;

            return dnbStock.Website?.Contains(yahooUrl?? "", StringComparison.OrdinalIgnoreCase) == true;
        }

        private async Task<HtmlDocument?> GetCompanyDetailAsync(string url)
        {
            var emptyResult = new HtmlDocument();
            var tcs = new TaskCompletionSource<HtmlDocument>();
            var result = default(HtmlDocument);
            int retry = 0;

            do
            {
                async void OnLoadingStateChanged(object? s, LoadingStateChangedEventArgs e)
                {
                    if (!e.IsLoading && s is ChromiumWebBrowser browser)
                    {
                        var html = await GetBrowserContent(browser, true);
                        if (!string.IsNullOrEmpty(html))
                        {
                            var doc = new HtmlDocument();
                            doc.LoadHtml(html);
                            var industryNode =
                                doc.DocumentNode.SelectSingleNode(
                                    "//div[@class='company_profile_overview_header_title']");
                            if (industryNode != null)
                                tcs.TrySetResult(doc);

                            var businessDirectory =
                                doc.DocumentNode.SelectSingleNode(
                                    "//div[@class='html basecomp RootBaseComponent section']");
                            if (businessDirectory != null)
                                tcs.TrySetResult(emptyResult);
                        }
                    }
                }

                result = await LoadUrlWithTimeout(url, OnLoadingStateChanged, tcs.Task!, Timeout, null);
                retry++;
            } while (result == null && retry < 3);

            return result == emptyResult ? null : result;
        }

        private async Task<T?> GetRequestAsync<T>(string url) where T: class
        {
            var tcs = new TaskCompletionSource<T?>();
            var result = default(T?);
            int retry = 0;

            do
            {
                async void OnLoadingStateChanged(object? s, LoadingStateChangedEventArgs e)
                {
                    if (!e.IsLoading && s is ChromiumWebBrowser browser)
                    {
                        var html = await GetBrowserContent(browser, false);
                        if (html?.StartsWith("{") == true)
                            tcs.TrySetResult(JsonConvert.DeserializeObject<T>(html));
                    }
                }

                result = await LoadUrlWithTimeout(url, OnLoadingStateChanged, tcs.Task, Timeout, null);
                retry++;
            } while(result == null && retry < 3);

            return result;
        }


        private async Task<string> GetBrowserContent(ChromiumWebBrowser browser, bool isHtml)
        {
            while (true)
            {
                try
                {
                    var script = isHtml ? "document.documentElement.outerHTML;" : "document.documentElement.innerText;";
                    var result = (await browser.EvaluateScriptAsync(script))?.Result as string ?? "";
                    /*try
                    {
                        File.WriteAllText(@"C:\Data\Sources\Github\Necksus\log.txt", $"{browser.Address}\n{result}");
                    }
                    catch
                    {
                    }*/
                    return result;
                }
                catch (Exception ex)
                {
                    await Task.Delay(1);
                }
            }
        }

        async Task<T> LoadUrlWithTimeout<T>(string url, EventHandler<LoadingStateChangedEventArgs> onLoading, Task<T> task, int timeoutMilliseconds, T defaultValue)
        {
            _browser.LoadingStateChanged += onLoading;
            _browser.Load(url);
            Task completedTask = await Task.WhenAny(task, Task.Delay(timeoutMilliseconds));
            _browser.LoadingStateChanged -= onLoading;

            if (completedTask == task && !task.IsFaulted && !task.IsCanceled)
            {
                return await task;
            }
            else
            {
                await ResetBrowser();
                return defaultValue;
            }
        }
    }
}
