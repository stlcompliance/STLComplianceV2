using LedgArr.Api;
using LedgArr.Api.Data;
using LedgArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<LedgArrDbContext>(
    new ProductDescriptor("ledgarr", "LedgArr", 5113),
    args,
    LedgArrServiceRegistration.ConfigureServices,
    LedgArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapLedgArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapStlProductAiAssistanceEndpoints();
        app.MapLedgArrEndpoints();
        app.MapLedgArrPayrollEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await Task.CompletedTask;
    });

namespace LedgArr.Api
{
    public partial class Program;
}
