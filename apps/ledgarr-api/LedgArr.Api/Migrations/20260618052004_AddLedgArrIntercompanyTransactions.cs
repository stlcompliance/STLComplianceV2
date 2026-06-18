using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LedgArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgArrIntercompanyTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntercompanyTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromFinancialLegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToFinancialLegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionNumber = table.Column<string>(type: "text", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SettlementStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntercompanyTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntercompanyTransactions_TenantId_TransactionNumber",
                table: "IntercompanyTransactions",
                columns: new[] { "TenantId", "TransactionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntercompanyTransactions");
        }
    }
}
