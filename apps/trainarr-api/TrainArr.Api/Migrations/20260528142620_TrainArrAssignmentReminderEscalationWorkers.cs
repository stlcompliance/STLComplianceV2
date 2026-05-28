using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrAssignmentReminderEscalationWorkers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DueReminderCount",
                table: "trainarr_training_assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EscalationCount",
                table: "trainarr_training_assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastDueReminderSentAt",
                table: "trainarr_training_assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastEscalatedAt",
                table: "trainarr_training_assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnAssignmentDueReminder",
                table: "trainarr_tenant_training_notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnAssignmentOverdueEscalation",
                table: "trainarr_tenant_training_notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "trainarr_assignment_due_reminder_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    RemindersSentCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_assignment_due_reminder_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_assignment_escalation_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalationCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_assignment_escalation_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_assignment_escalation_runs",
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
                    table.PrimaryKey("PK_trainarr_assignment_escalation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_assignment_due_reminder_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DueSoonLeadDays = table.Column<int>(type: "integer", nullable: false),
                    ReminderCooldownHours = table.Column<int>(type: "integer", nullable: false),
                    MaxRemindersPerAssignment = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_assignment_due_reminder_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_assignment_escalation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    OverdueEscalationAfterHours = table.Column<int>(type: "integer", nullable: false),
                    EscalationCooldownHours = table.Column<int>(type: "integer", nullable: false),
                    MaxEscalationsPerAssignment = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_assignment_escalation_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_due_reminder_runs_TenantId",
                table: "trainarr_assignment_due_reminder_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_due_reminder_runs_TenantId_CreatedAt",
                table: "trainarr_assignment_due_reminder_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_escalation_events_TenantId",
                table: "trainarr_assignment_escalation_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_escalation_events_TenantId_TrainingAssi~",
                table: "trainarr_assignment_escalation_events",
                columns: new[] { "TenantId", "TrainingAssignmentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_escalation_runs_TenantId",
                table: "trainarr_assignment_escalation_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_escalation_runs_TenantId_CreatedAt",
                table: "trainarr_assignment_escalation_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_assignment_due_reminder_settings_TenantId",
                table: "trainarr_tenant_assignment_due_reminder_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_assignment_escalation_settings_TenantId",
                table: "trainarr_tenant_assignment_escalation_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_assignment_due_reminder_runs");

            migrationBuilder.DropTable(
                name: "trainarr_assignment_escalation_events");

            migrationBuilder.DropTable(
                name: "trainarr_assignment_escalation_runs");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_assignment_due_reminder_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_assignment_escalation_settings");

            migrationBuilder.DropColumn(
                name: "DueReminderCount",
                table: "trainarr_training_assignments");

            migrationBuilder.DropColumn(
                name: "EscalationCount",
                table: "trainarr_training_assignments");

            migrationBuilder.DropColumn(
                name: "LastDueReminderSentAt",
                table: "trainarr_training_assignments");

            migrationBuilder.DropColumn(
                name: "LastEscalatedAt",
                table: "trainarr_training_assignments");

            migrationBuilder.DropColumn(
                name: "NotifyOnAssignmentDueReminder",
                table: "trainarr_tenant_training_notification_settings");

            migrationBuilder.DropColumn(
                name: "NotifyOnAssignmentOverdueEscalation",
                table: "trainarr_tenant_training_notification_settings");
        }
    }
}
