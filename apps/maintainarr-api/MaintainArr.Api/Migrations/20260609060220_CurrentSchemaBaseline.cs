using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class CurrentSchemaBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_assignment_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentFieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_assignment_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_availability_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailabilityPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    PlannedDowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnplannedDowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    HasActiveDowntime = table.Column<bool>(type: "boolean", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_availability_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_compliance_state",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoverningBodyKeysJson = table.Column<string>(type: "text", nullable: false),
                    RulepackApplicabilityKeysJson = table.Column<string>(type: "text", nullable: false),
                    ComplianceCategoryKeysJson = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_compliance_state", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_components", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_custom_field_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_custom_field_values", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StatusKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_downtime_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsPlanned = table.Column<bool>(type: "boolean", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusTrigger = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_downtime_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_downtime_sync_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssetsScanned = table.Column<int>(type: "integer", nullable: false),
                    EventsOpened = table.Column<int>(type: "integer", nullable: false),
                    EventsClosed = table.Column<int>(type: "integer", nullable: false),
                    SnapshotsRefreshed = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_downtime_sync_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_external_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_external_mappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_location_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StaffarrSiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrSiteNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HomeLocationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CurrentLocationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Yard = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Bay = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ParkingSpot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_location_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_readiness_checks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReadinessStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReadinessBasis = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_readiness_checks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_readiness_state",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadinessStatusKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OperationalStatusKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AvailabilityStatusKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Basis = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_readiness_state", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_specs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_specs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusFieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StatusValueKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_status_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_status_rollup_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    RefreshedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    ScopeRollupsRefreshed = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_status_rollup_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_status_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LifecycleStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReadinessStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReadinessBasis = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlockerCount = table.Column<int>(type: "integer", nullable: false),
                    PrimaryBlockerMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OpenCriticalDefectCount = table.Column<int>(type: "integer", nullable: false),
                    OpenHighDefectCount = table.Column<int>(type: "integer", nullable: false),
                    ActiveWorkOrderCount = table.Column<int>(type: "integer", nullable: false),
                    PmDueCount = table.Column<int>(type: "integer", nullable: false),
                    PmOverdueCount = table.Column<int>(type: "integer", nullable: false),
                    FailedInspectionCount = table.Column<int>(type: "integer", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_status_rollups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_status_scope_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeEntityKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ScopeLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TotalAssets = table.Column<int>(type: "integer", nullable: false),
                    ReadyCount = table.Column<int>(type: "integer", nullable: false),
                    NotReadyCount = table.Column<int>(type: "integer", nullable: false),
                    ReadyPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_status_scope_rollups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_maintainarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_audit_package_generation_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_maintainarr_audit_package_generation_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_catalog_option_dependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DependsOnCatalogKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DependsOnOptionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RuleJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_catalog_option_dependencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_catalog_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ParentOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsTenantSpecific = table.Column<bool>(type: "boolean", nullable: false),
                    OptionTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_catalog_options", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_catalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Owner = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Scope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsTenantExtendable = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_catalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_compliance_regulatory_key_mirrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplianceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MaterialKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RegulatoryCitationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceRecordKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_compliance_regulatory_key_mirrors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_defect_escalation_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreviousSeverity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    NewSeverity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PreviousStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_defect_escalation_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_defect_escalation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    EscalatedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_defect_escalation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_external_provider_audit_log_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OperationKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ResultStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_external_provider_audit_log_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_external_provider_cache_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OperationKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestJson = table.Column<string>(type: "text", nullable: false),
                    ResponseJson = table.Column<string>(type: "text", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LastFetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_external_provider_cache_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_fieldset_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_fieldset_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_fieldset_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldsetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ControlType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    CatalogKey = table.Column<string>(type: "text", nullable: true),
                    ReferenceKey = table.Column<string>(type: "text", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceOfTruth = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SectionKey = table.Column<string>(type: "text", nullable: false),
                    DependencyJson = table.Column<string>(type: "text", nullable: false),
                    ValidationJson = table.Column<string>(type: "text", nullable: false),
                    DefaultValueJson = table.Column<string>(type: "text", nullable: false),
                    VisibilityJson = table.Column<string>(type: "text", nullable: false),
                    AllowCustom = table.Column<bool>(type: "boolean", nullable: false),
                    CustomRequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesLogic = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesInspectionBranching = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesPMApplicability = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesCompliance = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesReporting = table.Column<bool>(type: "boolean", nullable: false),
                    DrivesReadiness = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_fieldset_fields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_fleet_availability_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssetCount = table.Column<int>(type: "integer", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailabilityPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    PlannedDowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnplannedDowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActiveDowntimeEventCount = table.Column<int>(type: "integer", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_fleet_availability_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_import_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Phase = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DryRun = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_import_batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    TemplateCategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwningSiteRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwningTeamRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerRoleKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    TagsJson = table.Column<string>(type: "text", nullable: false),
                    SettingsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PublishedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetiredByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InspectionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_history_rollup_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    RefreshedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_history_rollup_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_history_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    InspectionCount = table.Column<int>(type: "integer", nullable: false),
                    DefectCount = table.Column<int>(type: "integer", nullable: false),
                    WorkOrderCount = table.Column<int>(type: "integer", nullable: false),
                    PmCount = table.Column<int>(type: "integer", nullable: false),
                    LastEventAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_history_rollups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_parts_kits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    KitNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    KitCategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    KitTypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PriorityKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwningSiteRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwningTeamRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerRoleKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TagsJson = table.Column<string>(type: "text", nullable: false),
                    AssetTypeApplicabilityJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    WorkOrderTypeApplicabilityJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    PmPlanRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DefinitionJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CloneSourcePartsKitId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActivatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RetiredByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_parts_kits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_notification_dispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WebhookHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_notification_dispatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_parts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NormalizedPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SupplyArrPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManufacturerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ManufacturerPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SdsDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComplianceCoreMaterialKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComplianceCoreHazardKeysJson = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_parts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_pending_catalog_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProposedKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProposedLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProposedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReviewedByPersonId = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pending_catalog_values", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_platform_event_processing_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PendingFound = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    RetriedCount = table.Column<int>(type: "integer", nullable: false),
                    AbandonedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_platform_event_processing_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_platform_outbox_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_platform_outbox_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_pm_due_scan_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    MarkedDueCount = table.Column<int>(type: "integer", nullable: false),
                    MarkedOverdueCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    WorkOrdersCreatedCount = table.Column<int>(type: "integer", nullable: false),
                    WorkOrdersLinkedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_due_scan_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_recall_audit_log_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecallCampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ServiceClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_recall_audit_log_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_recall_campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProviderRecordId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NhtsaCampaignNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NhtsaActionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ManufacturerCampaignNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CampaignTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Component = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReportReceivedDate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CampaignStartDate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CampaignEndDate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CampaignStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PotentialUnitsAffected = table.Column<int>(type: "integer", nullable: true),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Consequence = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Remedy = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ParkIt = table.Column<bool>(type: "boolean", nullable: false),
                    ParkOutside = table.Column<bool>(type: "boolean", nullable: false),
                    OverTheAirUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    RecallType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceRawJson = table.Column<string>(type: "text", nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    FetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_recall_campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_recall_make_model_aliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RawMake = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RawModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NormalizedMake = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NormalizedModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_recall_make_model_aliases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_reference_cache_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceOfTruth = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_reference_cache_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_staff_person_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActiveStatusSnapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PrimarySiteSnapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceCorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_staff_person_refs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_asset_status_rollup_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_asset_status_rollup_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_defect_escalation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LowThresholdHours = table.Column<int>(type: "integer", nullable: false),
                    MediumThresholdHours = table.Column<int>(type: "integer", nullable: false),
                    HighThresholdHours = table.Column<int>(type: "integer", nullable: false),
                    CriticalThresholdHours = table.Column<int>(type: "integer", nullable: false),
                    AutoAcknowledgeOnEscalation = table.Column<bool>(type: "boolean", nullable: false),
                    AutoCreateWorkOrderOnEscalation = table.Column<bool>(type: "boolean", nullable: false),
                    BumpSeverityOnRepeatEscalation = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnEscalation = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_defect_escalation_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_downtime_tracking_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoTrackOutOfService = table.Column<bool>(type: "boolean", nullable: false),
                    AutoTrackNotReady = table.Column<bool>(type: "boolean", nullable: false),
                    AvailabilityPeriodDays = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_downtime_tracking_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_maintenance_history_rollup_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_maintenance_history_rollup_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_notification_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationWebhookUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    NotifyOnWorkOrderCreated = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPmScheduleDue = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPmScheduleOverdue = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnDefectEscalated = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_notification_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_platform_event_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_platform_event_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_pm_due_scan_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ScanIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    BatchSize = table.Column<int>(type: "integer", nullable: false),
                    OverdueGraceDays = table.Column<int>(type: "integer", nullable: false),
                    LastRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_pm_due_scan_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_parts_demand_status_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrDemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrCallbackPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplyarrPurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrPurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_parts_demand_status_events", x => x.Id);
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
                name: "maintainarr_asset_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_types_maintainarr_asset_classes_AssetClas~",
                        column: x => x.AssetClassId,
                        principalTable: "maintainarr_asset_classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_template_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CanBeSkipped = table.Column<bool>(type: "boolean", nullable: false),
                    SkipReasonRequired = table.Column<bool>(type: "boolean", nullable: false),
                    TimingTracked = table.Column<bool>(type: "boolean", nullable: false),
                    SettingsJson = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_template_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_template_categories_maintainarr_insp~",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_history_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RollupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Detail = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_history_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_maintenance_history_events_maintainarr_maintena~",
                        column: x => x.RollupId,
                        principalTable: "maintainarr_maintenance_history_rollups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_parts_kit_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenancePartsKitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ItemDescriptionSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    SubstituteAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_parts_kit_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_maintenance_parts_kit_lines_maintainarr_mainten~",
                        column: x => x.MaintenancePartsKitId,
                        principalTable: "maintainarr_maintenance_parts_kits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_recall_campaign_applicabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecallCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelYear = table.Column<int>(type: "integer", nullable: true),
                    Make = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AssetClass = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AssetType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BodyClass = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehicleType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FuelType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EngineFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EngineManufacturer = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComponentCategory = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TireBrand = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TireLine = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TireSize = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EquipmentMake = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EquipmentModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SerialRangeStart = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SerialRangeEnd = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProductionStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProductionEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SourceRawJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_recall_campaign_applicabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_recall_campaign_applicabilities_maintainarr_rec~",
                        column: x => x.RecallCampaignId,
                        principalTable: "maintainarr_recall_campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LifecycleStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SiteRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StaffarrSiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrSiteNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: ""),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_assets_maintainarr_asset_types_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "maintainarr_asset_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_template_asset_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_template_asset_types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_template_asset_types_maintainarr_ass~",
                        column: x => x.AssetTypeId,
                        principalTable: "maintainarr_asset_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_template_asset_types_maintainarr_ins~",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_checklist_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Prompt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    HelpText = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SettingsJson = table.Column<string>(type: "text", nullable: false),
                    ControlledOptionsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    AcceptableRangeMin = table.Column<decimal>(type: "numeric", nullable: true),
                    AcceptableRangeMax = table.Column<decimal>(type: "numeric", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_checklist_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_checklist_items_maintainarr_inspecti~",
                        column: x => x.CategoryId,
                        principalTable: "maintainarr_inspection_template_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_checklist_items_maintainarr_inspect~1",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_enrichment_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SnapshotType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_enrichment_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_enrichment_snapshots_maintainarr_assets_A~",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_external_identifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdentifierType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdentifierValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    ObservedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_external_identifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_external_identifiers_maintainarr_assets_A~",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_installed_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ParentAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentComponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ComponentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Make = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PartNumberSnapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstalledPartUsageRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstallDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InstalledByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstalledMeterReading = table.Column<decimal>(type: "numeric", nullable: true),
                    RemovedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RemovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RemovedMeterReading = table.Column<decimal>(type: "numeric", nullable: true),
                    RemovalReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    WarrantyStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WarrantyEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpectedLifeHours = table.Column<decimal>(type: "numeric", nullable: true),
                    ExpectedLifeMiles = table.Column<decimal>(type: "numeric", nullable: true),
                    ExpectedLifeCycles = table.Column<int>(type: "integer", nullable: true),
                    Condition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReplacementPartRefsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DocumentRefsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DefectRefsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    WorkOrderRefsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_installed_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_installed_components_maintainarr_assets_P~",
                        column: x => x.ParentAssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_meters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeterKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BaselineReading = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentReading = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LastReadingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_meters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_meters_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_quality_holds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    HoldType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReleaseReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_quality_holds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_quality_holds_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_recall_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecallCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchBasis = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MatchConfidence = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    MatchScore = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReadinessImpact = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastRefreshedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DismissedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DismissedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DismissalReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    VerificationSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VerificationMethod = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VerifiedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EvidenceText = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ProviderRawJson = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadinessHoldId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActionStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_recall_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_recall_cases_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_recall_cases_maintainarr_recall_campaigns~",
                        column: x => x.RecallCampaignId,
                        principalTable: "maintainarr_recall_campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_recall_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CampaignNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Component = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Consequence = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Remedy = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ModelYear = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Make = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReportReceivedDate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    QualityHoldId = table.Column<Guid>(type: "uuid", nullable: true),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_recall_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_recall_snapshots_maintainarr_assets_Asset~",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_pm_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssetTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "draft"),
                    CategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    WorkTypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PriorityKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwningSiteRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwningTeamRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwningDepartmentRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OwnerRoleKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TagsJson = table.Column<string>(type: "text", nullable: false),
                    ScopeDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    DueTriggerDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    WorkPackageDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    InspectionDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    ComplianceDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    AutomationDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    AutoGenerateWorkOrder = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DefaultWorkOrderTemplateRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AutoGenerateInspection = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActivatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PausedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PausedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetiredByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RetiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_programs_maintainarr_asset_types_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "maintainarr_asset_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_programs_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_programs_maintainarr_inspection_templates_In~",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_enrichment_suggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FieldLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CurrentValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProposedValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_enrichment_suggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_enrichment_suggestions_maintainarr_asset_~",
                        column: x => x.SnapshotId,
                        principalTable: "maintainarr_asset_enrichment_snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_enrichment_suggestions_maintainarr_assets~",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_meter_readings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetMeterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadingValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DeltaFromPrevious = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsCorrection = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_meter_readings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_meter_readings_maintainarr_asset_meters_AssetMe~",
                        column: x => x.AssetMeterId,
                        principalTable: "maintainarr_asset_meters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintainarr_meter_readings_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_pm_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ScheduleMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssetMeterId = table.Column<Guid>(type: "uuid", nullable: true),
                    IntervalUsage = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    NextDueAtUsage = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    LastCompletedUsage = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false),
                    NextDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastCompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SkippedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SkippedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkippedReason = table.Column<string>(type: "text", nullable: true),
                    DueStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastDueScanAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_schedules_maintainarr_asset_meters_AssetMete~",
                        column: x => x.AssetMeterId,
                        principalTable: "maintainarr_asset_meters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_schedules_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PmScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_runs_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_runs_maintainarr_inspection_template~",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_runs_maintainarr_pm_schedules_PmSche~",
                        column: x => x.PmScheduleId,
                        principalTable: "maintainarr_pm_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_pm_occurrences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PmScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurrenceNumber = table.Column<int>(type: "integer", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueMeterType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DueMeterValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GeneratedWorkOrderRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    GeneratedInspectionRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByWorkOrderRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SkippedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkippedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SkippedReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_occurrences_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_occurrences_maintainarr_pm_schedules_PmSched~",
                        column: x => x.PmScheduleId,
                        principalTable: "maintainarr_pm_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_pm_program_schedules",
                columns: table => new
                {
                    PmProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    PmScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_program_schedules", x => new { x.PmProgramId, x.PmScheduleId });
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_program_schedules_maintainarr_pm_programs_Pm~",
                        column: x => x.PmProgramId,
                        principalTable: "maintainarr_pm_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_program_schedules_maintainarr_pm_schedules_P~",
                        column: x => x.PmScheduleId,
                        principalTable: "maintainarr_pm_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_defects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "medium"),
                    DefectType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReportSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceReferenceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IncidentReferenceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedByPersonId = table.Column<string>(type: "text", nullable: true),
                    DiscoveredByPersonId = table.Column<string>(type: "text", nullable: true),
                    CreatedByPersonId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByPersonId = table.Column<string>(type: "text", nullable: true),
                    ReportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsSafetyCritical = table.Column<bool>(type: "boolean", nullable: false),
                    IsComplianceImpacting = table.Column<bool>(type: "boolean", nullable: false),
                    IsOperabilityImpacting = table.Column<bool>(type: "boolean", nullable: false),
                    FailureMode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SystemKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComponentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Symptom = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SidePosition = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OperatingCondition = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DeferralCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReadinessNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastEscalatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalationCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_defects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_defects_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_defects_maintainarr_inspection_checklist_items_~",
                        column: x => x.ChecklistItemId,
                        principalTable: "maintainarr_inspection_checklist_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_defects_maintainarr_inspection_runs_InspectionR~",
                        column: x => x.InspectionRunId,
                        principalTable: "maintainarr_inspection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_run_answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PassFailValue = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    NumericValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    TextValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SelectedOptionsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AnsweredByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_run_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_answers_maintainarr_inspection_c~",
                        column: x => x.ChecklistItemId,
                        principalTable: "maintainarr_inspection_checklist_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_answers_maintainarr_inspection_r~",
                        column: x => x.InspectionRunId,
                        principalTable: "maintainarr_inspection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_run_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_run_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_evidence_maintainarr_inspection_~",
                        column: x => x.ChecklistItemId,
                        principalTable: "maintainarr_inspection_checklist_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_evidence_maintainarr_inspection~1",
                        column: x => x.InspectionRunId,
                        principalTable: "maintainarr_inspection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_run_pause_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    PausedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PausedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResumedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_run_pause_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_pause_events_maintainarr_inspect~",
                        column: x => x.InspectionRunId,
                        principalTable: "maintainarr_inspection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_defect_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_defect_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_defect_evidence_maintainarr_defects_DefectId",
                        column: x => x.DefectId,
                        principalTable: "maintainarr_defects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inbound_platform_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedDefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inbound_platform_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inbound_platform_events_maintainarr_defects_Cre~",
                        column: x => x.CreatedDefectId,
                        principalTable: "maintainarr_defects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    PmScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    TemplateRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WorkOrderType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OriginType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StaffarrLocationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RequiredQualificationRefsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    QualificationCheckResultsJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    DraftPlanJson = table.Column<string>(type: "text", nullable: true),
                    PlannedStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssignedTechnicianPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_orders_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_orders_maintainarr_defects_DefectId",
                        column: x => x.DefectId,
                        principalTable: "maintainarr_defects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_orders_maintainarr_pm_schedules_PmSchedule~",
                        column: x => x.PmScheduleId,
                        principalTable: "maintainarr_pm_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_permit_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecordRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StatusSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_permit_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_maintenance_permit_refs_maintainarr_work_orders~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_vendor_works",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VendorContactSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WorkDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    QuoteRecordRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApprovalRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CostEstimateSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InvoiceRecordRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WarrantyFlag = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_vendor_works", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_maintenance_vendor_works_maintainarr_work_order~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_return_to_services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiredChecksJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CompletedChecksJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    FinalInspectionRef = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FinalReadinessStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RecordRefsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_return_to_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_return_to_services_maintainarr_work_orders_Work~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_blockers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiredAction = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_blockers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_blockers_maintainarr_work_orders_Wor~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_closeouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletionSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RootCause = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PreventiveActionRecommendation = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    AssetReturnedToService = table.Column<bool>(type: "boolean", nullable: false),
                    ReturnToServiceAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnToServiceByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PostRepairInspectionRequired = table.Column<bool>(type: "boolean", nullable: false),
                    PostRepairInspectionRef = table.Column<Guid>(type: "uuid", nullable: true),
                    SupervisorReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SupervisorReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SupervisorReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ComplianceReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ComplianceReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComplianceReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    QualityReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    QualityReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    QualityReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    EvidenceRecordRefsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    UnresolvedDefectRefs = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    FollowUpWorkOrderRefs = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CustomerImpactSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    DowntimeSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FinalAssetReadinessStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FinalStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PermitRecordRefsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_closeouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_closeouts_maintainarr_work_orders_Wo~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EditedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EditedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Pinned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_comments_maintainarr_work_orders_Wor~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_evidence_maintainarr_work_orders_Wor~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_parts_demand_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    MaintenancePartId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MaintainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrDemandRefId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplyarrPurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrPurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ProcurementStatusMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LastProcurementStatusAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_parts_demand_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_parts_demand_lines_maintainarr_parts~",
                        column: x => x.MaintenancePartId,
                        principalTable: "maintainarr_parts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_parts_demand_lines_maintainarr_work_~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_task_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_task_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_task_lines_maintainarr_work_orders_W~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_technician_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssignmentRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequiredQualificationRefsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    QualificationCheckSnapshotJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_technician_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_technician_assignments_maintainarr_w~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_timeline_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActorServiceClientId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BeforeSnapshot = table.Column<string>(type: "text", nullable: true),
                    AfterSnapshot = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_timeline_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_timeline_events_maintainarr_work_ord~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_labor_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderTaskLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HoursWorked = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    LaborTypeKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LoggedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoggedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_labor_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_labor_entries_maintainarr_work_order~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_labor_entries_maintainarr_work_orde~1",
                        column: x => x.WorkOrderTaskLineId,
                        principalTable: "maintainarr_work_order_task_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_assignment_history_TenantId_AssetId_Assig~",
                table: "maintainarr_asset_assignment_history",
                columns: new[] { "TenantId", "AssetId", "AssignmentFieldKey", "EffectiveAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_availability_snapshots_TenantId",
                table: "maintainarr_asset_availability_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_availability_snapshots_TenantId_AssetId",
                table: "maintainarr_asset_availability_snapshots",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_classes_TenantId",
                table: "maintainarr_asset_classes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_classes_TenantId_ClassKey",
                table: "maintainarr_asset_classes",
                columns: new[] { "TenantId", "ClassKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_compliance_state_TenantId_AssetId",
                table: "maintainarr_asset_compliance_state",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_components_TenantId_AssetId_ComponentKey",
                table: "maintainarr_asset_components",
                columns: new[] { "TenantId", "AssetId", "ComponentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_custom_field_values_TenantId_AssetId_Fiel~",
                table: "maintainarr_asset_custom_field_values",
                columns: new[] { "TenantId", "AssetId", "FieldKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_events_TenantId",
                table: "maintainarr_asset_downtime_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_events_TenantId_AssetId_EndedAt",
                table: "maintainarr_asset_downtime_events",
                columns: new[] { "TenantId", "AssetId", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_events_TenantId_AssetId_StartedAt",
                table: "maintainarr_asset_downtime_events",
                columns: new[] { "TenantId", "AssetId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_events_TenantId_Source_EndedAt",
                table: "maintainarr_asset_downtime_events",
                columns: new[] { "TenantId", "Source", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_sync_runs_TenantId_CreatedAt",
                table: "maintainarr_asset_downtime_sync_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_snapshots_AssetId",
                table: "maintainarr_asset_enrichment_snapshots",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_snapshots_TenantId",
                table: "maintainarr_asset_enrichment_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_snapshots_TenantId_AssetId",
                table: "maintainarr_asset_enrichment_snapshots",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_snapshots_TenantId_AssetId_Pro~",
                table: "maintainarr_asset_enrichment_snapshots",
                columns: new[] { "TenantId", "AssetId", "ProviderKey", "SnapshotType", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_AssetId",
                table: "maintainarr_asset_enrichment_suggestions",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_SnapshotId",
                table: "maintainarr_asset_enrichment_suggestions",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_TenantId",
                table: "maintainarr_asset_enrichment_suggestions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_TenantId_AssetId",
                table: "maintainarr_asset_enrichment_suggestions",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_TenantId_AssetId_P~",
                table: "maintainarr_asset_enrichment_suggestions",
                columns: new[] { "TenantId", "AssetId", "ProviderKey", "FieldKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_enrichment_suggestions_TenantId_AssetId_S~",
                table: "maintainarr_asset_enrichment_suggestions",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_identifiers_AssetId",
                table: "maintainarr_asset_external_identifiers",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_identifiers_TenantId",
                table: "maintainarr_asset_external_identifiers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_identifiers_TenantId_AssetId",
                table: "maintainarr_asset_external_identifiers",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_identifiers_TenantId_AssetId_Sou~",
                table: "maintainarr_asset_external_identifiers",
                columns: new[] { "TenantId", "AssetId", "SourceSystem", "IdentifierType", "NormalizedValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_mappings_TenantId_AssetId",
                table: "maintainarr_asset_external_mappings",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_external_mappings_TenantId_SourceSystem_E~",
                table: "maintainarr_asset_external_mappings",
                columns: new[] { "TenantId", "SourceSystem", "ExternalEntityType", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_ParentAssetId",
                table: "maintainarr_asset_installed_components",
                column: "ParentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_TenantId",
                table: "maintainarr_asset_installed_components",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_TenantId_ParentAsse~1",
                table: "maintainarr_asset_installed_components",
                columns: new[] { "TenantId", "ParentAssetId", "ComponentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_TenantId_ParentAsset~",
                table: "maintainarr_asset_installed_components",
                columns: new[] { "TenantId", "ParentAssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_TenantId_ParentCompo~",
                table: "maintainarr_asset_installed_components",
                columns: new[] { "TenantId", "ParentComponentId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_location_history_TenantId_AssetId_Effecti~",
                table: "maintainarr_asset_location_history",
                columns: new[] { "TenantId", "AssetId", "EffectiveAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_location_history_TenantId_StaffarrSiteOrg~",
                table: "maintainarr_asset_location_history",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_meters_AssetId",
                table: "maintainarr_asset_meters",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_meters_TenantId",
                table: "maintainarr_asset_meters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_meters_TenantId_AssetId_MeterKey",
                table: "maintainarr_asset_meters",
                columns: new[] { "TenantId", "AssetId", "MeterKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_meters_TenantId_AssetId_Status",
                table: "maintainarr_asset_meters",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_quality_holds_AssetId",
                table: "maintainarr_asset_quality_holds",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_quality_holds_TenantId",
                table: "maintainarr_asset_quality_holds",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_quality_holds_TenantId_AssetId_Status",
                table: "maintainarr_asset_quality_holds",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_quality_holds_TenantId_SourceProduct_Sour~",
                table: "maintainarr_asset_quality_holds",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_readiness_checks_TenantId",
                table: "maintainarr_asset_readiness_checks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_readiness_checks_TenantId_AssetId_Created~",
                table: "maintainarr_asset_readiness_checks",
                columns: new[] { "TenantId", "AssetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_readiness_state_TenantId_AssetId",
                table: "maintainarr_asset_readiness_state",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_AssetId",
                table: "maintainarr_asset_recall_cases",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_RecallCampaignId",
                table: "maintainarr_asset_recall_cases",
                column: "RecallCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId",
                table: "maintainarr_asset_recall_cases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId_AssetId",
                table: "maintainarr_asset_recall_cases",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId_AssetId_RecallCampa~",
                table: "maintainarr_asset_recall_cases",
                columns: new[] { "TenantId", "AssetId", "RecallCampaignId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId_Status",
                table: "maintainarr_asset_recall_cases",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_cases_TenantId_VerificationStatus",
                table: "maintainarr_asset_recall_cases",
                columns: new[] { "TenantId", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_AssetId",
                table: "maintainarr_asset_recall_snapshots",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_TenantId",
                table: "maintainarr_asset_recall_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_TenantId_AssetId",
                table: "maintainarr_asset_recall_snapshots",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_TenantId_AssetId_Campaig~",
                table: "maintainarr_asset_recall_snapshots",
                columns: new[] { "TenantId", "AssetId", "CampaignNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_recall_snapshots_TenantId_AssetId_Status",
                table: "maintainarr_asset_recall_snapshots",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_specs_TenantId_AssetId_SpecKey",
                table: "maintainarr_asset_specs",
                columns: new[] { "TenantId", "AssetId", "SpecKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_history_TenantId_AssetId_ChangedAt",
                table: "maintainarr_asset_status_history",
                columns: new[] { "TenantId", "AssetId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_rollup_runs_TenantId_CreatedAt",
                table: "maintainarr_asset_status_rollup_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_rollups_TenantId",
                table: "maintainarr_asset_status_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_rollups_TenantId_AssetId",
                table: "maintainarr_asset_status_rollups",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_rollups_TenantId_ComputedAt",
                table: "maintainarr_asset_status_rollups",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_scope_rollups_TenantId",
                table: "maintainarr_asset_status_scope_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_scope_rollups_TenantId_ScopeType_C~",
                table: "maintainarr_asset_status_scope_rollups",
                columns: new[] { "TenantId", "ScopeType", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_scope_rollups_TenantId_ScopeType_S~",
                table: "maintainarr_asset_status_scope_rollups",
                columns: new[] { "TenantId", "ScopeType", "ScopeEntityId", "ScopeEntityKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_types_AssetClassId",
                table: "maintainarr_asset_types",
                column: "AssetClassId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_types_TenantId",
                table: "maintainarr_asset_types",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_types_TenantId_TypeKey",
                table: "maintainarr_asset_types",
                columns: new[] { "TenantId", "TypeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_AssetTypeId",
                table: "maintainarr_assets",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_TenantId",
                table: "maintainarr_assets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_TenantId_AssetTag",
                table: "maintainarr_assets",
                columns: new[] { "TenantId", "AssetTag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_TenantId_AssetTypeId",
                table: "maintainarr_assets",
                columns: new[] { "TenantId", "AssetTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_TenantId_StaffarrSiteOrgUnitId",
                table: "maintainarr_assets",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_audit_events_TenantId",
                table: "maintainarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_audit_events_TenantId_OccurredAt",
                table: "maintainarr_audit_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_audit_package_generation_jobs_CreatedAt",
                table: "maintainarr_audit_package_generation_jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_audit_package_generation_jobs_TenantId_Status_C~",
                table: "maintainarr_audit_package_generation_jobs",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_catalog_options_TenantId_CatalogId_Key_OptionTe~",
                table: "maintainarr_catalog_options",
                columns: new[] { "TenantId", "CatalogId", "Key", "OptionTenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_catalogs_TenantId_Scope_Key",
                table: "maintainarr_catalogs",
                columns: new[] { "TenantId", "Scope", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_compliance_regulatory_key_mirrors_TenantId",
                table: "maintainarr_compliance_regulatory_key_mirrors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_compliance_regulatory_key_mirrors_TenantId_Comp~",
                table: "maintainarr_compliance_regulatory_key_mirrors",
                columns: new[] { "TenantId", "ComplianceKey" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_compliance_regulatory_key_mirrors_TenantId_Subj~",
                table: "maintainarr_compliance_regulatory_key_mirrors",
                columns: new[] { "TenantId", "SubjectType", "SubjectId", "ComplianceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_escalation_events_TenantId_CreatedAt",
                table: "maintainarr_defect_escalation_events",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_escalation_events_TenantId_DefectId_Crea~",
                table: "maintainarr_defect_escalation_events",
                columns: new[] { "TenantId", "DefectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_escalation_runs_TenantId_CreatedAt",
                table: "maintainarr_defect_escalation_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_evidence_DefectId",
                table: "maintainarr_defect_evidence",
                column: "DefectId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_evidence_TenantId",
                table: "maintainarr_defect_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defect_evidence_TenantId_DefectId_CreatedAt",
                table: "maintainarr_defect_evidence",
                columns: new[] { "TenantId", "DefectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_AssetId",
                table: "maintainarr_defects",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_ChecklistItemId",
                table: "maintainarr_defects",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_InspectionRunId",
                table: "maintainarr_defects",
                column: "InspectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId",
                table: "maintainarr_defects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_AssetId_Status",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_DefectType_Status",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "DefectType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_InspectionRunId",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "InspectionRunId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_InspectionRunId_ChecklistItemId",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "InspectionRunId", "ChecklistItemId" },
                unique: true,
                filter: "\"ChecklistItemId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_ReportedByPersonId_CreatedAt",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "ReportedByPersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_ReportedByUserId_CreatedAt",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "ReportedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_ReportSource_Status",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "ReportSource", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_Status_UpdatedAt",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_audit_log_entries_TenantId",
                table: "maintainarr_external_provider_audit_log_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_audit_log_entries_TenantId_Pr~",
                table: "maintainarr_external_provider_audit_log_entries",
                columns: new[] { "TenantId", "ProviderKey", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_cache_entries_TenantId",
                table: "maintainarr_external_provider_cache_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_cache_entries_TenantId_Provi~1",
                table: "maintainarr_external_provider_cache_entries",
                columns: new[] { "TenantId", "ProviderKey", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_external_provider_cache_entries_TenantId_Provid~",
                table: "maintainarr_external_provider_cache_entries",
                columns: new[] { "TenantId", "ProviderKey", "CacheKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_fieldset_definitions_TenantId_Key_Purpose",
                table: "maintainarr_fieldset_definitions",
                columns: new[] { "TenantId", "Key", "Purpose" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_fieldset_fields_TenantId_FieldsetId_Key",
                table: "maintainarr_fieldset_fields",
                columns: new[] { "TenantId", "FieldsetId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_fleet_availability_snapshots_TenantId",
                table: "maintainarr_fleet_availability_snapshots",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_import_batches_TenantId_CreatedAt",
                table: "maintainarr_import_batches",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_import_batches_TenantId_ImportType_CreatedAt",
                table: "maintainarr_import_batches",
                columns: new[] { "TenantId", "ImportType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_CreatedDefectId",
                table: "maintainarr_inbound_platform_events",
                column: "CreatedDefectId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_TenantId",
                table: "maintainarr_inbound_platform_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_TenantId_CreatedDefectId",
                table: "maintainarr_inbound_platform_events",
                columns: new[] { "TenantId", "CreatedDefectId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_TenantId_EventKind_Crea~",
                table: "maintainarr_inbound_platform_events",
                columns: new[] { "TenantId", "EventKind", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inbound_platform_events_TenantId_SourceProduct_~",
                table: "maintainarr_inbound_platform_events",
                columns: new[] { "TenantId", "SourceProduct", "SourceEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_CategoryId",
                table: "maintainarr_inspection_checklist_items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_InspectionTemplateId",
                table: "maintainarr_inspection_checklist_items",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_TenantId",
                table: "maintainarr_inspection_checklist_items",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_TenantId_Inspection~1",
                table: "maintainarr_inspection_checklist_items",
                columns: new[] { "TenantId", "InspectionTemplateId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_checklist_items_TenantId_InspectionT~",
                table: "maintainarr_inspection_checklist_items",
                columns: new[] { "TenantId", "InspectionTemplateId", "ItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_answers_ChecklistItemId",
                table: "maintainarr_inspection_run_answers",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_answers_InspectionRunId",
                table: "maintainarr_inspection_run_answers",
                column: "InspectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_answers_TenantId",
                table: "maintainarr_inspection_run_answers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_answers_TenantId_InspectionRunId~",
                table: "maintainarr_inspection_run_answers",
                columns: new[] { "TenantId", "InspectionRunId", "ChecklistItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_evidence_ChecklistItemId",
                table: "maintainarr_inspection_run_evidence",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_evidence_InspectionRunId",
                table: "maintainarr_inspection_run_evidence",
                column: "InspectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_evidence_TenantId",
                table: "maintainarr_inspection_run_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_evidence_TenantId_InspectionRunI~",
                table: "maintainarr_inspection_run_evidence",
                columns: new[] { "TenantId", "InspectionRunId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_pause_events_InspectionRunId",
                table: "maintainarr_inspection_run_pause_events",
                column: "InspectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_pause_events_TenantId",
                table: "maintainarr_inspection_run_pause_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_pause_events_TenantId_Inspection~",
                table: "maintainarr_inspection_run_pause_events",
                columns: new[] { "TenantId", "InspectionRunId", "PausedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_AssetId",
                table: "maintainarr_inspection_runs",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_InspectionTemplateId",
                table: "maintainarr_inspection_runs",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_PmScheduleId",
                table: "maintainarr_inspection_runs",
                column: "PmScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId",
                table: "maintainarr_inspection_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_AssetId_Status",
                table: "maintainarr_inspection_runs",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_InspectionTemplateId",
                table: "maintainarr_inspection_runs",
                columns: new[] { "TenantId", "InspectionTemplateId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_PmScheduleId_Status",
                table: "maintainarr_inspection_runs",
                columns: new[] { "TenantId", "PmScheduleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_StartedByUserId_Starte~",
                table: "maintainarr_inspection_runs",
                columns: new[] { "TenantId", "StartedByUserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_asset_types_AssetTypeId",
                table: "maintainarr_inspection_template_asset_types",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_asset_types_InspectionTempl~",
                table: "maintainarr_inspection_template_asset_types",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_asset_types_TenantId",
                table: "maintainarr_inspection_template_asset_types",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_asset_types_TenantId_Inspec~",
                table: "maintainarr_inspection_template_asset_types",
                columns: new[] { "TenantId", "InspectionTemplateId", "AssetTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_categories_InspectionTempla~",
                table: "maintainarr_inspection_template_categories",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_categories_TenantId",
                table: "maintainarr_inspection_template_categories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_template_categories_TenantId_Inspect~",
                table: "maintainarr_inspection_template_categories",
                columns: new[] { "TenantId", "InspectionTemplateId", "CategoryKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_templates_TenantId",
                table: "maintainarr_inspection_templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_templates_TenantId_Status",
                table: "maintainarr_inspection_templates",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_templates_TenantId_TemplateKey",
                table: "maintainarr_inspection_templates",
                columns: new[] { "TenantId", "TemplateKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_events_RollupId",
                table: "maintainarr_maintenance_history_events",
                column: "RollupId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_events_TenantId_AssetId_Occ~",
                table: "maintainarr_maintenance_history_events",
                columns: new[] { "TenantId", "AssetId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_events_TenantId_RollupId",
                table: "maintainarr_maintenance_history_events",
                columns: new[] { "TenantId", "RollupId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_rollup_runs_TenantId_Create~",
                table: "maintainarr_maintenance_history_rollup_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_rollups_TenantId",
                table: "maintainarr_maintenance_history_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_rollups_TenantId_AssetId",
                table: "maintainarr_maintenance_history_rollups",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_rollups_TenantId_ComputedAt",
                table: "maintainarr_maintenance_history_rollups",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kit_lines_MaintenancePartsKit~",
                table: "maintainarr_maintenance_parts_kit_lines",
                column: "MaintenancePartsKitId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kit_lines_TenantId",
                table: "maintainarr_maintenance_parts_kit_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kit_lines_TenantId_Maintenan~1",
                table: "maintainarr_maintenance_parts_kit_lines",
                columns: new[] { "TenantId", "MaintenancePartsKitId", "ItemRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kit_lines_TenantId_Maintenanc~",
                table: "maintainarr_maintenance_parts_kit_lines",
                columns: new[] { "TenantId", "MaintenancePartsKitId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kits_TenantId",
                table: "maintainarr_maintenance_parts_kits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kits_TenantId_KitNumber",
                table: "maintainarr_maintenance_parts_kits",
                columns: new[] { "TenantId", "KitNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_permit_refs_TenantId",
                table: "maintainarr_maintenance_permit_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_permit_refs_TenantId_WorkOrderId_Re~",
                table: "maintainarr_maintenance_permit_refs",
                columns: new[] { "TenantId", "WorkOrderId", "RecordRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_permit_refs_WorkOrderId",
                table: "maintainarr_maintenance_permit_refs",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_vendor_works_TenantId",
                table: "maintainarr_maintenance_vendor_works",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_vendor_works_TenantId_WorkOrderId_S~",
                table: "maintainarr_maintenance_vendor_works",
                columns: new[] { "TenantId", "WorkOrderId", "SupplierRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_vendor_works_WorkOrderId",
                table: "maintainarr_maintenance_vendor_works",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_AssetId",
                table: "maintainarr_meter_readings",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_AssetMeterId",
                table: "maintainarr_meter_readings",
                column: "AssetMeterId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_TenantId",
                table: "maintainarr_meter_readings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_TenantId_AssetId_ReadAt",
                table: "maintainarr_meter_readings",
                columns: new[] { "TenantId", "AssetId", "ReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_TenantId_AssetMeterId_ReadAt",
                table: "maintainarr_meter_readings",
                columns: new[] { "TenantId", "AssetMeterId", "ReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_notification_dispatches_TenantId",
                table: "maintainarr_notification_dispatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_notification_dispatches_TenantId_DispatchStatus~",
                table: "maintainarr_notification_dispatches",
                columns: new[] { "TenantId", "DispatchStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_notification_dispatches_TenantId_EventKind_Rela~",
                table: "maintainarr_notification_dispatches",
                columns: new[] { "TenantId", "EventKind", "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_parts_TenantId",
                table: "maintainarr_parts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_parts_TenantId_NormalizedPartNumber",
                table: "maintainarr_parts",
                columns: new[] { "TenantId", "NormalizedPartNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_parts_TenantId_Status",
                table: "maintainarr_parts",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_parts_TenantId_SupplyArrPartId",
                table: "maintainarr_parts",
                columns: new[] { "TenantId", "SupplyArrPartId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_platform_event_processing_runs_TenantId_Created~",
                table: "maintainarr_platform_event_processing_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_platform_outbox_events_TenantId",
                table: "maintainarr_platform_outbox_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_platform_outbox_events_TenantId_EventKind_Creat~",
                table: "maintainarr_platform_outbox_events",
                columns: new[] { "TenantId", "EventKind", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_platform_outbox_events_TenantId_IdempotencyKey",
                table: "maintainarr_platform_outbox_events",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_platform_outbox_events_TenantId_ProcessingStatu~",
                table: "maintainarr_platform_outbox_events",
                columns: new[] { "TenantId", "ProcessingStatus", "NextRetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_due_scan_runs_TenantId_CreatedAt",
                table: "maintainarr_pm_due_scan_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_AssetId",
                table: "maintainarr_pm_occurrences",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_PmScheduleId",
                table: "maintainarr_pm_occurrences",
                column: "PmScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_TenantId",
                table: "maintainarr_pm_occurrences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_TenantId_PmScheduleId_DueAt",
                table: "maintainarr_pm_occurrences",
                columns: new[] { "TenantId", "PmScheduleId", "DueAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_TenantId_PmScheduleId_Occurrence~",
                table: "maintainarr_pm_occurrences",
                columns: new[] { "TenantId", "PmScheduleId", "OccurrenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_TenantId_Status_DueAt",
                table: "maintainarr_pm_occurrences",
                columns: new[] { "TenantId", "Status", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_program_schedules_PmProgramId_SortOrder",
                table: "maintainarr_pm_program_schedules",
                columns: new[] { "PmProgramId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_program_schedules_PmScheduleId",
                table: "maintainarr_pm_program_schedules",
                column: "PmScheduleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_AssetId",
                table: "maintainarr_pm_programs",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_AssetTypeId",
                table: "maintainarr_pm_programs",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_InspectionTemplateId",
                table: "maintainarr_pm_programs",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId",
                table: "maintainarr_pm_programs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId_AssetId",
                table: "maintainarr_pm_programs",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId_AssetTypeId",
                table: "maintainarr_pm_programs",
                columns: new[] { "TenantId", "AssetTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId_ProgramKey",
                table: "maintainarr_pm_programs",
                columns: new[] { "TenantId", "ProgramKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId_Status",
                table: "maintainarr_pm_programs",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_AssetId",
                table: "maintainarr_pm_schedules",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_AssetMeterId",
                table: "maintainarr_pm_schedules",
                column: "AssetMeterId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_TenantId",
                table: "maintainarr_pm_schedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_TenantId_AssetId_ScheduleKey",
                table: "maintainarr_pm_schedules",
                columns: new[] { "TenantId", "AssetId", "ScheduleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_TenantId_AssetMeterId_Status",
                table: "maintainarr_pm_schedules",
                columns: new[] { "TenantId", "AssetMeterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_TenantId_Status_DueStatus_NextDueAt",
                table: "maintainarr_pm_schedules",
                columns: new[] { "TenantId", "Status", "DueStatus", "NextDueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_audit_log_entries_TenantId",
                table: "maintainarr_recall_audit_log_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_audit_log_entries_TenantId_AssetId_Creat~",
                table: "maintainarr_recall_audit_log_entries",
                columns: new[] { "TenantId", "AssetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_audit_log_entries_TenantId_RecallCampaig~",
                table: "maintainarr_recall_audit_log_entries",
                columns: new[] { "TenantId", "RecallCampaignId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_campaign_applicabilities_RecallCampaignI~",
                table: "maintainarr_recall_campaign_applicabilities",
                columns: new[] { "RecallCampaignId", "ModelYear", "Make", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_campaigns_TenantId",
                table: "maintainarr_recall_campaigns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_campaigns_TenantId_SourceProvider_Source~",
                table: "maintainarr_recall_campaigns",
                columns: new[] { "TenantId", "SourceProvider", "SourceProviderRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_make_model_aliases_Provider_NormalizedMa~",
                table: "maintainarr_recall_make_model_aliases",
                columns: new[] { "Provider", "NormalizedMake", "NormalizedModel" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_recall_make_model_aliases_Provider_RawMake_RawM~",
                table: "maintainarr_recall_make_model_aliases",
                columns: new[] { "Provider", "RawMake", "RawModel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_reference_cache_entries_TenantId_SourceOfTruth_~",
                table: "maintainarr_reference_cache_entries",
                columns: new[] { "TenantId", "SourceOfTruth", "ReferenceKey", "ExternalKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_return_to_services_TenantId",
                table: "maintainarr_return_to_services",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_return_to_services_TenantId_WorkOrderId",
                table: "maintainarr_return_to_services",
                columns: new[] { "TenantId", "WorkOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_return_to_services_WorkOrderId",
                table: "maintainarr_return_to_services",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_staff_person_refs_TenantId",
                table: "maintainarr_staff_person_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_staff_person_refs_TenantId_StaffarrPersonId",
                table: "maintainarr_staff_person_refs",
                columns: new[] { "TenantId", "StaffarrPersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_asset_status_rollup_settings_TenantId",
                table: "maintainarr_tenant_asset_status_rollup_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_defect_escalation_settings_TenantId",
                table: "maintainarr_tenant_defect_escalation_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_downtime_tracking_settings_TenantId",
                table: "maintainarr_tenant_downtime_tracking_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_maintenance_history_rollup_settings_Tena~",
                table: "maintainarr_tenant_maintenance_history_rollup_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_notification_settings_TenantId",
                table: "maintainarr_tenant_notification_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_platform_event_settings_TenantId",
                table: "maintainarr_tenant_platform_event_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_pm_due_scan_settings_TenantId",
                table: "maintainarr_tenant_pm_due_scan_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_blockers_TenantId",
                table: "maintainarr_work_order_blockers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_blockers_TenantId_WorkOrderId_Create~",
                table: "maintainarr_work_order_blockers",
                columns: new[] { "TenantId", "WorkOrderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_blockers_TenantId_WorkOrderId_Source~",
                table: "maintainarr_work_order_blockers",
                columns: new[] { "TenantId", "WorkOrderId", "SourceProduct", "SourceObjectRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_blockers_WorkOrderId",
                table: "maintainarr_work_order_blockers",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_closeouts_TenantId",
                table: "maintainarr_work_order_closeouts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_closeouts_TenantId_WorkOrderId",
                table: "maintainarr_work_order_closeouts",
                columns: new[] { "TenantId", "WorkOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_closeouts_WorkOrderId",
                table: "maintainarr_work_order_closeouts",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_comments_TenantId",
                table: "maintainarr_work_order_comments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_comments_TenantId_WorkOrderId_Pinned~",
                table: "maintainarr_work_order_comments",
                columns: new[] { "TenantId", "WorkOrderId", "Pinned", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_comments_WorkOrderId",
                table: "maintainarr_work_order_comments",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_evidence_TenantId",
                table: "maintainarr_work_order_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_evidence_TenantId_WorkOrderId_Create~",
                table: "maintainarr_work_order_evidence",
                columns: new[] { "TenantId", "WorkOrderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_evidence_WorkOrderId",
                table: "maintainarr_work_order_evidence",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId",
                table: "maintainarr_work_order_labor_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId_PersonId_Logg~",
                table: "maintainarr_work_order_labor_entries",
                columns: new[] { "TenantId", "PersonId", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId_WorkOrderId_L~",
                table: "maintainarr_work_order_labor_entries",
                columns: new[] { "TenantId", "WorkOrderId", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId_WorkOrderId_S~",
                table: "maintainarr_work_order_labor_entries",
                columns: new[] { "TenantId", "WorkOrderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_WorkOrderId",
                table: "maintainarr_work_order_labor_entries",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_WorkOrderTaskLineId",
                table: "maintainarr_work_order_labor_entries",
                column: "WorkOrderTaskLineId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_MaintenancePartId",
                table: "maintainarr_work_order_parts_demand_lines",
                column: "MaintenancePartId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId",
                table: "maintainarr_work_order_parts_demand_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_Maintain~",
                table: "maintainarr_work_order_parts_demand_lines",
                columns: new[] { "TenantId", "MaintainarrPublicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_Procurem~",
                table: "maintainarr_work_order_parts_demand_lines",
                columns: new[] { "TenantId", "ProcurementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_WorkOrd~1",
                table: "maintainarr_work_order_parts_demand_lines",
                columns: new[] { "TenantId", "WorkOrderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_WorkOrde~",
                table: "maintainarr_work_order_parts_demand_lines",
                columns: new[] { "TenantId", "WorkOrderId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_WorkOrderId",
                table: "maintainarr_work_order_parts_demand_lines",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_status_events_TenantId",
                table: "maintainarr_work_order_parts_demand_status_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_status_events_TenantId_~",
                table: "maintainarr_work_order_parts_demand_status_events",
                columns: new[] { "TenantId", "SupplyarrCallbackPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_status_events_TenantId~1",
                table: "maintainarr_work_order_parts_demand_status_events",
                columns: new[] { "TenantId", "MaintainarrPublicationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_task_lines_TenantId",
                table: "maintainarr_work_order_task_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_task_lines_TenantId_WorkOrderId_Sort~",
                table: "maintainarr_work_order_task_lines",
                columns: new[] { "TenantId", "WorkOrderId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_task_lines_WorkOrderId",
                table: "maintainarr_work_order_task_lines",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_technician_assignments_TenantId",
                table: "maintainarr_work_order_technician_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_technician_assignments_TenantId_Wor~1",
                table: "maintainarr_work_order_technician_assignments",
                columns: new[] { "TenantId", "WorkOrderId", "PersonId", "AssignmentRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_technician_assignments_TenantId_Work~",
                table: "maintainarr_work_order_technician_assignments",
                columns: new[] { "TenantId", "WorkOrderId", "AssignedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_technician_assignments_WorkOrderId",
                table: "maintainarr_work_order_technician_assignments",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_timeline_events_TenantId",
                table: "maintainarr_work_order_timeline_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_timeline_events_TenantId_WorkOrderId~",
                table: "maintainarr_work_order_timeline_events",
                columns: new[] { "TenantId", "WorkOrderId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_timeline_events_WorkOrderId",
                table: "maintainarr_work_order_timeline_events",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_AssetId",
                table: "maintainarr_work_orders",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_DefectId",
                table: "maintainarr_work_orders",
                column: "DefectId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_PmScheduleId",
                table: "maintainarr_work_orders",
                column: "PmScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId",
                table: "maintainarr_work_orders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_AssetId_Status",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_AssignedTechnicianPersonId~",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "AssignedTechnicianPersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_CreatedByUserId_CreatedAt",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "CreatedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_DefectId",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "DefectId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_PmScheduleId_Status",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "PmScheduleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_WorkOrderNumber",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "WorkOrderNumber" },
                unique: true);

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
                name: "maintainarr_asset_assignment_history");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_availability_snapshots");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_compliance_state");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_components");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_custom_field_values");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_documents");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_downtime_events");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_downtime_sync_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_enrichment_suggestions");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_external_identifiers");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_external_mappings");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_installed_components");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_location_history");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_quality_holds");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_readiness_checks");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_readiness_state");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_recall_cases");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_recall_snapshots");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_specs");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_status_history");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_status_rollup_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_status_rollups");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_status_scope_rollups");

            migrationBuilder.DropTable(
                name: "maintainarr_audit_events");

            migrationBuilder.DropTable(
                name: "maintainarr_audit_package_generation_jobs");

            migrationBuilder.DropTable(
                name: "maintainarr_catalog_option_dependencies");

            migrationBuilder.DropTable(
                name: "maintainarr_catalog_options");

            migrationBuilder.DropTable(
                name: "maintainarr_catalogs");

            migrationBuilder.DropTable(
                name: "maintainarr_compliance_regulatory_key_mirrors");

            migrationBuilder.DropTable(
                name: "maintainarr_defect_escalation_events");

            migrationBuilder.DropTable(
                name: "maintainarr_defect_escalation_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_defect_evidence");

            migrationBuilder.DropTable(
                name: "maintainarr_external_provider_audit_log_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_external_provider_cache_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_fieldset_definitions");

            migrationBuilder.DropTable(
                name: "maintainarr_fieldset_fields");

            migrationBuilder.DropTable(
                name: "maintainarr_fleet_availability_snapshots");

            migrationBuilder.DropTable(
                name: "maintainarr_import_batches");

            migrationBuilder.DropTable(
                name: "maintainarr_inbound_platform_events");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_run_answers");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_run_evidence");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_run_pause_events");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_template_asset_types");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_history_events");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_history_rollup_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_parts_kit_lines");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_permit_refs");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_vendor_works");

            migrationBuilder.DropTable(
                name: "maintainarr_meter_readings");

            migrationBuilder.DropTable(
                name: "maintainarr_notification_dispatches");

            migrationBuilder.DropTable(
                name: "maintainarr_pending_catalog_values");

            migrationBuilder.DropTable(
                name: "maintainarr_platform_event_processing_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_platform_outbox_events");

            migrationBuilder.DropTable(
                name: "maintainarr_pm_due_scan_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_pm_occurrences");

            migrationBuilder.DropTable(
                name: "maintainarr_pm_program_schedules");

            migrationBuilder.DropTable(
                name: "maintainarr_recall_audit_log_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_recall_campaign_applicabilities");

            migrationBuilder.DropTable(
                name: "maintainarr_recall_make_model_aliases");

            migrationBuilder.DropTable(
                name: "maintainarr_reference_cache_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_return_to_services");

            migrationBuilder.DropTable(
                name: "maintainarr_staff_person_refs");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_asset_status_rollup_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_defect_escalation_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_downtime_tracking_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_maintenance_history_rollup_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_notification_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_platform_event_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_pm_due_scan_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_blockers");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_closeouts");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_comments");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_evidence");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_labor_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_parts_demand_status_events");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_technician_assignments");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_timeline_events");

            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_enrichment_snapshots");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_history_rollups");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropTable(
                name: "maintainarr_pm_programs");

            migrationBuilder.DropTable(
                name: "maintainarr_recall_campaigns");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_task_lines");

            migrationBuilder.DropTable(
                name: "maintainarr_parts");

            migrationBuilder.DropTable(
                name: "maintainarr_work_orders");

            migrationBuilder.DropTable(
                name: "maintainarr_defects");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_checklist_items");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_template_categories");

            migrationBuilder.DropTable(
                name: "maintainarr_pm_schedules");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_templates");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_meters");

            migrationBuilder.DropTable(
                name: "maintainarr_assets");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_types");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_classes");
        }
    }
}
