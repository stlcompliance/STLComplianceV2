using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableAuditEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditEventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Action = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActorServiceClientId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExternalShareId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PreviousEventHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EventHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_audit_events_TenantId",
                table: "recordarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_audit_events_TenantId_Action_OccurredAt",
                table: "recordarr_audit_events",
                columns: new[] { "TenantId", "Action", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_audit_events_TenantId_AuditEventId",
                table: "recordarr_audit_events",
                columns: new[] { "TenantId", "AuditEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_audit_events_TenantId_EventHash",
                table: "recordarr_audit_events",
                columns: new[] { "TenantId", "EventHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_audit_events_TenantId_RecordId_OccurredAt",
                table: "recordarr_audit_events",
                columns: new[] { "TenantId", "RecordId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_audit_events");
        }
    }
}
