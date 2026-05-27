using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrReceivingExceptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "QuantityExpected",
                table: "supplyarr_receiving_receipt_lines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE supplyarr_receiving_receipt_lines
                SET "QuantityExpected" = "QuantityReceived"
                WHERE "QuantityExpected" = 0;
                """);

            migrationBuilder.CreateTable(
                name: "supplyarr_receiving_exceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivingReceiptLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_receiving_exceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_exceptions_supplyarr_receiving_receipt_~",
                        column: x => x.ReceivingReceiptLineId,
                        principalTable: "supplyarr_receiving_receipt_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_exceptions_supplyarr_receiving_receipts~",
                        column: x => x.ReceivingReceiptId,
                        principalTable: "supplyarr_receiving_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_ReceivingReceiptId",
                table: "supplyarr_receiving_exceptions",
                column: "ReceivingReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_ReceivingReceiptLineId",
                table: "supplyarr_receiving_exceptions",
                column: "ReceivingReceiptLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_TenantId",
                table: "supplyarr_receiving_exceptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_TenantId_ReceivingReceiptId",
                table: "supplyarr_receiving_exceptions",
                columns: new[] { "TenantId", "ReceivingReceiptId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_TenantId_ReceivingReceiptLi~1",
                table: "supplyarr_receiving_exceptions",
                columns: new[] { "TenantId", "ReceivingReceiptLineId", "ExceptionType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_TenantId_ReceivingReceiptLin~",
                table: "supplyarr_receiving_exceptions",
                columns: new[] { "TenantId", "ReceivingReceiptLineId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_receiving_exceptions");

            migrationBuilder.DropColumn(
                name: "QuantityExpected",
                table: "supplyarr_receiving_receipt_lines");
        }
    }
}
