using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrDemandProcessingMultiSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_supplyarr_demand_processing_states_supplyarr_maintainarr_de~",
                table: "supplyarr_demand_processing_states");

            migrationBuilder.DropIndex(
                name: "IX_supplyarr_demand_processing_states_DemandRefId",
                table: "supplyarr_demand_processing_states");

            migrationBuilder.AddColumn<bool>(
                name: "ProcessMaintainarrDemandRefs",
                table: "supplyarr_tenant_demand_processing_settings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProcessRoutarrDemandRefs",
                table: "supplyarr_tenant_demand_processing_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcessStaffarrDemandRefs",
                table: "supplyarr_tenant_demand_processing_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcessTrainarrDemandRefs",
                table: "supplyarr_tenant_demand_processing_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DemandRefSource",
                table: "supplyarr_demand_processing_states",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "maintainarr");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId_DemandRefSource",
                table: "supplyarr_demand_processing_states",
                columns: new[] { "TenantId", "DemandRefSource" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId_DemandRefSource",
                table: "supplyarr_demand_processing_states");

            migrationBuilder.DropColumn(
                name: "ProcessMaintainarrDemandRefs",
                table: "supplyarr_tenant_demand_processing_settings");

            migrationBuilder.DropColumn(
                name: "ProcessRoutarrDemandRefs",
                table: "supplyarr_tenant_demand_processing_settings");

            migrationBuilder.DropColumn(
                name: "ProcessStaffarrDemandRefs",
                table: "supplyarr_tenant_demand_processing_settings");

            migrationBuilder.DropColumn(
                name: "ProcessTrainarrDemandRefs",
                table: "supplyarr_tenant_demand_processing_settings");

            migrationBuilder.DropColumn(
                name: "DemandRefSource",
                table: "supplyarr_demand_processing_states");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_DemandRefId",
                table: "supplyarr_demand_processing_states",
                column: "DemandRefId");

            migrationBuilder.AddForeignKey(
                name: "FK_supplyarr_demand_processing_states_supplyarr_maintainarr_de~",
                table: "supplyarr_demand_processing_states",
                column: "DemandRefId",
                principalTable: "supplyarr_maintainarr_demand_refs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
