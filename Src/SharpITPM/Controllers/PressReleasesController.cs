using IPTMGrabber.InvestorWebsite;
using Microsoft.AspNetCore.Mvc;

namespace SharpITPM.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PressReleasesController : ControllerBase
    {
        private readonly ILogger<PressReleasesController> _logger;
        private readonly NewsAndEventsGrabber _pressReleasesGrabber;

        public PressReleasesController(ILogger<PressReleasesController> logger, NewsAndEventsGrabber pressReleasesGrabber)
        {
            _logger = logger;
            _pressReleasesGrabber = pressReleasesGrabber;
        }

        [HttpGet(Name = "GetPressReleases")]
        public async Task<string> GetAsync(string ticker, CancellationToken cancellationToken)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await _pressReleasesGrabber.GrabPressReleasesAsync(ticker, memoryStream, cancellationToken);
                memoryStream.Position = 0;
                return await new StreamReader(memoryStream).ReadToEndAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                return $"{ex.Message}\n{ex.StackTrace}";
            }
        }
    }
}