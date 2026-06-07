using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderLaborApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                table: "maintainarr_work_order_labor_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByPersonId",
                table: "maintainarr_work_order_labor_entries",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "maintainarr_work_order_labor_entries",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "maintainarr_work_order_labor_entries",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "submitted");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmittedAt",
                table: "maintainarr_work_order_labor_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId_WorkOrderId_S~",
                table: "maintainarr_work_order_labor_entries",
                columns: new[] { "TenantId", "WorkOrderId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId_WorkOrderId_S~",
                table: "maintainarr_work_order_labor_entries");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "maintainarr_work_order_labor_entries");

            migrationBuilder.DropColumn(
                name: "ApprovedByPersonId",
                table: "maintainarr_work_order_labor_entries");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "maintainarr_work_order_labor_entries");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "maintainarr_work_order_labor_entries");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "maintainarr_work_order_labor_entries");
        }
    }
}
