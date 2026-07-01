using System.IO;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Data;
using NexArr.Api.Options;
using NexArr.Api.Services;
using STLCompliance.Shared.Ai;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;

namespace NexArr.Api;

public static class NexArrServiceRegistration
{
    internal const string RunMasterReferenceDataOnStartupKey = "Seed:RunMasterReferenceDataOnStartup";

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        LocalDevAuthBypassPolicy.ValidateStartupConfiguration(builder.Configuration, builder.Environment);

        builder.Services.AddStlJwtAuthentication(builder.Configuration);
        builder.Services.AddStlAiServices(builder.Configuration);
        builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IPlatformAuditService, PlatformAuditService>();
        builder.Services.AddScoped<AiAssistanceService>();
        builder.Services.AddScoped<SmartImportService>();
        builder.Services.AddScoped<SmartImportDestinationClient>();
        builder.Services.AddScoped<RecordArrSmartImportClient>();
        builder.Services.AddHttpClient(RecordArrSmartImportClient.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromMinutes(2);
        });
        builder.Services.AddHttpClient(SmartImportDestinationClient.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromMinutes(2);
        });
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<LocalDevAuthBypassPolicy>();
        builder.Services.AddScoped<MfaService>();
        builder.Services.AddScoped<PasswordResetService>();
        builder.Services.AddScoped<PlatformAuthorizationService>();
        builder.Services.AddScoped<FixedSuiteProductAccessService>();
        builder.Services.AddScoped<TenantAdminService>();
        builder.Services.AddScoped<TenantMembershipAdminService>();
        builder.Services.AddScoped<PlatformUserAdminService>();
        builder.Services.AddScoped<ProductCatalogService>();
        builder.Services.AddScoped<ProductManifestService>();
        builder.Services.AddScoped<ReferenceDataService>();
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
        builder.Services.AddScoped<LaunchDestinationReconciliationSettingsService>();
        builder.Services.AddScoped<LaunchDestinationReconciliationWorkerService>();
        builder.Services.AddScoped<CompatibilityLegacyEntitlementReconciliationSettingsService>();
        builder.Services.AddScoped<CompatibilityLegacyEntitlementReconciliationWorkerService>();
        builder.Services.AddScoped<TenantLifecycleSettingsService>();
        builder.Services.AddScoped<TenantLifecycleWorkerService>();
        builder.Services.AddScoped<PersonLoginDisableService>();
        builder.Services.AddScoped<PersonLoginEnableService>();
        builder.Services.AddScoped<PlatformIdentityIntegrationService>();
        builder.Services.AddScoped<PlatformIdentitySecurityService>();
        builder.Services.AddScoped<PlatformLifecycleOverviewService>();
        builder.Services.AddScoped<PlatformWorkerHealthOrchestrationService>();
        builder.Services.AddScoped<TenantIntegrationService>();
        builder.Services.AddScoped<TenantIntegrationCredentialProtector>();
        builder.Services.AddScoped<MfaSecretProtector>();
        builder.Services.AddScoped<HybridDataPlaneService>();
        builder.Services.AddHttpClient(HybridDataPlaneService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        builder.Services.AddHttpClient<IStaffArrPersonProvisioningClient, StaffArrPersonProvisioningClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        builder.Services.AddScoped<FieldCompanionAuthService>();
        builder.Services.AddScoped<FieldCompanionFieldInboxService>();
        builder.Services.AddScoped<FieldCompanionProductClient>();
        builder.Services.AddHttpClient(nameof(FieldCompanionProductClient));
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.Configure<ProductDatabaseNukeOptions>(builder.Configuration.GetSection(ProductDatabaseNukeOptions.SectionName));
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.AddScoped<FieldCompanionNotificationSettingsService>();
        builder.Services.AddScoped<FieldCompanionNotificationEnqueueService>();
        builder.Services.AddScoped<FieldCompanionNotificationDispatchService>();
        builder.Services.AddScoped<FieldCompanionPushSubscriptionService>();
        builder.Services.AddSingleton<IFieldCompanionWebPushSender, FieldCompanionWebPushSender>();
        builder.Services.Configure<FieldCompanionWebPushOptions>(builder.Configuration.GetSection(FieldCompanionWebPushOptions.SectionName));
        builder.Services.AddHttpClient(FieldCompanionNotificationDispatchService.WebhookHttpClientName);
        builder.Services.AddScoped<FieldCompanionOfflineSyncService>();
        builder.Services.AddScoped<FieldCompanionFieldEvidenceService>();
        builder.Services.AddScoped<FieldCompanionFieldDvirService>();
        builder.Services.AddScoped<FieldCompanionClockService>();
        builder.Services.AddScoped<FieldCompanionFieldInspectionService>();
        builder.Services.AddScoped<FieldCompanionFieldWorkOrderService>();
        builder.Services.AddScoped<FieldCompanionFieldReceivingService>();
        builder.Services.AddScoped<FieldCompanionFieldSubmissionService>();
        builder.Services.AddScoped<FieldCompanionFieldTaskValidationService>();
        builder.Services.AddScoped<FieldCompanionScanResolveService>();
        builder.Services.AddScoped<PlatformHealthService>();
        builder.Services.AddScoped<ProductDatabaseNukeService>();
        builder.Services.AddHttpClient(PlatformHealthService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        builder.Services.AddHttpClient(TenantIntegrationService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        builder.Services.Configure<FieldCompanionProductUrlsOptions>(options =>
        {
            var configuration = builder.Configuration;
            options.StaffArrBaseUrl = ResolveProductBaseUrl(configuration, "StaffArr", options.StaffArrBaseUrl);
            options.TrainArrBaseUrl = ResolveProductBaseUrl(configuration, "TrainArr", options.TrainArrBaseUrl);
            options.MaintainArrBaseUrl = ResolveProductBaseUrl(configuration, "MaintainArr", options.MaintainArrBaseUrl);
            options.RoutArrBaseUrl = ResolveProductBaseUrl(configuration, "RoutArr", options.RoutArrBaseUrl);
            options.SupplyArrBaseUrl = ResolveProductBaseUrl(configuration, "SupplyArr", options.SupplyArrBaseUrl);
            options.LoadArrBaseUrl = ResolveProductBaseUrl(configuration, "LoadArr", options.LoadArrBaseUrl);
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
            options.CustomArrBaseUrl = ResolveProductBaseUrl(configuration, "CustomArr", options.CustomArrBaseUrl);
            options.OrdArrBaseUrl = ResolveProductBaseUrl(configuration, "OrdArr", options.OrdArrBaseUrl);
            options.ComplianceCoreBaseUrl = ResolveProductBaseUrl(configuration, "ComplianceCore", options.ComplianceCoreBaseUrl);
            options.LoadArrBaseUrl = ResolveProductBaseUrl(configuration, "LoadArr", options.LoadArrBaseUrl);
            options.AssurArrBaseUrl = ResolveProductBaseUrl(configuration, "AssurArr", options.AssurArrBaseUrl);
            options.ReportArrBaseUrl = ResolveProductBaseUrl(configuration, "ReportArr", options.ReportArrBaseUrl);
            options.RecordArrBaseUrl = ResolveProductBaseUrl(configuration, "RecordArr", options.RecordArrBaseUrl);
            options.FieldCompanionBaseUrl = ResolveProductBaseUrl(configuration, "FieldCompanion", options.FieldCompanionBaseUrl);
        });
        builder.Services.Configure<StlLaunchOptions>(builder.Configuration.GetSection(StlLaunchOptions.SectionName));
        builder.Services.Configure<TenantIntegrationOptions>(
            builder.Configuration.GetSection(TenantIntegrationOptions.SectionName));

        var suiteFrontendOrigin = builder.Configuration["Cors:SuiteFrontendOrigin"] ?? "http://localhost:5174";
        var FieldCompanionFrontendOrigin = builder.Configuration["Cors:FieldCompanionFrontendOrigin"] ?? "http://localhost:5181";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "NexArrBrowserClients",
            suiteFrontendOrigin,
            "http://127.0.0.1:5174",
            FieldCompanionFrontendOrigin,
            "http://127.0.0.1:5181");

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

    internal static bool ShouldSeedMasterReferenceDataOnStartup(
        IConfiguration configuration,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
    {
        var configured = configuration[RunMasterReferenceDataOnStartupKey];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (bool.TryParse(configured, out var enabled))
            {
                return enabled;
            }

            throw new InvalidOperationException($"{RunMasterReferenceDataOnStartupKey} must be true or false.");
        }

        return string.Equals(environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase)
               || string.Equals(environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase);
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
        var launchOptions = scope.ServiceProvider.GetService<IOptions<StlLaunchOptions>>()?.Value;
        var platformProductUrls = scope.ServiceProvider.GetService<IOptions<PlatformProductUrlsOptions>>()?.Value;
        var seedCsvPath = app.Configuration["Seed:ReferenceDataCsvPath"];
        if (string.IsNullOrWhiteSpace(seedCsvPath))
        {
            seedCsvPath = Path.Combine(app.Environment.ContentRootPath, "Data", "stl_acceptable_values_platform_only.csv");
        }

        await PlatformSeeder.SeedInfrastructureAsync(db, launchOptions, platformProductUrls);
        var mfaSecretProtector = scope.ServiceProvider.GetRequiredService<MfaSecretProtector>();
        var migratedMfaSecrets = await mfaSecretProtector.MigrateLegacySecretsAsync(db);
        if (migratedMfaSecrets > 0)
        {
            app.Logger.LogInformation("Migrated {Count} legacy MFA secrets to encrypted payloads.", migratedMfaSecrets);
        }
        var firstAdminUserId = await PlatformSeeder.SeedFirstAdminAsync(
            db,
            passwordHasher,
            app.Configuration,
            app.Environment);
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
        {
            await PlatformSeeder.SeedDemoBusinessDataAsync(db, passwordHasher, cancellationToken: CancellationToken.None);
        }
        if (ShouldSeedMasterReferenceDataOnStartup(app.Configuration, app.Environment))
        {
            app.Logger.LogInformation("Running NexArr master reference data startup seed from {SeedCsvPath}.", seedCsvPath);
            await PlatformSeeder.SeedMasterReferenceDataAsync(db, seedCsvPath, firstAdminUserId);
        }
        else
        {
            app.Logger.LogInformation(
                "Skipping NexArr master reference data startup seed. Set {ConfigurationKey}=true to enable it.",
                RunMasterReferenceDataOnStartupKey);
        }

        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
        {
            await PlatformSeeder.EnsureDevSuiteShellOriginsAsync(db);
            await PlatformSeeder.EnsureDevLoopbackCallbackPrefixesAsync(db);
        }
        else if (app.Environment.IsProduction())
        {
            var productionOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var origin in new[]
                     {
                         app.Configuration["Cors:SuiteFrontendOrigin"],
                         app.Configuration["Cors:FieldCompanionFrontendOrigin"],
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
