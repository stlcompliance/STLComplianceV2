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
    public string? ParentCustomerId { get; set; }
    public string? PrimaryContactId { get; set; }
    public string? PrimaryBillingAddressId { get; set; }
    public string? PrimaryShippingAddressId { get; set; }
    public string? PrimaryServiceAddressId { get; set; }
    public string? AccountOwnerPersonId { get; set; }
    public string? AssignedTeamId { get; set; }
    public DateTimeOffset? CustomerSinceDate { get; set; }
    public string SourceKey { get; set; } = "manual";
    public string Notes { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
    public string HoldStatusKey { get; set; } = "clear";
    public string RiskRatingKey { get; set; } = "low";
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
    public string? DeliveryInstructions { get; set; }
    public bool AppointmentRequired { get; set; }
    public string? ReceivingHours { get; set; }
    public string? DockDoorNotes { get; set; }
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
    public string Message { get; set; } = string.Empty;
    public string SourceProductKey { get; set; } = "customarr";
    public string? ActorPersonId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
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
