using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPlatformFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_audit_findings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FindingType = table.Column<string>(type: "text", nullable: false),
                    AuditRef = table.Column<string>(type: "text", nullable: true),
                    NonconformanceRef = table.Column<string>(type: "text", nullable: true),
                    CapaRef = table.Column<string>(type: "text", nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_audit_findings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_capas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CapaType = table.Column<string>(type: "text", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    SponsorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RootCauseSummary = table.Column<string>(type: "text", nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RelatedNonconformanceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RelatedAuditFindingRefs = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_capas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_nonconformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NonconformanceType = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CustomerImpact = table.Column<string>(type: "text", nullable: true),
                    SupplierImpact = table.Column<string>(type: "text", nullable: true),
                    SafetyImpact = table.Column<string>(type: "text", nullable: true),
                    ComplianceImpact = table.Column<string>(type: "text", nullable: true),
                    RecurrenceFlag = table.Column<bool>(type: "boolean", nullable: false),
                    RepeatOfNonconformanceRef = table.Column<string>(type: "text", nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_nonconformances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_audits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AuditType = table.Column<string>(type: "text", nullable: false),
                    AuditScope = table.Column<string>(type: "text", nullable: true),
                    AuditorPersonIds = table.Column<string[]>(type: "text[]", nullable: false),
                    LeadAuditorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffArrSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffArrLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierRef = table.Column<string>(type: "text", nullable: true),
                    CustomerRef = table.Column<string>(type: "text", nullable: true),
                    PlannedStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChecklistRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    FindingRefs = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_holds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HoldType = table.Column<string>(type: "text", nullable: false),
                    HoldScope = table.Column<string>(type: "text", nullable: false),
                    HoldReason = table.Column<string>(type: "text", nullable: true),
                    ReleaseReason = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ConditionalReleaseTerms = table.Column<string>(type: "text", nullable: true),
                    QuantityHeld = table.Column<decimal>(type: "numeric", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: true),
                    LotNumber = table.Column<string>(type: "text", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    PlacedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlacedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_holds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_scorecards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TargetType = table.Column<string>(type: "text", nullable: false),
                    TargetRef = table.Column<string>(type: "text", nullable: false),
                    PeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OverallScore = table.Column<decimal>(type: "numeric", nullable: true),
                    QualityStatus = table.Column<string>(type: "text", nullable: false),
                    Trend = table.Column<string>(type: "text", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<string>(type: "text", nullable: false),
                    ReviewedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetricRefs = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_scorecards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_status_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TargetProduct = table.Column<string>(type: "text", nullable: false),
                    TargetObjectRef = table.Column<string>(type: "text", nullable: false),
                    QualityStatus = table.Column<string>(type: "text", nullable: false),
                    ActiveHoldRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OpenNonconformanceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OpenCapaRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OpenFindingRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    LastReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_status_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_timeline_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Details = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_timeline_events", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_audit_findings_TenantId",
                table: "assurarr_audit_findings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_audit_findings_TenantId_Number",
                table: "assurarr_audit_findings",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_audit_findings_TenantId_Status",
                table: "assurarr_audit_findings",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capas_TenantId",
                table: "assurarr_capas",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capas_TenantId_Number",
                table: "assurarr_capas",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capas_TenantId_Status",
                table: "assurarr_capas",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_nonconformances_TenantId",
                table: "assurarr_nonconformances",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_nonconformances_TenantId_Number",
                table: "assurarr_nonconformances",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_nonconformances_TenantId_Status",
                table: "assurarr_nonconformances",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audits_TenantId",
                table: "assurarr_quality_audits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audits_TenantId_Number",
                table: "assurarr_quality_audits",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audits_TenantId_Status",
                table: "assurarr_quality_audits",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_holds_TenantId",
                table: "assurarr_quality_holds",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_holds_TenantId_Number",
                table: "assurarr_quality_holds",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_holds_TenantId_Status",
                table: "assurarr_quality_holds",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_scorecards_TenantId",
                table: "assurarr_quality_scorecards",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_scorecards_TenantId_Number",
                table: "assurarr_quality_scorecards",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_scorecards_TenantId_Status",
                table: "assurarr_quality_scorecards",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_status_snapshots_TenantId",
                table: "assurarr_quality_status_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_status_snapshots_TenantId_Number",
                table: "assurarr_quality_status_snapshots",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_status_snapshots_TenantId_Status",
                table: "assurarr_quality_status_snapshots",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_timeline_events_TenantId_OccurredAt",
                table: "assurarr_timeline_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_timeline_events_TenantId_SubjectType_SubjectId",
                table: "assurarr_timeline_events",
                columns: new[] { "TenantId", "SubjectType", "SubjectId" });

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
                name: "assurarr_audit_findings");

            migrationBuilder.DropTable(
                name: "assurarr_capas");

            migrationBuilder.DropTable(
                name: "assurarr_nonconformances");

            migrationBuilder.DropTable(
                name: "assurarr_quality_audits");

            migrationBuilder.DropTable(
                name: "assurarr_quality_holds");

            migrationBuilder.DropTable(
                name: "assurarr_quality_scorecards");

            migrationBuilder.DropTable(
                name: "assurarr_quality_status_snapshots");

            migrationBuilder.DropTable(
                name: "assurarr_timeline_events");

            migrationBuilder.DropTable(
                name: "platform_metadata");
        }
    }
}
