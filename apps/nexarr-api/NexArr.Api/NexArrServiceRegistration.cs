using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Data;
using NexArr.Api.Options;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api;

public static class NexArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddStlJwtAuthentication(builder.Configuration);
        builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IPlatformAuditService, PlatformAuditService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<PlatformAuthorizationService>();
        builder.Services.AddScoped<TenantAdminService>();
        builder.Services.AddScoped<ProductCatalogService>();
        builder.Services.AddScoped<EntitlementAdminService>();
        builder.Services.AddScoped<ServiceTokenAdminService>();
        builder.Services.AddScoped<IntegrationTokenBootstrapService>();
        builder.Services.AddScoped<LaunchService>();
        builder.Services.AddScoped<CallbackAllowlistAdminService>();
        builder.Services.AddScoped<PlatformAdminService>();
        builder.Services.AddScoped<CompanionAuthService>();
        builder.Services.AddScoped<CompanionFieldInboxService>();
        builder.Services.AddScoped<CompanionProductClient>();
        builder.Services.AddHttpClient(nameof(CompanionProductClient));
        builder.Services.AddScoped<PlatformHealthService>();
        builder.Services.AddHttpClient(PlatformHealthService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        builder.Services.Configure<CompanionProductUrlsOptions>(options =>
        {
            var configuration = builder.Configuration;
            options.StaffArrBaseUrl = configuration["StaffArr__BaseUrl"] ?? options.StaffArrBaseUrl;
            options.TrainArrBaseUrl = configuration["TrainArr__BaseUrl"] ?? options.TrainArrBaseUrl;
            options.MaintainArrBaseUrl = configuration["MaintainArr__BaseUrl"] ?? options.MaintainArrBaseUrl;
            options.RoutArrBaseUrl = configuration["RoutArr__BaseUrl"] ?? options.RoutArrBaseUrl;
            options.SupplyArrBaseUrl = configuration["SupplyArr__BaseUrl"] ?? options.SupplyArrBaseUrl;
        });
        builder.Services.Configure<PlatformProductUrlsOptions>(options =>
        {
            var configuration = builder.Configuration;
            options.StaffArrBaseUrl = configuration["StaffArr__BaseUrl"] ?? options.StaffArrBaseUrl;
            options.TrainArrBaseUrl = configuration["TrainArr__BaseUrl"] ?? options.TrainArrBaseUrl;
            options.MaintainArrBaseUrl = configuration["MaintainArr__BaseUrl"] ?? options.MaintainArrBaseUrl;
            options.RoutArrBaseUrl = configuration["RoutArr__BaseUrl"] ?? options.RoutArrBaseUrl;
            options.SupplyArrBaseUrl = configuration["SupplyArr__BaseUrl"] ?? options.SupplyArrBaseUrl;
            options.ComplianceCoreBaseUrl = configuration["ComplianceCore__BaseUrl"] ?? options.ComplianceCoreBaseUrl;
        });
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.Configure<StlLaunchOptions>(builder.Configuration.GetSection(StlLaunchOptions.SectionName));
    }

    public static async Task InitializeAsync(WebApplication app)
    {
        if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("Database"))
            && string.IsNullOrWhiteSpace(app.Configuration["DATABASE_URL"]))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
        {
            var launchOptions = scope.ServiceProvider.GetService<IOptions<StlLaunchOptions>>()?.Value;
            await PlatformSeeder.SeedAsync(db, passwordHasher, launchOptions);
            await PlatformSeeder.EnsureDevSuiteShellOriginsAsync(db);
        }
        else if (app.Environment.IsProduction())
        {
            await db.Database.MigrateAsync();
            var bootstrap = scope.ServiceProvider.GetRequiredService<IntegrationTokenBootstrapService>();
            await bootstrap.EnsureProvisionedAsync();
        }
    }
}
