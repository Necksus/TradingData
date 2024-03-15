using HtmlAgilityPack;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IPTMGrabber.Nasdaq;
using IPTMGrabber.Utils;

namespace IPTMGrabber.ISM
{
    internal abstract class ROBBase
    {
        private readonly string[] _industriesNames;

        public DateTime Date { get; set; }

        public string Value { get; private set; }

        public string Name { get; }

        public NasdaqType Type { get; }

        public IReadOnlyList<PMIDetail> Details { get; }

        public Sectors Sectors { get; private set; }

        public ROBBase(string name, NasdaqType type, PMIDetail[] details, string[] industriesNames)
        {
            _industriesNames = industriesNames;
            Name = name;
            Type = type;
            Details = details;
        }

        public void Fill(HtmlNode tableNode)
        {
            var trNodes = tableNode.SelectNodes(".//tr");
            if (trNodes.Count < 2) 
                return;
            var headerNodes = trNodes[0].SelectNodes(".//td|.//th");
            var valueNodes = trNodes[1].SelectNodes(".//td|.//th");

            if (headerNodes.Count >= 2 && valueNodes.Count >= 2 && headerNodes[1].InnerTextEqual(Name))
            {
                if (string.IsNullOrEmpty(Value))
                {
                    var valueDate = DateTime.ParseExact(valueNodes[0].InnerText.Trim(), "MMM yyyy", CultureInfo.InvariantCulture);

                    if (valueDate.Year != Date.Year || valueDate.Month != Date.Month)
                        throw new InvalidOperationException($"{Name} value is not for the expected date");
                    Value = valueNodes[1].InnerText.Trim();
                }
            }
            else if (!string.IsNullOrEmpty(Value))
            {
                foreach (var detail in Details)
                {
                    if (detail.Fill(headerNodes, valueNodes))
                        break;
                }
            }
        }

        public void FillSectors(HtmlNode node)
        {
            var fullDescription = node.InnerText
                    .Replace("&amp;", "&")
                    .Replace(" is ", " are: ")      // FIXME: ugly fix to manage "The only industry reporting a decrease in April is ..."
                    .Replace("&nbsp;", " ");

            var result = _industriesNames.Order().ToDictionary(n => n, _ => 0);
            foreach (var sentence in fullDescription.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                var match = Regex.Match(sentence, @"(?<=:\s)(?<industries>.*?)(?=\s*$)");

                if (match.Success)
                {
                    var industrieNames = match.Value
                        .Replace("; and", ";")
                        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    var weight = 0;
                    var direction = Regex.Match(sentence, @"\b(?:reported|reporting|report)\b\s+(?<direction>.*)").Value;
                    if (direction == null)
                        throw new InvalidOperationException("Cannot find reporting direction");

                    if (direction.Contains("growth"))
                    {
                        weight = industrieNames.Length;
                    }
                    else if (direction.Contains("contraction") ||
                             direction.Contains("decline") ||
                             direction.Contains("decrease"))
                    {
                        weight = -1;
                    }
                    else
                    {
                        return;
                        throw new InvalidOperationException($"Reported direction '{direction}' is not supported");
                    }

                    foreach (var element in industrieNames)
                    {
                        if (!result.ContainsKey(element))
                            throw new InvalidOperationException($"Unknown industry {element} found!");
                        result[element] = weight--;
                    }
                }
            }

            Sectors = new Sectors(Name, result);
        }

        public string GetCsvLine()
            => $"{Date:yyyy-MM-dd},{Value}";

        public string GetDetailCsvLine(string detailName)
            => Details.Single(d => d.Name.Equals(detailName, StringComparison.OrdinalIgnoreCase)).GetCsvLine(Date);

        public string GetSectorCsvLine(bool isValue = true)
        {
            var sb = new StringBuilder();
            sb.Append(isValue ? Date.ToString("yyyy-MM-dd") : "Date");
            foreach (var sector in Sectors.Industries)
            {
                sb.Append($", {(isValue ? sector.Value : sector.Key.Replace(",", ""))}");
            }
            return sb.ToString();
        }
    }
}
