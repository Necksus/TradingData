using IPTMGrabber.Zacks;
using Microsoft.AspNetCore.Mvc;

namespace SharpITPM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZacksController : ControllerBase
    {
        private readonly ILogger<ZacksController> _logger;
        private readonly ZacksService _zacksService;

        public ZacksController(ILogger<ZacksController> logger, ZacksService zacksService)
        {
            _logger = logger;
            _zacksService = zacksService;
        }

        [HttpGet(Name = "GetStockScreener")]
        public async Task<string> GetAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await _zacksService.GetScreenerAsync(memoryStream, cancellationToken);
                return await new StreamReader(memoryStream).ReadToEndAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return $"{ex.Message}\n{ex.StackTrace}";
            }
        }
    }
}
