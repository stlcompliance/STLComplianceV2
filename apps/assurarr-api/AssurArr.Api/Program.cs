using AssurArr.Api;
using AssurArr.Api.Data;
using AssurArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<AssurArrDbContext>(
    new ProductDescriptor("assurarr", "AssurArr", 5109),
    args,
    AssurArrServiceRegistration.ConfigureServices,
    AssurArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapAssurArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapAssurArrEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await AssurArrServiceRegistration.SeedAsync(app);
    });
