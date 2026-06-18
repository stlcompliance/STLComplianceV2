using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutArrTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_tenant_setting_audit_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SettingGroup = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChangedKeys = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ChangedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreviousVersion = table.Column<int>(type: "integer", nullable: false),
                    NewVersion = table.Column<int>(type: "integer", nullable: false),
                    AffectedScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AffectedScopeRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Summary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    PreviousSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    NewSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_setting_audit_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_setting_list_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingGroup = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SettingKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ItemKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsTenantConfigured = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_setting_list_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_tenant_setting_list_items_routarr_tenant_settings_T~",
                        column: x => x.TenantSettingsId,
                        principalTable: "routarr_tenant_settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_setting_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeSourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScopeStableId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScopeDisplayLabelSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ScopeStatusSnapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScopeSnapshotAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SettingGroup = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SettingKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                    IntegerValue = table.Column<int>(type: "integer", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    TextValue = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EnumValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TimeValue = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    DurationMinutesValue = table.Column<int>(type: "integer", nullable: true),
                    IsEmergencyOverride = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_setting_overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_tenant_setting_overrides_routarr_tenant_settings_Te~",
                        column: x => x.TenantSettingsId,
                        principalTable: "routarr_tenant_settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_setting_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingGroup = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SettingKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                    IntegerValue = table.Column<int>(type: "integer", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    TextValue = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EnumValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TimeValue = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    DurationMinutesValue = table.Column<int>(type: "integer", nullable: true),
                    IsTenantConfigured = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_setting_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_tenant_setting_values_routarr_tenant_settings_Tenan~",
                        column: x => x.TenantSettingsId,
                        principalTable: "routarr_tenant_settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_setting_override_list_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverrideId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_setting_override_list_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_tenant_setting_override_list_items_routarr_tenant_s~",
                        column: x => x.OverrideId,
                        principalTable: "routarr_tenant_setting_overrides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_audit_entries_TenantId",
                table: "routarr_tenant_setting_audit_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_audit_entries_TenantId_PublicKey",
                table: "routarr_tenant_setting_audit_entries",
                columns: new[] { "TenantId", "PublicKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_audit_entries_TenantId_SettingGroup_~",
                table: "routarr_tenant_setting_audit_entries",
                columns: new[] { "TenantId", "SettingGroup", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_list_items_TenantId",
                table: "routarr_tenant_setting_list_items",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_list_items_TenantId_SettingGroup_Set~",
                table: "routarr_tenant_setting_list_items",
                columns: new[] { "TenantId", "SettingGroup", "SettingKey", "ItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_list_items_TenantSettingsId",
                table: "routarr_tenant_setting_list_items",
                column: "TenantSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_override_list_items_OverrideId",
                table: "routarr_tenant_setting_override_list_items",
                column: "OverrideId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_override_list_items_TenantId",
                table: "routarr_tenant_setting_override_list_items",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_override_list_items_TenantId_Overrid~",
                table: "routarr_tenant_setting_override_list_items",
                columns: new[] { "TenantId", "OverrideId", "ItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_overrides_TenantId",
                table: "routarr_tenant_setting_overrides",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_overrides_TenantId_PublicKey",
                table: "routarr_tenant_setting_overrides",
                columns: new[] { "TenantId", "PublicKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_overrides_TenantId_ScopeType_ScopeSo~",
                table: "routarr_tenant_setting_overrides",
                columns: new[] { "TenantId", "ScopeType", "ScopeSourceProduct", "ScopeEntityType", "ScopeStableId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_overrides_TenantId_SettingGroup_Sett~",
                table: "routarr_tenant_setting_overrides",
                columns: new[] { "TenantId", "SettingGroup", "SettingKey" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_overrides_TenantSettingsId",
                table: "routarr_tenant_setting_overrides",
                column: "TenantSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_values_TenantId",
                table: "routarr_tenant_setting_values",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_values_TenantId_SettingGroup_Setting~",
                table: "routarr_tenant_setting_values",
                columns: new[] { "TenantId", "SettingGroup", "SettingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_setting_values_TenantSettingsId",
                table: "routarr_tenant_setting_values",
                column: "TenantSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_settings_TenantId",
                table: "routarr_tenant_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_tenant_setting_audit_entries");

            migrationBuilder.DropTable(
                name: "routarr_tenant_setting_list_items");

            migrationBuilder.DropTable(
                name: "routarr_tenant_setting_override_list_items");

            migrationBuilder.DropTable(
                name: "routarr_tenant_setting_values");

            migrationBuilder.DropTable(
                name: "routarr_tenant_setting_overrides");

            migrationBuilder.DropTable(
                name: "routarr_tenant_settings");
        }
    }
}
