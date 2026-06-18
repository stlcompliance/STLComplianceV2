using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoadArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLoadArrTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loadarr_tenant_setting_audit_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingsVersionBefore = table.Column<int>(type: "integer", nullable: false),
                    SettingsVersionAfter = table.Column<int>(type: "integer", nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ChangedByDisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ChangeSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BeforeSummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    AfterSummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    ChangedFieldsJson = table.Column<string>(type: "jsonb", nullable: false),
                    WarningsAcknowledgedJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadarr_tenant_setting_audit_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loadarr_tenant_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: false),
                    NormalizedSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    RowVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedByDisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadarr_tenant_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_tenant_setting_audit_entries_TenantId",
                table: "loadarr_tenant_setting_audit_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_tenant_setting_audit_entries_TenantId_ChangedAt",
                table: "loadarr_tenant_setting_audit_entries",
                columns: new[] { "TenantId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_tenant_setting_audit_entries_TenantId_SectionKey_Ch~",
                table: "loadarr_tenant_setting_audit_entries",
                columns: new[] { "TenantId", "SectionKey", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_tenant_settings_TenantId",
                table: "loadarr_tenant_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_tenant_settings_TenantId_IsActive",
                table: "loadarr_tenant_settings",
                columns: new[] { "TenantId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loadarr_tenant_setting_audit_entries");

            migrationBuilder.DropTable(
                name: "loadarr_tenant_settings");
        }
    }
}
