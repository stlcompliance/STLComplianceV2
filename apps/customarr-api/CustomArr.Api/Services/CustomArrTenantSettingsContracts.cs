namespace CustomArr.Api.Services;

public sealed record CustomArrTenantSettingsResponse(
    string Scope,
    int SettingsVersion,
    bool IsActive,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset UpdatedAt,
    CustomerNumberingSettings Numbering,
    IReadOnlyList<CustomerLifecycleStageItem> LifecycleStages,
    IReadOnlyList<CustomerLifecycleTransitionRuleItem> TransitionRules,
    IReadOnlyList<CustomerClassificationCatalogItem> ClassificationCatalogs,
    IReadOnlyList<CustomerRequiredFieldRuleItem> RequiredFieldRules,
    IReadOnlyList<CustomerContactRoleItem> ContactRoles,
    IReadOnlyList<CustomerAddressTypeItem> AddressTypes,
    IReadOnlyList<CustomerOwnerRuleItem> OwnerRules,
    IReadOnlyList<CustomerOnboardingTemplateItem> OnboardingTemplates,
    IReadOnlyList<CustomerOnboardingChecklistItemTemplateItem> OnboardingChecklistItems,
    CustomerPortalTenantSettingsItem PortalSettings,
    IReadOnlyList<CustomerDocumentRequirementItem> DocumentRequirements,
    IReadOnlyList<CustomerDuplicateDetectionRuleItem> DuplicateDetectionRules,
    CustomerIntegrationSettingsItem IntegrationSettings,
    IReadOnlyList<CustomerExternalIdSourceItem> ExternalIdSources,
    IReadOnlyList<CustomerNotificationRuleItem> NotificationRules,
    IReadOnlyList<CustomerCustomFieldDefinitionItem> CustomFieldDefinitions,
    IReadOnlyList<CustomerCustomFieldOptionItem> CustomFieldOptions,
    IReadOnlyList<CustomArrSettingsWarning> Warnings);

public sealed record CustomArrTenantSettingsUpdateRequest(
    CustomerNumberingSettings Numbering,
    IReadOnlyList<CustomerLifecycleStageItem> LifecycleStages,
    IReadOnlyList<CustomerLifecycleTransitionRuleItem> TransitionRules,
    IReadOnlyList<CustomerClassificationCatalogItem> ClassificationCatalogs,
    IReadOnlyList<CustomerRequiredFieldRuleItem> RequiredFieldRules,
    IReadOnlyList<CustomerContactRoleItem> ContactRoles,
    IReadOnlyList<CustomerAddressTypeItem> AddressTypes,
    IReadOnlyList<CustomerOwnerRuleItem> OwnerRules,
    IReadOnlyList<CustomerOnboardingTemplateItem> OnboardingTemplates,
    IReadOnlyList<CustomerOnboardingChecklistItemTemplateItem> OnboardingChecklistItems,
    CustomerPortalTenantSettingsItem PortalSettings,
    IReadOnlyList<CustomerDocumentRequirementItem> DocumentRequirements,
    IReadOnlyList<CustomerDuplicateDetectionRuleItem> DuplicateDetectionRules,
    CustomerIntegrationSettingsItem IntegrationSettings,
    IReadOnlyList<CustomerExternalIdSourceItem> ExternalIdSources,
    IReadOnlyList<CustomerNotificationRuleItem> NotificationRules,
    IReadOnlyList<CustomerCustomFieldDefinitionItem> CustomFieldDefinitions,
    IReadOnlyList<CustomerCustomFieldOptionItem> CustomFieldOptions);

public sealed record CustomerNumberingSettings(
    string Prefix,
    string SequenceName,
    int PaddingLength,
    int NextNumber,
    bool AllowManualOverride,
    bool ManualOverrideRequiresPermission,
    string DisplayFormat,
    string UniquenessScope,
    string Preview = "");

public sealed record CustomerLifecycleStageItem(
    string Key,
    string Label,
    string Description,
    int SortOrder,
    bool IsInitial,
    bool IsActiveCustomerStage,
    bool IsTerminal,
    bool BlocksOrders,
    bool BlocksPortalAccess,
    bool RequiresApprovalToEnter,
    bool RequiresReasonToExit,
    IReadOnlyList<string> AllowedNextStageKeys,
    string? ColorToken,
    bool IsSystemRequired);

public sealed record CustomerLifecycleTransitionRuleItem(
    string FromStageKey,
    string ToStageKey,
    bool RequiresApproval,
    string? RequiredPermission,
    string? RequiredChecklistTemplateKey,
    bool RequiredReason,
    bool BlockIfOpenIssues,
    bool BlockIfExpiredRequiredDocuments,
    bool BlockIfMissingRequiredFields);

public sealed record CustomerClassificationCatalogItem(
    string CatalogType,
    string Key,
    string Label,
    string Description,
    int SortOrder,
    bool IsActive,
    bool IsDefault,
    string? MetadataKey,
    string? MetadataValue);

public sealed record CustomerRequiredFieldRuleItem(
    string? CustomerTypeKey,
    string? LifecycleStageKey,
    string FieldKey,
    string RequirementLevel,
    string ValidationMessage,
    bool AppliesToPortal,
    bool AppliesToInternalCreate,
    bool AppliesToInternalEdit);

public sealed record CustomerContactRoleItem(
    string Key,
    string Label,
    string Description,
    bool IsRequiredForActiveCustomer,
    bool RequiresUniquePrimary,
    bool AllowsPortalAccess,
    bool CanReceiveOrderNotifications,
    bool CanReceiveBillingNotifications,
    bool CanReceiveComplianceNotifications,
    int SortOrder,
    bool IsActive);

public sealed record CustomerAddressTypeItem(
    string Key,
    string Label,
    string Description,
    bool IsRequiredForActiveCustomer,
    bool RequiresValidation,
    bool RequiresGeocode,
    bool UsableForBilling,
    bool UsableForPickup,
    bool UsableForDelivery,
    bool UsableForService,
    int SortOrder,
    bool IsActive);

public sealed record CustomerOwnerRuleItem(
    string RuleName,
    int Priority,
    bool IsActive,
    string? CustomerTypeKey,
    string? TerritoryKey,
    string? IndustryKey,
    string? SourceKey,
    string DefaultOwnerType,
    string DefaultOwnerRefId,
    string DefaultOwnerNameSnapshot,
    bool RequiresOwnerForActiveCustomer,
    bool RequiresApprovalForReassignment,
    string? ApprovalPermission);

public sealed record CustomerOnboardingTemplateItem(
    string Key,
    string Label,
    string Description,
    string? CustomerTypeKey,
    string? IndustryKey,
    string? PriorityTierKey,
    bool IsDefault,
    bool IsActive,
    bool BlocksActivationUntilComplete,
    int SortOrder);

public sealed record CustomerOnboardingChecklistItemTemplateItem(
    string TemplateKey,
    string Key,
    string Label,
    string Description,
    string ItemType,
    bool Required,
    int SortOrder,
    string? OwnerType,
    string? OwnerRefId,
    string? OwnerNameSnapshot,
    string? DocumentTypeKey,
    string? ComplianceQuestionnaireKey,
    bool BlocksActivation,
    bool BlocksOrders,
    bool BlocksPortalAccess);

public sealed record CustomerPortalTenantSettingsItem(
    bool PortalEnabled,
    bool InviteOnly,
    bool SelfRegistrationAllowed,
    bool RequireEmailVerification,
    bool RequireInternalApprovalForPortalUsers,
    IReadOnlyList<string> AllowedEmailDomains,
    string SupportContactName,
    string SupportContactEmail,
    string SupportContactPhone,
    string PortalDisplayName,
    string? LogoRecordArrDocumentId,
    CustomerPortalActionFlags AllowedActions,
    string DefaultPortalContactRoleKey,
    string PortalAdminContactRoleKey);

public sealed record CustomerPortalActionFlags(
    bool CanViewProfile,
    bool CanRequestQuote,
    bool CanPlaceOrderRequest,
    bool CanUploadDocuments,
    bool CanSubmitIssue,
    bool CanViewOrderStatus,
    bool CanViewInvoicesSnapshot);

public sealed record CustomerDocumentRequirementItem(
    string Key,
    string Label,
    string Description,
    string? CustomerTypeKey,
    string? LifecycleStageKey,
    bool Required,
    bool Expires,
    int? ExpirationWarningDays,
    string RecordArrDocumentTypeKey,
    bool CustomerCanUpload,
    bool VisibleInPortal,
    bool BlocksActivation,
    bool BlocksOrders,
    bool BlocksPortalAccess);

public sealed record CustomerDuplicateDetectionRuleItem(
    string Key,
    string Label,
    bool IsActive,
    int Priority,
    string MatchField,
    string MatchType,
    int Weight,
    int AutoBlockThreshold,
    int ReviewThreshold);

public sealed record CustomerIntegrationSettingsItem(
    string ErpSyncMode,
    string DefaultConflictResolution,
    bool EmitEventsForDraftCustomers,
    bool EmitEventsForProspects,
    bool EmitEventsOnlyAfterActivation,
    bool AllowExternalCreate,
    bool AllowExternalUpdate,
    bool RequireReviewForExternalUpdate);

public sealed record CustomerExternalIdSourceItem(
    string Key,
    string Label,
    string SourceType,
    bool Required,
    bool UniqueWithinTenant,
    bool VisibleInUi,
    bool EditableInUi,
    bool IsActive);

public sealed record CustomerNotificationRuleItem(
    string Key,
    string Label,
    string EventType,
    bool IsActive,
    string RecipientType,
    string? RecipientRefId,
    string? RecipientNameSnapshot,
    string? CustomerContactRoleKey,
    int DelayMinutes,
    int? EscalationAfterMinutes,
    string? TemplateKey);

public sealed record CustomerCustomFieldDefinitionItem(
    string Key,
    string Label,
    string Description,
    string FieldType,
    string? AppliesToCustomerTypeKey,
    string? AppliesToLifecycleStageKey,
    bool Required,
    bool VisibleInPortal,
    bool EditableInPortal,
    bool InternalOnly,
    int SortOrder,
    bool IsActive);

public sealed record CustomerCustomFieldOptionItem(
    string FieldKey,
    string Key,
    string Label,
    int SortOrder,
    bool IsActive);

public sealed record CustomArrSettingsWarning(string Key, string Message);

public sealed record CustomArrCustomerCreateMetadataResponse(
    string CustomerNumberPreview,
    string InitialLifecycleStageKey,
    IReadOnlyList<CustomerLifecycleStageItem> LifecycleStages,
    IReadOnlyList<CustomerClassificationCatalogItem> ClassificationCatalogs,
    IReadOnlyList<CustomerContactRoleItem> ContactRoles,
    IReadOnlyList<CustomerAddressTypeItem> AddressTypes,
    IReadOnlyList<CustomerRequiredFieldRuleItem> RequiredFieldRules,
    IReadOnlyList<CustomerOnboardingTemplateItem> OnboardingTemplates,
    IReadOnlyList<CustomerDocumentRequirementItem> DocumentRequirements,
    IReadOnlyList<CustomerCustomFieldDefinitionItem> CustomFieldDefinitions,
    IReadOnlyList<CustomerOwnerRuleItem> OwnerRules);

public sealed record CustomArrCustomerValidationRequest(
    string? CustomerId,
    string? LegalName,
    string? DbaName,
    string? DisplayName,
    string? LifecycleStageKey,
    string? CustomerTypeKey,
    string? IndustryKey,
    string? PriorityTierKey,
    string? ServiceModelKey,
    string? TerritoryKey,
    string? PaymentTermsKey,
    string? AccountOwnerRefId,
    string? CreditStatusKey,
    string? OnboardingTemplateKey,
    string? ExternalId,
    string? PrimaryEmail,
    string? PrimaryPhone,
    bool HasPrimaryContact,
    bool HasBillingAddress,
    bool HasValidAddress);

public sealed record CustomArrCustomerValidationResponse(
    bool IsValid,
    IReadOnlyList<CustomArrValidationError> Errors);

public sealed record CustomArrValidationError(string FieldKey, string Message);

public sealed record CustomArrDuplicateCheckRequest(
    string? LegalName,
    string? DbaName,
    string? PrimaryEmail,
    string? PrimaryPhone,
    string? TaxId,
    string? ExternalId,
    string? AddressLine1,
    string? PostalCode,
    string? WebsiteUrl);

public sealed record CustomArrDuplicateCheckResponse(
    string Recommendation,
    IReadOnlyList<CustomArrDuplicateCandidateResponse> Candidates);

public sealed record CustomArrDuplicateCandidateResponse(
    string CustomerId,
    string CustomerNumber,
    string DisplayName,
    int Score,
    string Recommendation,
    IReadOnlyList<string> Reasons);

public sealed record CustomArrStageTransitionPreviewRequest(
    string? FromStageKey,
    string ToStageKey,
    string? Reason);

public sealed record CustomArrStageTransitionPreviewResponse(
    bool Allowed,
    string FromStageKey,
    string ToStageKey,
    IReadOnlyList<string> Blockers,
    IReadOnlyList<string> Warnings,
    bool TargetBlocksOrders,
    bool TargetBlocksPortalAccess);

public static class CustomArrTenantSettingsResponseExtensions
{
    public static CustomArrTenantSettingsUpdateRequest ToUpdateRequest(this CustomArrTenantSettingsResponse settings) =>
        new(
            settings.Numbering,
            settings.LifecycleStages,
            settings.TransitionRules,
            settings.ClassificationCatalogs,
            settings.RequiredFieldRules,
            settings.ContactRoles,
            settings.AddressTypes,
            settings.OwnerRules,
            settings.OnboardingTemplates,
            settings.OnboardingChecklistItems,
            settings.PortalSettings,
            settings.DocumentRequirements,
            settings.DuplicateDetectionRules,
            settings.IntegrationSettings,
            settings.ExternalIdSources,
            settings.NotificationRules,
            settings.CustomFieldDefinitions,
            settings.CustomFieldOptions);
}
