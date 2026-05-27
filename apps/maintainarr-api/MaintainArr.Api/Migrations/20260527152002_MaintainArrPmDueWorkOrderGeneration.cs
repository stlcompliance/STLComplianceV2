using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrPmDueWorkOrderGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_PmScheduleId_Status",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "PmScheduleId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_maintainarr_work_orders_TenantId_PmScheduleId_Status",
                table: "maintainarr_work_orders");
        }
    }
}
