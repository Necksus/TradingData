using HtmlAgilityPack;

namespace IPTMGrabber.Utils
{
    internal static class HtmlNodeExtension
    {
        public static bool InnerTextEqual(this HtmlNode node, string value)
        {
            var cleanInnerText = node.InnerText
                .Replace("&nbsp;", "")
                .Replace("&reg;", "®")
                .Replace(" ", "")
                .Replace("% ", "%")
                .Trim();

            return string.Equals(cleanInnerText, value.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
