using Application.DTOs;

namespace Application.Interfaces
{
    public interface IExternalBusApiService
    {
        Task<SessionDto> GetSessionAsync();
        Task<IEnumerable<BusTourDto>> GetJourneysAsync(string sessionId, string deviceId, string originId, string destinationId, DateTime departureDate);
        Task<IEnumerable<BusLocationDto>> GetBusLocationsAsync(string sessionId, string deviceId);
        Task<IEnumerable<BusLocationDto>> GetBusLocationsAsync(string sessionId, string deviceId, string searchTerm);
    }
} 