using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionMeterReadingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AcceptableRangeMax",
                table: "maintainarr_inspection_checklist_items",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AcceptableRangeMin",
                table: "maintainarr_inspection_checklist_items",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "maintainarr_inspection_checklist_items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptableRangeMax",
                table: "maintainarr_inspection_checklist_items");

            migrationBuilder.DropColumn(
                name: "AcceptableRangeMin",
                table: "maintainarr_inspection_checklist_items");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "maintainarr_inspection_checklist_items");
        }
    }
}
