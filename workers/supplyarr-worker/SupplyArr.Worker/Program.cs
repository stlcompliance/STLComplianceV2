using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Workers;

await StlWorkerHost.RunAsync(
    new ProductDescriptor("supplyarr", "SupplyArr", 0),
    args);
