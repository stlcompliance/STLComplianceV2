using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreCsvBundleSdsReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_sds_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SdsKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MaterialKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RevisionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_sds_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_sds_references_compliancecore_material_keys_~",
                        column: x => x.MaterialKeyId,
                        principalTable: "compliancecore_material_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_sds_references_MaterialKeyId",
                table: "compliancecore_sds_references",
                column: "MaterialKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_sds_references_TenantId",
                table: "compliancecore_sds_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_sds_references_TenantId_SdsKey",
                table: "compliancecore_sds_references",
                columns: new[] { "TenantId", "SdsKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_sds_references");
        }
    }
}
