using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrApprovalReminderWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_approval_reminder_runs",
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
                    table.PrimaryKey("PK_supplyarr_approval_reminder_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_approval_reminder_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    PendingSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastReminderSentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReminderCount = table.Column<int>(type: "integer", nullable: false),
                    LastReminderEventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_approval_reminder_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_approval_reminder_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PrReminderAfterHours = table.Column<int>(type: "integer", nullable: false),
                    PoReminderAfterHours = table.Column<int>(type: "integer", nullable: false),
                    ReminderCooldownHours = table.Column<int>(type: "integer", nullable: false),
                    MaxRemindersPerSubject = table.Column<int>(type: "integer", nullable: false),
                    NotifyOnPrApprovalReminder = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPoApprovalReminder = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_approval_reminder_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_runs_TenantId",
                table: "supplyarr_approval_reminder_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_runs_TenantId_CreatedAt",
                table: "supplyarr_approval_reminder_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_states_TenantId",
                table: "supplyarr_approval_reminder_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_states_TenantId_LastReminderSen~",
                table: "supplyarr_approval_reminder_states",
                columns: new[] { "TenantId", "LastReminderSentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_states_TenantId_SubjectType_Sub~",
                table: "supplyarr_approval_reminder_states",
                columns: new[] { "TenantId", "SubjectType", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_approval_reminder_settings_TenantId",
                table: "supplyarr_tenant_approval_reminder_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_approval_reminder_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_approval_reminder_states");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_approval_reminder_settings");
        }
    }
}
