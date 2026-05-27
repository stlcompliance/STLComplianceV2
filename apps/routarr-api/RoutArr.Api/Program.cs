using RoutArr.Api;
using RoutArr.Api.Data;
using RoutArr.Api.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<RoutArrDbContext>(
    new ProductDescriptor("routarr", "RoutArr", 5105),
    args,
    RoutArrServiceRegistration.ConfigureServices,
    RoutArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapRoutArrAuthEndpoints();
        app.MapRoutArrTripEndpoints();
        app.MapRoutArrRouteEndpoints();
        app.MapRoutArrDispatchEndpoints();
        app.MapRoutArrDriverAvailabilityEndpoints();
        app.MapRoutArrEquipmentAvailabilityEndpoints();
        app.MapRoutArrDriverEligibilityEndpoints();
        app.MapRoutArrAssetDispatchabilityEndpoints();
        app.MapRoutArrDispatchWorkflowGateEndpoints();
        app.MapRoutArrFieldInboxEndpoints();
        await Task.CompletedTask;
    });
