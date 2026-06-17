using CustomArr.Api;
using CustomArr.Api.Data;
using CustomArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<CustomArrDbContext>(
    new ProductDescriptor("customarr", "CustomArr", 5111),
    args,
    CustomArrServiceRegistration.ConfigureServices,
    CustomArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapCustomArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapStlProductAiAssistanceEndpoints();
        app.MapCustomArrWorkspaceEndpoints();
        app.MapCustomArrCrmWorkspaceEndpoints();
        app.MapCustomArrReferenceIntegrationEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await Task.CompletedTask;
    });

namespace CustomArr.Api
{
    public partial class Program;
}
