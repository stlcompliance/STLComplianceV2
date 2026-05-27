using NexArr.Api;
using NexArr.Api.Data;
using NexArr.Api.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<NexArrDbContext>(
    new ProductDescriptor("nexarr", "NexArr", 5101),
    args,
    NexArrServiceRegistration.ConfigureServices,
    async app =>
    {
        app.MapAuthEndpoints();
        await NexArrServiceRegistration.InitializeAsync(app);
    });
