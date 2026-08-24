using EnterpriseEmployeeManagement.WebApi.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EnterpriseEmployeeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfoController : ControllerBase
    {
        private readonly ApiSettings _apiSettings;

        public InfoController(IOptions<ApiSettings> apiSettings)
        {
            _apiSettings = apiSettings.Value;
        }

        [HttpGet]
        public IActionResult GetApiInfo()
        {
            return Ok(new
            {
                ApplicationName = _apiSettings.ApplicationName,
                Version = _apiSettings.Version,
                EnableDetailedErrors = _apiSettings.EnableDetailedErrors
            });
        }
    }
}