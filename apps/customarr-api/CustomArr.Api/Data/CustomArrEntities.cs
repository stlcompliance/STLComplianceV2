namespace CustomArr.Api.Data;

public sealed class CustomArrCustomer
{
    public string CustomerId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? DbaName { get; set; }
    public string CustomerTypeKey { get; set; } = "business";
    public string StatusKey { get; set; } = "prospect";
    public string RelationshipRoleKey { get; set; } = "mixed";
    public string AccountClassKey { get; set; } = "standard";
    public string OnboardingStatusKey { get; set; } = "not_started";
    public string ServiceEligibilityStatusKey { get; set; } = "unknown";
    public string ComplianceStatusKey { get; set; } = "unknown";
    public string IndustryKey { get; set; } = string.Empty;
    public string? NaicsCode { get; set; }
    public string? SicCode { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? RegionKey { get; set; }
    public string? MarketKey { get; set; }
    public string? VerticalKey { get; set; }
    public string? RevenueBandKey { get; set; }
    public bool IsStrategicAccount { get; set; }
    public bool IsKeyAccount { get; set; }
    public string? ParentCustomerId { get; set; }
    public string? PrimaryContactId { get; set; }
    public string? PrimaryBillingAddressId { get; set; }
    public string? PrimaryShippingAddressId { get; set; }
    public string? PrimaryServiceAddressId { get; set; }
    public string? AccountOwnerPersonId { get; set; }
    public string? SalesOwnerPersonId { get; set; }
    public string? SupportOwnerPersonId { get; set; }
    public string? CustomerSuccessOwnerPersonId { get; set; }
    public string? AssignedTeamId { get; set; }
    public DateTimeOffset? LeadDate { get; set; }
    public DateTimeOffset? QualifiedDate { get; set; }
    public DateTimeOffset? FirstOrderDate { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    public DateTimeOffset? ChurnedAt { get; set; }
    public DateTimeOffset? CustomerSinceDate { get; set; }
    public string SourceKey { get; set; } = "manual";
    public string Notes { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
    public string HoldStatusKey { get; set; } = "clear";
    public string RiskRatingKey { get; set; } = "low";
    public string HealthScoreKey { get; set; } = "unknown";
    public int? HealthScore { get; set; }
    public string? HealthScoreReason { get; set; }
    public string? RelationshipSummary { get; set; }
    public string? InternalNotes { get; set; }
    public bool PortalEnabled { get; set; }
    public string? PortalDisplayName { get; set; }
    public bool AllowPortalOrderCreate { get; set; }
    public bool AllowPortalDocumentUpload { get; set; }
    public bool AllowPortalStatusView { get; set; }
    public string? DefaultPortalContactId { get; set; }
    public string PortalInviteStatusKey { get; set; } = "not_invited";
    public DateTimeOffset? PortalTermsAcceptedAt { get; set; }
    public string? PortalTermsAcceptedByPersonId { get; set; }
    public string? PortalNotes { get; set; }
    public string? DefaultOrderTypeKey { get; set; }
    public string? DefaultServiceLevelKey { get; set; }
    public string? DefaultPickupAddressId { get; set; }
    public string? DefaultDeliveryAddressId { get; set; }
    public string? DefaultContactId { get; set; }
    public bool RequiresAppointment { get; set; }
    public bool RequiresProofOfDelivery { get; set; }
    public bool RequiresCustomerReference { get; set; }
    public string? CustomerReferenceLabel { get; set; }
    public string? DefaultInstructions { get; set; }
    public string? RestrictedServiceNotes { get; set; }
    public string? NotificationPreferenceKey { get; set; }
    public bool OrderConfirmationRequired { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedByPersonId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedByPersonId { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public string? ArchivedByPersonId { get; set; }
    public long RowVersion { get; set; } = 1;

    public List<CustomArrCustomerContact> Contacts { get; set; } = [];
    public List<CustomArrCustomerAddress> Addresses { get; set; } = [];
    public List<CustomArrCustomerIdentifier> Identifiers { get; set; } = [];
    public List<CustomArrCustomerBillingProfile> BillingProfiles { get; set; } = [];
    public List<CustomArrCustomerRequirement> Requirements { get; set; } = [];
    public List<CustomArrCustomerExternalRef> ExternalRefs { get; set; } = [];
    public List<CustomArrCustomerRelationship> Relationships { get; set; } = [];
    public List<CustomArrCustomerCustomFieldValue> CustomFieldValues { get; set; } = [];
    public List<CustomArrCustomerActivity> Activity { get; set; } = [];
    public List<CustomArrLead> Leads { get; set; } = [];
    public List<CustomArrOpportunity> Opportunities { get; set; } = [];
    public List<CustomArrProposal> Proposals { get; set; } = [];
    public List<CustomArrAgreement> Agreements { get; set; } = [];
    public List<CustomArrCustomerCase> Cases { get; set; } = [];
    public List<CustomArrTask> Tasks { get; set; } = [];
    public List<CustomArrPortalAccessRecord> PortalAccessRecords { get; set; } = [];
    public List<CustomArrCustomerServiceProfile> ServiceProfiles { get; set; } = [];
    public List<CustomArrCustomerOnboarding> OnboardingRecords { get; set; } = [];
    public List<CustomArrCustomerHealthProfile> HealthProfiles { get; set; } = [];
    public List<CustomArrIntegrationReference> IntegrationReferences { get; set; } = [];
}

public sealed class CustomArrCustomerContact
{
    public string ContactId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Department { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? PhoneExtension { get; set; }
    public string PreferredContactMethodKey { get; set; } = "email";
    public string? PreferredLanguageKey { get; set; }
    public string? Timezone { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsBillingContact { get; set; }
    public bool IsOrderingContact { get; set; }
    public bool IsShippingContact { get; set; }
    public bool IsEmergencyContact { get; set; }
    public bool IsDecisionMaker { get; set; }
    public bool CanPlaceOrders { get; set; }
    public bool CanApproveQuotes { get; set; }
    public bool CanSignContracts { get; set; }
    public bool CanSubmitCases { get; set; }
    public bool CanUploadDocuments { get; set; }
    public string[] AuthorizationScopes { get; set; } = [];
    public bool EmailOptIn { get; set; }
    public bool SmsOptIn { get; set; }
    public bool MarketingConsent { get; set; }
    public DateTimeOffset? ConsentCapturedAt { get; set; }
    public string? ConsentLegalBasisKey { get; set; }
    public string? LocationId { get; set; }
    public bool PortalAccessEnabled { get; set; }
    public string? PortalRoleKey { get; set; }
    public string StatusKey { get; set; } = "active";
    public DateTimeOffset? LastVerifiedAt { get; set; }
}

public sealed class CustomArrCustomerAddress
{
    public string AddressId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string AddressTypeKey { get; set; } = "service";
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string? AttentionTo { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "US";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Timezone { get; set; }
    public string? MailingLine1 { get; set; }
    public string? MailingLine2 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingStateProvince { get; set; }
    public string? MailingPostalCode { get; set; }
    public string? MailingCountryCode { get; set; }
    public string? ShippingHours { get; set; }
    public string? AppointmentInstructions { get; set; }
    public bool DropTrailerAllowed { get; set; }
    public string? DriverCheckInRules { get; set; }
    public string? DeliveryInstructions { get; set; }
    public bool AppointmentRequired { get; set; }
    public string? ReceivingHours { get; set; }
    public string? DockDoorNotes { get; set; }
    public string? GateInstructions { get; set; }
    public string? ParkingInstructions { get; set; }
    public string? AfterHoursRules { get; set; }
    public string? TruckSizeRestrictions { get; set; }
    public string? HazmatRestrictions { get; set; }
    public string? PpeRequirements { get; set; }
    public string? TemperatureRules { get; set; }
    public bool LiftgateRequired { get; set; }
    public string[] ServiceAreaKeys { get; set; } = [];
    public string[] EligibleProductKeys { get; set; } = [];
    public string? ComplianceNotes { get; set; }
    public string? AccessRestrictions { get; set; }
    public bool IsDefaultBilling { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultService { get; set; }
    public string StatusKey { get; set; } = "active";
}

public sealed class CustomArrCustomerIdentifier
{
    public string IdentifierId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string IdentifierTypeKey { get; set; } = string.Empty;
    public string IdentifierValue { get; set; } = string.Empty;
    public string? JurisdictionKey { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTimeOffset? EffectiveDate { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public string VerificationStatusKey { get; set; } = "unverified";
    public string? RecordArrDocumentId { get; set; }
}

public sealed class CustomArrCustomerBillingProfile
{
    public string BillingProfileId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? BillingContactId { get; set; }
    public string? BillingAddressId { get; set; }
    public string PaymentTermsKey { get; set; } = "net_30";
    public string InvoiceDeliveryMethodKey { get; set; } = "email";
    public string? BillingEmail { get; set; }
    public bool PurchaseOrderRequired { get; set; }
    public bool TaxExempt { get; set; }
    public string? TaxExemptionRecordId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string CreditStatusKey { get; set; } = "good_standing";
    public decimal? CreditLimit { get; set; }
    public string? ExternalAccountingCustomerRef { get; set; }
}

public sealed class CustomArrCustomerRequirement
{
    public string RequirementId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string RequirementTypeKey { get; set; } = string.Empty;
    public string RequirementName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequiredBeforeKey { get; set; } = "before_order_creation";
    public string? RecordArrDocumentId { get; set; }
    public string? ComplianceCoreRuleRef { get; set; }
    public string StatusKey { get; set; } = "pending_review";
    public DateTimeOffset? EffectiveDate { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public string? ReviewedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? WaiverReason { get; set; }
    public string? WaivedByPersonId { get; set; }
    public string OwnerTeam { get; set; } = string.Empty;
}

public sealed class CustomArrCustomerExternalRef
{
    public string ExternalRefId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string SystemKey { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string? ExternalCode { get; set; }
    public string ExternalEntityType { get; set; } = "customer";
    public string SyncDirectionKey { get; set; } = "bidirectional";
    public DateTimeOffset? LastVerifiedAt { get; set; }
    public DateTimeOffset? LastSyncedAt { get; set; }
    public string SyncStatusKey { get; set; } = "pending";
}

public sealed class CustomArrCustomerRelationship
{
    public string RelationshipId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string RelatedCustomerId { get; set; } = string.Empty;
    public string RelationshipTypeKey { get; set; } = "parent";
    public DateTimeOffset? EffectiveDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

public sealed class CustomArrCustomerCustomFieldValue
{
    public string FieldValueId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string FieldDefinitionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? ValueText { get; set; }
    public decimal? ValueNumber { get; set; }
    public bool? ValueBoolean { get; set; }
    public DateTimeOffset? ValueDate { get; set; }
    public string? ValueOptionKey { get; set; }
    public DateTimeOffset? EffectiveDate { get; set; }
    public string SourceKey { get; set; } = "manual";
    public DateTimeOffset? LastVerifiedAt { get; set; }
    public string? UpdatedByPersonId { get; set; }
}

public sealed class CustomArrCustomerActivity
{
    public string ActivityId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string ActivityTypeKey { get; set; } = "note";
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string SourceProductKey { get; set; } = "customarr";
    public string? SourceObjectRef { get; set; }
    public string? ContactId { get; set; }
    public string? CustomerLocationId { get; set; }
    public string DirectionKey { get; set; } = "internal";
    public string VisibilityKey { get; set; } = "internal";
    public string[] RelatedObjectRefs { get; set; } = [];
    public string[] RecordRefs { get; set; } = [];
    public string? ActorPersonId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CustomArrLead
{
    public string LeadId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string LeadNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string SourceKey { get; set; } = "manual";
    public string StatusKey { get; set; } = "new";
    public int? FitScore { get; set; }
    public string? NeedSummary { get; set; }
    public string? BudgetSummary { get; set; }
    public string? TimingSummary { get; set; }
    public string? AuthoritySummary { get; set; }
    public string? ServiceInterest { get; set; }
    public string? OwnerPersonId { get; set; }
    public string? AssignedTeamId { get; set; }
    public DateTimeOffset? NextFollowUpAt { get; set; }
    public string? ConvertedCustomerId { get; set; }
    public string? ConvertedContactId { get; set; }
    public string? ConvertedOpportunityId { get; set; }
    public string? DisqualificationReason { get; set; }
    public DateTimeOffset? ConvertedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrOpportunity
{
    public string OpportunityId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string OpportunityNumber { get; set; } = string.Empty;
    public string? LeadId { get; set; }
    public string? CustomerId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public string StageKey { get; set; } = "discovery";
    public int ProbabilityPercent { get; set; }
    public string ForecastCategoryKey { get; set; } = "pipeline";
    public DateTimeOffset? ExpectedCloseDate { get; set; }
    public decimal? EstimatedRevenue { get; set; }
    public decimal? EstimatedMargin { get; set; }
    public decimal? RecurringRevenue { get; set; }
    public decimal? OneTimeRevenue { get; set; }
    public string[] ServiceInterestKeys { get; set; } = [];
    public string? ScopeSummary { get; set; }
    public string? PrimaryContactId { get; set; }
    public string[] StakeholderContactIds { get; set; } = [];
    public string? Competitor { get; set; }
    public string? IncumbentProvider { get; set; }
    public string? WinLossReason { get; set; }
    public string? NextStep { get; set; }
    public DateTimeOffset? NextFollowUpAt { get; set; }
    public string StatusKey { get; set; } = "open";
    public string? OutcomeKey { get; set; }
    public string? HandoffProductKey { get; set; }
    public string? HandoffObjectRef { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrProposal
{
    public string ProposalId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string ProposalNumber { get; set; } = string.Empty;
    public string? OpportunityId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public int VersionNumber { get; set; } = 1;
    public string StatusKey { get; set; } = "draft";
    public string ScopeSummary { get; set; } = string.Empty;
    public string? PricingSnapshotJson { get; set; }
    public string? TermsSnapshot { get; set; }
    public string? SlaSnapshot { get; set; }
    public string ApprovalStatusKey { get; set; } = "not_required";
    public string? ApprovedByPersonId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string CustomerResponseKey { get; set; } = "pending";
    public string? ExternalAccountingQuoteRef { get; set; }
    public string? CreatedOrdArrOrderRef { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrAgreement
{
    public string AgreementId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string AgreementNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string AgreementTypeKey { get; set; } = "master_service_agreement";
    public string Title { get; set; } = string.Empty;
    public string StatusKey { get; set; } = "draft";
    public DateTimeOffset? EffectiveDate { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public DateTimeOffset? RenewalDate { get; set; }
    public DateTimeOffset? TerminationDate { get; set; }
    public string? ScopeSummary { get; set; }
    public string? TermsSummary { get; set; }
    public string[] CoveredProductKeys { get; set; } = [];
    public string[] CoveredLocationIds { get; set; } = [];
    public string[] RequirementRefs { get; set; } = [];
    public string[] RecordRefs { get; set; } = [];
    public string? OwnerPersonId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrCustomerCase
{
    public string CaseId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? ContactId { get; set; }
    public string? CustomerLocationId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceKey { get; set; } = "internal";
    public string PriorityKey { get; set; } = "normal";
    public string SeverityKey { get; set; } = "medium";
    public string StatusKey { get; set; } = "new";
    public DateTimeOffset? FirstResponseDueAt { get; set; }
    public DateTimeOffset? ResolutionDueAt { get; set; }
    public bool SlaBreached { get; set; }
    public string? SupportOwnerPersonId { get; set; }
    public string? EscalationOwnerPersonId { get; set; }
    public string? OwningProductKey { get; set; }
    public string? OwningProductIssueRef { get; set; }
    public string? RootCauseCategoryKey { get; set; }
    public string? ResolutionSummary { get; set; }
    public string? CustomerSatisfactionKey { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrTask
{
    public string TaskId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string TaskNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? RelatedObjectType { get; set; }
    public string? RelatedObjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerPersonId { get; set; } = string.Empty;
    public DateTimeOffset? DueAt { get; set; }
    public string PriorityKey { get; set; } = "normal";
    public string StatusKey { get; set; } = "open";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class CustomArrPortalAccessRecord
{
    public string PortalAccessId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string ContactId { get; set; } = string.Empty;
    public string? NexArrExternalIdentityRef { get; set; }
    public string PortalRoleKey { get; set; } = "customer_viewer";
    public string StatusKey { get; set; } = "pending";
    public string[] AllowedLocationIds { get; set; } = [];
    public string[] AuthorizationRefs { get; set; } = [];
    public DateTimeOffset? InvitedAt { get; set; }
    public string? InvitedByPersonId { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokeReason { get; set; }
    public DateTimeOffset? LastAccessSnapshotAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrCustomerServiceProfile
{
    public string ServiceProfileId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? CustomerLocationId { get; set; }
    public string StatusKey { get; set; } = "draft";
    public string ServiceEligibilityStatusKey { get; set; } = "unknown";
    public string[] AllowedProductKeys { get; set; } = [];
    public string[] BlockedProductKeys { get; set; } = [];
    public string[] AllowedWorkflowKeys { get; set; } = [];
    public string[] BlockedWorkflowKeys { get; set; } = [];
    public string[] RequiredApprovalTypes { get; set; } = [];
    public string[] RequiredRequirementRefs { get; set; } = [];
    public string[] ActiveHoldRefs { get; set; } = [];
    public string? Restrictions { get; set; }
    public string ServiceLevelKey { get; set; } = "standard";
    public string ExternalCreditStatusSnapshotKey { get; set; } = "not_checked";
    public string? LastEligibilityReason { get; set; }
    public DateTimeOffset? LastEligibilityCalculatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrEligibilityCheck
{
    public string EligibilityCheckId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? CustomerLocationId { get; set; }
    public string? CustomerContactId { get; set; }
    public string WorkflowKey { get; set; } = string.Empty;
    public string SourceProductKey { get; set; } = "customarr";
    public string? SourceObjectRef { get; set; }
    public string ResultKey { get; set; } = "unknown";
    public string Explanation { get; set; } = string.Empty;
    public string[] Blockers { get; set; } = [];
    public string[] Warnings { get; set; } = [];
    public DateTimeOffset CheckedAt { get; set; }
    public string? ActorPersonId { get; set; }
}

public sealed class CustomArrCustomerOnboarding
{
    public string OnboardingId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string OnboardingNumber { get; set; } = string.Empty;
    public string OnboardingTypeKey { get; set; } = "new_customer";
    public string StatusKey { get; set; } = "draft";
    public string? OwnerPersonId { get; set; }
    public DateTimeOffset? LaunchDate { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string[] Blockers { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<CustomArrCustomerOnboardingChecklistItem> ChecklistItems { get; set; } = [];
}

public sealed class CustomArrCustomerOnboardingChecklistItem
{
    public string ChecklistItemId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string OnboardingId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ItemTypeKey { get; set; } = "custom";
    public bool Required { get; set; } = true;
    public string OwnerProductKey { get; set; } = "customarr";
    public string StatusKey { get; set; } = "not_started";
    public string[] EvidenceRecordRefs { get; set; } = [];
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class CustomArrCustomerHealthProfile
{
    public string HealthProfileId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string HealthStatusKey { get; set; } = "unknown";
    public int? Score { get; set; }
    public string? ScoreReason { get; set; }
    public int OpenCaseCount { get; set; }
    public int SlaBreachCount { get; set; }
    public int ActiveContactCount { get; set; }
    public int ActiveLocationCount { get; set; }
    public decimal? RevenueSnapshot { get; set; }
    public string ChurnRiskKey { get; set; } = "unknown";
    public string PaymentRiskKey { get; set; } = "unknown";
    public string? ExpansionSummary { get; set; }
    public DateTimeOffset? LastCheckInAt { get; set; }
    public DateTimeOffset? NextBusinessReviewAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrImportBatch
{
    public string ImportBatchId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string SourceKey { get; set; } = "manual";
    public string SourceFileName { get; set; } = string.Empty;
    public string ImporterPersonId { get; set; } = string.Empty;
    public string StatusKey { get; set; } = "staged";
    public int TotalRows { get; set; }
    public int AcceptedRows { get; set; }
    public int RejectedRows { get; set; }
    public string? MappingSummaryJson { get; set; }
    public string[] ValidationErrors { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrDedupeCandidate
{
    public string DedupeCandidateId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string? ImportBatchId { get; set; }
    public string CandidateTypeKey { get; set; } = "customer";
    public string SourceRecordRef { get; set; } = string.Empty;
    public string? MatchedCustomerId { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string StatusKey { get; set; } = "needs_review";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrMergeRecord
{
    public string MergeRecordId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string SurvivorCustomerId { get; set; } = string.Empty;
    public string[] MergedCustomerIds { get; set; } = [];
    public string MergeReason { get; set; } = string.Empty;
    public string MergeStrategyKey { get; set; } = "manual_review";
    public string StatusKey { get; set; } = "proposed";
    public string? FieldResolutionSummary { get; set; }
    public string? ProposedByPersonId { get; set; }
    public string? ApprovedByPersonId { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrIntegrationReference
{
    public string IntegrationReferenceId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? CustomerLocationId { get; set; }
    public string? CustomerContactId { get; set; }
    public string RelatedEntityType { get; set; } = "customer";
    public string RelatedEntityId { get; set; } = string.Empty;
    public string ExternalSystemKey { get; set; } = string.Empty;
    public string ExternalEntityType { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string? ExternalDisplayName { get; set; }
    public string SyncDirectionKey { get; set; } = "bidirectional";
    public string StatusKey { get; set; } = "active";
    public DateTimeOffset? LastVerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CustomArrPortalSubmission
{
    public string SubmissionId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Status { get; set; } = "created";
    public string CreatedEventType { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerNumberSnapshot { get; set; } = string.Empty;
    public string CustomerNameSnapshot { get; set; } = string.Empty;
    public string? CustomerAddressId { get; set; }
    public string? CustomerAddressSnapshot { get; set; }
    public string RequestType { get; set; } = "customer_order";
    public string OwnerPersonId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTimeOffset? RequestedWindowStart { get; set; }
    public DateTimeOffset? RequestedWindowEnd { get; set; }
    public DateTimeOffset? PromisedWindowStart { get; set; }
    public DateTimeOffset? PromisedWindowEnd { get; set; }
    public string[] FulfillmentProductKeys { get; set; } = [];
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? OrdArrOrderId { get; set; }
    public string? OrdArrOrderNumber { get; set; }
}

public sealed class CustomArrIdempotencyRecord
{
    public string IdempotencyRecordId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string OperationKey { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
