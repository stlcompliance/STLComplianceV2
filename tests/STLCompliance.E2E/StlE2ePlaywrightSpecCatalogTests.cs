using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E;

[Trait("Category", "E2e")]
public sealed class StlE2ePlaywrightSpecCatalogTests
{
    [Fact]
    public void Deep_link_smoke_specs_include_companion_and_trainarr_paths()
    {
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.CompanionFieldInboxTrainarrDeepLinkSpec,
            StlE2ePlaywrightSpecCatalog.DeepLinkSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.ProductTrainarrAssignmentDeepLinkSpec,
            StlE2ePlaywrightSpecCatalog.DeepLinkSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.CompanionFieldInboxMaintainarrDeepLinkSpec,
            StlE2ePlaywrightSpecCatalog.DeepLinkSmokeSpecs);
    }

    [Fact]
    public void Platform_admin_smoke_specs_include_audit_export()
    {
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.PlatformAdminAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.PlatformAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.PlatformAdminAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
    }

    [Fact]
    public void Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w248()
    {
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.MaintainArrSettingsAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.ComplianceCoreM12WorkerSettingsSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.ComplianceCoreAuditDeliveryOrchestrationSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.TrainArrAssignmentMaterialDemandSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchCommandCenterSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchExceptionQueueSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchActiveTripsSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchUnassignedWorkQueueSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDriverPortalSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchProofDvirReadSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.SupplyArrSettingsIntegrationEventsSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.SupplyArrReportsWorkspaceSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.StaffArrAdminAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.TrainArrSettingsAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrReportsAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.MaintainArrSettingsAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.ComplianceCoreM12WorkerSettingsSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.ComplianceCoreAuditDeliveryOrchestrationSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.TrainArrAssignmentMaterialDemandSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchCommandCenterSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchExceptionQueueSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchActiveTripsSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchUnassignedWorkQueueSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDriverPortalSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrDispatchProofDvirReadSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.SupplyArrSettingsIntegrationEventsSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.SupplyArrReportsWorkspaceSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.StaffArrAdminAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.TrainArrSettingsAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.RoutArrReportsAuditExportSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
    }

    [Fact]
    public void Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff()
    {
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.ComplianceCoreOperatorRuleEvaluateSmokeSpec,
            StlE2ePlaywrightSpecCatalog.OperatorJourneySmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.SuiteMultiProductHandoffJourneySpec,
            StlE2ePlaywrightSpecCatalog.OperatorJourneySmokeSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.ComplianceCoreOperatorRuleEvaluateSmokeSpec,
            StlE2ePlaywrightSpecCatalog.All);
    }

    [Fact]
    public void Companion_operational_specs_include_offline_queue_and_notifications()
    {
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.CompanionOfflineQueueNotificationSpec,
            StlE2ePlaywrightSpecCatalog.CompanionOperationalSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.CompanionFieldTaskEvidenceSpec,
            StlE2ePlaywrightSpecCatalog.CompanionOperationalSpecs);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.CompanionOfflineQueueNotificationSpec,
            StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(
            StlE2ePlaywrightSpecCatalog.CompanionFieldTaskEvidenceSpec,
            StlE2ePlaywrightSpecCatalog.All);
    }

    [Fact]
    public void All_specs_lists_handoff_and_deep_link_coverage()
    {
        Assert.Contains(StlE2ePlaywrightSpecCatalog.SuiteLoginHandoffSmokeSpec, StlE2ePlaywrightSpecCatalog.All);
        Assert.Contains(StlE2ePlaywrightSpecCatalog.ProductHandoffSmokeSpec, StlE2ePlaywrightSpecCatalog.All);
        Assert.True(StlE2ePlaywrightSpecCatalog.All.Count >= 26);
    }

    [Fact]
    public void Companion_frontend_uses_port_5181()
    {
        Assert.Equal(5181, StlE2eFrontendCatalog.CompanionPort);
        Assert.Equal("companion", StlE2eFrontendCatalog.CompanionFrontend.ProductKey);
    }
}
