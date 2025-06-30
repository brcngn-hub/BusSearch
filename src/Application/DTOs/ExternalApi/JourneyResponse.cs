using Newtonsoft.Json;

namespace Application.DTOs.ExternalApi
{
    public class JourneyResponse
    {
        public List<JourneyData>? Data { get; set; }
    }

    public class JourneyData
    {
        public int Id { get; set; }
        [JsonProperty("partner-name")]
        public string PartnerName { get; set; }
        [JsonProperty("bus-type-name")]
        public string BusTypeName { get; set; }
        [JsonProperty("total-seats")]
        public int TotalSeats { get; set; }
        [JsonProperty("available-seats")]
        public int AvailableSeats { get; set; }
        public JourneyInfo Journey { get; set; }
    }

    public class JourneyInfo
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        [JsonProperty("original-price")]
        public decimal OriginalPrice { get; set; }
        [JsonProperty("stops")]
        public List<StopInfo>? Stops { get; set; }
        [JsonProperty("features")]
        public List<string>? Features { get; set; }
    }

    public class StopInfo
    {
        public int Id { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("station")]
        public string? Station { get; set; }
        [JsonProperty("time")]
        public string? Time { get; set; }
        [JsonProperty("is-origin")]
        public bool IsOrigin { get; set; }
        [JsonProperty("is-destination")]
        public bool IsDestination { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }
    }
} 