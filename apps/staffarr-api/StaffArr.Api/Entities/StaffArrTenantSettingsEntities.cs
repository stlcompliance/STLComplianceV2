using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class StaffArrTenantSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string DisplayNameFormat { get; set; } = StaffArrTenantSettingsDefaults.DisplayNameFormat;

    public bool PreferredNameEnabled { get; set; } = true;

    public string EmployeeNumberLabel { get; set; } = StaffArrTenantSettingsDefaults.EmployeeNumberLabel;

    public bool EmployeeNumberRequired { get; set; }

    public string EmployeeNumberUniquenessScope { get; set; } = StaffArrTenantSettingsDefaults.EmployeeNumberUniquenessScope;

    public bool ProfilePhotoEnabled { get; set; }

    public string ContactVisibilityMode { get; set; } = StaffArrTenantSettingsDefaults.ContactVisibilityMode;

    public bool EmergencyContactEnabled { get; set; } = true;

    public bool PersonalAddressEnabled { get; set; }

    public string DefaultPersonStatusOnCreate { get; set; } = StaffArrTenantSettingsDefaults.DefaultPersonStatusOnCreate;

    public bool RequireManagerBeforeActivation { get; set; }

    public bool RequirePositionBeforeActivation { get; set; }

    public bool RequireHomeLocationBeforeActivation { get; set; }

    public bool AllowInactivePeopleToBeAssignedWork { get; set; }

    public string RehireMatchBehavior { get; set; } = StaffArrTenantSettingsDefaults.RehireMatchBehavior;

    public bool DeactivationReasonRequired { get; set; } = true;

    public bool AutoRemoveRolesOnDeactivation { get; set; }

    public bool AutoEndTeamAssignmentsOnDeactivation { get; set; }

    public string OrgHierarchyMode { get; set; } = StaffArrTenantSettingsDefaults.OrgHierarchyMode;

    public bool RequireEveryPersonInOrgUnit { get; set; }

    public bool RequireDepartmentUnderSite { get; set; } = true;

    public bool AllowMatrixMembership { get; set; } = true;

    public bool PrimaryAssignmentRequired { get; set; }

    public bool ManagerHierarchyRequired { get; set; }

    public bool AllowSkipLevelManagers { get; set; } = true;

    public bool PreventCircularReporting { get; set; } = true;

    public string LocationHierarchyMode { get; set; } = StaffArrTenantSettingsDefaults.LocationHierarchyMode;

    public bool RequireLocationCode { get; set; }

    public string LocationCodeUniquenessScope { get; set; } = StaffArrTenantSettingsDefaults.LocationCodeUniquenessScope;

    public bool AllowOperationalLocations { get; set; } = true;

    public bool AllowAddressableBinsShelves { get; set; } = true;

    public bool AllowMobileLocations { get; set; } = true;

    public bool RequireParentLocationExceptRoot { get; set; }

    public string ArchivedLocationAssignmentBehavior { get; set; } = StaffArrTenantSettingsDefaults.ArchivedLocationAssignmentBehavior;

    public bool RoleAssignmentApprovalRequired { get; set; }

    public bool AllowSelfServiceRoleRequests { get; set; }

    public bool RoleExpirationEnabled { get; set; }

    public int? DefaultRoleGrantDurationDays { get; set; } = StaffArrTenantSettingsDefaults.DefaultRoleGrantDurationDays;

    public bool RequireAssignmentReason { get; set; }

    public string PermissionReviewCadence { get; set; } = StaffArrTenantSettingsDefaults.PermissionReviewCadence;

    public bool AutoRemoveRolesOnInactivePerson { get; set; }

    public bool AllowDirectPermissions { get; set; }

    public bool PreferRolesOverDirectPermissions { get; set; } = true;

    public bool SiteScopedRoleAssignmentsEnabled { get; set; } = true;

    public string TeamMembershipMode { get; set; } = StaffArrTenantSettingsDefaults.TeamMembershipMode;

    public bool RequireTeamLead { get; set; }

    public bool AllowTemporaryAssignments { get; set; } = true;

    public int? TemporaryAssignmentMaxDurationDays { get; set; } = StaffArrTenantSettingsDefaults.TemporaryAssignmentMaxDurationDays;

    public bool AssignmentEffectiveDatingEnabled { get; set; } = true;

    public string HistoricalAssignmentVisibilityMode { get; set; } = StaffArrTenantSettingsDefaults.HistoricalAssignmentVisibilityMode;

    public bool AllowOpenPositions { get; set; } = true;

    public bool IncidentIntakeEnabled { get; set; } = true;

    public bool RequireIncidentCategory { get; set; } = true;

    public bool RequireInvolvedPerson { get; set; } = true;

    public string ManagerNotificationMode { get; set; } = StaffArrTenantSettingsDefaults.ManagerNotificationMode;

    public bool TrainArrRoutingEnabled { get; set; } = true;

    public int? RetrainingRecommendationThreshold { get; set; } = StaffArrTenantSettingsDefaults.RetrainingRecommendationThreshold;

    public string IncidentVisibilityMode { get; set; } = StaffArrTenantSettingsDefaults.IncidentVisibilityMode;

    public bool ClosureApprovalRequired { get; set; }

    public string RequiredProfileSectionsCsv { get; set; } = StaffArrTenantSettingsDefaults.RequiredProfileSectionsCsv;

    public string OptionalProfileSectionsCsv { get; set; } = StaffArrTenantSettingsDefaults.OptionalProfileSectionsCsv;

    public bool CustomProfileFieldsEnabled { get; set; }

    public bool FieldVisibilityByRoleEnabled { get; set; }

    public bool FieldEditabilityByRoleEnabled { get; set; }

    public bool FieldReviewRequired { get; set; }

    public bool FieldHistoryEnabled { get; set; } = true;

    public bool NotifyManagerOnNewPerson { get; set; } = true;

    public bool NotifyOnManagerChange { get; set; } = true;

    public bool NotifyOnRoleGrantRemoval { get; set; } = true;

    public bool NotifyBeforeRoleExpiration { get; set; } = true;

    public bool NotifyOnInactiveAssignmentConflict { get; set; } = true;

    public bool ReviewRemindersEnabled { get; set; } = true;

    public string DigestFrequency { get; set; } = StaffArrTenantSettingsDefaults.DigestFrequency;

    public bool AuditProfileChanges { get; set; } = true;

    public bool AuditRoleChanges { get; set; } = true;

    public bool AuditOrgLocationChanges { get; set; } = true;

    public bool RequireChangeReasonForSensitiveEdits { get; set; }

    public bool SoftArchiveOnly { get; set; } = true;

    public int? RecordRetentionHintDays { get; set; }

    public bool ExportEnabled { get; set; } = true;

    public bool BulkImportEnabled { get; set; } = true;

    public bool BulkImportReviewRequired { get; set; }

    public bool ExposePeopleReferenceApi { get; set; } = true;

    public bool ExposeLocationReferenceApi { get; set; } = true;

    public bool ExposeOrgUnitReferenceApi { get; set; } = true;

    public bool PublishPersonLifecycleEvents { get; set; } = true;

    public bool PublishOrgLocationEvents { get; set; } = true;

    public bool AllowProductOriginatedPersonProposals { get; set; }

    public bool RequireReviewForProductOriginatedProposals { get; set; } = true;

    public string SnapshotLabelPolicy { get; set; } = StaffArrTenantSettingsDefaults.SnapshotLabelPolicy;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class StaffArrTenantSettingsDefaults
{
    public const string DisplayNameFormat = "preferred_first_last";
    public const string EmployeeNumberLabel = "Employee number";
    public const string EmployeeNumberUniquenessScope = "tenant";
    public const string ContactVisibilityMode = "manager_admin";
    public const string DefaultPersonStatusOnCreate = "pending_start";
    public const string RehireMatchBehavior = "flag_possible_match";
    public const string OrgHierarchyMode = "standard";
    public const string LocationHierarchyMode = "site_required";
    public const string LocationCodeUniquenessScope = "parent";
    public const string ArchivedLocationAssignmentBehavior = "block_new_assignments";
    public const int DefaultRoleGrantDurationDays = 365;
    public const string PermissionReviewCadence = "quarterly";
    public const string TeamMembershipMode = "flexible";
    public const int TemporaryAssignmentMaxDurationDays = 90;
    public const string HistoricalAssignmentVisibilityMode = "admin_all";
    public const string ManagerNotificationMode = "optional";
    public const int RetrainingRecommendationThreshold = 3;
    public const string IncidentVisibilityMode = "management";
    public const string RequiredProfileSectionsCsv = "identity,work";
    public const string OptionalProfileSectionsCsv = "contact,emergency,address,photo";
    public const string DigestFrequency = "daily";
    public const string SnapshotLabelPolicy = "display_label_with_status";
}
