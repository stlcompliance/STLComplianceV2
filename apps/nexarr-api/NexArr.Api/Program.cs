using NexArr.Api.Data;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<NexArrDbContext>(
    new ProductDescriptor("nexarr", "NexArr", 5101),
    args);
