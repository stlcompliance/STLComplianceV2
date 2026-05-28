using StaffArr.Api;
using StaffArr.Api.Data;
using StaffArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<StaffArrDbContext>(
    new ProductDescriptor("staffarr", "StaffArr", 5102),
    args,
    StaffArrServiceRegistration.ConfigureServices,
    StaffArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapStaffArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapStaffArrPeopleEndpoints();
        app.MapStaffArrPersonLookupEndpoints();
        app.MapStaffArrPeopleExportEndpoints();
        app.MapStaffArrManagerHierarchyEndpoints();
        app.MapStaffArrOrgUnitEndpoints();
        app.MapStaffArrOrgUnitAssignmentEndpoints();
        app.MapStaffArrRoleTemplateEndpoints();
        app.MapStaffArrCertificationEndpoints();
        app.MapStaffArrReadinessEndpoints();
        app.MapStaffArrReadinessRollupEndpoints();
        app.MapStaffArrPersonnelHistoryEndpoints();
        app.MapStaffArrIncidentEndpoints();
        app.MapStaffArrIncidentSupplyDemandEndpoints();
        app.MapStaffArrPersonnelNoteEndpoints();
        app.MapStaffArrPersonnelDocumentEndpoints();
        app.MapStaffArrIntegrationEndpoints();
        app.MapStaffArrInternalCertificationExpirationEndpoints();
        app.MapStaffArrInternalReadinessRollupEndpoints();
        app.MapStaffArrInternalPersonnelHistoryEndpoints();
        app.MapStaffArrInternalPermissionProjectionEndpoints();
        app.MapStaffArrInternalPersonExportDeliveryEndpoints();
        app.MapStaffArrInternalAuditPackageGenerationEndpoints();
        app.MapStaffArrFieldInboxEndpoints();
        app.MapStaffArrAuditPackageEndpoints();
        await Task.CompletedTask;
    });
