namespace Application.DTOs
{
    public class SessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;

        public SessionDto(string sessionId, string deviceId)
        {
            SessionId = sessionId;
            DeviceId = deviceId;
        }
    }
} 