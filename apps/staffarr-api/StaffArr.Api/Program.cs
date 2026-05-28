using StaffArr.Api;
using StaffArr.Api.Data;
using StaffArr.Api.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<StaffArrDbContext>(
    new ProductDescriptor("staffarr", "StaffArr", 5102),
    args,
    StaffArrServiceRegistration.ConfigureServices,
    StaffArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapStaffArrAuthEndpoints();
        app.MapStaffArrPeopleEndpoints();
        app.MapStaffArrPeopleExportEndpoints();
        app.MapStaffArrManagerHierarchyEndpoints();
        app.MapStaffArrOrgUnitEndpoints();
        app.MapStaffArrOrgUnitAssignmentEndpoints();
        app.MapStaffArrRoleTemplateEndpoints();
        app.MapStaffArrCertificationEndpoints();
        app.MapStaffArrReadinessEndpoints();
        app.MapStaffArrReadinessRollupEndpoints();
        app.MapStaffArrIncidentEndpoints();
        app.MapStaffArrIntegrationEndpoints();
        app.MapStaffArrInternalCertificationExpirationEndpoints();
        app.MapStaffArrInternalReadinessRollupEndpoints();
        app.MapStaffArrInternalPermissionProjectionEndpoints();
        app.MapStaffArrInternalPersonExportDeliveryEndpoints();
        app.MapStaffArrInternalAuditPackageGenerationEndpoints();
        app.MapStaffArrFieldInboxEndpoints();
        app.MapStaffArrAuditPackageEndpoints();
        await Task.CompletedTask;
    });
