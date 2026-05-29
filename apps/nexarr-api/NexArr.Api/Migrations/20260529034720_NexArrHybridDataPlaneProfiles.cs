using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrHybridDataPlaneProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexarr_tenant_product_data_plane_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeploymentMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DataEndpointUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TrustStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_product_data_plane_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_product_data_plane_profiles_product_catalog_P~",
                        column: x => x.ProductKey,
                        principalTable: "product_catalog",
                        principalColumn: "ProductKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexarr_tenant_product_data_plane_profiles_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_data_plane_profiles_ProductKey",
                table: "nexarr_tenant_product_data_plane_profiles",
                column: "ProductKey");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_product_data_plane_profiles_TenantId_ProductK~",
                table: "nexarr_tenant_product_data_plane_profiles",
                columns: new[] { "TenantId", "ProductKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexarr_tenant_product_data_plane_profiles");
        }
    }
}
