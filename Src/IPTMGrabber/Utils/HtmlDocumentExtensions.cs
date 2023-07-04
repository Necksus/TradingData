using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace IPTMGrabber.Utils
{
    public static class HtmlDocumentExtensions
    {
        public static IEnumerable<T> ParseTable<T>(this HtmlDocument doc, string xpath, int index = 0, Func<int, string,string> preprocess = null) 
            where T: class, new()
        {
            var tableNodes = doc.DocumentNode.SelectNodes(xpath);
            var mapping = typeof(T).GetProperties();

            preprocess = preprocess ?? ((i,v) => v);

            if (tableNodes?.Count > index)
            {
                foreach (var row in tableNodes[index].SelectNodes("tr").Skip(1))
                {
                    var newResult = new T();
                    var tdNodes = row.SelectNodes("td");
                    for (int i = 0; i < tdNodes.Count; i++)
                    {
                        var currentProperty = mapping.FirstOrDefault(p =>
                            p.GetCustomAttributes(true).OfType<TableColumnAttribute>().FirstOrDefault()
                                ?.ColumnIndex == i);
                        var value = preprocess (i, tdNodes[i].InnerText.Replace("&nbsp;", ""));
                        if (currentProperty != null && !string.IsNullOrEmpty(value))
                        {
                            currentProperty.SetValue(newResult,
                                currentProperty.PropertyType.IsEnum
                                    ? Enum.Parse(currentProperty.PropertyType, value)
                                    : Convert.ChangeType(value, currentProperty.PropertyType));
                        }
                    }
                    yield return newResult;
                }
            }
        }
    }
}
