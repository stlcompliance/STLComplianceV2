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
        app.MapSupplyArrWorkflowAliasEndpoints();
        app.MapSupplyArrCoverageAliasEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapSupplyArrSettingsEndpoints();
        app.MapSupplyArrPartyRegistryEndpoints();
        app.MapSupplyArrPartCatalogEndpoints();
        app.MapSupplyArrPartAliasEndpoints();
        app.MapSupplyArrExternalReferenceEndpoints();
        app.MapSupplyArrInventoryEndpoints();
        app.MapSupplyArrStockReservationEndpoints();
        app.MapSupplyArrRfqEndpoints();
        app.MapSupplyArrQuoteAliasEndpoints();
        app.MapSupplyArrSupplierOnboardingEndpoints();
        app.MapSupplyArrVendorDocumentEndpoints();
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
        app.MapSupplyArrContractEndpoints();
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
        app.MapSupplyArrProcurementExceptionEscalationSettingsEndpoints();
        app.MapSupplyArrInternalProcurementExceptionEscalationEndpoints();
        app.MapSupplyArrDemandProcessingSettingsEndpoints();
        app.MapSupplyArrInternalDemandProcessingEndpoints();
        app.MapSupplyArrDemandProcessingEndpoints();
        app.MapSupplyArrIntegrationEventSettingsEndpoints();
        app.MapSupplyArrInternalIntegrationEventEndpoints();
        app.MapSupplyArrVendorReportEndpoints();
        app.MapSupplyArrPartsInventoryReportEndpoints();
        app.MapSupplyArrPurchasingReportEndpoints();
        app.MapSupplyArrComplianceReportEndpoints();
        app.MapSupplyArrReportIndexEndpoints();
        app.MapSupplyArrForgivingSearchEndpoints();
        app.MapSupplyArrAuditHistoryEndpoints();
        app.MapSupplyArrEventAndAuditEndpoints();
        app.MapSupplyArrSupplyReadinessEndpoints();
        app.MapSupplyArrLoadTestJourneySeedEndpoints();
        await Task.CompletedTask;
    });
