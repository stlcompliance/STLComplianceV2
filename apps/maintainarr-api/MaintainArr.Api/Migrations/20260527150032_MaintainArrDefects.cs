using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrDefects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_defects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_defects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_defects_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_defects_maintainarr_inspection_checklist_items_~",
                        column: x => x.ChecklistItemId,
                        principalTable: "maintainarr_inspection_checklist_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_defects_maintainarr_inspection_runs_InspectionR~",
                        column: x => x.InspectionRunId,
                        principalTable: "maintainarr_inspection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_AssetId",
                table: "maintainarr_defects",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_ChecklistItemId",
                table: "maintainarr_defects",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_InspectionRunId",
                table: "maintainarr_defects",
                column: "InspectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId",
                table: "maintainarr_defects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_AssetId_Status",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_InspectionRunId",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "InspectionRunId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_InspectionRunId_ChecklistItemId",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "InspectionRunId", "ChecklistItemId" },
                unique: true,
                filter: "\"ChecklistItemId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_ReportedByUserId_CreatedAt",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "ReportedByUserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_defects");
        }
    }
}
