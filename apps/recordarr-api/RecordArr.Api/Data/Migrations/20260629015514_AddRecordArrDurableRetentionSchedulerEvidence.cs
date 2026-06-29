using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableRetentionSchedulerEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_retention_scheduler_leases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaseId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SchedulerKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AcquiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcquiredByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SchedulerRunId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_retention_scheduler_leases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_retention_scheduler_outbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboxMessageId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SchedulerRunId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MessageType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisposalReviewRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DeduplicationKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_retention_scheduler_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_retention_scheduler_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchedulerRunId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LeaseId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RanAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExecutionPolicy = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    EvaluatedRecordCount = table.Column<int>(type: "integer", nullable: false),
                    EligibleRecordCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedReviewCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedExistingReviewCount = table.Column<int>(type: "integer", nullable: false),
                    BlockedByLegalHoldCount = table.Column<int>(type: "integer", nullable: false),
                    AutomaticExecutionCount = table.Column<int>(type: "integer", nullable: false),
                    NotificationMessageCount = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_retention_scheduler_runs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_leases_TenantId",
                table: "recordarr_retention_scheduler_leases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_leases_TenantId_LeaseId",
                table: "recordarr_retention_scheduler_leases",
                columns: new[] { "TenantId", "LeaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_leases_TenantId_SchedulerKey_~",
                table: "recordarr_retention_scheduler_leases",
                columns: new[] { "TenantId", "SchedulerKey", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId",
                table: "recordarr_retention_scheduler_outbox_messages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Dedu~",
                table: "recordarr_retention_scheduler_outbox_messages",
                columns: new[] { "TenantId", "DeduplicationKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Outb~",
                table: "recordarr_retention_scheduler_outbox_messages",
                columns: new[] { "TenantId", "OutboxMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_outbox_messages_TenantId_Stat~",
                table: "recordarr_retention_scheduler_outbox_messages",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_runs_TenantId",
                table: "recordarr_retention_scheduler_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_runs_TenantId_SchedulerRunId",
                table: "recordarr_retention_scheduler_runs",
                columns: new[] { "TenantId", "SchedulerRunId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_retention_scheduler_runs_TenantId_Status_RanAt",
                table: "recordarr_retention_scheduler_runs",
                columns: new[] { "TenantId", "Status", "RanAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_retention_scheduler_leases");

            migrationBuilder.DropTable(
                name: "recordarr_retention_scheduler_outbox_messages");

            migrationBuilder.DropTable(
                name: "recordarr_retention_scheduler_runs");
        }
    }
}
