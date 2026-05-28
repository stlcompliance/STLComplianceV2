using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrProcurementExceptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_exceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExceptionCategory = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    WaiveJustification = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    WaiveRejectionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvestigatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvestigatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WaiveRequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaiveRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WaivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_exceptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId",
                table: "supplyarr_procurement_exceptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_ExceptionKey",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "ExceptionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_Status_UpdatedAt",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_SubjectType_Subje~",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "SubjectType", "SubjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_procurement_exceptions");
        }
    }
}
