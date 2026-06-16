using OrdArr.Api;
using OrdArr.Api.Data;
using OrdArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<OrdArrDbContext>(
    new ProductDescriptor("ordarr", "OrdArr", 5112),
    args,
    OrdArrServiceRegistration.ConfigureServices,
    OrdArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapOrdArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapStlProductAiAssistanceEndpoints();
        app.MapOrdArrWorkspaceEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await Task.CompletedTask;
    });

namespace OrdArr.Api
{
    public partial class Program;
}
