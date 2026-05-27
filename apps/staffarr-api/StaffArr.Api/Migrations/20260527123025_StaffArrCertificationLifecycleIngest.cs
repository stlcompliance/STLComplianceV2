using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrCertificationLifecycleIngest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastExternalLifecyclePublicationId",
                table: "staffarr_person_certifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_LastExternalLifecyc~",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "LastExternalLifecyclePublicationId" },
                unique: true,
                filter: "\"LastExternalLifecyclePublicationId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staffarr_person_certifications_TenantId_LastExternalLifecyc~",
                table: "staffarr_person_certifications");

            migrationBuilder.DropColumn(
                name: "LastExternalLifecyclePublicationId",
                table: "staffarr_person_certifications");
        }
    }
}
