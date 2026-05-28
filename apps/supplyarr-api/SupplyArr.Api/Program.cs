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
        app.MapSupplyArrPurchaseRequestEndpoints();
        app.MapSupplyArrPurchaseOrderEndpoints();
        app.MapSupplyArrReceivingEndpoints();
        app.MapSupplyArrBackorderEndpoints();
        app.MapSupplyArrVendorReturnEndpoints();
        app.MapSupplyArrPricingSnapshotEndpoints();
        app.MapSupplyArrLeadTimeSnapshotEndpoints();
        app.MapSupplyArrAvailabilitySnapshotEndpoints();
        app.MapSupplyArrReorderEvaluationEndpoints();
        app.MapSupplyArrInternalReorderEvaluationEndpoints();
        app.MapSupplyArrIntegrationEndpoints();
        app.MapSupplyArrDemandRefEndpoints();
        app.MapSupplyArrFieldInboxEndpoints();
        app.MapSupplyArrNotificationSettingsEndpoints();
        app.MapSupplyArrInternalProcurementNotificationEndpoints();
        await Task.CompletedTask;
    });
