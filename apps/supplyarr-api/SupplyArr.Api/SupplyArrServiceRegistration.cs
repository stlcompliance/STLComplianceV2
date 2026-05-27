using SupplyArr.Api.Options;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace SupplyArr.Api;

public static class SupplyArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));
        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddScoped<SupplyArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<SupplyArrAuthorizationService>();
        builder.Services.AddScoped<ExternalPartyService>();
        builder.Services.AddScoped<PartCatalogService>();
        builder.Services.AddScoped<PartRegistryService>();
        builder.Services.AddScoped<InventoryLocationService>();
        builder.Services.AddScoped<PartStockService>();
        builder.Services.AddScoped<PurchaseRequestService>();
        builder.Services.AddScoped<PurchaseOrderService>();
        builder.Services.AddScoped<ReceivingService>();
        builder.Services.AddScoped<FieldInboxService>();
        builder.Services.AddScoped<ReceivingExceptionService>();
        builder.Services.AddScoped<BackorderService>();
        builder.Services.AddScoped<VendorReturnService>();
        builder.Services.AddScoped<PricingSnapshotService>();
        builder.Services.AddScoped<LeadTimeSnapshotService>();
        builder.Services.AddScoped<AvailabilitySnapshotService>();
        builder.Services.AddScoped<ReorderEvaluationService>();
        builder.Services.AddScoped<MaintainArrDemandIntakeService>();
        builder.Services.AddScoped<MaintainArrDemandStatusCallbackService>();
        builder.Services.Configure<MaintainArrClientOptions>(builder.Configuration.GetSection(MaintainArrClientOptions.SectionName));
        builder.Services.AddHttpClient<MaintainArrDemandStatusClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaintainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<ISupplyArrAuditService, SupplyArrAuditService>();
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));

        var frontendOrigin = builder.Configuration["Cors:SupplyArrFrontendOrigin"] ?? "http://localhost:5179";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("SupplyArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("SupplyArrFrontend");
    }
}
