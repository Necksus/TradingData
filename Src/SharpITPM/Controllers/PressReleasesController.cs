using IPTMGrabber.InvestorWebsite;
using Microsoft.AspNetCore.Mvc;

namespace SharpITPM.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PressReleasesController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<PressReleasesController> _logger;

        public PressReleasesController(ILogger<PressReleasesController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetPressReleases")]
        public async Task<string> GetAsync(string ticker)
        {
            var grabber = new NewsAndEventsGrabber();

            using var memoryStream = new MemoryStream();
            await grabber.GrabPressReleasesAsync(ticker, memoryStream, default);
            memoryStream.Position = 0;
            return await new StreamReader(memoryStream).ReadToEndAsync();
        }
    }
}