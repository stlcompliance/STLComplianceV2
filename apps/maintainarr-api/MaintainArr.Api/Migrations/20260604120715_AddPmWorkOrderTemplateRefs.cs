using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPmWorkOrderTemplateRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateRef",
                table: "maintainarr_work_orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoGenerateWorkOrder",
                table: "maintainarr_pm_programs",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultWorkOrderTemplateRef",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateRef",
                table: "maintainarr_work_orders");

            migrationBuilder.DropColumn(
                name: "AutoGenerateWorkOrder",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "DefaultWorkOrderTemplateRef",
                table: "maintainarr_pm_programs");
        }
    }
}
