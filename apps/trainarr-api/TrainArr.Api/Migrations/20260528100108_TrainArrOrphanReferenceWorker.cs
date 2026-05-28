using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrOrphanReferenceWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_orphan_reference_findings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SampleSourceEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SampleSourceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AffectedSourceCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FirstDetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastDetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_orphan_reference_findings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_orphan_reference_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferencesCheckedCount = table.Column<int>(type: "integer", nullable: false),
                    FindingsDetectedCount = table.Column<int>(type: "integer", nullable: false),
                    FindingsResolvedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_orphan_reference_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_orphan_reference_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ScanStalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_orphan_reference_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_findings_TenantId",
                table: "trainarr_orphan_reference_findings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_findings_TenantId_IsActive_LastDe~",
                table: "trainarr_orphan_reference_findings",
                columns: new[] { "TenantId", "IsActive", "LastDetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_findings_TenantId_ReferenceKind_R~",
                table: "trainarr_orphan_reference_findings",
                columns: new[] { "TenantId", "ReferenceKind", "ReferenceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_runs_TenantId",
                table: "trainarr_orphan_reference_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_runs_TenantId_ProcessedAt",
                table: "trainarr_orphan_reference_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_orphan_reference_settings_TenantId",
                table: "trainarr_tenant_orphan_reference_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_orphan_reference_findings");

            migrationBuilder.DropTable(
                name: "trainarr_orphan_reference_runs");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_orphan_reference_settings");
        }
    }
}
