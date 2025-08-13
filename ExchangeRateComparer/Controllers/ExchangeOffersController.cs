using ExchangeRateComparer.Common;
using ExchangeRateComparer.Core.Application.Interfaces.Service;
using ExchangeRateComparer.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExchangeRateComparer.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class ExchangeController(IRateComparisonService service) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Post([FromBody] Exchange request, CancellationToken cancellationToken)
        {
            return Ok(new ApiResponse<CompareResult>(await service.CompareAsync(request, cancellationToken)));
        }
    }

}