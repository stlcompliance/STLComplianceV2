using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrSupplierIncidents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_supplier_incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IncidentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingExceptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorRestrictionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_supplyarr_supplier_incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_supplier_incidents_supplyarr_external_parties_Ext~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_supplier_incidents_supplyarr_vendor_restrictions_~",
                        column: x => x.VendorRestrictionId,
                        principalTable: "supplyarr_vendor_restrictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_ExternalPartyId",
                table: "supplyarr_supplier_incidents",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId",
                table: "supplyarr_supplier_incidents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId_ExternalPartyId",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "ExternalPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId_IncidentKey",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "IncidentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId_Severity",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId_Status_UpdatedAt",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_VendorRestrictionId",
                table: "supplyarr_supplier_incidents",
                column: "VendorRestrictionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_supplier_incidents");
        }
    }
}
