using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrDefectEscalationWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnDefectEscalated",
                table: "maintainarr_tenant_notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EscalationCount",
                table: "maintainarr_defects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastEscalatedAt",
                table: "maintainarr_defects",
                type: "timestamp with time zone",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_Status_UpdatedAt",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

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
                name: "IX_maintainarr_tenant_defect_escalation_settings_TenantId",
                table: "maintainarr_tenant_defect_escalation_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_defect_escalation_events");

            migrationBuilder.DropTable(
                name: "maintainarr_defect_escalation_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_defect_escalation_settings");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_defects_TenantId_Status_UpdatedAt",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "NotifyOnDefectEscalated",
                table: "maintainarr_tenant_notification_settings");

            migrationBuilder.DropColumn(
                name: "EscalationCount",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "LastEscalatedAt",
                table: "maintainarr_defects");
        }
    }
}
