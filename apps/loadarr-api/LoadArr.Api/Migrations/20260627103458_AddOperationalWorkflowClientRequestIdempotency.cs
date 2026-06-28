using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoadArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalWorkflowClientRequestIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientRequestId",
                table: "loadarr_transfer_orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestFingerprint",
                table: "loadarr_transfer_orders",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientRequestId",
                table: "loadarr_receiving_sessions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestFingerprint",
                table: "loadarr_receiving_sessions",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_transfer_orders_TenantId_ClientRequestId",
                table: "loadarr_transfer_orders",
                columns: new[] { "TenantId", "ClientRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_receiving_sessions_TenantId_ClientRequestId",
                table: "loadarr_receiving_sessions",
                columns: new[] { "TenantId", "ClientRequestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_loadarr_transfer_orders_TenantId_ClientRequestId",
                table: "loadarr_transfer_orders");

            migrationBuilder.DropIndex(
                name: "IX_loadarr_receiving_sessions_TenantId_ClientRequestId",
                table: "loadarr_receiving_sessions");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "loadarr_transfer_orders");

            migrationBuilder.DropColumn(
                name: "RequestFingerprint",
                table: "loadarr_transfer_orders");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "loadarr_receiving_sessions");

            migrationBuilder.DropColumn(
                name: "RequestFingerprint",
                table: "loadarr_receiving_sessions");
        }
    }
}
