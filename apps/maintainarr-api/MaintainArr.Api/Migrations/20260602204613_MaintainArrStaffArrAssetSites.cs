using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrStaffArrAssetSites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffarrSiteNameSnapshot",
                table: "maintainarr_assets",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffarrSiteOrgUnitId",
                table: "maintainarr_assets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffarrSiteNameSnapshot",
                table: "maintainarr_asset_location_history",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffarrSiteOrgUnitId",
                table: "maintainarr_asset_location_history",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_assets_TenantId_StaffarrSiteOrgUnitId",
                table: "maintainarr_assets",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_location_history_TenantId_StaffarrSiteOrg~",
                table: "maintainarr_asset_location_history",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_maintainarr_assets_TenantId_StaffarrSiteOrgUnitId",
                table: "maintainarr_assets");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_asset_location_history_TenantId_StaffarrSiteOrg~",
                table: "maintainarr_asset_location_history");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteNameSnapshot",
                table: "maintainarr_assets");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteOrgUnitId",
                table: "maintainarr_assets");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteNameSnapshot",
                table: "maintainarr_asset_location_history");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteOrgUnitId",
                table: "maintainarr_asset_location_history");
        }
    }
}
