using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreHazComReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_hazcom_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    HazComKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    LinkedSdsKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_hazcom_references", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_hazcom_references_TenantId",
                table: "compliancecore_hazcom_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_hazcom_references_TenantId_HazComKey",
                table: "compliancecore_hazcom_references",
                columns: new[] { "TenantId", "HazComKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_hazcom_references");
        }
    }
}
