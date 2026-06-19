using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LedgArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgArrBankTransactionPurchaseOrderRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderRefDisplayName",
                table: "BankTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderRefId",
                table: "BankTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderRefProductKey",
                table: "BankTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderRefType",
                table: "BankTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseOrderApprovedAmountSnapshot",
                table: "BankTransactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderAmountStatus",
                table: "BankTransactions",
                type: "text",
                nullable: false,
                defaultValue: "not_applicable");

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseOrderVarianceAmount",
                table: "BankTransactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseOrderRefDisplayName",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderRefId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderRefProductKey",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderRefType",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderApprovedAmountSnapshot",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderAmountStatus",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderVarianceAmount",
                table: "BankTransactions");
        }
    }
}
