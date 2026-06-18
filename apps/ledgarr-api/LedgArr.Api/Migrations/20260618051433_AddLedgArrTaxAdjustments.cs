using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LedgArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgArrTaxAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialLegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "text", nullable: false),
                    AdjustmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxAdjustments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxAdjustments_TenantId_AdjustmentNumber",
                table: "TaxAdjustments",
                columns: new[] { "TenantId", "AdjustmentNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxAdjustments");
        }
    }
}
