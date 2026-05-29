using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreFactSourceSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_fact_source_sync_statuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HealthStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFailureAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConsecutiveFailureCount = table.Column<int>(type: "integer", nullable: false),
                    LastMirrorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_source_sync_statuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_source_sync_statuses_compliancecore_fac~",
                        column: x => x.FactSourceId,
                        principalTable: "compliancecore_fact_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_tenant_fact_source_sync_worker_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    LastBatchRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_tenant_fact_source_sync_worker_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_source_sync_statuses_FactSourceId",
                table: "compliancecore_fact_source_sync_statuses",
                column: "FactSourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_source_sync_statuses_TenantId",
                table: "compliancecore_fact_source_sync_statuses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_tenant_fact_source_sync_worker_settings_Tena~",
                table: "compliancecore_tenant_fact_source_sync_worker_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_fact_source_sync_statuses");

            migrationBuilder.DropTable(
                name: "compliancecore_tenant_fact_source_sync_worker_settings");
        }
    }
}
