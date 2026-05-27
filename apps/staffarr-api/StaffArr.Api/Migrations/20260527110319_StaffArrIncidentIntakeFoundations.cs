using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrIncidentIntakeFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_personnel_incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCategoryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_incidents_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_PersonId",
                table: "staffarr_personnel_incidents",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId",
                table: "staffarr_personnel_incidents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_PersonId_ReportedAt",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "PersonId", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_Status_ReportedAt",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "Status", "ReportedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_personnel_incidents");
        }
    }
}
