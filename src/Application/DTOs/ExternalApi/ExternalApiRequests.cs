using Newtonsoft.Json;

namespace Application.DTOs.ExternalApi
{
    public class GetSessionRequest
    {
        [JsonProperty("type")]
        public int Type { get; set; } = 7;  // Sabit değer

        [JsonProperty("connection")]
        public GetSessionConnection Connection { get; set; }

        [JsonProperty("browser")]
        public GetSessionBrowser Browser { get; set; }
    }

    public class GetSessionConnection
    {
        [JsonProperty("ip-address")]
        public string IpAddress { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }
    }

    public class GetSessionBrowser
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
    
    public class GetBusLocationsRequest
    {
        [JsonProperty("data")]
        public string Data { get; set; }
        
        [JsonProperty("device-session")]
        public SessionData DeviceSession { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }
    
    public class GetJourneysRequest
    {
        [JsonProperty("device-session")]
        public SessionData DeviceSession { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("data")]
        public GetJourneysData Data { get; set; }
    }

    public class GetJourneysData
    {
        [JsonProperty("origin-id")]
        public int OriginId { get; set; }

        [JsonProperty("destination-id")]
        public int DestinationId { get; set; }

        [JsonProperty("departure-date")]
        public string DepartureDate { get; set; }
    }
} 