using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrEmergencyPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmergencyExpeditedAt",
                table: "supplyarr_purchase_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmergencyExpeditedByUserId",
                table: "supplyarr_purchase_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyReason",
                table: "supplyarr_purchase_requests",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmergency",
                table: "supplyarr_purchase_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ManagerOverrideApproved",
                table: "supplyarr_purchase_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ManagerOverrideApprovedAt",
                table: "supplyarr_purchase_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerOverrideApprovedByUserId",
                table: "supplyarr_purchase_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerOverrideJustification",
                table: "supplyarr_purchase_requests",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_TenantId_IsEmergency_Status",
                table: "supplyarr_purchase_requests",
                columns: new[] { "TenantId", "IsEmergency", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_supplyarr_purchase_requests_TenantId_IsEmergency_Status",
                table: "supplyarr_purchase_requests");

            migrationBuilder.DropColumn(
                name: "EmergencyExpeditedAt",
                table: "supplyarr_purchase_requests");

            migrationBuilder.DropColumn(
                name: "EmergencyExpeditedByUserId",
                table: "supplyarr_purchase_requests");

            migrationBuilder.DropColumn(
                name: "EmergencyReason",
                table: "supplyarr_purchase_requests");

            migrationBuilder.DropColumn(
                name: "IsEmergency",
                table: "supplyarr_purchase_requests");

            migrationBuilder.DropColumn(
                name: "ManagerOverrideApproved",
                table: "supplyarr_purchase_requests");

            migrationBuilder.DropColumn(
                name: "ManagerOverrideApprovedAt",
                table: "supplyarr_purchase_requests");

            migrationBuilder.DropColumn(
                name: "ManagerOverrideApprovedByUserId",
                table: "supplyarr_purchase_requests");

            migrationBuilder.DropColumn(
                name: "ManagerOverrideJustification",
                table: "supplyarr_purchase_requests");
        }
    }
}
