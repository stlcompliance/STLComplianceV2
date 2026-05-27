using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Workers;

await StlWorkerHost.RunAsync(
    new ProductDescriptor("staffarr", "StaffArr", 0),
    args);
