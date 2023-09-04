using IPTMGrabber.InvestorWebsite;
using Microsoft.Extensions.DependencyInjection;

namespace IPTMGrabber.Utils
{
    public static class Services
    {
        public static void AddITPM(this IServiceCollection services)
        {
            services.AddSingleton<NewsAndEventsGrabber>();
        }
    }
}
