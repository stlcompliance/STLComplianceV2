using TrainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class IntegrationSettingsRulesTests
{
    [Fact]
    public void ResolveStaffArrIncidentIntakeEnabled_defaults_to_enabled_when_missing() =>
        Assert.True(IntegrationSettingsRules.ResolveStaffArrIncidentIntakeEnabled(null));

    [Fact]
    public void ResolveStaffArrIncidentIntakeEnabled_respects_master_disable() =>
        Assert.False(IntegrationSettingsRules.ResolveStaffArrIncidentIntakeEnabled(
            new TenantIntegrationSettingsSnapshot(
                StaffArrIntegrationEnabled: false,
                StaffArrIncidentIntakeEnabled: true,
                StaffArrPublicationDeliveryEnabled: true,
                ComplianceCoreIntegrationEnabled: true,
                ComplianceCoreQualificationChecksEnabled: true,
                RoutarrIntegrationEnabled: true,
                RoutarrQualificationDispatchEnabled: true)));

    [Fact]
    public void ResolveComplianceCoreQualificationChecksEnabled_respects_feature_disable() =>
        Assert.False(IntegrationSettingsRules.ResolveComplianceCoreQualificationChecksEnabled(
            new TenantIntegrationSettingsSnapshot(
                StaffArrIntegrationEnabled: true,
                StaffArrIncidentIntakeEnabled: true,
                StaffArrPublicationDeliveryEnabled: true,
                ComplianceCoreIntegrationEnabled: true,
                ComplianceCoreQualificationChecksEnabled: false,
                RoutarrIntegrationEnabled: true,
                RoutarrQualificationDispatchEnabled: true)));

    [Fact]
    public void ResolveRoutarrQualificationDispatchEnabled_respects_master_disable() =>
        Assert.False(IntegrationSettingsRules.ResolveRoutarrQualificationDispatchEnabled(
            new TenantIntegrationSettingsSnapshot(
                StaffArrIntegrationEnabled: true,
                StaffArrIncidentIntakeEnabled: true,
                StaffArrPublicationDeliveryEnabled: true,
                ComplianceCoreIntegrationEnabled: true,
                ComplianceCoreQualificationChecksEnabled: true,
                RoutarrIntegrationEnabled: false,
                RoutarrQualificationDispatchEnabled: true)));
}
