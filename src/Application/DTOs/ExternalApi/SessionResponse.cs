using Newtonsoft.Json;

namespace Application.DTOs.ExternalApi
{
    public class SessionResponse
    {
        public SessionData? Data { get; set; }
    }

    public class SessionData
    {
        [JsonProperty("session-id")]
        public string SessionId { get; set; }

        [JsonProperty("device-id")]
        public string DeviceId { get; set; }
    }
} 