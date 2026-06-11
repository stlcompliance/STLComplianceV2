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
        app.MapStaffArrSettingsEndpoints();
        app.MapStaffArrMePortalEndpoints();
        app.MapStaffArrPersonnelUpdateRequestEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapStaffArrPeopleEndpoints();
        app.MapStaffArrPersonLookupEndpoints();
        app.MapStaffArrPeopleExportEndpoints();
        app.MapStaffArrWorkerAdminEndpoints();
        app.MapStaffArrManagerHierarchyEndpoints();
        app.MapStaffArrOrgUnitEndpoints();
        app.MapStaffArrLocationEndpoints();
        app.MapStaffArrOrgUnitAssignmentEndpoints();
        app.MapStaffArrRoleTemplateEndpoints();
        app.MapStaffArrRoleManagementEndpoints();
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
        app.MapStaffArrTrainingAcknowledgementEndpoints();
        app.MapStaffArrOffboardingEndpoints();
        app.MapStaffArrFieldInboxEndpoints();
        app.MapStaffArrEventAndAuditEndpoints();
        app.MapStaffArrV1FeatureAliasEndpoints();
        app.MapStaffArrEntityExportEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await Task.CompletedTask;
    });
