using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Workers;

await StlWorkerHost.RunAsync(
    new ProductDescriptor("compliancecore", "Compliance Core", 0),
    args);
