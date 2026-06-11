using ReportArr.Api;
using ReportArr.Api.Data;
using ReportArr.Api.Endpoints;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Endpoints;

await StlApiHost.RunAsync<ReportArrDbContext>(
    new ProductDescriptor("reportarr", "ReportArr", 5111),
    args,
    ReportArrServiceRegistration.ConfigureServices,
    ReportArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapReportArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapReportArrWorkspaceEndpoints();
        app.MapReportArrIntegrationEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await Task.CompletedTask;
    });

namespace ReportArr.Api
{
    public partial class Program;
}
