using HtmlAgilityPack;
using IPTMGrabber.Utils;
using IPTMGrabber.Zacks;
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
        private const string SearchUrlFormat = "https://www.dnb.com/apps/dnb/servlets/CompanySearchServlet?familyTreeRolesPlayed=9141&pageNumber=1&pageSize=25&resourcePath=%2Fcontent%2Fdnb-us%2Fen%2Fhome%2Fsite-search-results%2Fjcr:content%2Fcontent-ipar-cta%2Fsinglepagesearch&returnNav=true&searchTerm={0}&token=eyJwNCI6IlQxaWc4QmVzZ3RDenJzRjVCR0pBSkFJQUdKQUhHIiwicDIiOjAsInAzIjoyOSwicDEiOjE2OTA5MDgwNjkwNzZ9";

        public DNBGrabber()
        {
        }

        public async Task ExecuteAsync(string dataroot)
        {
            var dnbFilename = Path.Combine(dataroot, "DNB", "stocks.csv");
            var dnbStocks = File.Exists(dnbFilename) ? Enumerators.EnumerateFromCsv<DNBStock>(dnbFilename).ToList() : new List<DNBStock>();

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
                if (dnbStocks.Any(s => s.Ticker == zacksStock.Ticker))
                {
                    //Console.WriteLine($"Already exists {stock.CompanyName}");
                    continue;
                }

                var searchResult = await GetRequestAsync<SearchResult2>(string.Format(SearchUrlFormat, zacksStock.CleanedCompanyName.Replace(" ", "+").Replace("&", "%26")));
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
                            var dnbStock = new DNBStock(zacksStock.Ticker, dnbName, companyWebsite, industries, stockExchange);

                            var yahooStock = yahooStocks.SingleOrDefault(y => y.Ticker == zacksStock.Ticker);
                            if (SameTicker(dnbStock, zacksStock) || SameUrl(dnbStock, yahooStock?.YahooWebsite))
                            {
                                Console.WriteLine($"Search {zacksStock.CompanyName}, found {dnbStock.Name} ({dnbStock.Website})");
                                foreach (var industry in industries)
                                {
                                    Console.WriteLine($"  - {industry}");
                                }

                                writer.WriteRecord(dnbStock);
                                await writer.NextRecordAsync();
                                await writer.FlushAsync();

                                found = true;
                                break;
                            }
                        }
                        await Task.Delay(100);
                    }

                    if (!found)
                        Console.WriteLine($"Cannot find data for {zacksStock.Ticker} ({zacksStock.CompanyName})");
                }
                else
                    Console.WriteLine($"Not found for {zacksStock.CompanyName}!");
                await Task.Delay(100);
            }
        }

        private async Task ResetBrowser()
        {
            //Console.WriteLine("===> Reset browser");
            var settings = new CefSettings
            {
                LogSeverity = LogSeverity.Disable
            };
            //settings.CefCommandLineArgs.Add("disable-application-cache", "1");
            //settings.CefCommandLineArgs.Add("disable-session-storage", "1"); 
            _browser.Dispose();
            Cef.GetGlobalCookieManager().DeleteCookies(string.Empty, string.Empty);
            await Cef.InitializeAsync(settings);
            _browser = new ChromiumWebBrowser();
            await Task.Delay(5000);
        }

        private bool SameTicker(DNBStock dnbStock, ZacksStock zacksStock)
        {
            return (dnbStock.DNBExchange.Equals(zacksStock.Exchange) || (dnbStock.DNBExchange.Equals("NYSE MKT") && zacksStock.Exchange.Equals("AMEX"))) &&
                    dnbStock.DNBTicker.Equals(zacksStock.Ticker);
        }

        private bool SameUrl(DNBStock dnbStock, string yahooUrl)
        {
            if (dnbStock.Ticker == "ABBNY" && dnbStock.Website == "www.dodge.com")
                dnbStock.Website = "global.abb";
            if (dnbStock.Ticker == "AHKSY" && dnbStock.Website == "https://www.asahi-kasei.co.jp/")
                dnbStock.Website = "www.asahi-kasei.com";
            if (dnbStock.Ticker == "AIRC" && dnbStock.Website == "www.aimco.com")
                dnbStock.Website = "https://www.aircommunities.com/";
            if (dnbStock.Ticker == "AMFPF" && dnbStock.Website == "www.amplifon.com")
                dnbStock.Website = "corporate.amplifon.com";
            if (dnbStock.Ticker == "AMN" && dnbStock.Website == "www.nursefinders.com")
                dnbStock.Website = "www.amnhealthcare.com";
            if (dnbStock.Ticker == "AMZN" && dnbStock.Website == "www.amazon.com")
                dnbStock.Website = "https://www.aboutamazon.com";
            if (dnbStock.Ticker == "APPF" && dnbStock.Website == "www.appfolio.com")
                return true;
            if (dnbStock.Ticker == "ASEKY" && dnbStock.Website == "www.aisin-china.com.cn")     // TODO: exactly the same?? BAD!!!! remove!!!!
                return true;
            if (dnbStock.Ticker == "AVB" && dnbStock.Website == "www.avaloncommunities.com")
                return true;
            if (dnbStock.Ticker == "AVIFY" && dnbStock.Website == "investor.ais.co.th")
                return true;
            if (dnbStock.Ticker == "AWI" && dnbStock.Website == "www.armstrong.com")
                return true;
            if (dnbStock.Ticker == "AWR" && dnbStock.Website == "americanstateswatercompany.gcs-web.com")
                return true;
            if (dnbStock.Ticker == "BBD" && dnbStock.Website == "www.bradesco.com.br")
                return true;
            if (dnbStock.Ticker == "BBDO" && dnbStock.Website == "www.bradesco.com.br")
                return true;
           if (dnbStock.Ticker == "BBWI" && dnbStock.Website == "www.lb.com")
                return true;
           if (dnbStock.Ticker == "BBY" && dnbStock.Website == "www.bestbuy.com")
                return true;
           if (dnbStock.Ticker == "BCH" && dnbStock.Website == "www.bancochile.cl")
                return true;
           if (dnbStock.Ticker == "BIP" && dnbStock.Website == "www.brookfieldinfrastructure.com")
                return true;
           if (dnbStock.Ticker == "BKHYY" && dnbStock.Website == "www.bankhapoalim.co.il")
                return true;
           if (dnbStock.Ticker == "BNTX" && dnbStock.Website == "www.biontech.com")
                return true;
           if (dnbStock.Ticker == "BOWFF" && dnbStock.Website == "www.bwalk.com")
                return true;
           if (dnbStock.Ticker == "BRTHY" && dnbStock.Website == "https://www.brother.co.jp/")
                return true;
           if (dnbStock.Ticker == "BSAC" && dnbStock.Website == "www.santander.cl")
               return true;
           if (dnbStock.Ticker == "BURL" && dnbStock.Website == "www.burlington.com")
               return true;
            if (dnbStock.Ticker == "BRP" && dnbStock.Website == "www.capitalgroup.com")
                return true;
            if (dnbStock.Ticker == "CABO" && dnbStock.Website == "www.sparklight.com")
                return true;
            if (dnbStock.Ticker == "CARR" && dnbStock.Website == "www.johnsoncontrols.com")
                return true;
            if (dnbStock.Ticker == "CATY" && dnbStock.Website == "www.cathaybank.com")
                return true;
            if (dnbStock.Ticker == "CBU" && dnbStock.Website == "ir.communitybanksystem.com")
                return true;
            if (dnbStock.Ticker == "CIGI" && dnbStock.Website == "www.collierscanada.com")
                return true;
            if (dnbStock.Ticker == "CMPGY" && dnbStock.Website == "www.compass-group.co.uk")
                return true;
            if (dnbStock.Ticker == "COLB" && dnbStock.Website == "www.umpquabank.com")
                return true;
            if (dnbStock.Ticker == "CPA" && dnbStock.Website == "www.copa.com")
                return true;
            if (dnbStock.Ticker == "CPNG" && dnbStock.Website == "www.coupang.jobs")
                return true;
            if (dnbStock.Ticker == "CRHKY" && dnbStock.Website == "www.cre.com.hk")
                return true;
            if (dnbStock.Ticker == "CSGP" && dnbStock.Website == "www.costar.com")
                return true;
            if (dnbStock.Ticker == "CSIOY" && dnbStock.Website == "https://www.casio.com/jp/")
                return true;
            if (dnbStock.Ticker == "CVCO" && dnbStock.Website == "www.cavcoindustries.com")
                return true;
            if (dnbStock.Ticker == "DBRG" && dnbStock.Website == "www.hilton.com")
                return true;
            if (dnbStock.Ticker == "DKILY" && dnbStock.Website == "https://www.daikin.co.jp/")
                return true;
            if (dnbStock.Ticker == "DPZ" && dnbStock.Website == "www.dominos.com")
                return true;
            if (dnbStock.Ticker == "DRVN" && dnbStock.Website == "www.carstar.com")
                return true;
            if (dnbStock.Ticker == "EBC" && dnbStock.Website == "www.nationwide.com")
                return true;
            if (dnbStock.Ticker == "ED" && dnbStock.Website == "www.coned.com")
                return true;
            if (dnbStock.Ticker == "ELUXY" && dnbStock.Website == "www.electrolux.se")
                return true;
            if (dnbStock.Ticker == "EQH" && dnbStock.Website == "www.equitable.com")
                return true;
            if (dnbStock.Ticker == "FBIN" && dnbStock.Website == "www.fbhs.com")
                return true;
            if (dnbStock.Ticker == "FHI" && dnbStock.Website == "www.federatedhermes.com")
                return true;
            if (dnbStock.Ticker == "FL" && dnbStock.Website == "www.footlocker.com")
                return true;
            if (dnbStock.Ticker == "FOX" && dnbStock.Website == "www.fox.com")
                return true;
            if (dnbStock.Ticker == "FSK" && dnbStock.Website == "www.fsinvestmentcorp.com")
                return true;
            if (dnbStock.Ticker == "FUJIY" && dnbStock.Website == "https://www.fujifilmholdings.com")
                return true;
            if (dnbStock.Ticker == "FWRD" && dnbStock.Website == "www.forwardair.com")
                return true;
            if (dnbStock.Ticker == "GEN" && dnbStock.Website == "www.nortonlifelock.com")
                return true;
            if (dnbStock.Ticker == "GGB" && dnbStock.Website == "www.gerdau.com.br")
                return true;
            if (dnbStock.Ticker == "HAYPY" && dnbStock.Website == "www.hays.co.uk")
                return true;
            if (dnbStock.Ticker == "HAYW" && dnbStock.Website == "www.hayward-pool.com")
                return true;
            if (dnbStock.Ticker == "HENKY" && dnbStock.Website == "www.henkel.de")
                return true;
             if (dnbStock.Ticker == "HII" && dnbStock.Website == "www.hii.com")
                return true;
            if (dnbStock.Ticker == "HNLGY" && dnbStock.Website == "www.hanglunggroup.co")
                return true;
            if (dnbStock.Ticker == "HNNMY" && dnbStock.Website == "www.hm.com")
                return true;
            if (dnbStock.Ticker == "HP" && dnbStock.Website == "www.helmerichpayne.com")
                return true;
             if (dnbStock.Ticker == "JSAIY" && dnbStock.Website == "www.j-sainsbury.co.uk")
                return true;
            if (dnbStock.Ticker == "JTKWY" && dnbStock.Website == "www.grubhub.com")
                return true;
            if (dnbStock.Ticker == "JWN" && dnbStock.Website == "www.nordstrom.com")
                return true;
            if (dnbStock.Ticker == "KEP" && dnbStock.Website == "www.kepco.co.kr")
                return true;
             if (dnbStock.Ticker == "KMTUY" && dnbStock.Website == "https://home.komatsu/jp/")
                return true;
            if (dnbStock.Ticker == "LAD" && dnbStock.Website == "www.lithia.com")
                return true;
            if (dnbStock.Ticker == "LNC" && dnbStock.Website == "www.lincolnfinancial.com")
                return true;
            if (dnbStock.Ticker == "LSPD" && dnbStock.Website == "www.lightspeedhq.com")
                return true;
             if (dnbStock.Ticker == "LYSDY" && dnbStock.Website == "www.lynasre.com")
                return true;
            if (dnbStock.Ticker == "LYV" && dnbStock.Website == "www.livenation.com")
                return true;
            if (dnbStock.Ticker == "MAKSY" && dnbStock.Website == "www.marksandspencer.com")
                return true;
            if (dnbStock.Ticker == "MBGAF" && dnbStock.Website == "info.daimler.com")
                return true;
             if (dnbStock.Ticker == "MDC" && dnbStock.Website == "www.richmondamerican.com")
                return true;
             /*
             if (dnbStock.Ticker == "" && dnbStock.Website == "")
                 return true;
             if (dnbStock.Ticker == "" && dnbStock.Website == "")
                 return true;
             if (dnbStock.Ticker == "" && dnbStock.Website == "")
                 return true;
              if (dnbStock.Ticker == "" && dnbStock.Website == "")
                 return true;
             if (dnbStock.Ticker == "" && dnbStock.Website == "")
                 return true;
             if (dnbStock.Ticker == "" && dnbStock.Website == "")
                 return true;
            */

            var cleanUrl = Uri.IsWellFormedUriString(dnbStock.Website, UriKind.Absolute)
                ? new Uri(dnbStock.Website).Host
                : dnbStock.Website;

            return cleanUrl?.Contains(yahooUrl?? "", StringComparison.OrdinalIgnoreCase) == true;
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
                    try
                    {
                        File.WriteAllText(@"C:\Data\Sources\Github\Necksus\log.txt", $"{browser.Address}\n{result}");
                    }
                    catch
                    {
                    }
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
