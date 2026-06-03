using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPersonRoleAssignmentExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "staffarr_person_role_assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_role_assignments_TenantId_PersonId_Status_E~",
                table: "staffarr_person_role_assignments",
                columns: new[] { "TenantId", "PersonId", "Status", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staffarr_person_role_assignments_TenantId_PersonId_Status_E~",
                table: "staffarr_person_role_assignments");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "staffarr_person_role_assignments");
        }
    }
}
