using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPmInspectionGenerationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SkippedAt",
                table: "maintainarr_pm_schedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkippedByPersonId",
                table: "maintainarr_pm_schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkippedReason",
                table: "maintainarr_pm_schedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoGenerateInspection",
                table: "maintainarr_pm_programs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "InspectionTemplateId",
                table: "maintainarr_pm_programs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PmScheduleId",
                table: "maintainarr_inspection_runs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_programs_InspectionTemplateId",
                table: "maintainarr_pm_programs",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_PmScheduleId",
                table: "maintainarr_inspection_runs",
                column: "PmScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_PmScheduleId_Status",
                table: "maintainarr_inspection_runs",
                columns: new[] { "TenantId", "PmScheduleId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_maintainarr_inspection_runs_maintainarr_pm_schedules_PmSche~",
                table: "maintainarr_inspection_runs",
                column: "PmScheduleId",
                principalTable: "maintainarr_pm_schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_maintainarr_pm_programs_maintainarr_inspection_templates_In~",
                table: "maintainarr_pm_programs",
                column: "InspectionTemplateId",
                principalTable: "maintainarr_inspection_templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintainarr_inspection_runs_maintainarr_pm_schedules_PmSche~",
                table: "maintainarr_inspection_runs");

            migrationBuilder.DropForeignKey(
                name: "FK_maintainarr_pm_programs_maintainarr_inspection_templates_In~",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_pm_programs_InspectionTemplateId",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_inspection_runs_PmScheduleId",
                table: "maintainarr_inspection_runs");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_inspection_runs_TenantId_PmScheduleId_Status",
                table: "maintainarr_inspection_runs");

            migrationBuilder.DropColumn(
                name: "SkippedAt",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropColumn(
                name: "SkippedByPersonId",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropColumn(
                name: "SkippedReason",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropColumn(
                name: "AutoGenerateInspection",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "InspectionTemplateId",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "PmScheduleId",
                table: "maintainarr_inspection_runs");
        }
    }
}
