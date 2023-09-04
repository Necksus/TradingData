using HtmlAgilityPack;
using System.Web;

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

        public static string? GetUnescapedText(this HtmlNode node)
            => HttpUtility.HtmlDecode(node?.InnerText?.Trim().Trim('\n'));

        public static string? GetUnescapedAttribute(this HtmlNode node, string attributeName)
            => HttpUtility.HtmlDecode(node.Attributes[attributeName]?.Value?.Trim());

        public static string? GetQuerySelector(this HtmlNode node)
        {
            // Try with id
            var nodeId = node.GetUnescapedAttribute("id");
            if (!string.IsNullOrEmpty(nodeId))
                return $"#{nodeId}";

            // Try with class name
            var nodeClass = node.GetUnescapedAttribute("class");
            if (!string.IsNullOrEmpty(nodeClass))
                return $"{node.Name}[class='{nodeClass}']";

            return null;
        }
    }
}
