using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrPartCatalogFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_part_catalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_catalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_parts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCatalogId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ManufacturerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_parts_supplyarr_part_catalogs_PartCatalogId",
                        column: x => x.PartCatalogId,
                        principalTable: "supplyarr_part_catalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_manufacturer_aliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ManufacturerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_manufacturer_aliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_manufacturer_aliases_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_links_supplyarr_external_parties_Exte~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_links_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_catalogs_TenantId",
                table: "supplyarr_part_catalogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_catalogs_TenantId_CatalogKey",
                table: "supplyarr_part_catalogs",
                columns: new[] { "TenantId", "CatalogKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_catalogs_TenantId_Status",
                table: "supplyarr_part_catalogs",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_manufacturer_aliases_PartId",
                table: "supplyarr_part_manufacturer_aliases",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_manufacturer_aliases_TenantId",
                table: "supplyarr_part_manufacturer_aliases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_manufacturer_aliases_TenantId_PartId",
                table: "supplyarr_part_manufacturer_aliases",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_manufacturer_aliases_TenantId_PartId_AliasKey",
                table: "supplyarr_part_manufacturer_aliases",
                columns: new[] { "TenantId", "PartId", "AliasKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_ExternalPartyId",
                table: "supplyarr_part_vendor_links",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_PartId",
                table: "supplyarr_part_vendor_links",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_TenantId",
                table: "supplyarr_part_vendor_links",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_TenantId_PartId",
                table: "supplyarr_part_vendor_links",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_TenantId_PartId_ExternalPartyId",
                table: "supplyarr_part_vendor_links",
                columns: new[] { "TenantId", "PartId", "ExternalPartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_PartCatalogId",
                table: "supplyarr_parts",
                column: "PartCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_TenantId",
                table: "supplyarr_parts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_TenantId_CategoryKey_Status",
                table: "supplyarr_parts",
                columns: new[] { "TenantId", "CategoryKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_TenantId_PartCatalogId",
                table: "supplyarr_parts",
                columns: new[] { "TenantId", "PartCatalogId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_TenantId_PartKey",
                table: "supplyarr_parts",
                columns: new[] { "TenantId", "PartKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_part_manufacturer_aliases");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_links");

            migrationBuilder.DropTable(
                name: "supplyarr_parts");

            migrationBuilder.DropTable(
                name: "supplyarr_part_catalogs");
        }
    }
}
