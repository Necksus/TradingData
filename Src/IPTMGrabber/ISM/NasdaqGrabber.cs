using System.Net;

namespace IPTMGrabber.ISM
{
    internal class NasdaqGrabber
    {
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
