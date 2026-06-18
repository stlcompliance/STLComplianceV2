using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintainArrTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    SettingsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_settings_audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    BeforeJson = table.Column<string>(type: "text", nullable: false),
                    AfterJson = table.Column<string>(type: "text", nullable: false),
                    DiffJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_settings_audit", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_settings_TenantId",
                table: "maintainarr_tenant_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_settings_audit_TenantId",
                table: "maintainarr_tenant_settings_audit",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_settings_audit_TenantId_ChangedAtUtc",
                table: "maintainarr_tenant_settings_audit",
                columns: new[] { "TenantId", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_settings_audit_TenantId_SettingsId",
                table: "maintainarr_tenant_settings_audit",
                columns: new[] { "TenantId", "SettingsId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_tenant_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_settings_audit");
        }
    }
}
