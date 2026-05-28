using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrEventProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_person_training_history_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDomainEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_person_training_history_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_event_processing_settings",
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
                    table.PrimaryKey("PK_trainarr_tenant_event_processing_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_domain_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_domain_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_person_training_history_entries_TenantId",
                table: "trainarr_person_training_history_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_person_training_history_entries_TenantId_SourceDom~",
                table: "trainarr_person_training_history_entries",
                columns: new[] { "TenantId", "SourceDomainEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_person_training_history_entries_TenantId_StaffarrP~",
                table: "trainarr_person_training_history_entries",
                columns: new[] { "TenantId", "StaffarrPersonId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_event_processing_settings_TenantId",
                table: "trainarr_tenant_event_processing_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_domain_events_TenantId",
                table: "trainarr_training_domain_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_domain_events_TenantId_IdempotencyKey",
                table: "trainarr_training_domain_events",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_domain_events_TenantId_ProcessingStatus_N~",
                table: "trainarr_training_domain_events",
                columns: new[] { "TenantId", "ProcessingStatus", "NextRetryAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_domain_events_TenantId_StaffarrPersonId_C~",
                table: "trainarr_training_domain_events",
                columns: new[] { "TenantId", "StaffarrPersonId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_person_training_history_entries");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_event_processing_settings");

            migrationBuilder.DropTable(
                name: "trainarr_training_domain_events");
        }
    }
}
