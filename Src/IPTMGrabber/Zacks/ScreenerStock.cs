using CsvHelper.Configuration.Attributes;

namespace IPTMGrabber.Zacks
{
    public class ScreenerStock
    {

        [Name("Company Name")]
        public string CompanyName { get; set; }

        public string Ticker { get; set; }

        [Name("Market Cap (mil)")]
        public double MarketCap { get; set; }

        [Name("Exchange")]
        public string Exchange { get; set; }

        [Name("Month of Fiscal Yr End")]
        public int MonthOfFiscalYearEnd { get; set; }

        [Name("Sector")]
        public string ZacksSector { get; set; }

        [Name("Industry")]
        public string ZacksIndustry { get; set; }

        [Name("Last Close")]
        public double LastClosePrice { get; set; }

        [Name("Last Reported Fiscal Yr  (yyyymm)")]
        public int LastReportedFiscalYear { get; set; }

        [Name("Last Yr`s EPS (F0) Before NRI")]
        public double? EPS0 { get; set; }

        [Name("F1 Consensus Est.")]
        public double? EPS1 { get; set; }

        [Name("F2 Consensus Est.")]
        public double? EPS2 { get; set; }

        public double? EG1
        {
            get
            {
                var result = EPS0.HasValue ? (EPS1 - EPS0) / Math.Abs(EPS0.Value) : null;
                if (result.HasValue && EPS0 < 0 && EPS1 > 0)
                    return Math.Min(result.Value, 1);
                if (result.HasValue && EPS0 > 0 && EPS1 < 0)
                    return Math.Max(result.Value, -1);

                return result;
            }
        }

        public double? EG2
        {
            get
            {
                var result = EPS1.HasValue ? (EPS2 - EPS1) / Math.Abs(EPS1.Value) : null;

                if (result.HasValue && EPS1 < 0 && EPS2 > 0)
                    return Math.Min(result.Value, 1);

                if (result.HasValue && EPS1 > 0 && EPS2 < 0)
                    Math.Max(result.Value, -1);

                return result;
            }
        }

        public double? PE1 => LastClosePrice / EPS1;

        public double? PE2 => LastClosePrice / EPS2;

        public double? PEG1 => PE1 / (EG1 * 100);

        public double? PEG2 => PE2 / (EG2 * 100);
    }
}
