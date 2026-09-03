using EnterpriseEmployeeManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseEmployeeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalApiController : ControllerBase
    {
        private readonly ExternalApiService _externalApiService;

        public ExternalApiController(
            ExternalApiService externalApiService)
        {
            _externalApiService =
                externalApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            CancellationToken cancellationToken)
        {
            var result =
                await _externalApiService
                    .GetDataAsync(cancellationToken);

            return Ok(new
            {
                message =
                    "External API call successful.",

                data = result
            });
        }
    }
}