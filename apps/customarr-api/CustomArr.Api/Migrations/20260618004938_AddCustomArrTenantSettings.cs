using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomArrTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customarr_customer_address_types",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsRequiredForActiveCustomer = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresValidation = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresGeocode = table.Column<bool>(type: "boolean", nullable: false),
                    UsableForBilling = table.Column<bool>(type: "boolean", nullable: false),
                    UsableForPickup = table.Column<bool>(type: "boolean", nullable: false),
                    UsableForDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    UsableForService = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_address_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_classification_catalogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    MetadataKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MetadataValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_classification_catalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_contact_roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsRequiredForActiveCustomer = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresUniquePrimary = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsPortalAccess = table.Column<bool>(type: "boolean", nullable: false),
                    CanReceiveOrderNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    CanReceiveBillingNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    CanReceiveComplianceNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_contact_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_custom_field_definitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FieldType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AppliesToCustomerTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AppliesToLifecycleStageKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    VisibleInPortal = table.Column<bool>(type: "boolean", nullable: false),
                    EditableInPortal = table.Column<bool>(type: "boolean", nullable: false),
                    InternalOnly = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_custom_field_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_custom_field_options",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_custom_field_options", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_document_requirements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CustomerTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LifecycleStageKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Expires = table.Column<bool>(type: "boolean", nullable: false),
                    ExpirationWarningDays = table.Column<int>(type: "integer", nullable: true),
                    RecordArrDocumentTypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CustomerCanUpload = table.Column<bool>(type: "boolean", nullable: false),
                    VisibleInPortal = table.Column<bool>(type: "boolean", nullable: false),
                    BlocksActivation = table.Column<bool>(type: "boolean", nullable: false),
                    BlocksOrders = table.Column<bool>(type: "boolean", nullable: false),
                    BlocksPortalAccess = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_document_requirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_duplicate_detection_rules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    MatchField = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MatchType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    AutoBlockThreshold = table.Column<int>(type: "integer", nullable: false),
                    ReviewThreshold = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_duplicate_detection_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_external_id_sources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    UniqueWithinTenant = table.Column<bool>(type: "boolean", nullable: false),
                    VisibleInUi = table.Column<bool>(type: "boolean", nullable: false),
                    EditableInUi = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_external_id_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_integration_settings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErpSyncMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DefaultConflictResolution = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmitEventsForDraftCustomers = table.Column<bool>(type: "boolean", nullable: false),
                    EmitEventsForProspects = table.Column<bool>(type: "boolean", nullable: false),
                    EmitEventsOnlyAfterActivation = table.Column<bool>(type: "boolean", nullable: false),
                    AllowExternalCreate = table.Column<bool>(type: "boolean", nullable: false),
                    AllowExternalUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    RequireReviewForExternalUpdate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_integration_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_lifecycle_stages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsInitial = table.Column<bool>(type: "boolean", nullable: false),
                    IsActiveCustomerStage = table.Column<bool>(type: "boolean", nullable: false),
                    IsTerminal = table.Column<bool>(type: "boolean", nullable: false),
                    BlocksOrders = table.Column<bool>(type: "boolean", nullable: false),
                    BlocksPortalAccess = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApprovalToEnter = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresReasonToExit = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedNextStageKeys = table.Column<string[]>(type: "text[]", nullable: false),
                    ColorToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsSystemRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_lifecycle_stages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_lifecycle_transition_rules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStageKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ToStageKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredPermission = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RequiredChecklistTemplateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RequiredReason = table.Column<bool>(type: "boolean", nullable: false),
                    BlockIfOpenIssues = table.Column<bool>(type: "boolean", nullable: false),
                    BlockIfExpiredRequiredDocuments = table.Column<bool>(type: "boolean", nullable: false),
                    BlockIfMissingRequiredFields = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_lifecycle_transition_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_notification_rules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RecipientType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecipientRefId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RecipientNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomerContactRoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DelayMinutes = table.Column<int>(type: "integer", nullable: false),
                    EscalationAfterMinutes = table.Column<int>(type: "integer", nullable: true),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_notification_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_numbering_settings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Prefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SequenceName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PaddingLength = table.Column<int>(type: "integer", nullable: false),
                    NextNumber = table.Column<int>(type: "integer", nullable: false),
                    AllowManualOverride = table.Column<bool>(type: "boolean", nullable: false),
                    ManualOverrideRequiresPermission = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayFormat = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UniquenessScope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_numbering_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_onboarding_checklist_item_templates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    OwnerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OwnerRefId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DocumentTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ComplianceQuestionnaireKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BlocksActivation = table.Column<bool>(type: "boolean", nullable: false),
                    BlocksOrders = table.Column<bool>(type: "boolean", nullable: false),
                    BlocksPortalAccess = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_onboarding_checklist_item_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_onboarding_templates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CustomerTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IndustryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PriorityTierKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BlocksActivationUntilComplete = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_onboarding_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_owner_rules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TerritoryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IndustryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DefaultOwnerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DefaultOwnerRefId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DefaultOwnerNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RequiresOwnerForActiveCustomer = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApprovalForReassignment = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalPermission = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_owner_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_portal_settings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    InviteOnly = table.Column<bool>(type: "boolean", nullable: false),
                    SelfRegistrationAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    RequireEmailVerification = table.Column<bool>(type: "boolean", nullable: false),
                    RequireInternalApprovalForPortalUsers = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedEmailDomains = table.Column<string[]>(type: "text[]", nullable: false),
                    SupportContactName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SupportContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SupportContactPhone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PortalDisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LogoRecordArrDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CanViewProfile = table.Column<bool>(type: "boolean", nullable: false),
                    CanRequestQuote = table.Column<bool>(type: "boolean", nullable: false),
                    CanPlaceOrderRequest = table.Column<bool>(type: "boolean", nullable: false),
                    CanUploadDocuments = table.Column<bool>(type: "boolean", nullable: false),
                    CanSubmitIssue = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewOrderStatus = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewInvoicesSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultPortalContactRoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PortalAdminContactRoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_portal_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_required_field_rules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LifecycleStageKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FieldKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequirementLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidationMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AppliesToPortal = table.Column<bool>(type: "boolean", nullable: false),
                    AppliesToInternalCreate = table.Column<bool>(type: "boolean", nullable: false),
                    AppliesToInternalEdit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_required_field_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_tenant_settings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingsVersion = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_tenant_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customarr_tenant_settings_audit_events",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingsVersion = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChangeSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_tenant_settings_audit_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_address_types_TenantId_Key",
                table: "customarr_customer_address_types",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_address_types_TenantId_SortOrder",
                table: "customarr_customer_address_types",
                columns: new[] { "TenantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_classification_catalogs_TenantId_Catalo~1",
                table: "customarr_customer_classification_catalogs",
                columns: new[] { "TenantId", "CatalogType", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_classification_catalogs_TenantId_Catalog~",
                table: "customarr_customer_classification_catalogs",
                columns: new[] { "TenantId", "CatalogType", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_contact_roles_TenantId_Key",
                table: "customarr_customer_contact_roles",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_contact_roles_TenantId_SortOrder",
                table: "customarr_customer_contact_roles",
                columns: new[] { "TenantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_custom_field_definitions_TenantId_Key",
                table: "customarr_customer_custom_field_definitions",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_custom_field_definitions_TenantId_SortOr~",
                table: "customarr_customer_custom_field_definitions",
                columns: new[] { "TenantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_custom_field_options_TenantId_FieldKey_K~",
                table: "customarr_customer_custom_field_options",
                columns: new[] { "TenantId", "FieldKey", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_custom_field_options_TenantId_FieldKey_S~",
                table: "customarr_customer_custom_field_options",
                columns: new[] { "TenantId", "FieldKey", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_document_requirements_TenantId_Key",
                table: "customarr_customer_document_requirements",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_duplicate_detection_rules_TenantId_Key",
                table: "customarr_customer_duplicate_detection_rules",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_duplicate_detection_rules_TenantId_Prior~",
                table: "customarr_customer_duplicate_detection_rules",
                columns: new[] { "TenantId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_external_id_sources_TenantId_Key",
                table: "customarr_customer_external_id_sources",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_integration_settings_TenantId",
                table: "customarr_customer_integration_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_lifecycle_stages_TenantId_IsInitial",
                table: "customarr_customer_lifecycle_stages",
                columns: new[] { "TenantId", "IsInitial" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_lifecycle_stages_TenantId_Key",
                table: "customarr_customer_lifecycle_stages",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_lifecycle_stages_TenantId_SortOrder",
                table: "customarr_customer_lifecycle_stages",
                columns: new[] { "TenantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_lifecycle_transition_rules_TenantId_From~",
                table: "customarr_customer_lifecycle_transition_rules",
                columns: new[] { "TenantId", "FromStageKey", "ToStageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_notification_rules_TenantId_Key",
                table: "customarr_customer_notification_rules",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_numbering_settings_TenantId",
                table: "customarr_customer_numbering_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_checklist_item_templates_Ten~1",
                table: "customarr_customer_onboarding_checklist_item_templates",
                columns: new[] { "TenantId", "TemplateKey", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_checklist_item_templates_Tena~",
                table: "customarr_customer_onboarding_checklist_item_templates",
                columns: new[] { "TenantId", "TemplateKey", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_templates_TenantId_Key",
                table: "customarr_customer_onboarding_templates",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_templates_TenantId_SortOrder",
                table: "customarr_customer_onboarding_templates",
                columns: new[] { "TenantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_owner_rules_TenantId_Priority",
                table: "customarr_customer_owner_rules",
                columns: new[] { "TenantId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_portal_settings_TenantId",
                table: "customarr_customer_portal_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_required_field_rules_TenantId_FieldKey_C~",
                table: "customarr_customer_required_field_rules",
                columns: new[] { "TenantId", "FieldKey", "CustomerTypeKey", "LifecycleStageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tenant_settings_TenantId_IsActive",
                table: "customarr_tenant_settings",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tenant_settings_TenantId_SettingsVersion",
                table: "customarr_tenant_settings",
                columns: new[] { "TenantId", "SettingsVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tenant_settings_audit_events_TenantId_OccurredAt",
                table: "customarr_tenant_settings_audit_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tenant_settings_audit_events_TenantId_SettingsVer~",
                table: "customarr_tenant_settings_audit_events",
                columns: new[] { "TenantId", "SettingsVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customarr_customer_address_types");

            migrationBuilder.DropTable(
                name: "customarr_customer_classification_catalogs");

            migrationBuilder.DropTable(
                name: "customarr_customer_contact_roles");

            migrationBuilder.DropTable(
                name: "customarr_customer_custom_field_definitions");

            migrationBuilder.DropTable(
                name: "customarr_customer_custom_field_options");

            migrationBuilder.DropTable(
                name: "customarr_customer_document_requirements");

            migrationBuilder.DropTable(
                name: "customarr_customer_duplicate_detection_rules");

            migrationBuilder.DropTable(
                name: "customarr_customer_external_id_sources");

            migrationBuilder.DropTable(
                name: "customarr_customer_integration_settings");

            migrationBuilder.DropTable(
                name: "customarr_customer_lifecycle_stages");

            migrationBuilder.DropTable(
                name: "customarr_customer_lifecycle_transition_rules");

            migrationBuilder.DropTable(
                name: "customarr_customer_notification_rules");

            migrationBuilder.DropTable(
                name: "customarr_customer_numbering_settings");

            migrationBuilder.DropTable(
                name: "customarr_customer_onboarding_checklist_item_templates");

            migrationBuilder.DropTable(
                name: "customarr_customer_onboarding_templates");

            migrationBuilder.DropTable(
                name: "customarr_customer_owner_rules");

            migrationBuilder.DropTable(
                name: "customarr_customer_portal_settings");

            migrationBuilder.DropTable(
                name: "customarr_customer_required_field_rules");

            migrationBuilder.DropTable(
                name: "customarr_tenant_settings");

            migrationBuilder.DropTable(
                name: "customarr_tenant_settings_audit_events");
        }
    }
}
