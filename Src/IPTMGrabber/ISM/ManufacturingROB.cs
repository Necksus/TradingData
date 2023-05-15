using HtmlAgilityPack;
using System.Globalization;

namespace IPTMGrabber.ISM
{
    internal class ManufacturingROB : ROBBase
    {
        public ManufacturingROB()
            : base(
                "Manufacturing PMI®",
                NasdaqType.MAN_PMI,
                new PMIDetail[]
                {
                    new(NasdaqType.MAN_NEWORDERS, "New Orders", "%Higher", "%Same", "%Lower", "Net", "Index"),
                    new(NasdaqType.MAN_PROD, "Production", "%Higher", "%Same", "%Lower", "Net", "Index"),
                    new(NasdaqType.MAN_EMPL, "Employment", "%Higher", "%Same", "%Lower", "Net", "Index"),
                    new(NasdaqType.MAN_DELIV, "Supplier Deliveries", "%Slower", "%Same", "%Faster", "Net", "Index"),
                    new(NasdaqType.MAN_INVENT, "Inventories", "%Higher", "%Same", "%Lower", "Net", "Index"),
                    new(NasdaqType.MAN_CUSTINV, "Customers' Inventories", "%Reporting", "%Too High", "%About Right", "%Too Low", "Net", "Index"),
                    new(NasdaqType.MAN_PRICES, "Prices", "%Higher", "%Same", "%Lower", "Net", "Index"),
                    new(NasdaqType.MAN_BACKLOG, "Backlog of Orders", "%Reporting", "%Higher", "%Same", "%Lower", "Net", "Index"),
                    new(NasdaqType.MAN_EXPORTS, "New Export Orders", "%Reporting", "%Higher", "%Same", "%Lower", "Net", "Index"),
                    new(NasdaqType.MAN_IMPORTS, "Imports", "%Reporting", "%Higher", "%Same", "%Lower", "Net", "Index")
                },
                new[]
                {
                    "Printing & Related Support Activities",
                    "Apparel, Leather & Allied Products",
                    "Petroleum & Coal Products",
                    "Fabricated Metal Products",
                    "Transportation Equipment",
                    "Textile Mills",
                    "Paper Products",
                    "Furniture & Related Products",
                    "Wood Products",
                    "Nonmetallic Mineral Products",
                    "Electrical Equipment, Appliances & Components",
                    "Plastics & Rubber Products",
                    "Chemical Products",
                    "Machinery",
                    "Primary Metals",
                    "Computer & Electronic Products",
                    "Food, Beverage & Tobacco Products",
                    "Miscellaneous Manufacturing"
                })
        {
        }
    }
}
