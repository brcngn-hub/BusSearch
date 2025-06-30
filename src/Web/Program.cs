using Application.Interfaces;
using Application.Services;
using Application.Validators;
using Infrastructure.Configuration;
using Web.Middleware;
using Web.Services.HealthChecks;
using Serilog;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Servis kayıtları
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<BusTourDtoValidator>();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<IBusTourService, BusTourService>();
builder.Services.AddHttpContextAccessor();

// Session yapılandırması
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<CacheHealthCheck>("cache", tags: new[] { "cache", "critical" })
    .AddCheck<ExternalApiHealthCheck>("external_api", tags: new[] { "external", "critical" })
    .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "system", "critical" })
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "self" });

// Rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimit"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Engin Bus Tour API", 
        Version = "v1",
        Description = "API for bus tour search and booking"
    });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Engin Bus Tour API V1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseIpRateLimiting();
app.UseCors("AllowAll");
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health");
app.MapControllers();

try
{
    Log.Information("Engin Bus Tour uygulaması başlatılıyor");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama beklenmedik şekilde sonlandı");
}
finally
{
    Log.CloseAndFlush();
}
