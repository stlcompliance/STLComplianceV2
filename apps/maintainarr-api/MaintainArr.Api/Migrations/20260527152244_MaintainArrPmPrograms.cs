using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrPmPrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_pm_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssetTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_programs_maintainarr_asset_types_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "maintainarr_asset_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_programs_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_pm_program_schedules",
                columns: table => new
                {
                    PmProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    PmScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_program_schedules", x => new { x.PmProgramId, x.PmScheduleId });
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_program_schedules_maintainarr_pm_programs_Pm~",
                        column: x => x.PmProgramId,
                        principalTable: "maintainarr_pm_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_program_schedules_maintainarr_pm_schedules_P~",
                        column: x => x.PmScheduleId,
                        principalTable: "maintainarr_pm_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_program_schedules_PmProgramId_SortOrder",
                table: "maintainarr_pm_program_schedules",
                columns: new[] { "PmProgramId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_program_schedules_PmScheduleId",
                table: "maintainarr_pm_program_schedules",
                column: "PmScheduleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_AssetId",
                table: "maintainarr_pm_programs",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_AssetTypeId",
                table: "maintainarr_pm_programs",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId",
                table: "maintainarr_pm_programs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId_AssetId",
                table: "maintainarr_pm_programs",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId_AssetTypeId",
                table: "maintainarr_pm_programs",
                columns: new[] { "TenantId", "AssetTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId_ProgramKey",
                table: "maintainarr_pm_programs",
                columns: new[] { "TenantId", "ProgramKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_TenantId_Status",
                table: "maintainarr_pm_programs",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_pm_program_schedules");

            migrationBuilder.DropTable(
                name: "maintainarr_pm_programs");
        }
    }
}
