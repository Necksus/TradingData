using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IPTMGrabber.ISM
{
    internal class PrNewsWireGrabber
    {
        public ROBBase ParseISMReport<T>(string url) where T : ROBBase, new()
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
            var doc = web.Load(url);

            var dateNode = doc.DocumentNode.SelectSingleNode("//p[@class='mb-no']");
            if (dateNode != null)
            {
                // Url from prnewswire.com
                var date = DateTime.ParseExact(dateNode.InnerText, "MMM dd, yyyy, HH:mm 'ET'", System.Globalization.CultureInfo.InvariantCulture);

                var report = new T
                {
                    Date = new DateTime(date.Year, date.Month, 1).AddMonths(-1)
                };

                foreach (var divNode in doc.DocumentNode.SelectNodes("//div[@class='table-responsive']"))
                {
                    var tableNode = divNode.SelectSingleNode(".//table");
                    report.Fill(tableNode);
                }

                var respondentsNode = 
                    doc.DocumentNode.SelectSingleNode("//b[text()='WHAT RESPONDENTS ARE SAYING']")?.ParentNode ??
                    doc.DocumentNode.SelectSingleNode("//p[text()='WHAT RESPONDENTS ARE SAYING']");
                report.FillSectors(respondentsNode.PreviousSibling.PreviousSibling);

                return report;
            }
            else
            {
                // Url from ismworld.org
                var dateNodes = doc.DocumentNode.SelectNodes("//h1[@class='text-center']");
                var match = Regex.Match(dateNodes.Last().InnerText, @"(?<date>\w+\s+\d{4})");
                if (match.Success)
                {
                    var report = new T
                    {
                        Date = DateTime.ParseExact(match.Groups["date"].Value, "MMMM yyyy", CultureInfo.InvariantCulture)
                    };

                    foreach (var tableNode in doc.DocumentNode.SelectNodes("//table[@class='table table-bordered table-hover']"))
                    {
                        report.Fill(tableNode);
                    }

                    var respondentsNode = doc.DocumentNode.SelectSingleNode("//h3[text()='WHAT RESPONDENTS ARE SAYING']");
                    report.FillSectors(respondentsNode.PreviousSibling.PreviousSibling);

                    return report;
                }
                throw new NotImplementedException();
            }
        }
    }
}
