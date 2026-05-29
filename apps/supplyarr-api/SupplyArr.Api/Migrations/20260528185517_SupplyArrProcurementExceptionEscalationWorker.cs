using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrProcurementExceptionEscalationWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EscalationCount",
                table: "supplyarr_procurement_exceptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastEscalatedAt",
                table: "supplyarr_procurement_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_exception_escalation_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcurementExceptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: false),
                    ActionKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NotificationDispatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_exception_escalation_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_exception_escalation_runs",
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
                    table.PrimaryKey("PK_supplyarr_procurement_exception_escalation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_procurement_exception_escalation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EscalationCooldownHours = table.Column<int>(type: "integer", nullable: false),
                    MaxEscalationsPerException = table.Column<int>(type: "integer", nullable: false),
                    NotifyOnProcurementExceptionSlaEscalation = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_procurement_exception_escalation_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_LastEscalatedAt",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "LastEscalatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_events_TenantId",
                table: "supplyarr_procurement_exception_escalation_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_events_TenantId_~",
                table: "supplyarr_procurement_exception_escalation_events",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_events_TenantId~1",
                table: "supplyarr_procurement_exception_escalation_events",
                columns: new[] { "TenantId", "ProcurementExceptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_runs_TenantId",
                table: "supplyarr_procurement_exception_escalation_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_runs_TenantId_Cr~",
                table: "supplyarr_procurement_exception_escalation_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_procurement_exception_escalation_settings_~",
                table: "supplyarr_tenant_procurement_exception_escalation_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_procurement_exception_escalation_events");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_exception_escalation_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_procurement_exception_escalation_settings");

            migrationBuilder.DropIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_LastEscalatedAt",
                table: "supplyarr_procurement_exceptions");

            migrationBuilder.DropColumn(
                name: "EscalationCount",
                table: "supplyarr_procurement_exceptions");

            migrationBuilder.DropColumn(
                name: "LastEscalatedAt",
                table: "supplyarr_procurement_exceptions");
        }
    }
}
