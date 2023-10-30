using IPTMGrabber.Edgar;
using IPTMGrabber.InvestorWebsite;
using IPTMGrabber.Utils.Browser;
using IPTMGrabber.YahooFinance;
using IPTMGrabber.Zacks;
using Microsoft.Extensions.DependencyInjection;

namespace IPTMGrabber.Utils
{
    public static class Services
    {
        public static void AddITPM(this IServiceCollection services)
        {
            services.AddSingleton<BrowserService>();
            services.AddSingleton<NewsAndEventsGrabber>();
            services.AddSingleton<EarningPredictionModel>();
            services.AddSingleton<EdgarService>();
            services.AddSingleton<YahooService>();
            services.AddSingleton<ZacksService>();
        }
    }
}
