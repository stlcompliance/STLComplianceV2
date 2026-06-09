using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class CurrentSchemaBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "staffarr_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_staffarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_audit_package_generation_jobs",
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
                    table.PrimaryKey("PK_staffarr_audit_package_generation_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_certification_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DefaultValidityDays = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_certification_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_incident_supply_demand_status_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_staffarr_incident_supply_demand_status_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_permission_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PermissionScope = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Sensitivity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_permission_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_export_delivery_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeliveryStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WebhookHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ExportId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersonCount = table.Column<int>(type: "integer", nullable: true),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_export_delivery_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_export_delivery_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExportId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkipReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_export_delivery_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_role_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_role_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_tenant_person_export_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    LastDeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NotificationWebhookUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    NotifyOnSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnFailure = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_tenant_person_export_schedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_tenant_worker_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ScanIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    BatchSize = table.Column<int>(type: "integer", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: true),
                    LastRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_tenant_worker_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_worker_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_worker_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_role_template_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_role_template_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_role_template_permissions_staffarr_permission_temp~",
                        column: x => x.PermissionTemplateId,
                        principalTable: "staffarr_permission_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_role_template_permissions_staffarr_role_templates_~",
                        column: x => x.RoleTemplateId,
                        principalTable: "staffarr_role_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_incident_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_incident_attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_incident_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteTypeKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_incident_notes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_incident_supply_demand_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    SupplyarrPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StaffarrPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_staffarr_incident_supply_demand_lines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_incident_trainarr_routings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrRemediationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RoutedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_incident_trainarr_routings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_internal_locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LocationType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ParentLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AllowedProductUsage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_internal_locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_internal_locations_staffarr_internal_locations_Par~",
                        column: x => x.ParentLocationId,
                        principalTable: "staffarr_internal_locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_org_unit_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentOrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamOrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionOrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_org_unit_assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_org_units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ParentOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectiveStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SiteType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    EmergencyContact = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TeamType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PositionCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DefaultSiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComplianceSensitive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SafetySensitive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanSupervise = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanApprove = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_org_units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_org_units_staffarr_org_units_DefaultSiteOrgUnitId",
                        column: x => x.DefaultSiteOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_org_units_staffarr_org_units_ParentOrgUnitId",
                        column: x => x.ParentOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GivenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FamilyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LegalFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LegalMiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LegalLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreferredName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Pronouns = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrimaryEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    AlternateEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AlternatePhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    EmploymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WorkRelationshipType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    EmploymentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PrimaryOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    WorkPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpectedStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HomeBaseLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CanLoginSnapshot = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasUserAccountSnapshot = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_people", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_people_staffarr_internal_locations_HomeBaseLocatio~",
                        column: x => x.HomeBaseLocationId,
                        principalTable: "staffarr_internal_locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_people_staffarr_org_units_PrimaryOrgUnitId",
                        column: x => x.PrimaryOrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_people_staffarr_people_ManagerPersonId",
                        column: x => x.ManagerPersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_readiness_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgUnitName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TotalMembers = table.Column<int>(type: "integer", nullable: false),
                    ReadyCount = table.Column<int>(type: "integer", nullable: false),
                    NotReadyCount = table.Column<int>(type: "integer", nullable: false),
                    OverrideCount = table.Column<int>(type: "integer", nullable: false),
                    ReadyPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    ConfidenceLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "low"),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_readiness_rollups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_readiness_rollups_staffarr_org_units_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_tenant_person_export_presets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    PresetKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_tenant_person_export_presets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_tenant_person_export_presets_staffarr_org_units_Or~",
                        column: x => x.OrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_certifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastExternalLifecyclePublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_certifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_certifications_staffarr_certification_defin~",
                        column: x => x.CertificationDefinitionId,
                        principalTable: "staffarr_certification_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_person_certifications_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_offboarding_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SeparationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SeparationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TargetEmploymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisableLoginRequested = table.Column<bool>(type: "boolean", nullable: false),
                    NewManagerPersonIdForReports = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_offboarding_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_offboarding_records_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_permission_projections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCount = table.Column<int>(type: "integer", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_permission_projections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_permission_projections_staffarr_people_Pers~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_readiness_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClearedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClearedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_readiness_overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_readiness_overrides_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_role_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_role_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_role_assignments_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_person_role_assignments_staffarr_role_templates_Ro~",
                        column: x => x.RoleTemplateId,
                        principalTable: "staffarr_role_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_training_acknowledgements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrAcknowledgementRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AssignmentReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_training_acknowledgements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_training_acknowledgements_staffarr_people_P~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_training_blockers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QualificationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BlockerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClearedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_training_blockers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_training_blockers_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_personnel_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_documents_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_personnel_history_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    IncidentCount = table.Column<int>(type: "integer", nullable: false),
                    CertificationCount = table.Column<int>(type: "integer", nullable: false),
                    PermissionCount = table.Column<int>(type: "integer", nullable: false),
                    ReadinessCount = table.Column<int>(type: "integer", nullable: false),
                    TrainingBlockerCount = table.Column<int>(type: "integer", nullable: false),
                    PersonnelNoteCount = table.Column<int>(type: "integer", nullable: false),
                    PersonnelDocumentCount = table.Column<int>(type: "integer", nullable: false),
                    LastEventAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_history_rollups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_history_rollups_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_personnel_incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCategoryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    IncidentSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IncidentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocationDetail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WitnessPersonIdsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    AdditionalInvolvedPersonIdsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EmployeeSelfReport = table.Column<bool>(type: "boolean", nullable: false),
                    ImmediateActionsTaken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RootCause = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CategoryKeysJson = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ReadinessDecision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    WorkRestriction = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReturnToWorkNeeded = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PpeConcern = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MedicalAttention = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OutOfServiceRemoveFromDuty = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FollowUpRequired = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    TrainingReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    TrainingReviewReason = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RelatedAssetReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RelatedWorkOrderReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RelatedRouteReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RelatedSupplierReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RelatedDocumentReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RelatedPolicyReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EvidencePackageRequested = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyManager = table.Column<bool>(type: "boolean", nullable: false),
                    NotifySafetyCompliance = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyHr = table.Column<bool>(type: "boolean", nullable: false),
                    CreateFollowUpTask = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceIncidentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceReferenceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_incidents_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_personnel_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VisibilityKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_notes_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_personnel_update_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CurrentValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequestedValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Details = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_update_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_update_requests_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_offboarding_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OffboardingRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Detail = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BlockerDetail = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_offboarding_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_offboarding_steps_staffarr_person_offboardi~",
                        column: x => x.OffboardingRecordId,
                        principalTable: "staffarr_person_offboarding_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_person_permission_projection_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_permission_projection_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_person_permission_projection_entries_staffarr_peop~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_person_permission_projection_entries_staffarr_pers~",
                        column: x => x.ProjectionId,
                        principalTable: "staffarr_person_permission_projections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_permission_history_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignmentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PermissionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PermissionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_permission_history_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_permission_history_events_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_permission_history_events_staffarr_permission_temp~",
                        column: x => x.PermissionTemplateId,
                        principalTable: "staffarr_permission_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_permission_history_events_staffarr_person_role_ass~",
                        column: x => x.AssignmentId,
                        principalTable: "staffarr_person_role_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_permission_history_events_staffarr_role_templates_~",
                        column: x => x.RoleTemplateId,
                        principalTable: "staffarr_role_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_personnel_history_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RollupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Detail = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalReferenceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_history_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_history_events_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_history_events_staffarr_personnel_histor~",
                        column: x => x.RollupId,
                        principalTable: "staffarr_personnel_history_rollups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_staffarr_audit_events_OccurredAt",
                table: "staffarr_audit_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_audit_events_TenantId",
                table: "staffarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_audit_package_generation_jobs_CreatedAt",
                table: "staffarr_audit_package_generation_jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_audit_package_generation_jobs_TenantId_Status_Crea~",
                table: "staffarr_audit_package_generation_jobs",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_certification_definitions_TenantId",
                table: "staffarr_certification_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_certification_definitions_TenantId_CertificationKey",
                table: "staffarr_certification_definitions",
                columns: new[] { "TenantId", "CertificationKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_attachments_IncidentId",
                table: "staffarr_incident_attachments",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_attachments_TenantId",
                table: "staffarr_incident_attachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_attachments_TenantId_IncidentId_CreatedAt",
                table: "staffarr_incident_attachments",
                columns: new[] { "TenantId", "IncidentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_notes_IncidentId",
                table: "staffarr_incident_notes",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_notes_TenantId",
                table: "staffarr_incident_notes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_notes_TenantId_IncidentId_CreatedAt",
                table: "staffarr_incident_notes",
                columns: new[] { "TenantId", "IncidentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_notes_TenantId_IncidentId_Status",
                table: "staffarr_incident_notes",
                columns: new[] { "TenantId", "IncidentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_lines_IncidentId",
                table: "staffarr_incident_supply_demand_lines",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_lines_TenantId",
                table: "staffarr_incident_supply_demand_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_lines_TenantId_IncidentId",
                table: "staffarr_incident_supply_demand_lines",
                columns: new[] { "TenantId", "IncidentId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_lines_TenantId_IncidentId_L~",
                table: "staffarr_incident_supply_demand_lines",
                columns: new[] { "TenantId", "IncidentId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_lines_TenantId_ProcurementS~",
                table: "staffarr_incident_supply_demand_lines",
                columns: new[] { "TenantId", "ProcurementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_lines_TenantId_StaffarrPubl~",
                table: "staffarr_incident_supply_demand_lines",
                columns: new[] { "TenantId", "StaffarrPublicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_status_events_TenantId",
                table: "staffarr_incident_supply_demand_status_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_status_events_TenantId_Staf~",
                table: "staffarr_incident_supply_demand_status_events",
                columns: new[] { "TenantId", "StaffarrPublicationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_supply_demand_status_events_TenantId_Supp~",
                table: "staffarr_incident_supply_demand_status_events",
                columns: new[] { "TenantId", "SupplyarrCallbackPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_trainarr_routings_IncidentId",
                table: "staffarr_incident_trainarr_routings",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_trainarr_routings_TenantId",
                table: "staffarr_incident_trainarr_routings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_trainarr_routings_TenantId_IncidentId",
                table: "staffarr_incident_trainarr_routings",
                columns: new[] { "TenantId", "IncidentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_trainarr_routings_TenantId_TrainarrRemedi~",
                table: "staffarr_incident_trainarr_routings",
                columns: new[] { "TenantId", "TrainarrRemediationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_ArchivedByUserId",
                table: "staffarr_internal_locations",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_ParentLocationId",
                table: "staffarr_internal_locations",
                column: "ParentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_SiteOrgUnitId",
                table: "staffarr_internal_locations",
                column: "SiteOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_Status",
                table: "staffarr_internal_locations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_TenantId",
                table: "staffarr_internal_locations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_TenantId_LocationNumber",
                table: "staffarr_internal_locations",
                columns: new[] { "TenantId", "LocationNumber" },
                unique: true,
                filter: "\"ParentLocationId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_internal_locations_TenantId_ParentLocationId_Locat~",
                table: "staffarr_internal_locations",
                columns: new[] { "TenantId", "ParentLocationId", "LocationNumber" },
                unique: true,
                filter: "\"ParentLocationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_DepartmentOrgUnitId",
                table: "staffarr_org_unit_assignments",
                column: "DepartmentOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_PersonId",
                table: "staffarr_org_unit_assignments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_PositionOrgUnitId",
                table: "staffarr_org_unit_assignments",
                column: "PositionOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_SiteOrgUnitId",
                table: "staffarr_org_unit_assignments",
                column: "SiteOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TeamOrgUnitId",
                table: "staffarr_org_unit_assignments",
                column: "TeamOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId",
                table: "staffarr_org_unit_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId",
                table: "staffarr_org_unit_assignments",
                columns: new[] { "TenantId", "PersonId" },
                unique: true,
                filter: "\"IsPrimary\" = TRUE AND \"Status\" IN ('planned','active')");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId_SiteOrgUnit~",
                table: "staffarr_org_unit_assignments",
                columns: new[] { "TenantId", "PersonId", "SiteOrgUnitId", "DepartmentOrgUnitId", "TeamOrgUnitId", "PositionOrgUnitId" },
                unique: true,
                filter: "\"Status\" IN ('planned','active')");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId_Status",
                table: "staffarr_org_unit_assignments",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_ArchivedByUserId",
                table: "staffarr_org_units",
                column: "ArchivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_DefaultSiteOrgUnitId",
                table: "staffarr_org_units",
                column: "DefaultSiteOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_ManagerPersonId",
                table: "staffarr_org_units",
                column: "ManagerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_ParentOrgUnitId",
                table: "staffarr_org_units",
                column: "ParentOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_TenantId",
                table: "staffarr_org_units",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_TenantId_Code",
                table: "staffarr_org_units",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "\"Code\" IS NOT NULL AND \"ParentOrgUnitId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_TenantId_ParentOrgUnitId_Code",
                table: "staffarr_org_units",
                columns: new[] { "TenantId", "ParentOrgUnitId", "Code" },
                unique: true,
                filter: "\"Code\" IS NOT NULL AND \"ParentOrgUnitId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_TenantId_UnitType_Name",
                table: "staffarr_org_units",
                columns: new[] { "TenantId", "UnitType", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_HomeBaseLocationId",
                table: "staffarr_people",
                column: "HomeBaseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_ManagerPersonId",
                table: "staffarr_people",
                column: "ManagerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_PrimaryOrgUnitId",
                table: "staffarr_people",
                column: "PrimaryOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_TenantId",
                table: "staffarr_people",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_TenantId_ExternalUserId",
                table: "staffarr_people",
                columns: new[] { "TenantId", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_TenantId_ManagerPersonId",
                table: "staffarr_people",
                columns: new[] { "TenantId", "ManagerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_TenantId_PrimaryEmail",
                table: "staffarr_people",
                columns: new[] { "TenantId", "PrimaryEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_AssignmentId",
                table: "staffarr_permission_history_events",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_PermissionTemplateId",
                table: "staffarr_permission_history_events",
                column: "PermissionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_PersonId",
                table: "staffarr_permission_history_events",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_RoleTemplateId",
                table: "staffarr_permission_history_events",
                column: "RoleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_TenantId",
                table: "staffarr_permission_history_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_TenantId_AssignmentId_Oc~",
                table: "staffarr_permission_history_events",
                columns: new[] { "TenantId", "AssignmentId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_history_events_TenantId_PersonId_Occurr~",
                table: "staffarr_permission_history_events",
                columns: new[] { "TenantId", "PersonId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_templates_TenantId",
                table: "staffarr_permission_templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_templates_TenantId_PermissionKey",
                table: "staffarr_permission_templates",
                columns: new[] { "TenantId", "PermissionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_templates_TenantId_ProductKey_Permissio~",
                table: "staffarr_permission_templates",
                columns: new[] { "TenantId", "ProductKey", "PermissionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_CertificationDefinitionId",
                table: "staffarr_person_certifications",
                column: "CertificationDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_PersonId",
                table: "staffarr_person_certifications",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId",
                table: "staffarr_person_certifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_ExternalPublication~",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "ExternalPublicationId" },
                unique: true,
                filter: "\"ExternalPublicationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_LastExternalLifecyc~",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "LastExternalLifecyclePublicationId" },
                unique: true,
                filter: "\"LastExternalLifecyclePublicationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_PersonId_Certificat~",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "PersonId", "CertificationDefinitionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_PersonId_Status",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_Status_ExpiresAt",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_export_delivery_notifications_AttemptedAt",
                table: "staffarr_person_export_delivery_notifications",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_export_delivery_notifications_TenantId",
                table: "staffarr_person_export_delivery_notifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_export_delivery_runs_StartedAt",
                table: "staffarr_person_export_delivery_runs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_export_delivery_runs_TenantId",
                table: "staffarr_person_export_delivery_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_records_PersonId",
                table: "staffarr_person_offboarding_records",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_records_TenantId",
                table: "staffarr_person_offboarding_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_records_TenantId_PersonId_Start~",
                table: "staffarr_person_offboarding_records",
                columns: new[] { "TenantId", "PersonId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_records_TenantId_PersonId_Status",
                table: "staffarr_person_offboarding_records",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_steps_OffboardingRecordId",
                table: "staffarr_person_offboarding_steps",
                column: "OffboardingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_steps_TenantId",
                table: "staffarr_person_offboarding_steps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_offboarding_steps_TenantId_OffboardingRecor~",
                table: "staffarr_person_offboarding_steps",
                columns: new[] { "TenantId", "OffboardingRecordId", "StepKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projection_entries_PersonId",
                table: "staffarr_person_permission_projection_entries",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projection_entries_ProjectionId",
                table: "staffarr_person_permission_projection_entries",
                column: "ProjectionId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projection_entries_TenantId",
                table: "staffarr_person_permission_projection_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projection_entries_TenantId_Pers~",
                table: "staffarr_person_permission_projection_entries",
                columns: new[] { "TenantId", "PersonId", "PermissionKey", "ScopeType", "ScopeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projections_PersonId",
                table: "staffarr_person_permission_projections",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projections_TenantId",
                table: "staffarr_person_permission_projections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projections_TenantId_ComputedAt",
                table: "staffarr_person_permission_projections",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_permission_projections_TenantId_PersonId",
                table: "staffarr_person_permission_projections",
                columns: new[] { "TenantId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_readiness_overrides_PersonId",
                table: "staffarr_person_readiness_overrides",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_readiness_overrides_TenantId",
                table: "staffarr_person_readiness_overrides",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_readiness_overrides_TenantId_PersonId_Grant~",
                table: "staffarr_person_readiness_overrides",
                columns: new[] { "TenantId", "PersonId", "GrantedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_readiness_overrides_TenantId_PersonId_Status",
                table: "staffarr_person_readiness_overrides",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_PersonId",
                table: "staffarr_person_role_assignments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_RoleTemplateId",
                table: "staffarr_person_role_assignments",
                column: "RoleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_TenantId",
                table: "staffarr_person_role_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_TenantId_PersonId_RoleTemp~",
                table: "staffarr_person_role_assignments",
                columns: new[] { "TenantId", "PersonId", "RoleTemplateId", "ScopeType", "ScopeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_TenantId_PersonId_Status",
                table: "staffarr_person_role_assignments",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_TenantId_PersonId_Status_E~",
                table: "staffarr_person_role_assignments",
                columns: new[] { "TenantId", "PersonId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_acknowledgements_PersonId",
                table: "staffarr_person_training_acknowledgements",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_acknowledgements_TenantId",
                table: "staffarr_person_training_acknowledgements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_acknowledgements_TenantId_PersonId~",
                table: "staffarr_person_training_acknowledgements",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_acknowledgements_TenantId_Trainarr~",
                table: "staffarr_person_training_acknowledgements",
                columns: new[] { "TenantId", "TrainarrAcknowledgementRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_blockers_PersonId",
                table: "staffarr_person_training_blockers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_blockers_TenantId",
                table: "staffarr_person_training_blockers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_blockers_TenantId_PersonId_Status",
                table: "staffarr_person_training_blockers",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_training_blockers_TenantId_TrainarrPublicat~",
                table: "staffarr_person_training_blockers",
                columns: new[] { "TenantId", "TrainarrPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_documents_PersonId",
                table: "staffarr_personnel_documents",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_documents_TenantId",
                table: "staffarr_personnel_documents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_documents_TenantId_DocumentTypeKey_Status",
                table: "staffarr_personnel_documents",
                columns: new[] { "TenantId", "DocumentTypeKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_documents_TenantId_PersonId_CreatedAt",
                table: "staffarr_personnel_documents",
                columns: new[] { "TenantId", "PersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_PersonId",
                table: "staffarr_personnel_history_events",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_RollupId",
                table: "staffarr_personnel_history_events",
                column: "RollupId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_TenantId",
                table: "staffarr_personnel_history_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_TenantId_PersonId_EntryId",
                table: "staffarr_personnel_history_events",
                columns: new[] { "TenantId", "PersonId", "EntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_events_TenantId_PersonId_Occurre~",
                table: "staffarr_personnel_history_events",
                columns: new[] { "TenantId", "PersonId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_rollups_PersonId",
                table: "staffarr_personnel_history_rollups",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_rollups_TenantId",
                table: "staffarr_personnel_history_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_rollups_TenantId_ComputedAt",
                table: "staffarr_personnel_history_rollups",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_history_rollups_TenantId_PersonId",
                table: "staffarr_personnel_history_rollups",
                columns: new[] { "TenantId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_PersonId",
                table: "staffarr_personnel_incidents",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_source_incident",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "SourceProduct", "SourceIncidentId" },
                unique: true,
                filter: "\"SourceIncidentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId",
                table: "staffarr_personnel_incidents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_IncidentType_Reported~",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "IncidentType", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_PersonId_ReportedAt",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "PersonId", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_ReadinessDecision_Rep~",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "ReadinessDecision", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_Status_ReportedAt",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "Status", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_notes_PersonId",
                table: "staffarr_personnel_notes",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_notes_TenantId",
                table: "staffarr_personnel_notes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_notes_TenantId_PersonId_CreatedAt",
                table: "staffarr_personnel_notes",
                columns: new[] { "TenantId", "PersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_update_requests_PersonId",
                table: "staffarr_personnel_update_requests",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_update_requests_TenantId",
                table: "staffarr_personnel_update_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_update_requests_TenantId_PersonId_Submit~",
                table: "staffarr_personnel_update_requests",
                columns: new[] { "TenantId", "PersonId", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_update_requests_TenantId_Status_Submitte~",
                table: "staffarr_personnel_update_requests",
                columns: new[] { "TenantId", "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_readiness_rollups_OrgUnitId",
                table: "staffarr_readiness_rollups",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_readiness_rollups_TenantId",
                table: "staffarr_readiness_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_readiness_rollups_TenantId_ScopeType_ComputedAt",
                table: "staffarr_readiness_rollups",
                columns: new[] { "TenantId", "ScopeType", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_readiness_rollups_TenantId_ScopeType_OrgUnitId",
                table: "staffarr_readiness_rollups",
                columns: new[] { "TenantId", "ScopeType", "OrgUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_template_permissions_PermissionTemplateId",
                table: "staffarr_role_template_permissions",
                column: "PermissionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_template_permissions_RoleTemplateId",
                table: "staffarr_role_template_permissions",
                column: "RoleTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_template_permissions_TenantId",
                table: "staffarr_role_template_permissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_template_permissions_TenantId_RoleTemplateId_~",
                table: "staffarr_role_template_permissions",
                columns: new[] { "TenantId", "RoleTemplateId", "PermissionTemplateId", "ScopeType", "ScopeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_templates_TenantId",
                table: "staffarr_role_templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_role_templates_TenantId_RoleKey",
                table: "staffarr_role_templates",
                columns: new[] { "TenantId", "RoleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_person_export_presets_OrgUnitId",
                table: "staffarr_tenant_person_export_presets",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_person_export_presets_TenantId",
                table: "staffarr_tenant_person_export_presets",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_person_export_schedules_TenantId",
                table: "staffarr_tenant_person_export_schedules",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_person_export_schedules_TenantId_IsEnabled_~",
                table: "staffarr_tenant_person_export_schedules",
                columns: new[] { "TenantId", "IsEnabled", "LastDeliveredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_worker_settings_TenantId",
                table: "staffarr_tenant_worker_settings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_worker_settings_TenantId_WorkerKey",
                table: "staffarr_tenant_worker_settings",
                columns: new[] { "TenantId", "WorkerKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_worker_runs_TenantId",
                table: "staffarr_worker_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_worker_runs_TenantId_WorkerKey_StartedAt",
                table: "staffarr_worker_runs",
                columns: new[] { "TenantId", "WorkerKey", "StartedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_incident_attachments_staffarr_personnel_incidents_~",
                table: "staffarr_incident_attachments",
                column: "IncidentId",
                principalTable: "staffarr_personnel_incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_incident_notes_staffarr_personnel_incidents_Incide~",
                table: "staffarr_incident_notes",
                column: "IncidentId",
                principalTable: "staffarr_personnel_incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_incident_supply_demand_lines_staffarr_personnel_in~",
                table: "staffarr_incident_supply_demand_lines",
                column: "IncidentId",
                principalTable: "staffarr_personnel_incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_incident_trainarr_routings_staffarr_personnel_inci~",
                table: "staffarr_incident_trainarr_routings",
                column: "IncidentId",
                principalTable: "staffarr_personnel_incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_internal_locations_staffarr_org_units_SiteOrgUnitId",
                table: "staffarr_internal_locations",
                column: "SiteOrgUnitId",
                principalTable: "staffarr_org_units",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_internal_locations_staffarr_people_ArchivedByUserId",
                table: "staffarr_internal_locations",
                column: "ArchivedByUserId",
                principalTable: "staffarr_people",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_unit_assignments_staffarr_org_units_Department~",
                table: "staffarr_org_unit_assignments",
                column: "DepartmentOrgUnitId",
                principalTable: "staffarr_org_units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_unit_assignments_staffarr_org_units_PositionOr~",
                table: "staffarr_org_unit_assignments",
                column: "PositionOrgUnitId",
                principalTable: "staffarr_org_units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_unit_assignments_staffarr_org_units_SiteOrgUni~",
                table: "staffarr_org_unit_assignments",
                column: "SiteOrgUnitId",
                principalTable: "staffarr_org_units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_unit_assignments_staffarr_org_units_TeamOrgUni~",
                table: "staffarr_org_unit_assignments",
                column: "TeamOrgUnitId",
                principalTable: "staffarr_org_units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_unit_assignments_staffarr_people_PersonId",
                table: "staffarr_org_unit_assignments",
                column: "PersonId",
                principalTable: "staffarr_people",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_units_staffarr_people_ArchivedByUserId",
                table: "staffarr_org_units",
                column: "ArchivedByUserId",
                principalTable: "staffarr_people",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_units_staffarr_people_ManagerPersonId",
                table: "staffarr_org_units",
                column: "ManagerPersonId",
                principalTable: "staffarr_people",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_internal_locations_staffarr_org_units_SiteOrgUnitId",
                table: "staffarr_internal_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_people_staffarr_org_units_PrimaryOrgUnitId",
                table: "staffarr_people");

            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_internal_locations_staffarr_people_ArchivedByUserId",
                table: "staffarr_internal_locations");

            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "staffarr_audit_events");

            migrationBuilder.DropTable(
                name: "staffarr_audit_package_generation_jobs");

            migrationBuilder.DropTable(
                name: "staffarr_incident_attachments");

            migrationBuilder.DropTable(
                name: "staffarr_incident_notes");

            migrationBuilder.DropTable(
                name: "staffarr_incident_supply_demand_lines");

            migrationBuilder.DropTable(
                name: "staffarr_incident_supply_demand_status_events");

            migrationBuilder.DropTable(
                name: "staffarr_incident_trainarr_routings");

            migrationBuilder.DropTable(
                name: "staffarr_org_unit_assignments");

            migrationBuilder.DropTable(
                name: "staffarr_permission_history_events");

            migrationBuilder.DropTable(
                name: "staffarr_person_certifications");

            migrationBuilder.DropTable(
                name: "staffarr_person_export_delivery_notifications");

            migrationBuilder.DropTable(
                name: "staffarr_person_export_delivery_runs");

            migrationBuilder.DropTable(
                name: "staffarr_person_offboarding_steps");

            migrationBuilder.DropTable(
                name: "staffarr_person_permission_projection_entries");

            migrationBuilder.DropTable(
                name: "staffarr_person_readiness_overrides");

            migrationBuilder.DropTable(
                name: "staffarr_person_training_acknowledgements");

            migrationBuilder.DropTable(
                name: "staffarr_person_training_blockers");

            migrationBuilder.DropTable(
                name: "staffarr_personnel_documents");

            migrationBuilder.DropTable(
                name: "staffarr_personnel_history_events");

            migrationBuilder.DropTable(
                name: "staffarr_personnel_notes");

            migrationBuilder.DropTable(
                name: "staffarr_personnel_update_requests");

            migrationBuilder.DropTable(
                name: "staffarr_readiness_rollups");

            migrationBuilder.DropTable(
                name: "staffarr_role_template_permissions");

            migrationBuilder.DropTable(
                name: "staffarr_tenant_person_export_presets");

            migrationBuilder.DropTable(
                name: "staffarr_tenant_person_export_schedules");

            migrationBuilder.DropTable(
                name: "staffarr_tenant_worker_settings");

            migrationBuilder.DropTable(
                name: "staffarr_worker_runs");

            migrationBuilder.DropTable(
                name: "staffarr_personnel_incidents");

            migrationBuilder.DropTable(
                name: "staffarr_person_role_assignments");

            migrationBuilder.DropTable(
                name: "staffarr_certification_definitions");

            migrationBuilder.DropTable(
                name: "staffarr_person_offboarding_records");

            migrationBuilder.DropTable(
                name: "staffarr_person_permission_projections");

            migrationBuilder.DropTable(
                name: "staffarr_personnel_history_rollups");

            migrationBuilder.DropTable(
                name: "staffarr_permission_templates");

            migrationBuilder.DropTable(
                name: "staffarr_role_templates");

            migrationBuilder.DropTable(
                name: "staffarr_org_units");

            migrationBuilder.DropTable(
                name: "staffarr_people");

            migrationBuilder.DropTable(
                name: "staffarr_internal_locations");
        }
    }
}
