using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrCertificationGrantIngest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_certifications_TenantId_ExternalPublication~",
                table: "staffarr_person_certifications",
                columns: new[] { "TenantId", "ExternalPublicationId" },
                unique: true,
                filter: "\"ExternalPublicationId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staffarr_person_certifications_TenantId_ExternalPublication~",
                table: "staffarr_person_certifications");
        }
    }
}
