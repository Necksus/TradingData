using CsvHelper.Configuration.Attributes;

namespace IPTMGrabber.Zacks
{
    internal class ZacksStock
    {
        // Company Name,Ticker,Market Cap (mil),Exchange,Month of Fiscal Yr End,Sector,Industry,Last Close,Last Reported Fiscal Yr  (yyyymm),Last Yr`s EPS (F0) Before NRI,F1 Consensus Est.,F2 Consensus Est.,EBITDA ($mil),EBIT ($mil),Net Income  ($mil),Cash Flow ($mil),Pretax Income ($mil),Receivables ($mil),Inventory ($mil),Book Value,Common Equity ($mil),Preferred Equity ($mil),Long Term Debt ($mil),Current Liabilities ($mil),Current Assets  ($mil),Intangibles ($mil),Debt/Total Capital,Debt/Equity Ratio,Current Ratio,Quick Ratio,Cash Ratio,Annual Sales ($mil),Beta

        public string Ticker { get; set; }

        [Name("Company Name")]
        public string CompanyName { get; set; }    
    }
}
