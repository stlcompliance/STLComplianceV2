using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintainArrWorkOrderMetadataAndInspectionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginRef",
                table: "maintainarr_work_orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginType",
                table: "maintainarr_work_orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QualificationCheckResultsJson",
                table: "maintainarr_work_orders",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredQualificationRefsJson",
                table: "maintainarr_work_orders",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StaffarrLocationId",
                table: "maintainarr_work_orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkOrderType",
                table: "maintainarr_work_orders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InspectionType",
                table: "maintainarr_inspection_templates",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginRef",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "OriginType",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "QualificationCheckResultsJson",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "RequiredQualificationRefsJson",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "StaffarrLocationId",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "WorkOrderType",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "InspectionType",
                table: "maintainarr_inspection_templates");
        }
    }
}
