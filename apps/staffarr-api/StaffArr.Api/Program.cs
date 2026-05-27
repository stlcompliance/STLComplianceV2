using StaffArr.Api;
using StaffArr.Api.Data;
using StaffArr.Api.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<StaffArrDbContext>(
    new ProductDescriptor("staffarr", "StaffArr", 5102),
    args,
    StaffArrServiceRegistration.ConfigureServices,
    async app =>
    {
        StaffArrServiceRegistration.ConfigurePipeline(app);
        app.MapStaffArrAuthEndpoints();
        app.MapStaffArrPeopleEndpoints();
        app.MapStaffArrManagerHierarchyEndpoints();
        app.MapStaffArrOrgUnitEndpoints();
        app.MapStaffArrOrgUnitAssignmentEndpoints();
        await Task.CompletedTask;
    });
