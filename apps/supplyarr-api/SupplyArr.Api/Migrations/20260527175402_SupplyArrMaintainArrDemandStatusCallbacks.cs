using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrMaintainArrDemandStatusCallbacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusCallbackAt",
                table: "supplyarr_maintainarr_demand_refs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementStatus",
                table: "supplyarr_maintainarr_demand_refs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseOrderId",
                table: "supplyarr_maintainarr_demand_refs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_PurchaseOrderId",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseRequestId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_PurchaseOrderId",
                table: "supplyarr_maintainarr_demand_refs");

            migrationBuilder.DropIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_maintainarr_demand_refs");

            migrationBuilder.DropColumn(
                name: "LastStatusCallbackAt",
                table: "supplyarr_maintainarr_demand_refs");

            migrationBuilder.DropColumn(
                name: "ProcurementStatus",
                table: "supplyarr_maintainarr_demand_refs");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "supplyarr_maintainarr_demand_refs");
        }
    }
}
