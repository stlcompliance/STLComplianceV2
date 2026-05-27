using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrReadinessRollups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_readiness_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgUnitName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TotalMembers = table.Column<int>(type: "integer", nullable: false),
                    ReadyCount = table.Column<int>(type: "integer", nullable: false),
                    NotReadyCount = table.Column<int>(type: "integer", nullable: false),
                    OverrideCount = table.Column<int>(type: "integer", nullable: false),
                    ReadyPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_readiness_rollups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_readiness_rollups_staffarr_org_units_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalTable: "staffarr_org_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_readiness_rollups_OrgUnitId",
                table: "staffarr_readiness_rollups",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_readiness_rollups_TenantId",
                table: "staffarr_readiness_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_readiness_rollups_TenantId_ScopeType_ComputedAt",
                table: "staffarr_readiness_rollups",
                columns: new[] { "TenantId", "ScopeType", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_readiness_rollups_TenantId_ScopeType_OrgUnitId",
                table: "staffarr_readiness_rollups",
                columns: new[] { "TenantId", "ScopeType", "OrgUnitId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_readiness_rollups");
        }
    }
}
