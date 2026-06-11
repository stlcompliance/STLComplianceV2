using LoadArr.Api;
using LoadArr.Api.Data;
using LoadArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<LoadArrDbContext>(
    new ProductDescriptor("loadarr", "LoadArr", 5108),
    args,
    LoadArrServiceRegistration.ConfigureServices,
    LoadArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapLoadArrAuthEndpoints();
        app.MapLoadArrFieldInboxEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapLoadArrWorkspaceEndpoints();
        app.MapLoadArrInventoryManagementEndpoints();
        app.MapLoadArrRouteSurfaceEndpoints();
        app.MapLoadArrIntegrationEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        app.MapLoadArrAdminEndpoints();
        await Task.CompletedTask;
    });

namespace LoadArr.Api
{
    public partial class Program;
}
