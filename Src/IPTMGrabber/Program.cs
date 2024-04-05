using IPTMGrabber.DNB;
using IPTMGrabber.Edgar;
using IPTMGrabber.Investing;
using IPTMGrabber.InvestorWebsite;
using IPTMGrabber.ISM;
using IPTMGrabber.Nasdaq;
using IPTMGrabber.Utils;
using IPTMGrabber.Utils.Browser;
using IPTMGrabber.YahooFinance;
using IPTMGrabber.Zacks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.IO;
using System.Threading;

namespace IPTMGrabber
{
    internal class Program
    {
        // Official link: https://www.ismworld.org/supply-management-news-and-reports/reports/ism-report-on-business/pmi/april/
        public static readonly string[] ISMManufactoringHistoryUrls =
        {
//            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-57-6-january-2022-manufacturing-ism-report-on-business-301472006.html",
//            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-58-6-february-2022-manufacturing-ism-report-on-business-301492024.html",
//            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-57-1-march-2022-manufacturing-ism-report-on-business-301515115.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-55-4-april-2022-manufacturing-ism-report-on-business-301536612.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-56-1-may-2022-manufacturing-ism-report-on-business-301558163.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-53-june-2022-manufacturing-ism-report-on-business-301579263.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-52-8-july-2022-manufacturing-ism-report-on-business-301596347.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-52-8-august-2022-manufacturing-ism-report-on-business-301615866.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-50-9-september-2022-manufacturing-ism-report-on-business-301638361.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-50-2-october-2022-manufacturing-ism-report-on-business-301663845.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-49-november-2022-manufacturing-ism-report-on-business-301690752.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-48-4-december-2022-manufacturing-ism-report-on-business-301712602.html",

            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-47-4-january-2023-manufacturing-ism-report-on-business-301735466.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-47-7-february-2023-manufacturing-ism-report-on-business-301758406.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-46-3-march-2023-manufacturing-ism-report-on-business-301787309.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-47-1-april-2023-manufacturing-ism-report-on-business-301811325.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-46-9-may-2023-manufacturing-ism-report-on-business-301839274.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-46-june-2023-manufacturing-ism-report-on-business-301868365.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-46-4-july-2023-manufacturing-ism-report-on-business-301889651.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-47-6-august-2023-manufacturing-ism-report-on-business-301915209.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-49-september-2023-manufacturing-ism-report-on-business-301943535.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-46-7-october-2023-manufacturing-ism-report-on-business-301973296.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-46-7-november-2023-manufacturing-ism-report-on-business-302002512.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-47-4-december-2023-manufacturing-ism-report-on-business-302024715.html",

            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-49-1-january-2024-manufacturing-ism-report-on-business-302049663.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-47-8-february-2024-manufacturing-ism-report-on-business-302076127.html",
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-50-3-march-2024-manufacturing-ism-report-on-business-302104149.html"
        };

        // Official link: https://www.ismworld.org/supply-management-news-and-reports/reports/ism-report-on-business/services/april/
        public static readonly string[] ISMServiceROBHistoryUrls =
        {
            "https://www.prnewswire.com/news-releases/services-pmi-at-57-1-april-2022-services-ism-report-on-business-301539647.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-55-9-may-2022-services-ism-report-on-business-301560637.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-55-3-june-2022-services-ism-report-on-business-301580808.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-56-7-july-2022-services-ism-report-on-business-301598327.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-56-9-august-2022-services-ism-report-on-business-301617838.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-56-7-september-2022-services-ism-report-on-business-301640833.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-54-4-october-2022-services-ism-report-on-business-301666703.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-56-5-november-2022-services-ism-report-on-business-301694620.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-49-6-december-2022-services-ism-report-on-business-301715010.html",

            "https://www.prnewswire.com/news-releases/services-pmi-at-55-2-january-2023-services-ism-report-on-business-301737926.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-55-1-february-2023-services-ism-report-on-business-301761637.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-51-2-march-2023-services-ism-report-on-business-301789944.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-51-9-april-2023-services-ism-report-on-business-301813773.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-50-3-may-2023-services-ism-report-on-business-301841530.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-52-7-july-2023-services-ism-report-on-business-301892116.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-54-5-august-2023-services-ism-report-on-business-301918588.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-53-6-september-2023-services-ism-report-on-business-301946224.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-51-8-october-2023-services-ism-report-on-business-301976221.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-52-7-november-2023-services-ism-report-on-business-302004936.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-50-6-december-2023-services-ism-report-on-business-302026587.html",

            "https://www.prnewswire.com/news-releases/services-pmi-at-53-4-january-2024-services-ism-report-on-business-302052175.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-52-6-february-2024-services-ism-report-on-business-302078785.html",
            "https://www.prnewswire.com/news-releases/services-pmi-at-51-4-march-2024-services-ism-report-on-business-302106133.html"
        };

        static async Task Main(string[] args)
        {
            var stream = new MemoryStream();
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            var browserService = new BrowserService(loggerFactory.CreateLogger<BrowserService>());
            var newsWireGrabber = new ISMGrabber();
            var nasdaqGrabber = new NasdaqGrabber();
            var investingGrabber = new InvestingGrabber();
            var financialModelingService = new FinancialModelingService();
            var yahooService = new YahooService();
            var edgarService = new EdgarService(loggerFactory.CreateLogger<EdgarService>(), browserService, financialModelingService);
            var dnbGrabber = new DNBGrabber();
            var newsGrabber = new NewsAndEventsGrabber(loggerFactory.CreateLogger<NewsAndEventsGrabber>(), browserService, new EarningPredictionModel());
            var zacksGrabber = new ZacksService(loggerFactory.CreateLogger<ZacksService>(), browserService);

            //await newsGrabber.ExecuteAsync(default);
            /*
            await using (var zacksStream = File.Open(Config.GetZacksScreener(), FileMode.OpenOrCreate))
            {
                await zacksGrabber.GetScreenerAsync(zacksStream, default);
            }

            await yahooService.ExecuteAsync();
            return;*/

            //            await zacksGrabber.ExecuteAsync("casimir666@free.fr", "RPWLp$59eQ3C!GfFRmG7wlrrv0", default);

            /*
            foreach (var ticker in new[] { "MDLZ" })
            {
                await edgarService.GrabInsidersAsync(ticker, stream, default);

                //await edgarService.GrabFillings(ticker, fileStream, default);
            }
            return;
            */
            //await edgar.GrabFillings("66756", stream, default);

            /*
            foreach (var url in new[]
                     {
//                         "https://ir.hubspot.com/events",     => ENDLESS LOOP!
//                         "https://investor.harley-davidson.com/events-and-presentations/default.aspx",    => Select pager, without "all"!!

                         "https://ir.kla.com/news-events/press-releases?category=all",
//                         "https://about.underarmour.com/en/investors/press-releases--events---presentations.html",
"https://investors.delltechnologies.com/news-events/press-release?af23095e_year%5Bvalue%5D=_none&op=Filter",
                         "https://investor.analog.com/press-releases?a9d908dd_year%5Bvalue%5D=_none&op=Filter",
                         "https://ir.mobileye.com/news-events/news-releases?a9d908dd_year%5Bvalue%5D=_none",
                         "https://investors.paccar.com/financial-news/default.aspx",
                         "https://investor.fluor.com/events-and-presentations/default.aspx",
                         "https://www.lyondellbasell.com/en/investors/investor-events/",
                     })
            {
                await newsGrabber.ExecuteAsync(url, "MMM dd, yyyy h:mm tt", default);
            }*/

            /*            var ia = new EarningPredictionModel(_dataRoot);
                        ia.TrainModel();*/

            //await edgar.GrabInsidersAsync("1600033", stream, default);
            //stream.Position = 0;
            //var result = new StreamReader(stream).ReadToEnd();

            //await newsGrabber.ExecuteAsync(default);

            //await yahooGrabber.ExecuteAsync(_dataRoot);
            //await newsGrabber.ExecuteAsync(_dataRoot, default);

            //await yahooGrabber.ExecuteAsync(_dataRoot);
            //await dnbGrabber.ExecuteAsync(_dataRoot);
            var newPMI = ISMManufactoringHistoryUrls
                .Select(url => newsWireGrabber.ParseISMReport<ManufacturingROB>(url))
                .OrderByDescending(r => r.Date)
                .ToArray();

            var pmiReport = newPMI.First();

            // Write PMI file
            nasdaqGrabber.Download(pmiReport.Type, Config.GetISMManufacturingFilename(pmiReport.Type));
            AddRecords(Config.GetISMManufacturingFilename(pmiReport.Type), newPMI.Select(r => r.GetCsvLine()));

            // Write all details
            foreach (var detail in pmiReport.Details)
            {
                nasdaqGrabber.Download(detail.Type, Config.GetISMManufacturingFilename(detail.Type));
                AddRecords(Config.GetISMManufacturingFilename(detail.Type), newPMI.Select(r => r.GetDetailCsvLine(detail.Name)));
            }

            // Write Sector file
            File.WriteAllText(Config.GetISMSectorFilename(true), pmiReport.GetSectorCsvLine(false));
            AddRecords(Config.GetISMSectorFilename(true), newPMI.Select(r => r.GetSectorCsvLine()));
            

            var newMMI = ISMServiceROBHistoryUrls
                .Select(url => newsWireGrabber.ParseISMReport<ServiceROB>(url))
                .OrderByDescending(r => r.Date)
                .ToArray();
            var mmiReport = newMMI.First();
            // Write NMI file
            nasdaqGrabber.Download(mmiReport.Type, Config.GetISMServiceFilename(mmiReport.Type));
            AddRecords(Config.GetISMServiceFilename(mmiReport.Type), newMMI.Select(r => r.GetCsvLine()));

            // Write all details
            foreach (var detail in mmiReport.Details)
            {
                nasdaqGrabber.Download(detail.Type, Config.GetISMServiceFilename(detail.Type));
                AddRecords(Config.GetISMServiceFilename(detail.Type), newMMI.Select(r => r.GetDetailCsvLine(detail.Name)));
            }

            // Write Sector file
            File.WriteAllText(Config.GetISMSectorFilename(false), mmiReport.GetSectorCsvLine(false));
            AddRecords(Config.GetISMSectorFilename(false), newMMI.Select(r => r.GetSectorCsvLine()));

            //await investingGrabber.DownloadAllAsync();
        }

        private static void AddRecords(string filePath, IEnumerable<string> newResults)
        {
            // Lire le contenu existant du fichier CSV sans la première ligne (header)
            string[] lines = File.ReadAllLines(filePath);
            string header = lines[0];
            string[] data = lines.Skip(1).ToArray();

            // Ajouter les nouvelles lignes au début du tableau de données
            data = newResults.Concat(data).ToArray();

            // Écrire les données dans le fichier CSV, y compris le header
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                    writer.WriteLine(header);
                foreach (string line in data)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}