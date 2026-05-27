using TrainArr.Api.Data;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<TrainArrDbContext>(
    new ProductDescriptor("trainarr", "TrainArr", 5103),
    args);
