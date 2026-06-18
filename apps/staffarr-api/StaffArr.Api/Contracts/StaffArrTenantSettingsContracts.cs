namespace StaffArr.Api.Contracts;

public sealed record StaffArrTenantSettingsResponse(
    Guid TenantId,
    PersonDirectorySettingsDto PersonDirectory,
    PersonLifecycleSettingsDto PersonLifecycle,
    OrgStructureSettingsDto OrgStructure,
    LocationHierarchySettingsDto LocationHierarchy,
    RolePermissionSettingsDto RolePermissions,
    TeamAssignmentSettingsDto TeamsAssignments,
    IncidentRoutingSettingsDto Incidents,
    ProfileFieldGovernanceSettingsDto ProfileFieldGovernance,
    NotificationReviewSettingsDto NotificationsReviews,
    DataGovernanceAuditSettingsDto DataGovernanceAudit,
    CrossProductReferenceSettingsDto CrossProductReferences,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertStaffArrTenantSettingsRequest(
    PersonDirectorySettingsDto PersonDirectory,
    PersonLifecycleSettingsDto PersonLifecycle,
    OrgStructureSettingsDto OrgStructure,
    LocationHierarchySettingsDto LocationHierarchy,
    RolePermissionSettingsDto RolePermissions,
    TeamAssignmentSettingsDto TeamsAssignments,
    IncidentRoutingSettingsDto Incidents,
    ProfileFieldGovernanceSettingsDto ProfileFieldGovernance,
    NotificationReviewSettingsDto NotificationsReviews,
    DataGovernanceAuditSettingsDto DataGovernanceAudit,
    CrossProductReferenceSettingsDto CrossProductReferences);

public sealed record PersonDirectorySettingsDto(
    string DisplayNameFormat,
    bool PreferredNameEnabled,
    string EmployeeNumberLabel,
    bool EmployeeNumberRequired,
    string EmployeeNumberUniquenessScope,
    bool ProfilePhotoEnabled,
    string ContactVisibilityMode,
    bool EmergencyContactEnabled,
    bool PersonalAddressEnabled);

public sealed record PersonLifecycleSettingsDto(
    string DefaultPersonStatusOnCreate,
    bool RequireManagerBeforeActivation,
    bool RequirePositionBeforeActivation,
    bool RequireHomeLocationBeforeActivation,
    bool AllowInactivePeopleToBeAssignedWork,
    string RehireMatchBehavior,
    bool DeactivationReasonRequired,
    bool AutoRemoveRolesOnDeactivation,
    bool AutoEndTeamAssignmentsOnDeactivation);

public sealed record OrgStructureSettingsDto(
    string OrgHierarchyMode,
    bool RequireEveryPersonInOrgUnit,
    bool RequireDepartmentUnderSite,
    bool AllowMatrixMembership,
    bool PrimaryAssignmentRequired,
    bool ManagerHierarchyRequired,
    bool AllowSkipLevelManagers,
    bool PreventCircularReporting);

public sealed record LocationHierarchySettingsDto(
    string LocationHierarchyMode,
    bool RequireLocationCode,
    string LocationCodeUniquenessScope,
    bool AllowOperationalLocations,
    bool AllowAddressableBinsShelves,
    bool AllowMobileLocations,
    bool RequireParentLocationExceptRoot,
    string ArchivedLocationAssignmentBehavior);

public sealed record RolePermissionSettingsDto(
    bool RoleAssignmentApprovalRequired,
    bool AllowSelfServiceRoleRequests,
    bool RoleExpirationEnabled,
    int? DefaultRoleGrantDurationDays,
    bool RequireAssignmentReason,
    string PermissionReviewCadence,
    bool AutoRemoveRolesOnInactivePerson,
    bool AllowDirectPermissions,
    bool PreferRolesOverDirectPermissions,
    bool SiteScopedRoleAssignmentsEnabled);

public sealed record TeamAssignmentSettingsDto(
    string TeamMembershipMode,
    bool RequireTeamLead,
    bool AllowTemporaryAssignments,
    int? TemporaryAssignmentMaxDurationDays,
    bool AssignmentEffectiveDatingEnabled,
    string HistoricalAssignmentVisibilityMode,
    bool AllowOpenPositions);

public sealed record IncidentRoutingSettingsDto(
    bool IncidentIntakeEnabled,
    bool RequireIncidentCategory,
    bool RequireInvolvedPerson,
    string ManagerNotificationMode,
    bool TrainArrRoutingEnabled,
    int? RetrainingRecommendationThreshold,
    string IncidentVisibilityMode,
    bool ClosureApprovalRequired);

public sealed record ProfileFieldGovernanceSettingsDto(
    IReadOnlyList<string> RequiredProfileSections,
    IReadOnlyList<string> OptionalProfileSections,
    bool CustomProfileFieldsEnabled,
    bool FieldVisibilityByRoleEnabled,
    bool FieldEditabilityByRoleEnabled,
    bool FieldReviewRequired,
    bool FieldHistoryEnabled);

public sealed record NotificationReviewSettingsDto(
    bool NotifyManagerOnNewPerson,
    bool NotifyOnManagerChange,
    bool NotifyOnRoleGrantRemoval,
    bool NotifyBeforeRoleExpiration,
    bool NotifyOnInactiveAssignmentConflict,
    bool ReviewRemindersEnabled,
    string DigestFrequency);

public sealed record DataGovernanceAuditSettingsDto(
    bool AuditProfileChanges,
    bool AuditRoleChanges,
    bool AuditOrgLocationChanges,
    bool RequireChangeReasonForSensitiveEdits,
    bool SoftArchiveOnly,
    int? RecordRetentionHintDays,
    bool ExportEnabled,
    bool BulkImportEnabled,
    bool BulkImportReviewRequired);

public sealed record CrossProductReferenceSettingsDto(
    bool ExposePeopleReferenceApi,
    bool ExposeLocationReferenceApi,
    bool ExposeOrgUnitReferenceApi,
    bool PublishPersonLifecycleEvents,
    bool PublishOrgLocationEvents,
    bool AllowProductOriginatedPersonProposals,
    bool RequireReviewForProductOriginatedProposals,
    string SnapshotLabelPolicy);
