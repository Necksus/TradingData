using IPTMGrabber.Nasdaq;

namespace IPTMGrabber.ISM
{
    internal class ServiceROB : ROBBase
    {
        public ServiceROB()
            : base(
                "Services PMI®",
                NasdaqType.NONMAN_NMI,
                new PMIDetail[]
                {
                    new(NasdaqType.NONMAN_BUSACT, "Business Activity", "%Higher", "%Same", "%Lower", "Index"),
                    new(NasdaqType.NONMAN_NEWORD, "New Orders", "%Higher", "%Same", "%Lower", "Index"),
                    new(NasdaqType.NONMAN_EMPL, "Employment", "%Higher", "%Same", "%Lower", "Index"),
                    new(NasdaqType.NONMAN_DELIV, "Supplier Deliveries", "%Slower", "%Same", "%Faster", "Index"),
                    new(NasdaqType.NONMAN_INVENT, "Inventories", "%Higher", "%Same", "%Lower", "Index"),
                    new(NasdaqType.NONMAN_PRICES, "Prices", "%Higher", "%Same", "%Lower", "Index"),
                    new(NasdaqType.NONMAN_BACKLOG, "Backlog of Orders", "%Higher", "%Same", "%Lower", "Index"),
                    new(NasdaqType.NONMAN_EXPORTS, "New Export Orders", "%Higher", "%Same", "%Lower", "Index"),
                    new(NasdaqType.NONMAN_IMPORTS, "Imports", "%Higher", "%Same", "%Lower", "Index"),
                    new(NasdaqType.NONMAN_INVSENT, "Inventory Sentiment", "%Too High", "%About Right", "%Too Low", "Index")
                },
                new[]
                {
                    "Arts, Entertainment & Recreation",
                    "Other Services",
                    "Real Estate, Rental & Leasing",
                    "Accommodation & Food Services",
                    "Utilities",
                    "Public Administration",
                    "Transportation & Warehousing",
                    "Professional, Scientific & Technical Services",
                    "Educational Services",
                    "Health Care & Social Assistance",
                    "Retail Trade",
                    "Construction",
                    "Finance & Insurance",
                    "Information",
                    "Management of Companies & Support Services",
                    "Mining",
                    "Agriculture, Forestry, Fishing & Hunting",
                    "Wholesale Trade"
                })
        {
        }
    }
}
