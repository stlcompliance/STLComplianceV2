using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Workers;

await StlWorkerHost.RunAsync(
    new ProductDescriptor("routarr", "RoutArr", 0),
    args);
