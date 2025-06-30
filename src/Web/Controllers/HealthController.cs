using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Controllers
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            
            var result = new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration,
                Entries = report.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration,
                    Data = entry.Value.Data
                })
            };

            return Ok(result);
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            
            if (report.Status == HealthStatus.Healthy)
            {
                return Ok(new { Status = "Hazır", Message = "Uygulama trafik almaya hazır" });
            }
            
            return StatusCode(503, new { Status = "Hazır Değil", Message = "Uygulama henüz hazır değil" });
        }

        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new { Status = "Canlı", Timestamp = DateTime.UtcNow });
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> Detailed()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            
            var detailedResult = new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration,
                Timestamp = DateTime.UtcNow,
                Entries = report.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration,
                    Tags = entry.Value.Tags,
                    Data = entry.Value.Data,
                    Exception = entry.Value.Exception?.Message
                })
            };

            return Ok(detailedResult);
        }
    }
} 