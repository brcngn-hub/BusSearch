using System.Text.Json;

namespace Web.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;

        public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);

            if (context.Response.StatusCode == 429)
            {
                memStream.Seek(0, SeekOrigin.Begin);
                var originalResponse = await new StreamReader(memStream).ReadToEndAsync();

                var clientId = context.User?.Identity?.Name ?? "anonymous";
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                _logger.LogWarning("Hız sınırı aşıldı. ClientId: {ClientId}, IP: {IpAddress}, Yol: {Path}",
                    clientId, ipAddress, context.Request.Path);

                context.Response.ContentType = "application/json";
                memStream.SetLength(0); 

                var response = new
                {
                    Error = "Rate limit exceeded",
                    Message = "Çok fazla istek gönderdiniz. Lütfen bir süre bekleyin.",
                    RetryAfter = context.Response.Headers.ContainsKey("Retry-After") ? context.Response.Headers["Retry-After"].ToString() : "60",
                    Timestamp = DateTime.UtcNow
                };

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(jsonResponse);
            }
            else
            {
                memStream.Seek(0, SeekOrigin.Begin);
                await memStream.CopyToAsync(originalBodyStream);
            }
            context.Response.Body = originalBodyStream;
        }
    }
} 