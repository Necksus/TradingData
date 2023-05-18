using IPTMGrabber.Investing;
using IPTMGrabber.ISM;
using IPTMGrabber.Nasdaq;

namespace IPTMGrabber
{
    internal class Program
    {
        private static string _dataRoot;

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
            "https://www.prnewswire.com/news-releases/manufacturing-pmi-at-47-1-april-2023-manufacturing-ism-report-on-business-301811325.html"
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
            "https://www.prnewswire.com/news-releases/services-pmi-at-51-9-april-2023-services-ism-report-on-business-301813773.html"
        };

        static async Task Main(string[] args)
        {
            _dataRoot = args.Single();

            var newsWireGrabber = new ISMGrabber();
            var nasdaqGrabber = new NasdaqGrabber();
            var investingGrabber = new InvestingGrabber(_dataRoot);

            var newPMI = ISMManufactoringHistoryUrls
                .Select(url => newsWireGrabber.ParseISMReport<ManufacturingROB>(url))
                .OrderByDescending(r => r.Date)
                .ToArray();

            var pmiReport = newPMI.First();

            // Write PMI file
            nasdaqGrabber.Download(pmiReport.Type, GetISMManufacturingFilename(pmiReport.Type));
            AddRecords(GetISMManufacturingFilename(pmiReport.Type), newPMI.Select(r => r.GetCsvLine()));

            // Write all details
            foreach (var detail in pmiReport.Details)
            {
                nasdaqGrabber.Download(detail.Type, GetISMManufacturingFilename(detail.Type));
                AddRecords(GetISMManufacturingFilename(detail.Type), newPMI.Select(r => r.GetDetailCsvLine(detail.Name)));
            }

            // Write Sector file
            File.WriteAllText(GetISMSectorFilename(true), pmiReport.GetSectorCsvLine(false));
            AddRecords(GetISMSectorFilename(true), newPMI.Select(r => r.GetSectorCsvLine()));

            var newMMI = ISMServiceROBHistoryUrls
                .Select(url => newsWireGrabber.ParseISMReport<ServiceROB>(url))
                .OrderByDescending(r => r.Date)
                .ToArray();
            var mmiReport = newMMI.First();
            // Write NMI file
            nasdaqGrabber.Download(mmiReport.Type, GetISMServiceFilename(mmiReport.Type));
            AddRecords(GetISMServiceFilename(mmiReport.Type), newMMI.Select(r => r.GetCsvLine()));

            // Write all details
            foreach (var detail in mmiReport.Details)
            {
                nasdaqGrabber.Download(detail.Type, GetISMServiceFilename(detail.Type));
                AddRecords(GetISMServiceFilename(detail.Type), newMMI.Select(r => r.GetDetailCsvLine(detail.Name)));
            }

            // Write Sector file
            File.WriteAllText(GetISMSectorFilename(false), mmiReport.GetSectorCsvLine(false));
            AddRecords(GetISMSectorFilename(false), newMMI.Select(r => r.GetSectorCsvLine()));

            await investingGrabber.DownloadAllAsync();
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

        private static string GetISMManufacturingFilename(NasdaqType type) => Path.Combine(_dataRoot, "ISM", "Manufacturing ROB", $"{type}.csv");
        private static string GetISMServiceFilename(NasdaqType type) => Path.Combine(_dataRoot, "ISM", "Service ROB", $"{type}.csv");
        private static string GetISMSectorFilename(bool isManufacturing) => Path.Combine(_dataRoot, "ISM", isManufacturing ? "Manufacturing ROB" : "Service ROB", "Sectors.csv");
    }
}