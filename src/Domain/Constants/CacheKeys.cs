namespace Domain.Constants
{
    public static class CacheKeys
    {
        public const string SessionPrefix = "session:";
        public static string SessionKey(string sessionId) => $"{SessionPrefix}{sessionId}";
        
        public const string LocationsAll = "locations:all";
        public const string LocationsSearchPrefix = "locations:search:";
        public static string LocationsSearchKey(string searchTerm) => $"{LocationsSearchPrefix}{searchTerm?.ToLowerInvariant() ?? "all"}";
        
        public const string JourneysPrefix = "journeys:";
        public static string JourneysKey(string originId, string destinationId, DateTime departureDate) => 
            $"{JourneysPrefix}{originId}:{destinationId}:{departureDate:yyyy-MM-dd}";
        
        public static class Expiration
        {
            public static readonly TimeSpan Session = TimeSpan.FromHours(1);
            public static readonly TimeSpan Locations = TimeSpan.FromMinutes(30);
            public static readonly TimeSpan Journeys = TimeSpan.FromMinutes(15);
        }
    }
} 