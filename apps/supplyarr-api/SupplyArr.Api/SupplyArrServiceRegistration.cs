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
        builder.Services.AddScoped<StockReservationService>();
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
        builder.Services.AddScoped<PriceSnapshotSettingsService>();
        builder.Services.AddScoped<PriceSnapshotWorkerService>();
        builder.Services.AddScoped<LeadTimeSnapshotSettingsService>();
        builder.Services.AddScoped<LeadTimeSnapshotWorkerService>();
        builder.Services.AddScoped<AvailabilitySnapshotSettingsService>();
        builder.Services.AddScoped<AvailabilitySnapshotWorkerService>();
        builder.Services.AddScoped<ProcurementCoordinationSettingsService>();
        builder.Services.AddScoped<ProcurementCoordinationWorkerService>();
        builder.Services.AddScoped<ProcurementCoordinationService>();
        builder.Services.AddScoped<ApprovalReminderSettingsService>();
        builder.Services.AddScoped<ApprovalReminderWorkerService>();
        builder.Services.AddScoped<ApprovalReminderService>();
        builder.Services.AddScoped<ProcurementExceptionEscalationSettingsService>();
        builder.Services.AddScoped<ProcurementExceptionEscalationWorkerService>();
        builder.Services.AddScoped<DemandProcessingSettingsService>();
        builder.Services.AddScoped<DemandProcessingWorkerService>();
        builder.Services.AddScoped<DemandProcessingService>();
        builder.Services.AddScoped<LoadTestJourneySeedService>();
        builder.Services.AddScoped<IntegrationEventSettingsService>();
        builder.Services.AddScoped<IntegrationEventProcessingService>();
        builder.Services.AddScoped<IntegrationOutboxEnqueueService>();
        builder.Services.AddScoped<IntegrationInboxEnqueueService>();
        builder.Services.AddScoped<RfqService>();
        builder.Services.AddScoped<PartyComplianceDocumentService>();
        builder.Services.AddScoped<SupplierOnboardingService>();
        builder.Services.AddScoped<VendorProcurementGuardService>();
        builder.Services.AddScoped<VendorRestrictionService>();
        builder.Services.AddScoped<SupplierIncidentService>();
        builder.Services.AddScoped<WarrantyClaimService>();
        builder.Services.AddScoped<ProcurementExceptionService>();
        builder.Services.AddScoped<StaffarrProcurementApprovalAuthorityService>();
        builder.Services.AddHttpClient<StaffArrProcurementApprovalAuthorityClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<EmergencyPurchaseService>();
        builder.Services.AddScoped<VendorReportService>();
        builder.Services.AddScoped<PartsInventoryReportService>();
        builder.Services.AddScoped<PurchasingReportService>();
        builder.Services.AddScoped<ComplianceReportService>();
        builder.Services.AddScoped<SupplyReadinessService>();
        builder.Services.AddScoped<SupplyReferenceResolutionService>();
        builder.Services.AddScoped<ForgivingSearchService>();
        builder.Services.AddScoped<AuditHistoryService>();
        builder.Services.AddScoped<MaintainArrDemandIntakeService>();
        builder.Services.AddScoped<RoutArrDemandIntakeService>();
        builder.Services.AddScoped<TrainArrDemandIntakeService>();
        builder.Services.AddScoped<StaffArrDemandIntakeService>();
        builder.Services.AddScoped<MaintainArrDemandStatusCallbackService>();
        builder.Services.AddScoped<RoutArrDemandStatusCallbackService>();
        builder.Services.AddScoped<TrainArrDemandStatusCallbackService>();
        builder.Services.AddScoped<StaffArrDemandStatusCallbackService>();
        builder.Services.AddScoped<SupplyArrDemandStatusCallbackCoordinator>();
        builder.Services.Configure<MaintainArrClientOptions>(builder.Configuration.GetSection(MaintainArrClientOptions.SectionName));
        builder.Services.Configure<RoutArrClientOptions>(builder.Configuration.GetSection(RoutArrClientOptions.SectionName));
        builder.Services.Configure<TrainArrClientOptions>(builder.Configuration.GetSection(TrainArrClientOptions.SectionName));
        builder.Services.Configure<StaffArrClientOptions>(builder.Configuration.GetSection(StaffArrClientOptions.SectionName));
        builder.Services.Configure<ComplianceCoreClientOptions>(builder.Configuration.GetSection(ComplianceCoreClientOptions.SectionName));
        builder.Services.AddHttpClient<ComplianceCoreFactPublicationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<ComplianceCoreFactPublisherService>();
        builder.Services.AddHttpClient<MaintainArrDemandStatusClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaintainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<RoutArrDemandStatusClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RoutArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<TrainArrDemandStatusClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TrainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<StaffArrDemandStatusClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<ISupplyArrAuditService, SupplyArrAuditService>();
        builder.Services.AddScoped<ProcurementNotificationSettingsService>();
        builder.Services.AddScoped<ProcurementNotificationEnqueueService>();
        builder.Services.AddScoped<ProcurementNotificationDispatchService>();
        builder.Services.AddHttpClient(ProcurementNotificationDispatchService.WebhookHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });
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
