using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceVendorWorkPortalAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PortalAccessCode",
                table: "maintainarr_maintenance_vendor_works",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PortalAccessCodeIssuedAt",
                table: "maintainarr_maintenance_vendor_works",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PortalAccessExpiresAt",
                table: "maintainarr_maintenance_vendor_works",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PortalAccessOpenedAt",
                table: "maintainarr_maintenance_vendor_works",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PortalAccessRevokedAt",
                table: "maintainarr_maintenance_vendor_works",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PortalAccessStatus",
                table: "maintainarr_maintenance_vendor_works",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "draft");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_vendor_works_PortalAccessCode",
                table: "maintainarr_maintenance_vendor_works",
                column: "PortalAccessCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_maintainarr_maintenance_vendor_works_PortalAccessCode",
                table: "maintainarr_maintenance_vendor_works");

            migrationBuilder.DropColumn(
                name: "PortalAccessCode",
                table: "maintainarr_maintenance_vendor_works");

            migrationBuilder.DropColumn(
                name: "PortalAccessCodeIssuedAt",
                table: "maintainarr_maintenance_vendor_works");

            migrationBuilder.DropColumn(
                name: "PortalAccessExpiresAt",
                table: "maintainarr_maintenance_vendor_works");

            migrationBuilder.DropColumn(
                name: "PortalAccessOpenedAt",
                table: "maintainarr_maintenance_vendor_works");

            migrationBuilder.DropColumn(
                name: "PortalAccessRevokedAt",
                table: "maintainarr_maintenance_vendor_works");

            migrationBuilder.DropColumn(
                name: "PortalAccessStatus",
                table: "maintainarr_maintenance_vendor_works");
        }
    }
}
