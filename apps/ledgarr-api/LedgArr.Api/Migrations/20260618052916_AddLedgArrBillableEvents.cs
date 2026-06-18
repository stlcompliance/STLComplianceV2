using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LedgArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgArrBillableEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillableEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialPacketId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialLegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventNumber = table.Column<string>(type: "text", nullable: false),
                    SourceProductKey = table.Column<string>(type: "text", nullable: false),
                    SourceRecordDisplayName = table.Column<string>(type: "text", nullable: false),
                    ChargeType = table.Column<string>(type: "text", nullable: false),
                    CustomerRefId = table.Column<string>(type: "text", nullable: true),
                    CustomerDisplayName = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "text", nullable: false),
                    InvoiceStatus = table.Column<string>(type: "text", nullable: false),
                    HoldReason = table.Column<string>(type: "text", nullable: true),
                    ExceptionReason = table.Column<string>(type: "text", nullable: true),
                    AccountingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillableEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillableEvents_TenantId_EventNumber",
                table: "BillableEvents",
                columns: new[] { "TenantId", "EventNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillableEvents");
        }
    }
}
