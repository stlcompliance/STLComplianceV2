using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionControlledOptionsAndSelectedOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedOptionsJson",
                table: "maintainarr_inspection_run_answers",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ControlledOptionsJson",
                table: "maintainarr_inspection_checklist_items",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedOptionsJson",
                table: "maintainarr_inspection_run_answers");

            migrationBuilder.DropColumn(
                name: "ControlledOptionsJson",
                table: "maintainarr_inspection_checklist_items");
        }
    }
}
