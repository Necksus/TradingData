using HtmlAgilityPack;
using System.Text;
using IPTMGrabber.Nasdaq;
using IPTMGrabber.Utils;

namespace IPTMGrabber.ISM
{
    internal class PMIDetail
    {
        private const string reportingPercent = "%Reporting";
        public NasdaqType Type { get; }
        public string Name { get; }
        public string[] Properties { get; }

        public Dictionary<string, string> Values { get; }

        public PMIDetail(NasdaqType type, string name, params string[] properties)
        {
            Type = type;
            Name = name;
            Properties = properties;
            Values = new Dictionary<string, string>();
        }

        public bool Fill(HtmlNodeCollection headerNodes, HtmlNodeCollection valueNodes)
        {
            // Check that the header is valid
            for (int i = 0; i < headerNodes.Count; i++)
            {
                if (i == 0)
                {
                    if (!headerNodes[i].InnerTextEqual(Name))
                        return false;
                }
                else if (i - 1 > Properties.Length || !headerNodes[i].InnerTextEqual(Properties[i - 1]))
                {
                    throw new InvalidOperationException("Unexpected header name");
                }
                else
                {
                    var value = valueNodes[i].InnerText.Trim().Replace("+", "").Trim();
                    Values.Add(Properties[i - 1], value);
                }
            }
            return true;
        }

        public string GetCsvLine(DateTime date)
        {
            var sb = new StringBuilder();

            sb.Append(date.ToString("yyyy-MM-dd"));

            // WARNING : %Reporting must be on last position!
            foreach (var property in Properties.Where(p =>p != reportingPercent).Union(Properties.Where(p => p == reportingPercent)))
            {
                sb.AppendFormat($",{Values[property]}");
            }
            return sb.ToString();
        }
    }
}
