using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrManagerHierarchySlice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_TenantId_ManagerPersonId",
                table: "staffarr_people",
                columns: new[] { "TenantId", "ManagerPersonId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staffarr_people_TenantId_ManagerPersonId",
                table: "staffarr_people");
        }
    }
}
