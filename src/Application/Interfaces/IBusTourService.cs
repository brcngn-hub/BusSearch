using Application.DTOs;

namespace Application.Interfaces
{
    public interface IBusTourService
    {
        Task<IEnumerable<BusTourDto>> GetJourneysAsync(string originId, string destinationId, DateTime departureDate, string sessionId, string deviceId);
        Task<IEnumerable<BusLocationDto>> GetBusLocationsAsync(string sessionId, string deviceId);
        Task<IEnumerable<BusLocationDto>> GetBusLocationsAsync(string sessionId, string deviceId, string searchTerm);
        Task<SessionDto> CreateObiletSessionAsync();
    }
} 