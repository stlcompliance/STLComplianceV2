namespace STLCompliance.Shared.Integration;

public sealed record StlIntegrationTokenProfile(
    string ProfileKey,
    string ConsumerService,
    string ConfigurationKey,
    string SourceProductKey,
    IReadOnlyList<string> AllowedProductKeys,
    string ActionScope);

public static class StlIntegrationTokenCatalog
{
    public static IReadOnlyList<StlIntegrationTokenProfile> All { get; } =
    [
        Profile("handoff-staffarr", "staffarr-api", "Handoff__ServiceToken", "staffarr", ["staffarr"], "launch.redeem"),
        Profile("staffarr-trainarr", "staffarr-api", "TrainArr__ServiceToken", "staffarr", ["trainarr"], "trainarr.incident_remediations.write"),

        Profile("handoff-trainarr", "trainarr-api", "Handoff__ServiceToken", "trainarr", ["trainarr"], "launch.redeem"),
        Profile(
            "trainarr-staffarr",
            "trainarr-api",
            "StaffArr__ServiceToken",
            "trainarr",
            ["staffarr"],
            "staffarr.training_blockers.write,staffarr.certification_grants.write,staffarr.certification_lifecycle.write"),
        Profile(
            "trainarr-compliancecore",
            "trainarr-api",
            "ComplianceCore__ServiceToken",
            "trainarr",
            ["compliancecore"],
            "compliancecore.rules.evaluate,compliancecore.citations.read,compliancecore.rulepacks.read"),

        Profile("handoff-maintainarr", "maintainarr-api", "Handoff__ServiceToken", "maintainarr", ["maintainarr"], "launch.redeem"),
        Profile("maintainarr-supplyarr", "maintainarr-api", "SupplyArr__ServiceToken", "maintainarr", ["supplyarr"], "supplyarr.demand_intake.write"),

        Profile("handoff-supplyarr", "supplyarr-api", "Handoff__ServiceToken", "supplyarr", ["supplyarr"], "launch.redeem"),
        Profile("supplyarr-maintainarr", "supplyarr-api", "MaintainArr__ServiceToken", "supplyarr", ["maintainarr"], "maintainarr.demand_status.write"),

        Profile("handoff-routarr", "routarr-api", "Handoff__ServiceToken", "routarr", ["routarr"], "launch.redeem"),
        Profile("routarr-trainarr", "routarr-api", "TrainArr__ServiceToken", "routarr", ["trainarr"], "trainarr.qualification_checks.dispatch"),
        Profile("routarr-staffarr", "routarr-api", "StaffArr__ServiceToken", "routarr", ["staffarr"], "staffarr.readiness.dispatch_gate"),
        Profile("routarr-maintainarr", "routarr-api", "MaintainArr__ServiceToken", "routarr", ["maintainarr"], "maintainarr.asset_readiness.dispatch_gate"),
        Profile("routarr-compliancecore", "routarr-api", "ComplianceCore__ServiceToken", "routarr", ["compliancecore"], "compliancecore.workflow.gates.check"),

        Profile("handoff-compliancecore", "compliancecore-api", "Handoff__ServiceToken", "compliancecore", ["compliancecore"], "launch.redeem"),

        Profile("worker-trainarr-expire", "shared-worker", "TrainArrQualificationExpiration__ServiceToken", "shared-worker", ["trainarr"], "trainarr.qualifications.expire"),
        Profile(
            "worker-trainarr-notifications",
            "shared-worker",
            "TrainArrNotificationDispatch__ServiceToken",
            "shared-worker",
            ["trainarr"],
            "trainarr.notifications.dispatch"),
        Profile("worker-staffarr-cert-expire", "shared-worker", "StaffArrCertificationExpiration__ServiceToken", "shared-worker", ["staffarr"], "staffarr.certifications.expire"),
        Profile("worker-staffarr-readiness", "shared-worker", "StaffArrReadinessRollup__ServiceToken", "shared-worker", ["staffarr"], "staffarr.readiness.rollup"),
        Profile("worker-staffarr-permissions", "shared-worker", "StaffArrPermissionProjection__ServiceToken", "shared-worker", ["staffarr"], "staffarr.permissions.project"),
        Profile(
            "worker-staffarr-audit-packages",
            "shared-worker",
            "StaffArrAuditPackageGeneration__ServiceToken",
            "shared-worker",
            ["staffarr"],
            "staffarr.audit_packages.generate"),
        Profile("worker-maintainarr-pm", "shared-worker", "MaintainArrPmDueScan__ServiceToken", "shared-worker", ["maintainarr"], "maintainarr.pm.scan"),
        Profile(
            "worker-maintainarr-notifications",
            "shared-worker",
            "MaintainArrNotificationDispatch__ServiceToken",
            "shared-worker",
            ["maintainarr"],
            "maintainarr.notifications.dispatch"),
        Profile(
            "worker-routarr-notifications",
            "shared-worker",
            "RoutArrNotificationDispatch__ServiceToken",
            "shared-worker",
            ["routarr"],
            "routarr.notifications.dispatch"),
        Profile("worker-supplyarr-reorder", "shared-worker", "SupplyArrReorderEvaluation__ServiceToken", "shared-worker", ["supplyarr"], "supplyarr.reorder.evaluate"),
        Profile(
            "worker-supplyarr-notifications",
            "shared-worker",
            "SupplyArrNotificationDispatch__ServiceToken",
            "shared-worker",
            ["supplyarr"],
            "supplyarr.notifications.dispatch"),
        Profile(
            "worker-compliancecore-scheduled",
            "shared-worker",
            "ComplianceCoreScheduledEvaluation__ServiceToken",
            "shared-worker",
            ["compliancecore"],
            "compliancecore.rules.evaluate.scheduled"),
        Profile(
            "worker-compliancecore-audit-packages",
            "shared-worker",
            "ComplianceCoreAuditPackageGeneration__ServiceToken",
            "shared-worker",
            ["compliancecore"],
            "compliancecore.audit_packages.generate"),
        Profile(
            "worker-nexarr-companion-notifications",
            "shared-worker",
            "NexArrCompanionNotificationDispatch__ServiceToken",
            "shared-worker",
            ["nexarr"],
            "nexarr.companion.notifications.dispatch"),
    ];

    public static IReadOnlyList<StlIntegrationTokenProfile> ForConsumer(string consumerService) =>
        All.Where(p => string.Equals(p.ConsumerService, consumerService, StringComparison.OrdinalIgnoreCase)).ToList();

    private static StlIntegrationTokenProfile Profile(
        string profileKey,
        string consumerService,
        string configurationKey,
        string sourceProductKey,
        IReadOnlyList<string> allowedProductKeys,
        string actionScope) =>
        new(profileKey, consumerService, configurationKey, sourceProductKey, allowedProductKeys, actionScope);
}
