using System.Net;

namespace IPTMGrabber.Nasdaq
{
    internal class NasdaqGrabber
    {

        /*
         *      WTI Crude (CL:NMX Historical Data)
         *      https://www.nasdaq.com/market-activity/commodities/cl:nmx/historical
         *      https://api.nasdaq.com/api/quote/CL%3ANMX/historical?assetclass=commodities&fromdate=2013-05-17&limit=9999
         *
         *      Brent (BZ:NMX Historical Data)
         *      https://www.nasdaq.com/market-activity/commodities/bz:nmx/historical
         *      https://api.nasdaq.com/api/quote/BZ%3ANMX/historical?assetclass=commodities&fromdate=2013-05-17&limit=9999
         *
         *      Copper COMEX (HG:CMX Historical Data)
         *      https://www.nasdaq.com/market-activity/commodities/hg:cmx/historical
         *      https://api.nasdaq.com/api/quote/HG%3ACMX/historical?assetclass=commodities&fromdate=2023-04-17&limit=9999
         *
         *      Copper LME
         *      https://www.investing.com/commodities/copper-historical-data?cid=959211

         *
         *      Lumber
         *      https://www.nasdaq.com/market-activity/commodities/lbs/historical
         *      https://api.nasdaq.com/api/quote/LBS/historical?assetclass=commodities&fromdate=2013-05-17&limit=9999
         *
         *      Iron Ore (CME)
         *      https://www.investing.com/commodities/iron-ore-62-cfr-futures
         *
         *      Iron Ore (DCE)
         *      https://www.investing.com/commodities/iron-ore-62-cfr-futures?cid=961741
         * */

        public void Download(NasdaqType type, string filename)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(GetCsvUrl(type), filename);
            }
        }

        private string GetCsvUrl(NasdaqType type)
            => $"https://data.nasdaq.com/api/v3/datasets/ISM/{type}.csv?api_key=sJkvPsKtKA8aXrkCn42z";
    }
}
