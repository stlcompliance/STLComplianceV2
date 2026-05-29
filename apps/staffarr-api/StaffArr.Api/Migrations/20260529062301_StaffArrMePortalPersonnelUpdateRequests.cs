using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrMePortalPersonnelUpdateRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_personnel_update_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CurrentValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequestedValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Details = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_personnel_update_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_personnel_update_requests_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_update_requests_PersonId",
                table: "staffarr_personnel_update_requests",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_update_requests_TenantId",
                table: "staffarr_personnel_update_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_update_requests_TenantId_PersonId_Submit~",
                table: "staffarr_personnel_update_requests",
                columns: new[] { "TenantId", "PersonId", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_update_requests_TenantId_Status_Submitte~",
                table: "staffarr_personnel_update_requests",
                columns: new[] { "TenantId", "Status", "SubmittedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_personnel_update_requests");
        }
    }
}
