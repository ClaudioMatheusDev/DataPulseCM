using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EtlMonitoring.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Verifica o status geral da aplicação
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var health = await _healthCheckService.CheckHealthAsync();

            var result = new
            {
                status = health.Status.ToString(),
                totalDuration = health.TotalDuration.TotalMilliseconds,
                checks = health.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    exception = e.Value.Exception?.Message,
                    data = e.Value.Data
                })
            };

            return health.Status == HealthStatus.Healthy 
                ? Ok(result) 
                : StatusCode(503, result);
        }

        /// <summary>
        /// Endpoint simples para verificação rápida
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new 
            { 
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "DataPulseCM API"
            });
        }
    }
}
