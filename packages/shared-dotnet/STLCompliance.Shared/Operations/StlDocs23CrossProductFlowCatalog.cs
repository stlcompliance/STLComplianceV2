namespace STLCompliance.Shared.Operations;

/// <summary>
/// docs/11 cross-product gate and docs/23 workflow journeys mapped to M13 integration and Playwright proof.
/// </summary>
public static class StlDocs23CrossProductFlowCatalog
{
    public const string E2eIntegrationTestProject = "STLCompliance.E2E";

    /// <summary>Minimum integration flow test classes covering docs/11 cross-product gate journeys.</summary>
    public const int MinimumIntegrationFlowTestClasses = 7;

    public static readonly IReadOnlyList<Docs23CrossProductFlowDescriptor> IntegrationFlows =
    [
        new(
            "new_employee_to_qualified_worker",
            "New employee to qualified worker",
            "StaffArrWorkforceOnboardingFlowTests",
            StlE2ePlaywrightSpecCatalog.StaffArrWorkforceOnboardingJourneySmokeSpec),
        new(
            "asset_to_dispatch_ready",
            "Asset to dispatch-ready",
            "RoutArrAssetDispatchReadyFlowTests"),
        new(
            "failed_inspection_to_work_order",
            "Failed inspection to work order",
            "MaintainArrInspectionToWorkOrderFlowTests",
            StlE2ePlaywrightSpecCatalog.MaintainArrDefectInspectionEvidenceSmokeSpec),
        new(
            "work_order_parts_demand_to_supplyarr",
            "Work order parts demand to SupplyArr request",
            "MaintainArrSupplyArrPartsDemandFlowTests",
            StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingDemandProcessingSmokeSpec),
        new(
            "training_completion_to_staffarr_readiness",
            "Training completion to StaffArr readiness",
            "TrainArrAssignmentCompleteFlowTests"),
        new(
            "route_assignment_with_checks",
            "Route assignment with driver and asset checks",
            "RoutArrDispatchAssignFlowTests",
            StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateAssignJourneySmokeSpec),
        new(
            "staffarr_technician_ref_to_maintainarr",
            "StaffArr technician mirror to MaintainArr",
            "StaffArrMaintainArrTechnicianSyncFlowTests"),
    ];

    public static readonly IReadOnlyList<Docs23CrossProductFlowDescriptor> PlaywrightOnlyFlows =
    [
        new(
            "compliance_core_operational_validation",
            "Compliance Core validation from an operational product",
            PlaywrightSpec: StlE2ePlaywrightSpecCatalog.ComplianceCoreOperatorWorkflowGateJourneySmokeSpec),
    ];
}

/// <param name="JourneyKey">Stable docs/23 journey identifier.</param>
/// <param name="Title">Human-readable journey title from docs/11 ship gate.</param>
/// <param name="IntegrationFlowTestTypeName">
/// Test class name under <c>STLCompliance.E2E.Flows</c> when integration proof exists.
/// </param>
/// <param name="PlaywrightSpec">Optional Playwright spec filename under tests/e2e-playwright/tests.</param>
public sealed record Docs23CrossProductFlowDescriptor(
    string JourneyKey,
    string Title,
    string? IntegrationFlowTestTypeName = null,
    string? PlaywrightSpec = null);
