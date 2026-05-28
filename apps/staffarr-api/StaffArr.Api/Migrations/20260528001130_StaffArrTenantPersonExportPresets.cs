using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrTenantPersonExportPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_tenant_person_export_presets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    PresetKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_tenant_person_export_presets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_tenant_person_export_presets_staffarr_org_units_Or~",
                        column: x => x.OrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_person_export_presets_OrgUnitId",
                table: "staffarr_tenant_person_export_presets",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_person_export_presets_TenantId",
                table: "staffarr_tenant_person_export_presets",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_tenant_person_export_presets");
        }
    }
}
