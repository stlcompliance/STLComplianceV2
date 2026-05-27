using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrIncidentTrainarrRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_incident_trainarr_routings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrRemediationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RoutedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_incident_trainarr_routings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_incident_trainarr_routings_staffarr_personnel_inci~",
                        column: x => x.IncidentId,
                        principalTable: "staffarr_personnel_incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_trainarr_routings_IncidentId",
                table: "staffarr_incident_trainarr_routings",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_trainarr_routings_TenantId",
                table: "staffarr_incident_trainarr_routings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_trainarr_routings_TenantId_IncidentId",
                table: "staffarr_incident_trainarr_routings",
                columns: new[] { "TenantId", "IncidentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_incident_trainarr_routings_TenantId_TrainarrRemedi~",
                table: "staffarr_incident_trainarr_routings",
                columns: new[] { "TenantId", "TrainarrRemediationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_incident_trainarr_routings");
        }
    }
}
