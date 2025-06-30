using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IExternalBusApiService, ExternalBusApiService>(client =>
            {
                var baseUrl = configuration["ExternalApi:BaseUrl"] ?? "https://v2-api.obilet.com";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var apiKey = configuration["ExternalApi:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {apiKey}");
                }
            });
 
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();
            services.AddScoped<CacheInvalidationService>();

            return services;
        }
    }
} 