using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreStaffArrHazComSites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffarrSiteNameSnapshot",
                table: "compliancecore_hazcom_references",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffarrSiteOrgUnitId",
                table: "compliancecore_hazcom_references",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_hazcom_references_TenantId_StaffarrSiteOrgUn~",
                table: "compliancecore_hazcom_references",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_compliancecore_hazcom_references_TenantId_StaffarrSiteOrgUn~",
                table: "compliancecore_hazcom_references");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteNameSnapshot",
                table: "compliancecore_hazcom_references");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteOrgUnitId",
                table: "compliancecore_hazcom_references");
        }
    }
}
