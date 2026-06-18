namespace CustomArr.Api.Data;

public sealed class CustomArrTenantSettings
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public int SettingsVersion { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedByPersonId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedByPersonId { get; set; }
}

public sealed class CustomArrCustomerNumberingSettings
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Prefix { get; set; } = "CUS";
    public string SequenceName { get; set; } = "customarr_customer";
    public int PaddingLength { get; set; } = 4;
    public int NextNumber { get; set; } = 1001;
    public bool AllowManualOverride { get; set; }
    public bool ManualOverrideRequiresPermission { get; set; } = true;
    public string DisplayFormat { get; set; } = "{prefix}-{number}";
    public string UniquenessScope { get; set; } = "tenant";
}

public sealed class CustomArrCustomerLifecycleStage
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsInitial { get; set; }
    public bool IsActiveCustomerStage { get; set; }
    public bool IsTerminal { get; set; }
    public bool BlocksOrders { get; set; }
    public bool BlocksPortalAccess { get; set; }
    public bool RequiresApprovalToEnter { get; set; }
    public bool RequiresReasonToExit { get; set; }
    public string[] AllowedNextStageKeys { get; set; } = [];
    public string? ColorToken { get; set; }
    public bool IsSystemRequired { get; set; }
}

public sealed class CustomArrCustomerLifecycleTransitionRule
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string FromStageKey { get; set; } = string.Empty;
    public string ToStageKey { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
    public string? RequiredPermission { get; set; }
    public string? RequiredChecklistTemplateKey { get; set; }
    public bool RequiredReason { get; set; }
    public bool BlockIfOpenIssues { get; set; }
    public bool BlockIfExpiredRequiredDocuments { get; set; }
    public bool BlockIfMissingRequiredFields { get; set; }
}

public sealed class CustomArrCustomerClassificationCatalog
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CatalogType { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public string? MetadataKey { get; set; }
    public string? MetadataValue { get; set; }
}

public sealed class CustomArrCustomerRequiredFieldRule
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string? CustomerTypeKey { get; set; }
    public string? LifecycleStageKey { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string RequirementLevel { get; set; } = "optional";
    public string ValidationMessage { get; set; } = string.Empty;
    public bool AppliesToPortal { get; set; }
    public bool AppliesToInternalCreate { get; set; } = true;
    public bool AppliesToInternalEdit { get; set; } = true;
}

public sealed class CustomArrCustomerContactRole
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequiredForActiveCustomer { get; set; }
    public bool RequiresUniquePrimary { get; set; }
    public bool AllowsPortalAccess { get; set; }
    public bool CanReceiveOrderNotifications { get; set; }
    public bool CanReceiveBillingNotifications { get; set; }
    public bool CanReceiveComplianceNotifications { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CustomArrCustomerAddressType
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequiredForActiveCustomer { get; set; }
    public bool RequiresValidation { get; set; }
    public bool RequiresGeocode { get; set; }
    public bool UsableForBilling { get; set; }
    public bool UsableForPickup { get; set; }
    public bool UsableForDelivery { get; set; }
    public bool UsableForService { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CustomArrCustomerOwnerRule
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CustomerTypeKey { get; set; }
    public string? TerritoryKey { get; set; }
    public string? IndustryKey { get; set; }
    public string? SourceKey { get; set; }
    public string DefaultOwnerType { get; set; } = "staffarr_person";
    public string DefaultOwnerRefId { get; set; } = string.Empty;
    public string DefaultOwnerNameSnapshot { get; set; } = string.Empty;
    public bool RequiresOwnerForActiveCustomer { get; set; } = true;
    public bool RequiresApprovalForReassignment { get; set; }
    public string? ApprovalPermission { get; set; }
}

public sealed class CustomArrCustomerOnboardingTemplate
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CustomerTypeKey { get; set; }
    public string? IndustryKey { get; set; }
    public string? PriorityTierKey { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public bool BlocksActivationUntilComplete { get; set; } = true;
    public int SortOrder { get; set; }
}

public sealed class CustomArrCustomerOnboardingChecklistItemTemplate
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ItemType { get; set; } = "task";
    public bool Required { get; set; } = true;
    public int SortOrder { get; set; }
    public string? OwnerType { get; set; }
    public string? OwnerRefId { get; set; }
    public string? OwnerNameSnapshot { get; set; }
    public string? DocumentTypeKey { get; set; }
    public string? ComplianceQuestionnaireKey { get; set; }
    public bool BlocksActivation { get; set; }
    public bool BlocksOrders { get; set; }
    public bool BlocksPortalAccess { get; set; }
}

public sealed class CustomArrCustomerPortalTenantSettings
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public bool PortalEnabled { get; set; }
    public bool InviteOnly { get; set; } = true;
    public bool SelfRegistrationAllowed { get; set; }
    public bool RequireEmailVerification { get; set; } = true;
    public bool RequireInternalApprovalForPortalUsers { get; set; } = true;
    public string[] AllowedEmailDomains { get; set; } = [];
    public string SupportContactName { get; set; } = string.Empty;
    public string SupportContactEmail { get; set; } = string.Empty;
    public string SupportContactPhone { get; set; } = string.Empty;
    public string PortalDisplayName { get; set; } = "Customer portal";
    public string? LogoRecordArrDocumentId { get; set; }
    public bool CanViewProfile { get; set; } = true;
    public bool CanRequestQuote { get; set; } = true;
    public bool CanPlaceOrderRequest { get; set; }
    public bool CanUploadDocuments { get; set; } = true;
    public bool CanSubmitIssue { get; set; } = true;
    public bool CanViewOrderStatus { get; set; } = true;
    public bool CanViewInvoicesSnapshot { get; set; }
    public string DefaultPortalContactRoleKey { get; set; } = "primary";
    public string PortalAdminContactRoleKey { get; set; } = "portal_admin";
}

public sealed class CustomArrCustomerDocumentRequirement
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CustomerTypeKey { get; set; }
    public string? LifecycleStageKey { get; set; }
    public bool Required { get; set; }
    public bool Expires { get; set; }
    public int? ExpirationWarningDays { get; set; }
    public string RecordArrDocumentTypeKey { get; set; } = string.Empty;
    public bool CustomerCanUpload { get; set; }
    public bool VisibleInPortal { get; set; }
    public bool BlocksActivation { get; set; }
    public bool BlocksOrders { get; set; }
    public bool BlocksPortalAccess { get; set; }
}

public sealed class CustomArrCustomerDuplicateDetectionRule
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }
    public string MatchField { get; set; } = string.Empty;
    public string MatchType { get; set; } = "exact";
    public int Weight { get; set; }
    public int AutoBlockThreshold { get; set; } = 90;
    public int ReviewThreshold { get; set; } = 60;
}

public sealed class CustomArrCustomerIntegrationSettings
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string ErpSyncMode { get; set; } = "none";
    public string DefaultConflictResolution { get; set; } = "manual_review";
    public bool EmitEventsForDraftCustomers { get; set; }
    public bool EmitEventsForProspects { get; set; } = true;
    public bool EmitEventsOnlyAfterActivation { get; set; }
    public bool AllowExternalCreate { get; set; }
    public bool AllowExternalUpdate { get; set; }
    public bool RequireReviewForExternalUpdate { get; set; } = true;
}

public sealed class CustomArrCustomerExternalIdSource
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string SourceType { get; set; } = "erp";
    public bool Required { get; set; }
    public bool UniqueWithinTenant { get; set; } = true;
    public bool VisibleInUi { get; set; } = true;
    public bool EditableInUi { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public sealed class CustomArrCustomerNotificationRule
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string RecipientType { get; set; } = "account_owner";
    public string? RecipientRefId { get; set; }
    public string? RecipientNameSnapshot { get; set; }
    public string? CustomerContactRoleKey { get; set; }
    public int DelayMinutes { get; set; }
    public int? EscalationAfterMinutes { get; set; }
    public string? TemplateKey { get; set; }
}

public sealed class CustomArrCustomerCustomFieldDefinition
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public string? AppliesToCustomerTypeKey { get; set; }
    public string? AppliesToLifecycleStageKey { get; set; }
    public bool Required { get; set; }
    public bool VisibleInPortal { get; set; }
    public bool EditableInPortal { get; set; }
    public bool InternalOnly { get; set; } = true;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CustomArrCustomerCustomFieldOption
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CustomArrTenantSettingsAuditEvent
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public int SettingsVersion { get; set; }
    public string Scope { get; set; } = "tenant";
    public string SectionKey { get; set; } = "all";
    public string ChangeSummary { get; set; } = string.Empty;
    public string? ActorPersonId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string SourceProductKey { get; set; } = "customarr";
}
