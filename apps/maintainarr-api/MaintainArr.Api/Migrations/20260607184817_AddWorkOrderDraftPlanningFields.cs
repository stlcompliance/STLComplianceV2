using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderDraftPlanningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DraftPlanJson",
                table: "maintainarr_work_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PlannedDueAt",
                table: "maintainarr_work_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PlannedStartAt",
                table: "maintainarr_work_orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DraftPlanJson",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "PlannedDueAt",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "PlannedStartAt",
                table: "maintainarr_work_orders");
        }
    }
}
