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
        app.MapStaffArrTenantSettingsEndpoints();
        app.MapStaffArrEmploymentApplicationEndpoints();
        app.MapStaffArrRecruitingEndpoints();
        app.MapStaffArrMePortalEndpoints();
        app.MapStaffArrPersonnelUpdateRequestEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapStlProductAiAssistanceEndpoints();
        app.MapStaffArrPeopleEndpoints();
        app.MapStaffArrFieldsetEndpoints();
        app.MapStaffArrPersonLookupEndpoints();
        app.MapStaffArrPeopleExportEndpoints();
        app.MapStaffArrWorkerAdminEndpoints();
        app.MapStaffArrManagerHierarchyEndpoints();
        app.MapStaffArrOrgUnitEndpoints();
        app.MapStaffArrLocationEndpoints();
        app.MapStaffArrOrgUnitAssignmentEndpoints();
        app.MapStaffArrRoleManagementEndpoints();
        app.MapStaffArrCertificationEndpoints();
        app.MapStaffArrReadinessEndpoints();
        app.MapStaffArrReadinessRollupEndpoints();
        app.MapStaffArrPersonnelHistoryEndpoints();
        app.MapStaffArrIncidentEndpoints();
        app.MapStaffArrIncidentSupplyDemandEndpoints();
        app.MapStaffArrPerformanceEndpoints();
        app.MapStaffArrBenefitsCompensationEndpoints();
        app.MapStaffArrPersonnelNoteEndpoints();
        app.MapStaffArrPersonnelDocumentEndpoints();
        app.MapStaffArrIntegrationEndpoints();
        app.MapStaffArrReferenceIntegrationEndpoints();
        app.MapStaffArrInternalCertificationExpirationEndpoints();
        app.MapStaffArrInternalReadinessRollupEndpoints();
        app.MapStaffArrInternalPersonnelHistoryEndpoints();
        app.MapStaffArrInternalPermissionProjectionEndpoints();
        app.MapStaffArrInternalPersonExportDeliveryEndpoints();
        app.MapStaffArrAuditPackageEndpoints();
        app.MapStaffArrInternalAuditPackageGenerationEndpoints();
        app.MapStaffArrTrainingAcknowledgementEndpoints();
        app.MapStaffArrOffboardingEndpoints();
        app.MapStaffArrFieldInboxEndpoints();
        app.MapStaffArrEventAndAuditEndpoints();
        app.MapStaffArrTimekeepingEndpoints();
        app.MapStaffArrV1FeatureAliasEndpoints();
        app.MapStaffArrEntityExportEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await Task.CompletedTask;
    });
