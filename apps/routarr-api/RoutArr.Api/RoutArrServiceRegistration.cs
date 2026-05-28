using RoutArr.Api.Options;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace RoutArr.Api;

public static class RoutArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));
        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));
        builder.Services.Configure<DriverEligibilityOptions>(builder.Configuration.GetSection(DriverEligibilityOptions.SectionName));
        builder.Services.Configure<TrainArrClientOptions>(builder.Configuration.GetSection(TrainArrClientOptions.SectionName));
        builder.Services.Configure<StaffArrClientOptions>(builder.Configuration.GetSection(StaffArrClientOptions.SectionName));
        builder.Services.Configure<AssetDispatchabilityOptions>(builder.Configuration.GetSection(AssetDispatchabilityOptions.SectionName));
        builder.Services.Configure<MaintainArrClientOptions>(builder.Configuration.GetSection(MaintainArrClientOptions.SectionName));
        builder.Services.Configure<SupplyArrClientOptions>(builder.Configuration.GetSection(SupplyArrClientOptions.SectionName));
        builder.Services.Configure<ComplianceCoreClientOptions>(builder.Configuration.GetSection(ComplianceCoreClientOptions.SectionName));
        builder.Services.Configure<DispatchWorkflowGateOptions>(builder.Configuration.GetSection(DispatchWorkflowGateOptions.SectionName));

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddHttpClient<TrainArrQualificationCheckClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TrainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<StaffArrReadinessClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<MaintainArrAssetReadinessClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaintainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<ComplianceCoreWorkflowGateClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<SupplyArrDemandClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddScoped<RoutArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<RoutArrAuthorizationService>();
        builder.Services.AddScoped<DriverEligibilityService>();
        builder.Services.AddScoped<AssetDispatchabilityService>();
        builder.Services.AddScoped<DispatchWorkflowGateService>();
        builder.Services.AddScoped<DispatchAssignmentService>();
        builder.Services.AddScoped<BulkDispatchService>();
        builder.Services.AddScoped<DispatchCloseoutService>();
        builder.Services.AddScoped<TripService>();
        builder.Services.AddScoped<TripPartsDemandService>();
        builder.Services.AddScoped<TripPartsDemandStatusIngestionService>();
        builder.Services.AddScoped<LoadTestJourneySeedService>();
        builder.Services.AddScoped<RouteService>();
        builder.Services.AddScoped<DispatchBoardService>();
        builder.Services.AddScoped<RouteCalendarService>();
        builder.Services.AddScoped<DriverAvailabilityService>();
        builder.Services.AddScoped<EquipmentAvailabilityService>();
        builder.Services.AddScoped<FieldInboxService>();
        builder.Services.AddScoped<IRoutArrAuditService, RoutArrAuditService>();
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddScoped<DispatchNotificationSettingsService>();
        builder.Services.AddScoped<DispatchNotificationEnqueueService>();
        builder.Services.AddScoped<DispatchNotificationDispatchService>();
        builder.Services.AddScoped<TripCompletionRollupSettingsService>();
        builder.Services.AddScoped<TripCompletionRollupWorkerService>();
        builder.Services.AddScoped<TripCompletionService>();
        builder.Services.AddHttpClient(DispatchNotificationDispatchService.WebhookHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        var frontendOrigin = builder.Configuration["Cors:RoutArrFrontendOrigin"] ?? "http://localhost:5180";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("RoutArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("RoutArrFrontend");
    }
}
