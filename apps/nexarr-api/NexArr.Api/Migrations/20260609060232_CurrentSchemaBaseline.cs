using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class CurrentSchemaBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexarr_entitlement_reconciliation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DriftFoundCount = table.Column<int>(type: "integer", nullable: false),
                    GrantedCount = table.Column<int>(type: "integer", nullable: false),
                    RevokedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_entitlement_reconciliation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_fieldcompanion_field_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubmissionKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DetailMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ClientSubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_fieldcompanion_field_submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_fieldcompanion_notification_dispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WebhookHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PushDeliveredCount = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_fieldcompanion_notification_dispatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_fieldcompanion_notification_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationWebhookUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    NotifyOnHandoffRedeemed = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnFieldInboxRefreshed = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_fieldcompanion_notification_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_fieldcompanion_offline_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActionKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TaskKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClientCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_fieldcompanion_offline_actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_fieldcompanion_push_subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    P256dhKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AuthKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_fieldcompanion_push_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_platform_audit_package_generation_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ToUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FilterJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArtifactZip = table.Column<byte[]>(type: "bytea", nullable: true),
                    ArtifactJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_audit_package_generation_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_platform_entitlement_reconciliation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoGrantFromLicense = table.Column<bool>(type: "boolean", nullable: false),
                    AutoRevokeStaleEntitlements = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_entitlement_reconciliation_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_platform_outbox_publisher_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    DeadLetterCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_outbox_publisher_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_platform_outbox_publisher_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxRetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_outbox_publisher_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_platform_service_token_cleanup_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RetentionDaysAfterExpiry = table.Column<int>(type: "integer", nullable: false),
                    RetentionDaysAfterRevoke = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_service_token_cleanup_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_platform_session_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessTokenMinutes = table.Column<int>(type: "integer", nullable: false),
                    RefreshTokenDays = table.Column<int>(type: "integer", nullable: false),
                    RememberedRefreshTokenDays = table.Column<int>(type: "integer", nullable: false),
                    RequirePlatformAdminMfa = table.Column<bool>(type: "boolean", nullable: true),
                    PasswordMinLength = table.Column<int>(type: "integer", nullable: false),
                    RequirePasswordComplexity = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_session_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_platform_tenant_lifecycle_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoSuspendWhenNoValidLicense = table.Column<bool>(type: "boolean", nullable: false),
                    SuspendGraceDaysAfterLastLicenseExpiry = table.Column<int>(type: "integer", nullable: false),
                    AutoReactivateWhenValidLicense = table.Column<bool>(type: "boolean", nullable: false),
                    RevokeSessionsOnSuspend = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_tenant_lifecycle_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_service_token_cleanup_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurgedCount = table.Column<int>(type: "integer", nullable: false),
                    ExpiredPurgeCount = table.Column<int>(type: "integer", nullable: false),
                    RevokedPurgeCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_service_token_cleanup_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_lifecycle_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PendingCount = table.Column<int>(type: "integer", nullable: false),
                    SuspendedCount = table.Column<int>(type: "integer", nullable: false),
                    ReactivatedCount = table.Column<int>(type: "integer", nullable: false),
                    SessionsRevokedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_lifecycle_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_audit_events", x => x.Id);
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
                name: "platform_outbox_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_outbox_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPlatformAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_catalog",
                columns: table => new
                {
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProductCategory = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProductOwner = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProductStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CanonicalCallbackPath = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    HealthUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ServiceAudience = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MarketingUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DocumentationUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SupportUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EnvironmentKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntitlementDependencyRules = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ProductDependencyMetadata = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_catalog", x => x.ProductKey);
                });

            migrationBuilder.CreateTable(
                name: "reference_datasets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerService = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrentPublishedVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_datasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reference_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConnectorType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuthorityRank = table.Column<int>(type: "integer", nullable: false),
                    RefreshCadence = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TermsNotes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubscriptionTier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BillingCustomerId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BillingSubscriptionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BillingGraceDays = table.Column<int>(type: "integer", nullable: true),
                    IsTrial = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsInternalTenant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "external_identity_provider_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalSubject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_identity_provider_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_external_identity_provider_mappings_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platform_role_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_role_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_role_assignments_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reference_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BeforeSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    AfterSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_audit_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_audit_events_platform_users_ActorPersonId",
                        column: x => x.ActorPersonId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_credentials",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsMfaEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MfaSecret = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MfaRecoveryCodeHashesJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    FailedLoginCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LockedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_credentials", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_credentials_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActiveTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRemembered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_sessions_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_launch_profiles",
                columns: table => new
                {
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LaunchPath = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_launch_profiles", x => x.ProductKey);
                    table.ForeignKey(
                        name: "FK_product_launch_profiles_product_catalog_ProductKey",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AllowedProductKeys = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AllowedTenantIds = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAuthenticationAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_clients_product_catalog_SourceProductKey",
                        column: x => x.SourceProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reference_publish_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_publish_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_publish_events_platform_users_PublishedByPersonId",
                        column: x => x.PublishedByPersonId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reference_publish_events_reference_datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "reference_datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ingestion_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RawObjectKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingestion_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ingestion_jobs_platform_users_RequestedByPersonId",
                        column: x => x.RequestedByPersonId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ingestion_jobs_reference_datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "reference_datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ingestion_jobs_reference_sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "reference_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "handoff_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CallbackUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RedeemedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_handoff_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_handoff_codes_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_handoff_codes_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_product_data_plane_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeploymentMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DataEndpointUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TrustStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_product_data_plane_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_product_data_plane_profiles_product_catalog_P~",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_product_data_plane_profiles_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_product_licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_product_licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_product_licenses_product_catalog_ProductKey",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_product_licenses_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_callback_allowlist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UrlPattern = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PatternType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_callback_allowlist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_callback_allowlist_product_catalog_ProductKey",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_callback_allowlist_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tenant_memberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_memberships_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_memberships_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_product_entitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_product_entitlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_product_entitlements_product_catalog_ProductKey",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_product_entitlements_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Jti = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllowedProductKeys = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ActionScope = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_tokens_service_clients_ServiceClientId",
                        column: x => x.ServiceClientId,
                        principalTable: "service_clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LocalEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MappingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_mappings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reference_crosswalks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalSystem = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_crosswalks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_crosswalks_reference_sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "reference_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "reference_entities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DatasetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CanonicalKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NormalizedFieldsJson = table.Column<string>(type: "jsonb", nullable: false),
                    FirstSeenSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_entities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_entities_reference_datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "reference_datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reference_entities_reference_sources_FirstSeenSourceId",
                        column: x => x.FirstSeenSourceId,
                        principalTable: "reference_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "reference_entity_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    FieldsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SourceEvidenceJson = table.Column<string>(type: "jsonb", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupersededByVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_entity_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reference_entity_versions_reference_entities_ReferenceEntit~",
                        column: x => x.ReferenceEntityId,
                        principalTable: "reference_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reference_entity_versions_reference_entity_versions_Superse~",
                        column: x => x.SupersededByVersionId,
                        principalTable: "reference_entity_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "staging_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetDatasetId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowNumber = table.Column<int>(type: "integer", nullable: true),
                    RawPayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    NormalizedPayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    ProposedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProposedCanonicalKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReviewReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ReviewerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staging_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staging_records_ingestion_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "ingestion_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staging_records_platform_users_ReviewerPersonId",
                        column: x => x.ReviewerPersonId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staging_records_reference_datasets_TargetDatasetId",
                        column: x => x.TargetDatasetId,
                        principalTable: "reference_datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staging_records_reference_entities_ReferenceEntityId",
                        column: x => x.ReferenceEntityId,
                        principalTable: "reference_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tenant_reference_overlays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Hidden = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_reference_overlays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_reference_overlays_reference_entities_ReferenceEntit~",
                        column: x => x.ReferenceEntityId,
                        principalTable: "reference_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_reference_overlays_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_external_identity_provider_mappings_ProviderKey_ExternalSub~",
                table: "external_identity_provider_mappings",
                columns: new[] { "ProviderKey", "ExternalSubject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_external_identity_provider_mappings_UserId_ProviderKey",
                table: "external_identity_provider_mappings",
                columns: new[] { "UserId", "ProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_CodeHash",
                table: "handoff_codes",
                column: "CodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_ExpiresAt",
                table: "handoff_codes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_TenantId_TargetProductKey",
                table: "handoff_codes",
                columns: new[] { "TenantId", "TargetProductKey" });

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_UserId",
                table: "handoff_codes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_jobs_DatasetId_SourceId_CreatedAt",
                table: "ingestion_jobs",
                columns: new[] { "DatasetId", "SourceId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_jobs_RequestedByPersonId",
                table: "ingestion_jobs",
                column: "RequestedByPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_jobs_SourceId",
                table: "ingestion_jobs",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_entitlement_reconciliation_runs_ProcessedAt",
                table: "nexarr_entitlement_reconciliation_runs",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_field_submissions_TenantId_UserId_Tas~",
                table: "nexarr_fieldcompanion_field_submissions",
                columns: new[] { "TenantId", "UserId", "TaskKey", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_notification_dispatches_TenantId",
                table: "nexarr_fieldcompanion_notification_dispatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_notification_dispatches_TenantId_Disp~",
                table: "nexarr_fieldcompanion_notification_dispatches",
                columns: new[] { "TenantId", "DispatchStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_notification_dispatches_TenantId_Even~",
                table: "nexarr_fieldcompanion_notification_dispatches",
                columns: new[] { "TenantId", "EventKind", "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_notification_settings_TenantId",
                table: "nexarr_fieldcompanion_notification_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_offline_actions_TenantId_IdempotencyK~",
                table: "nexarr_fieldcompanion_offline_actions",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_offline_actions_TenantId_UserId_Synce~",
                table: "nexarr_fieldcompanion_offline_actions",
                columns: new[] { "TenantId", "UserId", "SyncedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_push_subscriptions_TenantId",
                table: "nexarr_fieldcompanion_push_subscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_push_subscriptions_TenantId_UserId",
                table: "nexarr_fieldcompanion_push_subscriptions",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_fieldcompanion_push_subscriptions_TenantId_UserId_En~",
                table: "nexarr_fieldcompanion_push_subscriptions",
                columns: new[] { "TenantId", "UserId", "Endpoint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_platform_audit_package_generation_jobs_CreatedAt",
                table: "nexarr_platform_audit_package_generation_jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_platform_audit_package_generation_jobs_ScopeTenantId~",
                table: "nexarr_platform_audit_package_generation_jobs",
                columns: new[] { "ScopeTenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_platform_outbox_publisher_runs_ProcessedAt",
                table: "nexarr_platform_outbox_publisher_runs",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_service_token_cleanup_runs_ProcessedAt",
                table: "nexarr_service_token_cleanup_runs",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_lifecycle_runs_ProcessedAt",
                table: "nexarr_tenant_lifecycle_runs",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_data_plane_profiles_ProductKey",
                table: "nexarr_tenant_product_data_plane_profiles",
                column: "ProductKey");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_data_plane_profiles_TenantId_ProductK~",
                table: "nexarr_tenant_product_data_plane_profiles",
                columns: new[] { "TenantId", "ProductKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_licenses_ProductKey",
                table: "nexarr_tenant_product_licenses",
                column: "ProductKey");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_licenses_TenantId",
                table: "nexarr_tenant_product_licenses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_licenses_TenantId_ProductKey",
                table: "nexarr_tenant_product_licenses",
                columns: new[] { "TenantId", "ProductKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_licenses_ValidTo",
                table: "nexarr_tenant_product_licenses",
                column: "ValidTo");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_ExpiresAt",
                table: "password_reset_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_TokenHash",
                table: "password_reset_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_UserId_UsedAt",
                table: "password_reset_tokens",
                columns: new[] { "UserId", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_audit_events_OccurredAt",
                table: "platform_audit_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_platform_audit_events_TenantId",
                table: "platform_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId",
                table: "platform_metadata",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId_Key",
                table: "platform_metadata",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_outbox_events_IdempotencyKey",
                table: "platform_outbox_events",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_outbox_events_ProcessingStatus_OccurredAt",
                table: "platform_outbox_events",
                columns: new[] { "ProcessingStatus", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_outbox_events_TenantId",
                table: "platform_outbox_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_role_assignments_UserId_RoleKey_TenantId",
                table: "platform_role_assignments",
                columns: new[] { "UserId", "RoleKey", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_users_Email",
                table: "platform_users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_callback_allowlist_ProductKey_TenantId",
                table: "product_callback_allowlist",
                columns: new[] { "ProductKey", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_product_callback_allowlist_TenantId",
                table: "product_callback_allowlist",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_product_mappings_ReferenceEntityId",
                table: "product_mappings",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_product_mappings_TenantId_ProductCode_LocalEntityType_Local~",
                table: "product_mappings",
                columns: new[] { "TenantId", "ProductCode", "LocalEntityType", "LocalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_audit_events_ActorPersonId",
                table: "reference_audit_events",
                column: "ActorPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_audit_events_EntityType_EntityId_CreatedAt",
                table: "reference_audit_events",
                columns: new[] { "EntityType", "EntityId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_reference_crosswalks_ExternalSystem_ExternalKey",
                table: "reference_crosswalks",
                columns: new[] { "ExternalSystem", "ExternalKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_crosswalks_ReferenceEntityId",
                table: "reference_crosswalks",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_crosswalks_SourceId",
                table: "reference_crosswalks",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_datasets_Key",
                table: "reference_datasets",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_CurrentVersionId",
                table: "reference_entities",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_DatasetId_EntityType_CanonicalKey",
                table: "reference_entities",
                columns: new[] { "DatasetId", "EntityType", "CanonicalKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_DatasetId_Status",
                table: "reference_entities",
                columns: new[] { "DatasetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_FirstSeenSourceId",
                table: "reference_entities",
                column: "FirstSeenSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_entity_versions_ReferenceEntityId_Version",
                table: "reference_entity_versions",
                columns: new[] { "ReferenceEntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_entity_versions_SupersededByVersionId",
                table: "reference_entity_versions",
                column: "SupersededByVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_publish_events_DatasetId_CreatedAt",
                table: "reference_publish_events",
                columns: new[] { "DatasetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_reference_publish_events_PublishedByPersonId",
                table: "reference_publish_events",
                column: "PublishedByPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_sources_Key",
                table: "reference_sources",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_clients_ClientKey",
                table: "service_clients",
                column: "ClientKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_clients_SourceProductKey",
                table: "service_clients",
                column: "SourceProductKey");

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_ExpiresAt",
                table: "service_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_Jti",
                table: "service_tokens",
                column: "Jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_RevokedAt",
                table: "service_tokens",
                column: "RevokedAt");

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_ServiceClientId",
                table: "service_tokens",
                column: "ServiceClientId");

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_TenantId",
                table: "service_tokens",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_JobId",
                table: "staging_records",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_ReferenceEntityId",
                table: "staging_records",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_ReviewerPersonId",
                table: "staging_records",
                column: "ReviewerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_Status_Confidence",
                table: "staging_records",
                columns: new[] { "Status", "Confidence" });

            migrationBuilder.CreateIndex(
                name: "IX_staging_records_TargetDatasetId",
                table: "staging_records",
                column: "TargetDatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_memberships_TenantId_UserId",
                table: "tenant_memberships",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_memberships_UserId",
                table: "tenant_memberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_product_entitlements_ProductKey",
                table: "tenant_product_entitlements",
                column: "ProductKey");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_product_entitlements_TenantId_ProductKey",
                table: "tenant_product_entitlements",
                columns: new[] { "TenantId", "ProductKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_reference_overlays_ReferenceEntityId",
                table: "tenant_reference_overlays",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_reference_overlays_TenantId_ReferenceEntityId",
                table: "tenant_reference_overlays",
                columns: new[] { "TenantId", "ReferenceEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                table: "tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_RefreshTokenHash",
                table: "user_sessions",
                column: "RefreshTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_UserId",
                table: "user_sessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_product_mappings_reference_entities_ReferenceEntityId",
                table: "product_mappings",
                column: "ReferenceEntityId",
                principalTable: "reference_entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reference_crosswalks_reference_entities_ReferenceEntityId",
                table: "reference_crosswalks",
                column: "ReferenceEntityId",
                principalTable: "reference_entities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reference_entities_reference_entity_versions_CurrentVersion~",
                table: "reference_entities",
                column: "CurrentVersionId",
                principalTable: "reference_entity_versions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reference_entities_reference_datasets_DatasetId",
                table: "reference_entities");

            migrationBuilder.DropForeignKey(
                name: "FK_reference_entities_reference_sources_FirstSeenSourceId",
                table: "reference_entities");

            migrationBuilder.DropForeignKey(
                name: "FK_reference_entity_versions_reference_entities_ReferenceEntit~",
                table: "reference_entity_versions");

            migrationBuilder.DropTable(
                name: "external_identity_provider_mappings");

            migrationBuilder.DropTable(
                name: "handoff_codes");

            migrationBuilder.DropTable(
                name: "nexarr_entitlement_reconciliation_runs");

            migrationBuilder.DropTable(
                name: "nexarr_fieldcompanion_field_submissions");

            migrationBuilder.DropTable(
                name: "nexarr_fieldcompanion_notification_dispatches");

            migrationBuilder.DropTable(
                name: "nexarr_fieldcompanion_notification_settings");

            migrationBuilder.DropTable(
                name: "nexarr_fieldcompanion_offline_actions");

            migrationBuilder.DropTable(
                name: "nexarr_fieldcompanion_push_subscriptions");

            migrationBuilder.DropTable(
                name: "nexarr_platform_audit_package_generation_jobs");

            migrationBuilder.DropTable(
                name: "nexarr_platform_entitlement_reconciliation_settings");

            migrationBuilder.DropTable(
                name: "nexarr_platform_outbox_publisher_runs");

            migrationBuilder.DropTable(
                name: "nexarr_platform_outbox_publisher_settings");

            migrationBuilder.DropTable(
                name: "nexarr_platform_service_token_cleanup_settings");

            migrationBuilder.DropTable(
                name: "nexarr_platform_session_settings");

            migrationBuilder.DropTable(
                name: "nexarr_platform_tenant_lifecycle_settings");

            migrationBuilder.DropTable(
                name: "nexarr_service_token_cleanup_runs");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_lifecycle_runs");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_product_data_plane_profiles");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_product_licenses");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "platform_audit_events");

            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "platform_outbox_events");

            migrationBuilder.DropTable(
                name: "platform_role_assignments");

            migrationBuilder.DropTable(
                name: "product_callback_allowlist");

            migrationBuilder.DropTable(
                name: "product_launch_profiles");

            migrationBuilder.DropTable(
                name: "product_mappings");

            migrationBuilder.DropTable(
                name: "reference_audit_events");

            migrationBuilder.DropTable(
                name: "reference_crosswalks");

            migrationBuilder.DropTable(
                name: "reference_publish_events");

            migrationBuilder.DropTable(
                name: "service_tokens");

            migrationBuilder.DropTable(
                name: "staging_records");

            migrationBuilder.DropTable(
                name: "tenant_memberships");

            migrationBuilder.DropTable(
                name: "tenant_product_entitlements");

            migrationBuilder.DropTable(
                name: "tenant_reference_overlays");

            migrationBuilder.DropTable(
                name: "user_credentials");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "service_clients");

            migrationBuilder.DropTable(
                name: "ingestion_jobs");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "product_catalog");

            migrationBuilder.DropTable(
                name: "platform_users");

            migrationBuilder.DropTable(
                name: "reference_datasets");

            migrationBuilder.DropTable(
                name: "reference_sources");

            migrationBuilder.DropTable(
                name: "reference_entities");

            migrationBuilder.DropTable(
                name: "reference_entity_versions");
        }
    }
}
