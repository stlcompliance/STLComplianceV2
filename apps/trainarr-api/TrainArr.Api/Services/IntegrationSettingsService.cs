using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class IntegrationSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<IntegrationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantIntegrationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : Map(settings);
    }

    public async Task<IntegrationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertIntegrationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantIntegrationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantIntegrationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantIntegrationSettings.Add(entity);
        }

        entity.StaffArrIntegrationEnabled = request.StaffArrIntegrationEnabled;
        entity.StaffArrIncidentIntakeEnabled = request.StaffArrIncidentIntakeEnabled;
        entity.StaffArrPublicationDeliveryEnabled = request.StaffArrPublicationDeliveryEnabled;
        entity.ComplianceCoreIntegrationEnabled = request.ComplianceCoreIntegrationEnabled;
        entity.ComplianceCoreQualificationChecksEnabled = request.ComplianceCoreQualificationChecksEnabled;
        entity.RoutarrIntegrationEnabled = request.RoutarrIntegrationEnabled;
        entity.RoutarrQualificationDispatchEnabled = request.RoutarrQualificationDispatchEnabled;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "integration_settings.upsert",
            tenantId,
            actorUserId,
            "integration_settings",
            tenantId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<TenantIntegrationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantIntegrationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    public async Task EnsureStaffArrIncidentIntakeEnabledAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(tenantId, cancellationToken);
        if (!IntegrationSettingsRules.ResolveStaffArrIncidentIntakeEnabled(snapshot))
        {
            throw new StlApiException(
                "integration_settings.staffarr_incident_intake_disabled",
                "StaffArr incident remediation intake is disabled for this tenant.",
                403);
        }
    }

    public async Task EnsureStaffArrPublicationDeliveryEnabledAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(tenantId, cancellationToken);
        if (!IntegrationSettingsRules.ResolveStaffArrPublicationDeliveryEnabled(snapshot))
        {
            throw new StlApiException(
                "integration_settings.staffarr_publication_disabled",
                "StaffArr certification publication delivery is disabled for this tenant.",
                403);
        }
    }

    public async Task EnsureComplianceCoreQualificationChecksEnabledAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(tenantId, cancellationToken);
        if (!IntegrationSettingsRules.ResolveComplianceCoreQualificationChecksEnabled(snapshot))
        {
            throw new StlApiException(
                "integration_settings.compliancecore_qualification_checks_disabled",
                "Compliance Core qualification checks are disabled for this tenant.",
                403);
        }
    }

    public async Task EnsureRoutarrQualificationDispatchEnabledAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(tenantId, cancellationToken);
        if (!IntegrationSettingsRules.ResolveRoutarrQualificationDispatchEnabled(snapshot))
        {
            throw new StlApiException(
                "integration_settings.routarr_qualification_dispatch_disabled",
                "RoutArr qualification dispatch integration is disabled for this tenant.",
                403);
        }
    }

    internal static TenantIntegrationSettingsSnapshot ToSnapshot(TenantIntegrationSettings settings) =>
        new(
            settings.StaffArrIntegrationEnabled,
            settings.StaffArrIncidentIntakeEnabled,
            settings.StaffArrPublicationDeliveryEnabled,
            settings.ComplianceCoreIntegrationEnabled,
            settings.ComplianceCoreQualificationChecksEnabled,
            settings.RoutarrIntegrationEnabled,
            settings.RoutarrQualificationDispatchEnabled);

    private static IntegrationSettingsResponse DefaultResponse() =>
        new(
            StaffArrIntegrationEnabled: true,
            StaffArrIncidentIntakeEnabled: true,
            StaffArrPublicationDeliveryEnabled: true,
            ComplianceCoreIntegrationEnabled: true,
            ComplianceCoreQualificationChecksEnabled: true,
            RoutarrIntegrationEnabled: true,
            RoutarrQualificationDispatchEnabled: true,
            UpdatedAt: null);

    private static IntegrationSettingsResponse Map(TenantIntegrationSettings settings) =>
        new(
            settings.StaffArrIntegrationEnabled,
            settings.StaffArrIncidentIntakeEnabled,
            settings.StaffArrPublicationDeliveryEnabled,
            settings.ComplianceCoreIntegrationEnabled,
            settings.ComplianceCoreQualificationChecksEnabled,
            settings.RoutarrIntegrationEnabled,
            settings.RoutarrQualificationDispatchEnabled,
            settings.UpdatedAt);
}
