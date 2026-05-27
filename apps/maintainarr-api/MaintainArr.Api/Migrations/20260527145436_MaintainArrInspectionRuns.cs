using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrInspectionRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_runs_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_runs_maintainarr_inspection_template~",
                        column: x => x.InspectionTemplateId,
                        principalTable: "maintainarr_inspection_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_run_answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PassFailValue = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    NumericValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    TextValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AnsweredByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_run_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_answers_maintainarr_inspection_c~",
                        column: x => x.ChecklistItemId,
                        principalTable: "maintainarr_inspection_checklist_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_answers_maintainarr_inspection_r~",
                        column: x => x.InspectionRunId,
                        principalTable: "maintainarr_inspection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_answers_ChecklistItemId",
                table: "maintainarr_inspection_run_answers",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_answers_InspectionRunId",
                table: "maintainarr_inspection_run_answers",
                column: "InspectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_answers_TenantId",
                table: "maintainarr_inspection_run_answers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_answers_TenantId_InspectionRunId~",
                table: "maintainarr_inspection_run_answers",
                columns: new[] { "TenantId", "InspectionRunId", "ChecklistItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_AssetId",
                table: "maintainarr_inspection_runs",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_InspectionTemplateId",
                table: "maintainarr_inspection_runs",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId",
                table: "maintainarr_inspection_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_AssetId_Status",
                table: "maintainarr_inspection_runs",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_InspectionTemplateId",
                table: "maintainarr_inspection_runs",
                columns: new[] { "TenantId", "InspectionTemplateId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_StartedByUserId_Starte~",
                table: "maintainarr_inspection_runs",
                columns: new[] { "TenantId", "StartedByUserId", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_inspection_run_answers");

            migrationBuilder.DropTable(
                name: "maintainarr_inspection_runs");
        }
    }
}
