using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrTenantBillingReadiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingCustomerId",
                table: "tenants",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingGraceDays",
                table: "tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingSubscriptionId",
                table: "tenants",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInternalTenant",
                table: "tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrial",
                table: "tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionTier",
                table: "tenants",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingCustomerId",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "BillingGraceDays",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "BillingSubscriptionId",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "IsInternalTenant",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "IsTrial",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionTier",
                table: "tenants");
        }
    }
}
