using ComplianceCore.Api.Data;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<ComplianceCoreDbContext>(
    new ProductDescriptor("compliancecore", "Compliance Core", 5107),
    args);
