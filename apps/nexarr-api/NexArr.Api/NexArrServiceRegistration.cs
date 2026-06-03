using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
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
        builder.Services.AddScoped<MfaService>();
        builder.Services.AddScoped<PasswordResetService>();
        builder.Services.AddScoped<PlatformAuthorizationService>();
        builder.Services.AddScoped<TenantAdminService>();
        builder.Services.AddScoped<TenantMembershipAdminService>();
        builder.Services.AddScoped<PlatformUserAdminService>();
        builder.Services.AddScoped<ProductCatalogService>();
        builder.Services.AddScoped<ProductManifestService>();
        builder.Services.AddScoped<EntitlementAdminService>();
        builder.Services.AddScoped<ServiceTokenAdminService>();
        builder.Services.AddScoped<ServiceTokenDiscoveryService>();
        builder.Services.AddScoped<IntegrationTokenBootstrapService>();
        builder.Services.AddScoped<LaunchService>();
        builder.Services.AddScoped<CallbackAllowlistAdminService>();
        builder.Services.AddScoped<PlatformAdminService>();
        builder.Services.AddScoped<PlatformAuditPackageService>();
        builder.Services.AddScoped<PlatformAuditPackageGenerationService>();
        builder.Services.AddScoped<PlatformSessionSettingsService>();
        builder.Services.AddScoped<ServiceTokenCleanupSettingsService>();
        builder.Services.AddScoped<ServiceTokenCleanupWorkerService>();
        builder.Services.AddScoped<PlatformOutboxEnqueueService>();
        builder.Services.AddScoped<PlatformOutboxPublisherSettingsService>();
        builder.Services.AddScoped<PlatformOutboxPublisherWorkerService>();
        builder.Services.AddScoped<TenantProductLicenseAdminService>();
        builder.Services.AddScoped<EntitlementReconciliationSettingsService>();
        builder.Services.AddScoped<EntitlementReconciliationWorkerService>();
        builder.Services.AddScoped<TenantLifecycleSettingsService>();
        builder.Services.AddScoped<TenantLifecycleWorkerService>();
        builder.Services.AddScoped<PersonLoginDisableService>();
        builder.Services.AddScoped<PersonLoginEnableService>();
        builder.Services.AddScoped<PlatformIdentityIntegrationService>();
        builder.Services.AddScoped<PlatformLifecycleOverviewService>();
        builder.Services.AddScoped<PlatformWorkerHealthOrchestrationService>();
        builder.Services.AddScoped<HybridDataPlaneService>();
        builder.Services.AddHttpClient(HybridDataPlaneService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        builder.Services.AddScoped<CompanionAuthService>();
        builder.Services.AddScoped<CompanionFieldInboxService>();
        builder.Services.AddScoped<CompanionProductClient>();
        builder.Services.AddHttpClient(nameof(CompanionProductClient));
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.AddScoped<CompanionNotificationSettingsService>();
        builder.Services.AddScoped<CompanionNotificationEnqueueService>();
        builder.Services.AddScoped<CompanionNotificationDispatchService>();
        builder.Services.AddScoped<CompanionPushSubscriptionService>();
        builder.Services.AddSingleton<ICompanionWebPushSender, CompanionWebPushSender>();
        builder.Services.Configure<CompanionWebPushOptions>(builder.Configuration.GetSection(CompanionWebPushOptions.SectionName));
        builder.Services.AddHttpClient(CompanionNotificationDispatchService.WebhookHttpClientName);
        builder.Services.AddScoped<CompanionOfflineSyncService>();
        builder.Services.AddScoped<CompanionFieldEvidenceService>();
        builder.Services.AddScoped<CompanionFieldDvirService>();
        builder.Services.AddScoped<CompanionFieldInspectionService>();
        builder.Services.AddScoped<CompanionFieldWorkOrderService>();
        builder.Services.AddScoped<CompanionFieldReceivingService>();
        builder.Services.AddScoped<CompanionFieldSubmissionService>();
        builder.Services.AddScoped<CompanionFieldTaskValidationService>();
        builder.Services.AddScoped<CompanionScanResolveService>();
        builder.Services.AddScoped<PlatformHealthService>();
        builder.Services.AddHttpClient(PlatformHealthService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        builder.Services.Configure<CompanionProductUrlsOptions>(options =>
        {
            var configuration = builder.Configuration;
            options.StaffArrBaseUrl = ResolveProductBaseUrl(configuration, "StaffArr", options.StaffArrBaseUrl);
            options.TrainArrBaseUrl = ResolveProductBaseUrl(configuration, "TrainArr", options.TrainArrBaseUrl);
            options.MaintainArrBaseUrl = ResolveProductBaseUrl(configuration, "MaintainArr", options.MaintainArrBaseUrl);
            options.RoutArrBaseUrl = ResolveProductBaseUrl(configuration, "RoutArr", options.RoutArrBaseUrl);
            options.SupplyArrBaseUrl = ResolveProductBaseUrl(configuration, "SupplyArr", options.SupplyArrBaseUrl);
        });
        builder.Services.Configure<PlatformProductUrlsOptions>(options =>
        {
            var configuration = builder.Configuration;
            options.NexArrBaseUrl = ResolveProductBaseUrl(configuration, "NexArr", options.NexArrBaseUrl);
            options.StaffArrBaseUrl = ResolveProductBaseUrl(configuration, "StaffArr", options.StaffArrBaseUrl);
            options.TrainArrBaseUrl = ResolveProductBaseUrl(configuration, "TrainArr", options.TrainArrBaseUrl);
            options.MaintainArrBaseUrl = ResolveProductBaseUrl(configuration, "MaintainArr", options.MaintainArrBaseUrl);
            options.RoutArrBaseUrl = ResolveProductBaseUrl(configuration, "RoutArr", options.RoutArrBaseUrl);
            options.SupplyArrBaseUrl = ResolveProductBaseUrl(configuration, "SupplyArr", options.SupplyArrBaseUrl);
            options.ComplianceCoreBaseUrl = ResolveProductBaseUrl(configuration, "ComplianceCore", options.ComplianceCoreBaseUrl);
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

        var loginPermitLimit = builder.Configuration.GetValue("Auth:LoginRateLimitPermitLimit", 100);
        var loginWindowSeconds = builder.Configuration.GetValue("Auth:LoginRateLimitWindowSeconds", 60);
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("NexArrAuthThrottle", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = loginPermitLimit,
                        Window = TimeSpan.FromSeconds(loginWindowSeconds),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseRateLimiter();
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
            var platformProductUrls = scope.ServiceProvider.GetService<IOptions<PlatformProductUrlsOptions>>()?.Value;
            await PlatformSeeder.SeedAsync(db, passwordHasher, launchOptions, platformProductUrls);
            await PlatformSeeder.EnsureDevSuiteShellOriginsAsync(db);
        }
        else if (app.Environment.IsProduction())
        {
            var launchOptions = scope.ServiceProvider.GetService<IOptions<StlLaunchOptions>>()?.Value;
            var platformProductUrls = scope.ServiceProvider.GetService<IOptions<PlatformProductUrlsOptions>>()?.Value;
            await PlatformSeeder.SeedAsync(db, passwordHasher, launchOptions, platformProductUrls);

            var productionOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var origin in new[]
                     {
                         app.Configuration["Cors:SuiteFrontendOrigin"],
                         app.Configuration["Cors:CompanionFrontendOrigin"],
                     })
            {
                if (!string.IsNullOrWhiteSpace(origin))
                {
                    productionOrigins.Add(origin.Trim().TrimEnd('/'));
                }
            }

            if (launchOptions?.Products is not null)
            {
                foreach (var profile in launchOptions.Products.Values)
                {
                    if (!string.IsNullOrWhiteSpace(profile.BaseUrl))
                    {
                        productionOrigins.Add(profile.BaseUrl.Trim().TrimEnd('/'));
                    }
                }
            }

            await PlatformSeeder.EnsureSuiteShellOriginsAsync(db, productionOrigins);

            var bootstrap = scope.ServiceProvider.GetRequiredService<IntegrationTokenBootstrapService>();
            await bootstrap.EnsureProvisionedAsync();
        }
    }

    private static string ResolveProductBaseUrl(
        IConfiguration configuration,
        string productSectionName,
        string fallback)
    {
        var hierarchicalKey = $"{productSectionName}:BaseUrl";
        var legacyFlatKey = $"{productSectionName}__BaseUrl";
        return configuration[hierarchicalKey]
            ?? configuration[legacyFlatKey]
            ?? fallback;
    }
}
