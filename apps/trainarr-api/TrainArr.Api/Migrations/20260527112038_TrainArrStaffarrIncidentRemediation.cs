using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrStaffarrIncidentRemediation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_staffarr_incident_remediations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCategoryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_staffarr_incident_remediations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_audit_events_OccurredAt",
                table: "trainarr_audit_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_audit_events_TenantId",
                table: "trainarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_incident_remediations_TenantId",
                table: "trainarr_staffarr_incident_remediations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_incident_remediations_TenantId_StaffarrIn~",
                table: "trainarr_staffarr_incident_remediations",
                columns: new[] { "TenantId", "StaffarrIncidentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_incident_remediations_TenantId_StaffarrPe~",
                table: "trainarr_staffarr_incident_remediations",
                columns: new[] { "TenantId", "StaffarrPersonId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_audit_events");

            migrationBuilder.DropTable(
                name: "trainarr_staffarr_incident_remediations");
        }
    }
}
