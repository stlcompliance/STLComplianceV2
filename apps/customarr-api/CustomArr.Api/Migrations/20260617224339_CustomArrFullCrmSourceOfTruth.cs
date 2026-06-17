using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class CustomArrFullCrmSourceOfTruth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountClassKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivatedAt",
                table: "customarr_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ChurnedAt",
                table: "customarr_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceStatusKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerSuccessOwnerPersonId",
                table: "customarr_customers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstOrderDate",
                table: "customarr_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HealthScore",
                table: "customarr_customers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthScoreKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HealthScoreReason",
                table: "customarr_customers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndustryKey",
                table: "customarr_customers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                table: "customarr_customers",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsKeyAccount",
                table: "customarr_customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStrategicAccount",
                table: "customarr_customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastActivityAt",
                table: "customarr_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LeadDate",
                table: "customarr_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NaicsCode",
                table: "customarr_customers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingStatusKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "QualifiedDate",
                table: "customarr_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipRoleKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelationshipSummary",
                table: "customarr_customers",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevenueBandKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesOwnerPersonId",
                table: "customarr_customers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceEligibilityStatusKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SicCode",
                table: "customarr_customers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportOwnerPersonId",
                table: "customarr_customers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerticalKey",
                table: "customarr_customers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "customarr_customers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalEntityType",
                table: "customarr_customer_external_refs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastVerifiedAt",
                table: "customarr_customer_external_refs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SyncDirectionKey",
                table: "customarr_customer_external_refs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "AuthorizationScopes",
                table: "customarr_customer_contacts",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<bool>(
                name: "CanApproveQuotes",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanPlaceOrders",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSignContracts",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSubmitCases",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanUploadDocuments",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConsentCapturedAt",
                table: "customarr_customer_contacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentLegalBasisKey",
                table: "customarr_customer_contacts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailOptIn",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDecisionMaker",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocationId",
                table: "customarr_customer_contacts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarketingConsent",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SmsOptIn",
                table: "customarr_customer_contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AfterHoursRules",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppointmentInstructions",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceNotes",
                table: "customarr_customer_addresses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverCheckInRules",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DropTrailerAllowed",
                table: "customarr_customer_addresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string[]>(
                name: "EligibleProductKeys",
                table: "customarr_customer_addresses",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "GateInstructions",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HazmatRestrictions",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LiftgateRequired",
                table: "customarr_customer_addresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "customarr_customer_addresses",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MailingCity",
                table: "customarr_customer_addresses",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingCountryCode",
                table: "customarr_customer_addresses",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingLine1",
                table: "customarr_customer_addresses",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingLine2",
                table: "customarr_customer_addresses",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingPostalCode",
                table: "customarr_customer_addresses",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailingStateProvince",
                table: "customarr_customer_addresses",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParkingInstructions",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PpeRequirements",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "ServiceAreaKeys",
                table: "customarr_customer_addresses",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "ShippingHours",
                table: "customarr_customer_addresses",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemperatureRules",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TruckSizeRestrictions",
                table: "customarr_customer_addresses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivityTypeKey",
                table: "customarr_customer_activity",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "customarr_customer_activity",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactId",
                table: "customarr_customer_activity",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "customarr_customer_activity",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CustomerLocationId",
                table: "customarr_customer_activity",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DirectionKey",
                table: "customarr_customer_activity",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "RecordRefs",
                table: "customarr_customer_activity",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "RelatedObjectRefs",
                table: "customarr_customer_activity",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "SourceObjectRef",
                table: "customarr_customer_activity",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "customarr_customer_activity",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisibilityKey",
                table: "customarr_customer_activity",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "customarr_agreements",
                columns: table => new
                {
                    AgreementId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AgreementTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RenewalDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TerminationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScopeSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TermsSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CoveredProductKeys = table.Column<string[]>(type: "text[]", nullable: false),
                    CoveredLocationIds = table.Column<string[]>(type: "text[]", nullable: false),
                    RequirementRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_agreements", x => x.AgreementId);
                    table.ForeignKey(
                        name: "FK_customarr_agreements_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_cases",
                columns: table => new
                {
                    CaseId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerLocationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PriorityKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SeverityKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FirstResponseDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolutionDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SlaBreached = table.Column<bool>(type: "boolean", nullable: false),
                    SupportOwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EscalationOwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwningProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OwningProductIssueRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RootCauseCategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ResolutionSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CustomerSatisfactionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_cases", x => x.CaseId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_cases_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_health_profiles",
                columns: table => new
                {
                    HealthProfileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HealthStatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    ScoreReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OpenCaseCount = table.Column<int>(type: "integer", nullable: false),
                    SlaBreachCount = table.Column<int>(type: "integer", nullable: false),
                    ActiveContactCount = table.Column<int>(type: "integer", nullable: false),
                    ActiveLocationCount = table.Column<int>(type: "integer", nullable: false),
                    RevenueSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ChurnRiskKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PaymentRiskKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpansionSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastCheckInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextBusinessReviewAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_health_profiles", x => x.HealthProfileId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_health_profiles_customarr_customers_Cust~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_onboarding",
                columns: table => new
                {
                    OnboardingId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OnboardingNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OnboardingTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LaunchDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Blockers = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_onboarding", x => x.OnboardingId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_onboarding_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_service_profiles",
                columns: table => new
                {
                    ServiceProfileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerLocationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ServiceEligibilityStatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AllowedProductKeys = table.Column<string[]>(type: "text[]", nullable: false),
                    BlockedProductKeys = table.Column<string[]>(type: "text[]", nullable: false),
                    AllowedWorkflowKeys = table.Column<string[]>(type: "text[]", nullable: false),
                    BlockedWorkflowKeys = table.Column<string[]>(type: "text[]", nullable: false),
                    RequiredApprovalTypes = table.Column<string[]>(type: "text[]", nullable: false),
                    RequiredRequirementRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    ActiveHoldRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Restrictions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ServiceLevelKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalCreditStatusSnapshotKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastEligibilityReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastEligibilityCalculatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_service_profiles", x => x.ServiceProfileId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_service_profiles_customarr_customers_Cus~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_dedupe_candidates",
                columns: table => new
                {
                    DedupeCandidateId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CandidateTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceRecordRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MatchedCustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MatchReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_dedupe_candidates", x => x.DedupeCandidateId);
                });

            migrationBuilder.CreateTable(
                name: "customarr_eligibility_checks",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerLocationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    WorkflowKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResultKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Blockers = table.Column<string[]>(type: "text[]", nullable: false),
                    Warnings = table.Column<string[]>(type: "text[]", nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_eligibility_checks", x => x.EligibilityCheckId);
                });

            migrationBuilder.CreateTable(
                name: "customarr_import_batches",
                columns: table => new
                {
                    ImportBatchId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceFileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ImporterPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    AcceptedRows = table.Column<int>(type: "integer", nullable: false),
                    RejectedRows = table.Column<int>(type: "integer", nullable: false),
                    MappingSummaryJson = table.Column<string>(type: "jsonb", nullable: true),
                    ValidationErrors = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_import_batches", x => x.ImportBatchId);
                });

            migrationBuilder.CreateTable(
                name: "customarr_integration_references",
                columns: table => new
                {
                    IntegrationReferenceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerLocationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RelatedEntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalSystemKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SyncDirectionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_integration_references", x => x.IntegrationReferenceId);
                    table.ForeignKey(
                        name: "FK_customarr_integration_references_customarr_customers_Custom~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_leads",
                columns: table => new
                {
                    LeadId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PersonName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FitScore = table.Column<int>(type: "integer", nullable: true),
                    NeedSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BudgetSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TimingSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AuthoritySummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ServiceInterest = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AssignedTeamId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NextFollowUpAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConvertedCustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ConvertedContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ConvertedOpportunityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DisqualificationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConvertedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_leads", x => x.LeadId);
                    table.ForeignKey(
                        name: "FK_customarr_leads_customarr_customers_ConvertedCustomerId",
                        column: x => x.ConvertedCustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "customarr_merge_records",
                columns: table => new
                {
                    MergeRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SurvivorCustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MergedCustomerIds = table.Column<string[]>(type: "text[]", nullable: false),
                    MergeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MergeStrategyKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FieldResolutionSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ProposedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_merge_records", x => x.MergeRecordId);
                });

            migrationBuilder.CreateTable(
                name: "customarr_opportunities",
                columns: table => new
                {
                    OpportunityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpportunityNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LeadId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OpportunityName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StageKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProbabilityPercent = table.Column<int>(type: "integer", nullable: false),
                    ForecastCategoryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpectedCloseDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EstimatedRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EstimatedMargin = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RecurringRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OneTimeRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ServiceInterestKeys = table.Column<string[]>(type: "text[]", nullable: false),
                    ScopeSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PrimaryContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StakeholderContactIds = table.Column<string[]>(type: "text[]", nullable: false),
                    Competitor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IncumbentProvider = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WinLossReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NextStep = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NextFollowUpAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OutcomeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    HandoffProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    HandoffObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_opportunities", x => x.OpportunityId);
                    table.ForeignKey(
                        name: "FK_customarr_opportunities_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "customarr_portal_access_records",
                columns: table => new
                {
                    PortalAccessId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContactId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NexArrExternalIdentityRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PortalRoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AllowedLocationIds = table.Column<string[]>(type: "text[]", nullable: false),
                    AuthorizationRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    InvitedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InvitedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SuspendedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastAccessSnapshotAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_portal_access_records", x => x.PortalAccessId);
                    table.ForeignKey(
                        name: "FK_customarr_portal_access_records_customarr_customers_Custome~",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_proposals",
                columns: table => new
                {
                    ProposalId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OpportunityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScopeSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    PricingSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    TermsSnapshot = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SlaSnapshot = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovalStatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CustomerResponseKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalAccountingQuoteRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedOrdArrOrderRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_proposals", x => x.ProposalId);
                    table.ForeignKey(
                        name: "FK_customarr_proposals_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_tasks",
                columns: table => new
                {
                    TaskId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedObjectType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RelatedObjectId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PriorityKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_tasks", x => x.TaskId);
                    table.ForeignKey(
                        name: "FK_customarr_tasks_customarr_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customarr_customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customarr_customer_onboarding_checklist_items",
                columns: table => new
                {
                    ChecklistItemId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ItemTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customarr_customer_onboarding_checklist_items", x => x.ChecklistItemId);
                    table.ForeignKey(
                        name: "FK_customarr_customer_onboarding_checklist_items_customarr_cus~",
                        column: x => x.OnboardingId,
                        principalTable: "customarr_customer_onboarding",
                        principalColumn: "OnboardingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customers_TenantId_HealthScoreKey",
                table: "customarr_customers",
                columns: new[] { "TenantId", "HealthScoreKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customers_TenantId_OnboardingStatusKey",
                table: "customarr_customers",
                columns: new[] { "TenantId", "OnboardingStatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customers_TenantId_ServiceEligibilityStatusKey",
                table: "customarr_customers",
                columns: new[] { "TenantId", "ServiceEligibilityStatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_contacts_TenantId_LocationId",
                table: "customarr_customer_contacts",
                columns: new[] { "TenantId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_addresses_TenantId_LocationCode",
                table: "customarr_customer_addresses",
                columns: new[] { "TenantId", "LocationCode" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_activity_TenantId_ActivityTypeKey",
                table: "customarr_customer_activity",
                columns: new[] { "TenantId", "ActivityTypeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_agreements_CustomerId",
                table: "customarr_agreements",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_agreements_TenantId_AgreementNumber",
                table: "customarr_agreements",
                columns: new[] { "TenantId", "AgreementNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_agreements_TenantId_CustomerId",
                table: "customarr_agreements",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_agreements_TenantId_StatusKey",
                table: "customarr_agreements",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_cases_CustomerId",
                table: "customarr_customer_cases",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_cases_TenantId_CaseNumber",
                table: "customarr_customer_cases",
                columns: new[] { "TenantId", "CaseNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_cases_TenantId_CustomerId",
                table: "customarr_customer_cases",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_cases_TenantId_StatusKey",
                table: "customarr_customer_cases",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_health_profiles_CustomerId",
                table: "customarr_customer_health_profiles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_health_profiles_TenantId_CustomerId",
                table: "customarr_customer_health_profiles",
                columns: new[] { "TenantId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_health_profiles_TenantId_HealthStatusKey",
                table: "customarr_customer_health_profiles",
                columns: new[] { "TenantId", "HealthStatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_CustomerId",
                table: "customarr_customer_onboarding",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_TenantId_CustomerId",
                table: "customarr_customer_onboarding",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_TenantId_OnboardingNumber",
                table: "customarr_customer_onboarding",
                columns: new[] { "TenantId", "OnboardingNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_TenantId_StatusKey",
                table: "customarr_customer_onboarding",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_checklist_items_OnboardingId",
                table: "customarr_customer_onboarding_checklist_items",
                column: "OnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_checklist_items_TenantId_Cust~",
                table: "customarr_customer_onboarding_checklist_items",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_onboarding_checklist_items_TenantId_Onbo~",
                table: "customarr_customer_onboarding_checklist_items",
                columns: new[] { "TenantId", "OnboardingId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_service_profiles_CustomerId",
                table: "customarr_customer_service_profiles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_service_profiles_TenantId_CustomerId",
                table: "customarr_customer_service_profiles",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_customer_service_profiles_TenantId_ServiceEligibi~",
                table: "customarr_customer_service_profiles",
                columns: new[] { "TenantId", "ServiceEligibilityStatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_dedupe_candidates_TenantId_ImportBatchId",
                table: "customarr_dedupe_candidates",
                columns: new[] { "TenantId", "ImportBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_dedupe_candidates_TenantId_StatusKey",
                table: "customarr_dedupe_candidates",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_eligibility_checks_TenantId_CustomerId_CheckedAt",
                table: "customarr_eligibility_checks",
                columns: new[] { "TenantId", "CustomerId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_eligibility_checks_TenantId_ResultKey",
                table: "customarr_eligibility_checks",
                columns: new[] { "TenantId", "ResultKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_import_batches_TenantId_CreatedAt",
                table: "customarr_import_batches",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_import_batches_TenantId_StatusKey",
                table: "customarr_import_batches",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_integration_references_CustomerId",
                table: "customarr_integration_references",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_integration_references_TenantId_CustomerId",
                table: "customarr_integration_references",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_integration_references_TenantId_ExternalSystemKey~",
                table: "customarr_integration_references",
                columns: new[] { "TenantId", "ExternalSystemKey", "ExternalEntityType", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_leads_ConvertedCustomerId",
                table: "customarr_leads",
                column: "ConvertedCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_leads_TenantId_Email",
                table: "customarr_leads",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_leads_TenantId_LeadNumber",
                table: "customarr_leads",
                columns: new[] { "TenantId", "LeadNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_leads_TenantId_StatusKey",
                table: "customarr_leads",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_merge_records_TenantId_StatusKey",
                table: "customarr_merge_records",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_merge_records_TenantId_SurvivorCustomerId",
                table: "customarr_merge_records",
                columns: new[] { "TenantId", "SurvivorCustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_opportunities_CustomerId",
                table: "customarr_opportunities",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_opportunities_TenantId_CustomerId",
                table: "customarr_opportunities",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_opportunities_TenantId_OpportunityNumber",
                table: "customarr_opportunities",
                columns: new[] { "TenantId", "OpportunityNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_opportunities_TenantId_StageKey",
                table: "customarr_opportunities",
                columns: new[] { "TenantId", "StageKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_opportunities_TenantId_StatusKey",
                table: "customarr_opportunities",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_portal_access_records_CustomerId",
                table: "customarr_portal_access_records",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_portal_access_records_TenantId_ContactId",
                table: "customarr_portal_access_records",
                columns: new[] { "TenantId", "ContactId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_portal_access_records_TenantId_CustomerId",
                table: "customarr_portal_access_records",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_portal_access_records_TenantId_StatusKey",
                table: "customarr_portal_access_records",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_proposals_CustomerId",
                table: "customarr_proposals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_proposals_TenantId_CustomerId",
                table: "customarr_proposals",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_proposals_TenantId_ProposalNumber",
                table: "customarr_proposals",
                columns: new[] { "TenantId", "ProposalNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customarr_proposals_TenantId_StatusKey",
                table: "customarr_proposals",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tasks_CustomerId",
                table: "customarr_tasks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tasks_TenantId_CustomerId",
                table: "customarr_tasks",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tasks_TenantId_OwnerPersonId",
                table: "customarr_tasks",
                columns: new[] { "TenantId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tasks_TenantId_StatusKey",
                table: "customarr_tasks",
                columns: new[] { "TenantId", "StatusKey" });

            migrationBuilder.CreateIndex(
                name: "IX_customarr_tasks_TenantId_TaskNumber",
                table: "customarr_tasks",
                columns: new[] { "TenantId", "TaskNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customarr_agreements");

            migrationBuilder.DropTable(
                name: "customarr_customer_cases");

            migrationBuilder.DropTable(
                name: "customarr_customer_health_profiles");

            migrationBuilder.DropTable(
                name: "customarr_customer_onboarding_checklist_items");

            migrationBuilder.DropTable(
                name: "customarr_customer_service_profiles");

            migrationBuilder.DropTable(
                name: "customarr_dedupe_candidates");

            migrationBuilder.DropTable(
                name: "customarr_eligibility_checks");

            migrationBuilder.DropTable(
                name: "customarr_import_batches");

            migrationBuilder.DropTable(
                name: "customarr_integration_references");

            migrationBuilder.DropTable(
                name: "customarr_leads");

            migrationBuilder.DropTable(
                name: "customarr_merge_records");

            migrationBuilder.DropTable(
                name: "customarr_opportunities");

            migrationBuilder.DropTable(
                name: "customarr_portal_access_records");

            migrationBuilder.DropTable(
                name: "customarr_proposals");

            migrationBuilder.DropTable(
                name: "customarr_tasks");

            migrationBuilder.DropTable(
                name: "customarr_customer_onboarding");

            migrationBuilder.DropIndex(
                name: "IX_customarr_customers_TenantId_HealthScoreKey",
                table: "customarr_customers");

            migrationBuilder.DropIndex(
                name: "IX_customarr_customers_TenantId_OnboardingStatusKey",
                table: "customarr_customers");

            migrationBuilder.DropIndex(
                name: "IX_customarr_customers_TenantId_ServiceEligibilityStatusKey",
                table: "customarr_customers");

            migrationBuilder.DropIndex(
                name: "IX_customarr_customer_contacts_TenantId_LocationId",
                table: "customarr_customer_contacts");

            migrationBuilder.DropIndex(
                name: "IX_customarr_customer_addresses_TenantId_LocationCode",
                table: "customarr_customer_addresses");

            migrationBuilder.DropIndex(
                name: "IX_customarr_customer_activity_TenantId_ActivityTypeKey",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "AccountClassKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "ChurnedAt",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "ComplianceStatusKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "CustomerSuccessOwnerPersonId",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "FirstOrderDate",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "HealthScore",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "HealthScoreKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "HealthScoreReason",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "IndustryKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "InternalNotes",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "IsKeyAccount",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "IsStrategicAccount",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "LeadDate",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "MarketKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "NaicsCode",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "OnboardingStatusKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "QualifiedDate",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "RegionKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "RelationshipRoleKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "RelationshipSummary",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "RevenueBandKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "SalesOwnerPersonId",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "ServiceEligibilityStatusKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "SicCode",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "SupportOwnerPersonId",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "VerticalKey",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "customarr_customers");

            migrationBuilder.DropColumn(
                name: "ExternalEntityType",
                table: "customarr_customer_external_refs");

            migrationBuilder.DropColumn(
                name: "LastVerifiedAt",
                table: "customarr_customer_external_refs");

            migrationBuilder.DropColumn(
                name: "SyncDirectionKey",
                table: "customarr_customer_external_refs");

            migrationBuilder.DropColumn(
                name: "AuthorizationScopes",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "CanApproveQuotes",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "CanPlaceOrders",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "CanSignContracts",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "CanSubmitCases",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "CanUploadDocuments",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "ConsentCapturedAt",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "ConsentLegalBasisKey",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "EmailOptIn",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "IsDecisionMaker",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "MarketingConsent",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "SmsOptIn",
                table: "customarr_customer_contacts");

            migrationBuilder.DropColumn(
                name: "AfterHoursRules",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "AppointmentInstructions",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "ComplianceNotes",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "DriverCheckInRules",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "DropTrailerAllowed",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "EligibleProductKeys",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "GateInstructions",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "HazmatRestrictions",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "LiftgateRequired",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "MailingCity",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "MailingCountryCode",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "MailingLine1",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "MailingLine2",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "MailingPostalCode",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "MailingStateProvince",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "ParkingInstructions",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "PpeRequirements",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "ServiceAreaKeys",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "ShippingHours",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "TemperatureRules",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "TruckSizeRestrictions",
                table: "customarr_customer_addresses");

            migrationBuilder.DropColumn(
                name: "ActivityTypeKey",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "CustomerLocationId",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "DirectionKey",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "RecordRefs",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "RelatedObjectRefs",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "SourceObjectRef",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "customarr_customer_activity");

            migrationBuilder.DropColumn(
                name: "VisibilityKey",
                table: "customarr_customer_activity");
        }
    }
}
