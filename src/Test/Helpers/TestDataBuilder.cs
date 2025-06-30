using Application.DTOs;
using Application.DTOs.ExternalApi;

namespace Test.Helpers
{
    public static class TestDataBuilder
    {
        public static BusTourDto CreateValidJourney(int id = 1) => new BusTourDto
        {
            Name = $"Test Journey {id}",
            Description = $"Test Description {id}",
            DepartureDate = DateTime.Today.AddDays(1).AddHours(8),
            ReturnDate = DateTime.Today.AddDays(1).AddHours(13),
            Price = 100.00m + (id * 10),
            AvailableSeats = 50,
            DepartureLocation = "Istanbul",
            Destination = "Ankara",
            IsActive = true
        };

        public static List<BusTourDto> CreateJourneyList(int count = 5) =>
            Enumerable.Range(1, count)
                .Select(i => CreateValidJourney(i))
                .ToList();

        public static List<BusTourDto> CreateMockJourneys(int count = 3) =>
            Enumerable.Range(1, count)
                .Select(i => CreateValidJourney(i))
                .ToList();

        public static BusLocationDto CreateValidLocation(int id = 1) => new BusLocationDto
        {
            Id = id.ToString(),
            Name = id == 1 ? "Istanbul Otogarı" : $"Test Location {id}",
            Country = "Turkey",
            City = $"Test City {id}"
        };

        public static List<BusLocationDto> CreateMockLocations(int count = 3) =>
            Enumerable.Range(1, count)
                .Select(i => CreateValidLocation(i))
                .ToList();

        public static List<BusTourDto> CreateValidJourneys(int count = 3) => CreateMockJourneys(count);
        public static List<BusLocationDto> CreateValidLocations(int count = 3) => CreateMockLocations(count);

        public static SessionDto CreateMockSession()
        {
            return new SessionDto("test-session-id", "test-device-id");
        }
    }
} 