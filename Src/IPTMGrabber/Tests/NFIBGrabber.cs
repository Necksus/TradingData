using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTMGrabber.Tests
{
    internal class NFIBGrabber
    {
        // http://www.nfib-sbet.org/developers/

        private const string GetIndicators = "http://open.api.nfib-sbet.org/rest/sbetdb/_proc/getIndicators2";
        private const string GetTotal2 = "http://open.api.nfib-sbet.org/rest/sbetdb/_proc/getTotals2";

        public async Task Download()
        {
            var parameters = new Dictionary<string, string> { { "app_name", "sbet" } };

            AddParameter(parameters, "minYear", "IN", "1986");
            AddParameter(parameters, "minMonth", "IN", "1");
            AddParameter(parameters, "maxYear", "IN", DateTime.Now.Year.ToString());
            AddParameter(parameters, "maxMonth", "IN", "12");

            AddParameter(parameters, "indicator", "IN", "expand_employ");

            /*AddParameter(parameters, "questions", "IN", "Q14,Q15,Q18B,Q5,Q8,Q7,Q17,Q17A,Q4,Q22");
            AddParameter(parameters, "industry", "IN", "1,2,3,4,5,6,7,8,9");
            AddParameter(parameters, "employee", "IN", "");
            AddParameter(parameters, "statev", "IN", "");*/

            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(parameters);

                var response = await httpClient.PostAsync(GetIndicators, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine("Erreur lors de l'appel à l'API : " + response.StatusCode);
                }
            }

        }

        private void AddParameter(Dictionary<string, string> parameters, string name, string type, string value)
        {
            int index = (parameters.Count - 1) / 3;

            parameters.Add($"params[{index}][name]", name);
            parameters.Add($"params[{index}][param_type]", type);
            parameters.Add($"params[{index}][value]", value);
        }
    }
}
