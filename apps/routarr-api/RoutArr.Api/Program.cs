using RoutArr.Api;
using RoutArr.Api.Data;
using RoutArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<RoutArrDbContext>(
    new ProductDescriptor("routarr", "RoutArr", 5105),
    args,
    RoutArrServiceRegistration.ConfigureServices,
    RoutArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapRoutArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapRoutArrTripEndpoints();
        app.MapRoutArrRouteEndpoints();
        app.MapRoutArrDispatchEndpoints();
        app.MapRoutArrDriverAvailabilityEndpoints();
        app.MapRoutArrEquipmentAvailabilityEndpoints();
        app.MapRoutArrDriverEligibilityEndpoints();
        app.MapRoutArrAssetDispatchabilityEndpoints();
        app.MapRoutArrDispatchWorkflowGateEndpoints();
        app.MapRoutArrLoadTestJourneySeedEndpoints();
        app.MapRoutArrFieldInboxEndpoints();
        app.MapRoutArrNotificationSettingsEndpoints();
        app.MapRoutArrInternalDispatchNotificationEndpoints();
        app.MapRoutArrTripCompletionRollupSettingsEndpoints();
        app.MapRoutArrInternalTripCompletionRollupEndpoints();
        app.MapRoutArrTripCompletionEndpoints();
        await Task.CompletedTask;
    });
