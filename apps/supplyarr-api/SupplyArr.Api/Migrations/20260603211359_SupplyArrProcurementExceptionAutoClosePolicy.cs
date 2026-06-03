using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrProcurementExceptionAutoClosePolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoCloseCompletedExceptionsEnabled",
                table: "supplyarr_tenant_procurement_exception_escalation_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AutoCloseCompletedExceptionsAfterHours",
                table: "supplyarr_tenant_procurement_exception_escalation_settings",
                type: "integer",
                nullable: false,
                defaultValue: 48);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoCloseCompletedExceptionsEnabled",
                table: "supplyarr_tenant_procurement_exception_escalation_settings");

            migrationBuilder.DropColumn(
                name: "AutoCloseCompletedExceptionsAfterHours",
                table: "supplyarr_tenant_procurement_exception_escalation_settings");
        }
    }
}
