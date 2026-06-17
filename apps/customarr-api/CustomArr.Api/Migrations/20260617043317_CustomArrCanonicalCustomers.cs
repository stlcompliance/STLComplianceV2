using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class CustomArrCanonicalCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customarr_customers",
                columns: table => new
                {
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DbaName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomerTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ParentCustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PrimaryContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PrimaryBillingAddressId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PrimaryShippingAddressId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PrimaryServiceAddressId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AccountOwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AssignedTeamId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CustomerSinceDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SourceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    HoldStatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RiskRatingKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PortalEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PortalDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AllowPortalOrderCreate = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPortalDocumentUpload = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPortalStatusView = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultPortalContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PortalInviteStatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PortalTermsAcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PortalTermsAcceptedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PortalNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DefaultOrderTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DefaultServiceLevelKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DefaultPickupAddressId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DefaultDeliveryAddressId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DefaultContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RequiresAppointment = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresProofOfDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresCustomerReference = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerReferenceLabel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DefaultInstructions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RestrictedServiceNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NotificationPreferenceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OrderConfirmationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RowVersion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "customarr_idempotency_records",
                columns: table => new
                {
                    IdempotencyRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_idempotency_records", x => x.IdempotencyRecordId);
                });

            migrationBuilder.CreateTable(
                name: "customarr_portal_submissions",
                columns: table => new
                {
                    SubmissionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedEventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerNumberSnapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomerAddressId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerAddressSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequestType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RequestedWindowStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedWindowEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PromisedWindowStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PromisedWindowEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FulfillmentProductKeys = table.Column<string[]>(type: "text[]", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OrdArrOrderId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OrdArrOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_portal_submissions", x => x.SubmissionId);
                });

            migrationBuilder.CreateTable(
                name: "platform_metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_metadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_activity",
                columns: table => new
                {
                    ActivityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_activity", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_activity_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_addresses",
                columns: table => new
                {
                    AddressId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AddressTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LocationName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AttentionTo = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Line1 = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Line2 = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    City = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StateProvince = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Timezone = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DeliveryInstructions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AppointmentRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivingHours = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DockDoorNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AccessRestrictions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDefaultBilling = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefaultShipping = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefaultService = table.Column<bool>(type: "boolean", nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_addresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_addresses_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_billing_profiles",
                columns: table => new
                {
                    BillingProfileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BillingContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BillingAddressId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PaymentTermsKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InvoiceDeliveryMethodKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BillingEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PurchaseOrderRequired = table.Column<bool>(type: "boolean", nullable: false),
                    TaxExempt = table.Column<bool>(type: "boolean", nullable: false),
                    TaxExemptionRecordId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CreditStatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ExternalAccountingCustomerRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_billing_profiles", x => x.BillingProfileId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_billing_profiles_customarr_customers_Cus~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_contacts",
                columns: table => new
                {
                    ContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Department = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MobilePhone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PhoneExtension = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PreferredContactMethodKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreferredLanguageKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Timezone = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsBillingContact = table.Column<bool>(type: "boolean", nullable: false),
                    IsOrderingContact = table.Column<bool>(type: "boolean", nullable: false),
                    IsShippingContact = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmergencyContact = table.Column<bool>(type: "boolean", nullable: false),
                    PortalAccessEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PortalRoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_contacts", x => x.ContactId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_contacts_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_custom_field_values",
                columns: table => new
                {
                    FieldValueId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldDefinitionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ValueText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ValueNumber = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ValueBoolean = table.Column<bool>(type: "boolean", nullable: true),
                    ValueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValueOptionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SourceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_custom_field_values", x => x.FieldValueId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_custom_field_values_customarr_customers_~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_external_refs",
                columns: table => new
                {
                    ExternalRefId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SystemKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalCode = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SyncStatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_external_refs", x => x.ExternalRefId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_external_refs_customarr_customers_Custom~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_identifiers",
                columns: table => new
                {
                    IdentifierId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdentifierTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdentifierValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    JurisdictionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IssuingAuthority = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerificationStatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordArrDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_identifiers", x => x.IdentifierId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_identifiers_customarr_customers_Customer~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_relationships",
                columns: table => new
                {
                    RelationshipId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedCustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelationshipTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_relationships", x => x.RelationshipId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_relationships_customarr_customers_Custom~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_requirements",
                columns: table => new
                {
                    RequirementId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequirementTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequirementName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RequiredBeforeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordArrDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComplianceCoreRuleRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WaiverReason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    WaivedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerTeam = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_requirements", x => x.RequirementId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_requirements_customarr_customers_Custome~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_activity_CustomerId",
                table: "customarr_customer_activity",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_activity_TenantId_CustomerId_OccurredAt",
                table: "customarr_customer_activity",
                columns: new[] { "TenantId", "CustomerId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_addresses_CustomerId",
                table: "customarr_customer_addresses",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_addresses_TenantId_AddressTypeKey",
                table: "customarr_customer_addresses",
                columns: new[] { "TenantId", "AddressTypeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_addresses_TenantId_CustomerId",
                table: "customarr_customer_addresses",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_billing_profiles_CustomerId",
                table: "customarr_customer_billing_profiles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_billing_profiles_TenantId_CustomerId",
                table: "customarr_customer_billing_profiles",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_contacts_CustomerId",
                table: "customarr_customer_contacts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_contacts_TenantId_CustomerId",
                table: "customarr_customer_contacts",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_contacts_TenantId_Email",
                table: "customarr_customer_contacts",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_custom_field_values_CustomerId",
                table: "customarr_customer_custom_field_values",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_custom_field_values_TenantId_CustomerId",
                table: "customarr_customer_custom_field_values",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_custom_field_values_TenantId_FieldDefini~",
                table: "customarr_customer_custom_field_values",
                columns: new[] { "TenantId", "FieldDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_external_refs_CustomerId",
                table: "customarr_customer_external_refs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_external_refs_TenantId_CustomerId",
                table: "customarr_customer_external_refs",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_external_refs_TenantId_SystemKey_Externa~",
                table: "customarr_customer_external_refs",
                columns: new[] { "TenantId", "SystemKey", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_identifiers_CustomerId",
                table: "customarr_customer_identifiers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_identifiers_TenantId_CustomerId",
                table: "customarr_customer_identifiers",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_identifiers_TenantId_IdentifierTypeKey_I~",
                table: "customarr_customer_identifiers",
                columns: new[] { "TenantId", "IdentifierTypeKey", "IdentifierValue" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_relationships_CustomerId",
                table: "customarr_customer_relationships",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_relationships_TenantId_CustomerId",
                table: "customarr_customer_relationships",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_relationships_TenantId_RelatedCustomerId",
                table: "customarr_customer_relationships",
                columns: new[] { "TenantId", "RelatedCustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_requirements_CustomerId",
                table: "customarr_customer_requirements",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_requirements_TenantId_CustomerId",
                table: "customarr_customer_requirements",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_requirements_TenantId_StatusKey",
                table: "customarr_customer_requirements",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customers_TenantId",
                table: "customarr_customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customers_TenantId_CustomerCode",
                table: "customarr_customers",
                columns: new[] { "TenantId", "CustomerCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customers_TenantId_CustomerNumber",
                table: "customarr_customers",
                columns: new[] { "TenantId", "CustomerNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customers_TenantId_ParentCustomerId",
                table: "customarr_customers",
                columns: new[] { "TenantId", "ParentCustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customers_TenantId_StatusKey",
                table: "customarr_customers",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_idempotency_records_TenantId_OperationKey_Idempot~",
                table: "customarr_idempotency_records",
                columns: new[] { "TenantId", "OperationKey", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_portal_submissions_TenantId_CustomerId",
                table: "customarr_portal_submissions",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_portal_submissions_TenantId_SubmittedAt",
                table: "customarr_portal_submissions",
                columns: new[] { "TenantId", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId",
                table: "platform_metadata",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId_Key",
                table: "platform_metadata",
                columns: new[] { "TenantId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customarr_customer_activity");

            migrationBuilder.DropTable(
                name: "customarr_customer_addresses");

            migrationBuilder.DropTable(
                name: "customarr_customer_billing_profiles");

            migrationBuilder.DropTable(
                name: "customarr_customer_contacts");

            migrationBuilder.DropTable(
                name: "customarr_customer_custom_field_values");

            migrationBuilder.DropTable(
                name: "customarr_customer_external_refs");

            migrationBuilder.DropTable(
                name: "customarr_customer_identifiers");

            migrationBuilder.DropTable(
                name: "customarr_customer_relationships");

            migrationBuilder.DropTable(
                name: "customarr_customer_requirements");

            migrationBuilder.DropTable(
                name: "customarr_idempotency_records");

            migrationBuilder.DropTable(
                name: "customarr_portal_submissions");

            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "customarr_customers");
        }
    }
}
