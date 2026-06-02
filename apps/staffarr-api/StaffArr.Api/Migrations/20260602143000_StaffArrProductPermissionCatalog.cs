using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrProductPermissionCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "staffarr_permission_templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermissionScope",
                table: "staffarr_permission_templates",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "tenant");

            migrationBuilder.AddColumn<string>(
                name: "ProductKey",
                table: "staffarr_permission_templates",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "staffarr");

            migrationBuilder.AddColumn<string>(
                name: "Sensitivity",
                table: "staffarr_permission_templates",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "standard");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_permission_templates_TenantId_ProductKey_PermissionKey",
                table: "staffarr_permission_templates",
                columns: new[] { "TenantId", "ProductKey", "PermissionKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staffarr_permission_templates_TenantId_ProductKey_PermissionKey",
                table: "staffarr_permission_templates");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "staffarr_permission_templates");

            migrationBuilder.DropColumn(
                name: "PermissionScope",
                table: "staffarr_permission_templates");

            migrationBuilder.DropColumn(
                name: "ProductKey",
                table: "staffarr_permission_templates");

            migrationBuilder.DropColumn(
                name: "Sensitivity",
                table: "staffarr_permission_templates");
        }
    }
}
