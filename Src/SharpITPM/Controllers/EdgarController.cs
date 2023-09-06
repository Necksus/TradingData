using IPTMGrabber.Edgar;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SharpITPM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EdgarController : ControllerBase
    {
        private readonly EdgarGrabber _edgarGrabber;

        public EdgarController(ILogger<EdgarController> logger, EdgarGrabber _edgarGrabber)
        {
            this._edgarGrabber = _edgarGrabber;
        }

        [HttpGet(Name = "GetInsiders")]
        public async Task<string> GetAsync(string cik, CancellationToken cancellationToken)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await _edgarGrabber.GragInsidersAsync(cik, memoryStream, cancellationToken);
                memoryStream.Position = 0;
                return await new StreamReader(memoryStream).ReadToEndAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return $"{ex.Message}\n{ex.StackTrace}";
            }
        }

    }
}
