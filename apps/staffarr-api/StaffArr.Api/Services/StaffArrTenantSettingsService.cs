using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class StaffArrTenantSettingsService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlySet<string> DisplayNameFormats = ValueSet(
        "preferred_first_last",
        "legal_first_last",
        "last_first",
        "first_last_employee_number");

    private static readonly IReadOnlySet<string> EmployeeNumberUniquenessScopes = ValueSet("tenant", "site", "none");
    private static readonly IReadOnlySet<string> ContactVisibilityModes = ValueSet("admin_only", "manager_admin", "directory");
    private static readonly IReadOnlySet<string> RehireMatchBehaviors = ValueSet("none", "flag_possible_match", "block_until_review");
    private static readonly IReadOnlySet<string> OrgHierarchyModes = ValueSet("standard", "flat", "strict_site_department_team_position");
    private static readonly IReadOnlySet<string> LocationHierarchyModes = ValueSet("site_required", "flat_site", "strict_tree");
    private static readonly IReadOnlySet<string> LocationCodeUniquenessScopes = ValueSet("tenant", "site", "parent", "none");
    private static readonly IReadOnlySet<string> ArchivedLocationAssignmentBehaviors = ValueSet("block_new_assignments", "warn", "allow");
    private static readonly IReadOnlySet<string> PermissionReviewCadences = ValueSet("none", "monthly", "quarterly", "semiannual", "annual");
    private static readonly IReadOnlySet<string> TeamMembershipModes = ValueSet("flexible", "single_team", "matrix");
    private static readonly IReadOnlySet<string> HistoricalAssignmentVisibilityModes = ValueSet("admin_all", "manager_limited", "person_self");
    private static readonly IReadOnlySet<string> ManagerNotificationModes = ValueSet("none", "optional", "always");
    private static readonly IReadOnlySet<string> IncidentVisibilityModes = ValueSet("hr_only", "management", "site_management");
    private static readonly IReadOnlySet<string> ProfileSections = ValueSet("identity", "work", "contact", "emergency", "address", "photo");
    private static readonly IReadOnlySet<string> DigestFrequencies = ValueSet("none", "daily", "weekly");
    private static readonly IReadOnlySet<string> SnapshotLabelPolicies = ValueSet("display_label_only", "display_label_with_status", "display_label_with_source");

    public async Task<StaffArrTenantSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await LoadOrCreateAsync(tenantId, cancellationToken);
        return MapResponse(settings);
    }

    public StaffArrTenantSettingsResponse GetDefaults(Guid tenantId)
    {
        var now = DateTimeOffset.UtcNow;
        return MapResponse(CreateDefault(tenantId, now));
    }

    public async Task<StaffArrTenantSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertStaffArrTenantSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = CreateDefault(tenantId, now);
            db.TenantSettings.Add(entity);
        }

        var before = MapResponse(entity);
        ApplyRequest(entity, request, now);
        await db.SaveChangesAsync(cancellationToken);

        var after = MapResponse(entity);
        await audit.WriteWithMetadataAsync(
            "staffarr.tenant_settings.update",
            tenantId,
            actorUserId,
            "staffarr_tenant_settings",
            tenantId.ToString("D"),
            "success",
            JsonSerializer.Serialize(new { before, after }, JsonOptions),
            cancellationToken: cancellationToken);

        return after;
    }

    public async Task<StaffArrTenantSettings> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings ?? CreateDefault(tenantId, DateTimeOffset.UtcNow);
    }

    private async Task<StaffArrTenantSettings> LoadOrCreateAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var entity = await db.TenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (entity is not null)
        {
            return entity;
        }

        var now = DateTimeOffset.UtcNow;
        entity = CreateDefault(tenantId, now);
        db.TenantSettings.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private static StaffArrTenantSettings CreateDefault(Guid tenantId, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CreatedAt = now,
            UpdatedAt = now
        };

    private static void ApplyRequest(
        StaffArrTenantSettings entity,
        UpsertStaffArrTenantSettingsRequest request,
        DateTimeOffset now)
    {
        EnsureGroup(request, "settings");
        EnsureGroup(request.PersonDirectory, "personDirectory");
        EnsureGroup(request.PersonLifecycle, "personLifecycle");
        EnsureGroup(request.OrgStructure, "orgStructure");
        EnsureGroup(request.LocationHierarchy, "locationHierarchy");
        EnsureGroup(request.RolePermissions, "rolePermissions");
        EnsureGroup(request.TeamsAssignments, "teamsAssignments");
        EnsureGroup(request.Incidents, "incidents");
        EnsureGroup(request.ProfileFieldGovernance, "profileFieldGovernance");
        EnsureGroup(request.NotificationsReviews, "notificationsReviews");
        EnsureGroup(request.DataGovernanceAudit, "dataGovernanceAudit");
        EnsureGroup(request.CrossProductReferences, "crossProductReferences");

        var directory = request.PersonDirectory;
        entity.DisplayNameFormat = NormalizeControlled(directory.DisplayNameFormat, DisplayNameFormats, "displayNameFormat");
        entity.PreferredNameEnabled = directory.PreferredNameEnabled;
        entity.EmployeeNumberLabel = NormalizeText(directory.EmployeeNumberLabel, 64, "employeeNumberLabel");
        entity.EmployeeNumberRequired = directory.EmployeeNumberRequired;
        entity.EmployeeNumberUniquenessScope = NormalizeControlled(
            directory.EmployeeNumberUniquenessScope,
            EmployeeNumberUniquenessScopes,
            "employeeNumberUniquenessScope");
        entity.ProfilePhotoEnabled = directory.ProfilePhotoEnabled;
        entity.ContactVisibilityMode = NormalizeControlled(directory.ContactVisibilityMode, ContactVisibilityModes, "contactVisibilityMode");
        entity.EmergencyContactEnabled = directory.EmergencyContactEnabled;
        entity.PersonalAddressEnabled = directory.PersonalAddressEnabled;

        var lifecycle = request.PersonLifecycle;
        entity.DefaultPersonStatusOnCreate = NormalizeControlled(
            lifecycle.DefaultPersonStatusOnCreate,
            StaffArrControlledFieldCatalog.EmploymentStatusKeys,
            "defaultPersonStatusOnCreate");
        entity.RequireManagerBeforeActivation = lifecycle.RequireManagerBeforeActivation;
        entity.RequirePositionBeforeActivation = lifecycle.RequirePositionBeforeActivation;
        entity.RequireHomeLocationBeforeActivation = lifecycle.RequireHomeLocationBeforeActivation;
        entity.AllowInactivePeopleToBeAssignedWork = lifecycle.AllowInactivePeopleToBeAssignedWork;
        entity.RehireMatchBehavior = NormalizeControlled(lifecycle.RehireMatchBehavior, RehireMatchBehaviors, "rehireMatchBehavior");
        entity.DeactivationReasonRequired = lifecycle.DeactivationReasonRequired;
        entity.AutoRemoveRolesOnDeactivation = lifecycle.AutoRemoveRolesOnDeactivation;
        entity.AutoEndTeamAssignmentsOnDeactivation = lifecycle.AutoEndTeamAssignmentsOnDeactivation;

        var org = request.OrgStructure;
        entity.OrgHierarchyMode = NormalizeControlled(org.OrgHierarchyMode, OrgHierarchyModes, "orgHierarchyMode");
        entity.RequireEveryPersonInOrgUnit = org.RequireEveryPersonInOrgUnit;
        entity.RequireDepartmentUnderSite = org.RequireDepartmentUnderSite;
        entity.AllowMatrixMembership = org.AllowMatrixMembership;
        entity.PrimaryAssignmentRequired = org.PrimaryAssignmentRequired;
        entity.ManagerHierarchyRequired = org.ManagerHierarchyRequired;
        entity.AllowSkipLevelManagers = org.AllowSkipLevelManagers;
        entity.PreventCircularReporting = true;

        var location = request.LocationHierarchy;
        entity.LocationHierarchyMode = NormalizeControlled(location.LocationHierarchyMode, LocationHierarchyModes, "locationHierarchyMode");
        entity.RequireLocationCode = location.RequireLocationCode;
        entity.LocationCodeUniquenessScope = NormalizeControlled(
            location.LocationCodeUniquenessScope,
            LocationCodeUniquenessScopes,
            "locationCodeUniquenessScope");
        entity.AllowOperationalLocations = location.AllowOperationalLocations;
        entity.AllowAddressableBinsShelves = location.AllowAddressableBinsShelves;
        entity.AllowMobileLocations = location.AllowMobileLocations;
        entity.RequireParentLocationExceptRoot = location.RequireParentLocationExceptRoot;
        entity.ArchivedLocationAssignmentBehavior = NormalizeControlled(
            location.ArchivedLocationAssignmentBehavior,
            ArchivedLocationAssignmentBehaviors,
            "archivedLocationAssignmentBehavior");

        var role = request.RolePermissions;
        entity.RoleAssignmentApprovalRequired = role.RoleAssignmentApprovalRequired;
        entity.AllowSelfServiceRoleRequests = role.AllowSelfServiceRoleRequests;
        entity.RoleExpirationEnabled = role.RoleExpirationEnabled;
        entity.DefaultRoleGrantDurationDays = NormalizePositiveWhen(
            role.DefaultRoleGrantDurationDays,
            role.RoleExpirationEnabled,
            "defaultRoleGrantDurationDays");
        entity.RequireAssignmentReason = role.RequireAssignmentReason;
        entity.PermissionReviewCadence = NormalizeControlled(role.PermissionReviewCadence, PermissionReviewCadences, "permissionReviewCadence");
        entity.AutoRemoveRolesOnInactivePerson = role.AutoRemoveRolesOnInactivePerson;
        entity.AllowDirectPermissions = role.AllowDirectPermissions;
        entity.PreferRolesOverDirectPermissions = role.PreferRolesOverDirectPermissions;
        entity.SiteScopedRoleAssignmentsEnabled = role.SiteScopedRoleAssignmentsEnabled;

        var team = request.TeamsAssignments;
        entity.TeamMembershipMode = NormalizeControlled(team.TeamMembershipMode, TeamMembershipModes, "teamMembershipMode");
        entity.RequireTeamLead = team.RequireTeamLead;
        entity.AllowTemporaryAssignments = team.AllowTemporaryAssignments;
        entity.TemporaryAssignmentMaxDurationDays = NormalizePositiveWhen(
            team.TemporaryAssignmentMaxDurationDays,
            team.AllowTemporaryAssignments,
            "temporaryAssignmentMaxDurationDays");
        entity.AssignmentEffectiveDatingEnabled = team.AssignmentEffectiveDatingEnabled;
        entity.HistoricalAssignmentVisibilityMode = NormalizeControlled(
            team.HistoricalAssignmentVisibilityMode,
            HistoricalAssignmentVisibilityModes,
            "historicalAssignmentVisibilityMode");
        entity.AllowOpenPositions = team.AllowOpenPositions;

        var incident = request.Incidents;
        entity.IncidentIntakeEnabled = incident.IncidentIntakeEnabled;
        entity.RequireIncidentCategory = incident.RequireIncidentCategory;
        entity.RequireInvolvedPerson = incident.RequireInvolvedPerson;
        entity.ManagerNotificationMode = NormalizeControlled(
            incident.ManagerNotificationMode,
            ManagerNotificationModes,
            "managerNotificationMode");
        entity.TrainArrRoutingEnabled = incident.TrainArrRoutingEnabled;
        entity.RetrainingRecommendationThreshold = NormalizePositiveWhen(
            incident.RetrainingRecommendationThreshold,
            incident.TrainArrRoutingEnabled,
            "retrainingRecommendationThreshold");
        entity.IncidentVisibilityMode = NormalizeControlled(incident.IncidentVisibilityMode, IncidentVisibilityModes, "incidentVisibilityMode");
        entity.ClosureApprovalRequired = incident.ClosureApprovalRequired;

        var fields = request.ProfileFieldGovernance;
        entity.RequiredProfileSectionsCsv = JoinControlledList(fields.RequiredProfileSections, ProfileSections, "requiredProfileSections");
        entity.OptionalProfileSectionsCsv = JoinControlledList(fields.OptionalProfileSections, ProfileSections, "optionalProfileSections");
        entity.CustomProfileFieldsEnabled = fields.CustomProfileFieldsEnabled;
        entity.FieldVisibilityByRoleEnabled = fields.CustomProfileFieldsEnabled && fields.FieldVisibilityByRoleEnabled;
        entity.FieldEditabilityByRoleEnabled = fields.CustomProfileFieldsEnabled && fields.FieldEditabilityByRoleEnabled;
        entity.FieldReviewRequired = fields.FieldReviewRequired;
        entity.FieldHistoryEnabled = fields.FieldHistoryEnabled;

        var notifications = request.NotificationsReviews;
        entity.NotifyManagerOnNewPerson = notifications.NotifyManagerOnNewPerson;
        entity.NotifyOnManagerChange = notifications.NotifyOnManagerChange;
        entity.NotifyOnRoleGrantRemoval = notifications.NotifyOnRoleGrantRemoval;
        entity.NotifyBeforeRoleExpiration = role.RoleExpirationEnabled && notifications.NotifyBeforeRoleExpiration;
        entity.NotifyOnInactiveAssignmentConflict = notifications.NotifyOnInactiveAssignmentConflict;
        entity.ReviewRemindersEnabled = notifications.ReviewRemindersEnabled;
        entity.DigestFrequency = NormalizeControlled(notifications.DigestFrequency, DigestFrequencies, "digestFrequency");

        var governance = request.DataGovernanceAudit;
        entity.AuditProfileChanges = governance.AuditProfileChanges;
        entity.AuditRoleChanges = governance.AuditRoleChanges;
        entity.AuditOrgLocationChanges = governance.AuditOrgLocationChanges;
        entity.RequireChangeReasonForSensitiveEdits = governance.RequireChangeReasonForSensitiveEdits;
        entity.SoftArchiveOnly = governance.SoftArchiveOnly;
        entity.RecordRetentionHintDays = NormalizePositiveOptional(governance.RecordRetentionHintDays, "recordRetentionHintDays");
        entity.ExportEnabled = governance.ExportEnabled;
        entity.BulkImportEnabled = governance.BulkImportEnabled;
        entity.BulkImportReviewRequired = governance.BulkImportReviewRequired;

        var crossProduct = request.CrossProductReferences;
        entity.ExposePeopleReferenceApi = crossProduct.ExposePeopleReferenceApi;
        entity.ExposeLocationReferenceApi = crossProduct.ExposeLocationReferenceApi;
        entity.ExposeOrgUnitReferenceApi = crossProduct.ExposeOrgUnitReferenceApi;
        entity.PublishPersonLifecycleEvents = crossProduct.PublishPersonLifecycleEvents;
        entity.PublishOrgLocationEvents = crossProduct.PublishOrgLocationEvents;
        entity.AllowProductOriginatedPersonProposals = crossProduct.AllowProductOriginatedPersonProposals;
        entity.RequireReviewForProductOriginatedProposals = crossProduct.RequireReviewForProductOriginatedProposals;
        entity.SnapshotLabelPolicy = NormalizeControlled(crossProduct.SnapshotLabelPolicy, SnapshotLabelPolicies, "snapshotLabelPolicy");
        entity.UpdatedAt = now;
    }

    public static StaffArrTenantSettingsResponse MapResponse(StaffArrTenantSettings entity) =>
        new(
            entity.TenantId,
            new PersonDirectorySettingsDto(
                entity.DisplayNameFormat,
                entity.PreferredNameEnabled,
                entity.EmployeeNumberLabel,
                entity.EmployeeNumberRequired,
                entity.EmployeeNumberUniquenessScope,
                entity.ProfilePhotoEnabled,
                entity.ContactVisibilityMode,
                entity.EmergencyContactEnabled,
                entity.PersonalAddressEnabled),
            new PersonLifecycleSettingsDto(
                entity.DefaultPersonStatusOnCreate,
                entity.RequireManagerBeforeActivation,
                entity.RequirePositionBeforeActivation,
                entity.RequireHomeLocationBeforeActivation,
                entity.AllowInactivePeopleToBeAssignedWork,
                entity.RehireMatchBehavior,
                entity.DeactivationReasonRequired,
                entity.AutoRemoveRolesOnDeactivation,
                entity.AutoEndTeamAssignmentsOnDeactivation),
            new OrgStructureSettingsDto(
                entity.OrgHierarchyMode,
                entity.RequireEveryPersonInOrgUnit,
                entity.RequireDepartmentUnderSite,
                entity.AllowMatrixMembership,
                entity.PrimaryAssignmentRequired,
                entity.ManagerHierarchyRequired,
                entity.AllowSkipLevelManagers,
                true),
            new LocationHierarchySettingsDto(
                entity.LocationHierarchyMode,
                entity.RequireLocationCode,
                entity.LocationCodeUniquenessScope,
                entity.AllowOperationalLocations,
                entity.AllowAddressableBinsShelves,
                entity.AllowMobileLocations,
                entity.RequireParentLocationExceptRoot,
                entity.ArchivedLocationAssignmentBehavior),
            new RolePermissionSettingsDto(
                entity.RoleAssignmentApprovalRequired,
                entity.AllowSelfServiceRoleRequests,
                entity.RoleExpirationEnabled,
                entity.DefaultRoleGrantDurationDays,
                entity.RequireAssignmentReason,
                entity.PermissionReviewCadence,
                entity.AutoRemoveRolesOnInactivePerson,
                entity.AllowDirectPermissions,
                entity.PreferRolesOverDirectPermissions,
                entity.SiteScopedRoleAssignmentsEnabled),
            new TeamAssignmentSettingsDto(
                entity.TeamMembershipMode,
                entity.RequireTeamLead,
                entity.AllowTemporaryAssignments,
                entity.TemporaryAssignmentMaxDurationDays,
                entity.AssignmentEffectiveDatingEnabled,
                entity.HistoricalAssignmentVisibilityMode,
                entity.AllowOpenPositions),
            new IncidentRoutingSettingsDto(
                entity.IncidentIntakeEnabled,
                entity.RequireIncidentCategory,
                entity.RequireInvolvedPerson,
                entity.ManagerNotificationMode,
                entity.TrainArrRoutingEnabled,
                entity.RetrainingRecommendationThreshold,
                entity.IncidentVisibilityMode,
                entity.ClosureApprovalRequired),
            new ProfileFieldGovernanceSettingsDto(
                SplitCsv(entity.RequiredProfileSectionsCsv),
                SplitCsv(entity.OptionalProfileSectionsCsv),
                entity.CustomProfileFieldsEnabled,
                entity.FieldVisibilityByRoleEnabled,
                entity.FieldEditabilityByRoleEnabled,
                entity.FieldReviewRequired,
                entity.FieldHistoryEnabled),
            new NotificationReviewSettingsDto(
                entity.NotifyManagerOnNewPerson,
                entity.NotifyOnManagerChange,
                entity.NotifyOnRoleGrantRemoval,
                entity.NotifyBeforeRoleExpiration,
                entity.NotifyOnInactiveAssignmentConflict,
                entity.ReviewRemindersEnabled,
                entity.DigestFrequency),
            new DataGovernanceAuditSettingsDto(
                entity.AuditProfileChanges,
                entity.AuditRoleChanges,
                entity.AuditOrgLocationChanges,
                entity.RequireChangeReasonForSensitiveEdits,
                entity.SoftArchiveOnly,
                entity.RecordRetentionHintDays,
                entity.ExportEnabled,
                entity.BulkImportEnabled,
                entity.BulkImportReviewRequired),
            new CrossProductReferenceSettingsDto(
                entity.ExposePeopleReferenceApi,
                entity.ExposeLocationReferenceApi,
                entity.ExposeOrgUnitReferenceApi,
                entity.PublishPersonLifecycleEvents,
                entity.PublishOrgLocationEvents,
                entity.AllowProductOriginatedPersonProposals,
                entity.RequireReviewForProductOriginatedProposals,
                entity.SnapshotLabelPolicy),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeControlled(
        string? value,
        IReadOnlySet<string> allowedValues,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("staffarr_tenant_settings.validation", $"{fieldName} is required.", 400);
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!allowedValues.Contains(normalized))
        {
            throw new StlApiException("staffarr_tenant_settings.validation", $"{fieldName} is not supported.", 400);
        }

        return normalized;
    }

    private static void EnsureGroup(object? group, string groupName)
    {
        if (group is null)
        {
            throw new StlApiException(
                "staffarr_tenant_settings.validation",
                $"{groupName} is required.",
                400);
        }
    }

    private static string NormalizeText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("staffarr_tenant_settings.validation", $"{fieldName} is required.", 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException(
                "staffarr_tenant_settings.validation",
                $"{fieldName} must be {maxLength} characters or less.",
                400);
        }

        return normalized;
    }

    private static int? NormalizePositiveWhen(int? value, bool required, string fieldName)
    {
        if (!required)
        {
            return NormalizePositiveOptional(value, fieldName);
        }

        if (value is null or <= 0)
        {
            throw new StlApiException("staffarr_tenant_settings.validation", $"{fieldName} must be positive.", 400);
        }

        return value.Value;
    }

    private static int? NormalizePositiveOptional(int? value, string fieldName)
    {
        if (value is null)
        {
            return null;
        }

        if (value <= 0)
        {
            throw new StlApiException("staffarr_tenant_settings.validation", $"{fieldName} must be positive.", 400);
        }

        return value.Value;
    }

    private static string JoinControlledList(
        IReadOnlyList<string>? values,
        IReadOnlySet<string> allowedValues,
        string fieldName)
    {
        if (values is null)
        {
            return string.Empty;
        }

        var normalized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => NormalizeControlled(value, allowedValues, fieldName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return string.Join(',', normalized);
    }

    private static IReadOnlyList<string> SplitCsv(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static IReadOnlySet<string> ValueSet(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}
