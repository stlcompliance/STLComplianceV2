using RecordArr.Api;
using RecordArr.Api.Data;
using RecordArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<RecordArrDbContext>(
    new ProductDescriptor("recordarr", "RecordArr", 5110),
    args,
    RecordArrServiceRegistration.ConfigureServices,
    RecordArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapRecordArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapStlProductAiAssistanceEndpoints();
        app.MapRecordArrWorkspaceEndpoints();
        app.MapRecordArrReferenceIntegrationEndpoints();
        app.MapRecordArrIntegrationEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await Task.CompletedTask;
    });

namespace RecordArr.Api
{
    public partial class Program;
}
