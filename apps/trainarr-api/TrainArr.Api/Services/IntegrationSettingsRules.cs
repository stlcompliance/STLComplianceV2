namespace TrainArr.Api.Services;

public static class IntegrationSettingsRules
{
    public static bool ResolveStaffArrIncidentIntakeEnabled(TenantIntegrationSettingsSnapshot? snapshot) =>
        snapshot?.StaffArrIntegrationEnabled != false && snapshot?.StaffArrIncidentIntakeEnabled != false;

    public static bool ResolveStaffArrPublicationDeliveryEnabled(TenantIntegrationSettingsSnapshot? snapshot) =>
        snapshot?.StaffArrIntegrationEnabled != false && snapshot?.StaffArrPublicationDeliveryEnabled != false;

    public static bool ResolveComplianceCoreQualificationChecksEnabled(TenantIntegrationSettingsSnapshot? snapshot) =>
        snapshot?.ComplianceCoreIntegrationEnabled != false
        && snapshot?.ComplianceCoreQualificationChecksEnabled != false;

    public static bool ResolveRoutarrQualificationDispatchEnabled(TenantIntegrationSettingsSnapshot? snapshot) =>
        snapshot?.RoutarrIntegrationEnabled != false && snapshot?.RoutarrQualificationDispatchEnabled != false;
}

public sealed record TenantIntegrationSettingsSnapshot(
    bool StaffArrIntegrationEnabled,
    bool StaffArrIncidentIntakeEnabled,
    bool StaffArrPublicationDeliveryEnabled,
    bool ComplianceCoreIntegrationEnabled,
    bool ComplianceCoreQualificationChecksEnabled,
    bool RoutarrIntegrationEnabled,
    bool RoutarrQualificationDispatchEnabled);
