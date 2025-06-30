using Newtonsoft.Json;

namespace Application.DTOs.ExternalApi
{
    public class LocationResponse
    {
        public List<LocationData>? Data { get; set; }
    }

    public class LocationData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [JsonProperty("country-name")]
        public string Country { get; set; } = string.Empty;
        [JsonProperty("city-name")]
        public string City { get; set; } = string.Empty;
    }
} 