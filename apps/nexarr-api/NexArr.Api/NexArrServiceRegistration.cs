using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Data;
using NexArr.Api.Options;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Data;

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
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.AddScoped<CompanionNotificationSettingsService>();
        builder.Services.AddScoped<CompanionNotificationEnqueueService>();
        builder.Services.AddScoped<CompanionNotificationDispatchService>();
        builder.Services.AddHttpClient(CompanionNotificationDispatchService.WebhookHttpClientName);
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
        builder.Services.Configure<StlLaunchOptions>(builder.Configuration.GetSection(StlLaunchOptions.SectionName));

        var suiteFrontendOrigin = builder.Configuration["Cors:SuiteFrontendOrigin"] ?? "http://localhost:5174";
        var companionFrontendOrigin = builder.Configuration["Cors:CompanionFrontendOrigin"] ?? "http://localhost:5181";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("NexArrBrowserClients", policy =>
            {
                policy.WithOrigins(suiteFrontendOrigin, companionFrontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("NexArrBrowserClients");
    }

    public static async Task InitializeAsync(WebApplication app)
    {
        if (string.IsNullOrWhiteSpace(StlDatabaseConnection.Resolve(app.Configuration)))
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
            var launchOptions = scope.ServiceProvider.GetService<IOptions<StlLaunchOptions>>()?.Value;
            await PlatformSeeder.SeedAsync(db, passwordHasher, launchOptions);

            var productionOrigins = new[]
            {
                app.Configuration["Cors:SuiteFrontendOrigin"],
                app.Configuration["Cors:CompanionFrontendOrigin"],
            }.Where(static origin => !string.IsNullOrWhiteSpace(origin)).Select(static origin => origin!);
            await PlatformSeeder.EnsureSuiteShellOriginsAsync(db, productionOrigins);

            var bootstrap = scope.ServiceProvider.GetRequiredService<IntegrationTokenBootstrapService>();
            await bootstrap.EnsureProvisionedAsync();
        }
    }
}
