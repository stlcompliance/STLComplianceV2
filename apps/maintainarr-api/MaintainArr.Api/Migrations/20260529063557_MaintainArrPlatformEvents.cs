using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrPlatformEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_maintainarr_tenant_platform_event_settings_TenantId",
                table: "maintainarr_tenant_platform_event_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_platform_event_processing_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_platform_outbox_events");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_platform_event_settings");
        }
    }
}
