using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Workers;

await StlWorkerHost.RunAsync(
    new ProductDescriptor("nexarr", "NexArr", 0),
    args);
