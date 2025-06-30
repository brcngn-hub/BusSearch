using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusToursApiController : ControllerBase
    {
        private readonly IBusTourService _busTourService;
        private readonly ILogger<BusToursApiController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "ObiletSessionId";
        private const string DeviceKey = "ObiletDeviceId";

        public BusToursApiController(IBusTourService busTourService, ILogger<BusToursApiController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _busTourService = busTourService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<(string SessionId, string DeviceId)> GetOrCreateUserSessionAsync()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sessionId = session.GetString(SessionKey);
            var deviceId = session.GetString(DeviceKey);
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(deviceId))
            {
                var sessionPair = await _busTourService.CreateObiletSessionAsync();
                sessionId = sessionPair.SessionId;
                deviceId = sessionPair.DeviceId;
                session.SetString(SessionKey, sessionId);
                session.SetString(DeviceKey, deviceId);
            }
            return (sessionId, deviceId);
        }

        [HttpGet("locations")]
        public async Task<ActionResult<IEnumerable<BusLocationDto>>> GetLocations([FromQuery] string? search = null)
        {
            try
            {
                var (sessionId, deviceId) = await GetOrCreateUserSessionAsync();
                IEnumerable<BusLocationDto> locations;
                
                if (!string.IsNullOrWhiteSpace(search))
                {
                    locations = await _busTourService.GetBusLocationsAsync(sessionId, deviceId, search.Trim());
                }
                else
                {
                    locations = await _busTourService.GetBusLocationsAsync(sessionId, deviceId);
                }
                
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otobüs lokasyonları alınırken hata oluştu");
                return StatusCode(500, "Otobüs lokasyonları alınırken hata oluştu");
            }
        }

        [HttpGet("journeys")]
        public async Task<ActionResult<IEnumerable<BusTourDto>>> GetJourneys(
            [FromQuery] string originId, 
            [FromQuery] string destinationId, 
            [FromQuery] DateTime departureDate)
        {
            try
            {
                if (string.IsNullOrEmpty(originId) || string.IsNullOrEmpty(destinationId))
                {
                    return BadRequest("Kalkış noktası ID'si ve varış noktası ID'si gereklidir");
                }

                var (sessionId, deviceId) = await GetOrCreateUserSessionAsync();
                var journeys = await _busTourService.GetJourneysAsync(originId, destinationId, departureDate, sessionId, deviceId);
                
                return Ok(journeys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seferler alınırken hata oluştu");
                return StatusCode(500, "Seferler alınırken hata oluştu");
            }
        }
    }
} 