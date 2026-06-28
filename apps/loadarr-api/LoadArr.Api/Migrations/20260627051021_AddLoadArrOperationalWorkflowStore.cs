using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoadArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLoadArrOperationalWorkflowStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loadarr_receiving_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReceivingNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReceivingType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CompletedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadarr_receiving_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loadarr_transfer_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransferNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransferType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CompletedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadarr_transfer_orders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_receiving_sessions_TenantId_SessionId",
                table: "loadarr_receiving_sessions",
                columns: new[] { "TenantId", "SessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_receiving_sessions_TenantId_Status_StartedAtUtc",
                table: "loadarr_receiving_sessions",
                columns: new[] { "TenantId", "Status", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_transfer_orders_TenantId_OrderId",
                table: "loadarr_transfer_orders",
                columns: new[] { "TenantId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_transfer_orders_TenantId_Status_CreatedAtUtc",
                table: "loadarr_transfer_orders",
                columns: new[] { "TenantId", "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loadarr_receiving_sessions");

            migrationBuilder.DropTable(
                name: "loadarr_transfer_orders");
        }
    }
}
