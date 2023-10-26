using IPTMGrabber.Edgar;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SharpITPM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EdgarController : ControllerBase
    {
        private readonly EdgarService _edgarService;

        public EdgarController(ILogger<EdgarController> logger, EdgarService edgarService)
        {
            this._edgarService = edgarService;
        }

        [HttpGet("Insiders", Name = "GetInsiders")]
        public async Task<string> GetInsidersAsync(string ticker, CancellationToken cancellationToken)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await _edgarService.GrabInsidersAsync(ticker, memoryStream, cancellationToken);
                memoryStream.Position = 0;
                return await new StreamReader(memoryStream).ReadToEndAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return $"{ex.Message}\n{ex.StackTrace}";
            }
        }


        [HttpGet("Fillings", Name= "GetFillings")]
        public async Task<string> GetFillingsAsync(string ticker, CancellationToken cancellationToken)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await _edgarService.GrabFillings(ticker, memoryStream, cancellationToken);
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
