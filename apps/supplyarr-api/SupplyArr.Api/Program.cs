using SupplyArr.Api;
using SupplyArr.Api.Data;
using SupplyArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<SupplyArrDbContext>(
    new ProductDescriptor("supplyarr", "SupplyArr", 5106),
    args,
    SupplyArrServiceRegistration.ConfigureServices,
    SupplyArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapSupplyArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapSupplyArrPartyRegistryEndpoints();
        app.MapSupplyArrPartCatalogEndpoints();
        app.MapSupplyArrInventoryEndpoints();
        app.MapSupplyArrRfqEndpoints();
        app.MapSupplyArrSupplierOnboardingEndpoints();
        app.MapSupplyArrVendorRestrictionEndpoints();
        app.MapSupplyArrSupplierIncidentEndpoints();
        app.MapSupplyArrProcurementExceptionEndpoints();
        app.MapSupplyArrEmergencyPurchaseEndpoints();
        app.MapSupplyArrPurchaseRequestEndpoints();
        app.MapSupplyArrPurchaseOrderEndpoints();
        app.MapSupplyArrReceivingEndpoints();
        app.MapSupplyArrBackorderEndpoints();
        app.MapSupplyArrVendorReturnEndpoints();
        app.MapSupplyArrWarrantyClaimEndpoints();
        app.MapSupplyArrPricingSnapshotEndpoints();
        app.MapSupplyArrLeadTimeSnapshotEndpoints();
        app.MapSupplyArrAvailabilitySnapshotEndpoints();
        app.MapSupplyArrReorderEvaluationEndpoints();
        app.MapSupplyArrInternalReorderEvaluationEndpoints();
        app.MapSupplyArrIntegrationEndpoints();
        app.MapSupplyArrDemandRefEndpoints();
        app.MapSupplyArrRoutArrDemandRefEndpoints();
        app.MapSupplyArrTrainArrDemandRefEndpoints();
        app.MapSupplyArrStaffArrDemandRefEndpoints();
        app.MapSupplyArrFieldInboxEndpoints();
        app.MapSupplyArrNotificationSettingsEndpoints();
        app.MapSupplyArrInternalProcurementNotificationEndpoints();
        app.MapSupplyArrPriceSnapshotSettingsEndpoints();
        app.MapSupplyArrInternalPriceSnapshotEndpoints();
        app.MapSupplyArrLeadTimeSnapshotSettingsEndpoints();
        app.MapSupplyArrInternalLeadTimeSnapshotEndpoints();
        app.MapSupplyArrAvailabilitySnapshotSettingsEndpoints();
        app.MapSupplyArrInternalAvailabilitySnapshotEndpoints();
        app.MapSupplyArrProcurementCoordinationSettingsEndpoints();
        app.MapSupplyArrInternalProcurementCoordinationEndpoints();
        app.MapSupplyArrProcurementCoordinationEndpoints();
        app.MapSupplyArrApprovalReminderSettingsEndpoints();
        app.MapSupplyArrInternalApprovalReminderEndpoints();
        app.MapSupplyArrApprovalReminderEndpoints();
        app.MapSupplyArrDemandProcessingSettingsEndpoints();
        app.MapSupplyArrInternalDemandProcessingEndpoints();
        app.MapSupplyArrDemandProcessingEndpoints();
        app.MapSupplyArrIntegrationEventSettingsEndpoints();
        app.MapSupplyArrInternalIntegrationEventEndpoints();
        app.MapSupplyArrVendorReportEndpoints();
        app.MapSupplyArrPartsInventoryReportEndpoints();
        app.MapSupplyArrPurchasingReportEndpoints();
        app.MapSupplyArrComplianceReportEndpoints();
        app.MapSupplyArrForgivingSearchEndpoints();
        app.MapSupplyArrAuditHistoryEndpoints();
        app.MapSupplyArrSupplyReadinessEndpoints();
        await Task.CompletedTask;
    });
