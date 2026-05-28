using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrRoutArrDemandIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_routarr_demand_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutarrTripId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutarrTripNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoutarrVehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_routarr_demand_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_routarr_demand_refs_supplyarr_purchase_requests_P~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_routarr_demand_ref_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    RoutarrDemandLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_routarr_demand_ref_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_routarr_demand_ref_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_routarr_demand_ref_lines_supplyarr_routarr_demand~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_routarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_DemandRefId",
                table: "supplyarr_routarr_demand_ref_lines",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_PartId",
                table: "supplyarr_routarr_demand_ref_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_TenantId",
                table: "supplyarr_routarr_demand_ref_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_TenantId_DemandRefId_Lin~",
                table: "supplyarr_routarr_demand_ref_lines",
                columns: new[] { "TenantId", "DemandRefId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_TenantId_RoutarrDemandLi~",
                table: "supplyarr_routarr_demand_ref_lines",
                columns: new[] { "TenantId", "RoutarrDemandLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_PurchaseRequestId",
                table: "supplyarr_routarr_demand_refs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId",
                table: "supplyarr_routarr_demand_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_routarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId_RoutarrPublicationId",
                table: "supplyarr_routarr_demand_refs",
                columns: new[] { "TenantId", "RoutarrPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId_RoutarrTripId",
                table: "supplyarr_routarr_demand_refs",
                columns: new[] { "TenantId", "RoutarrTripId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId_Status_ReceivedAt",
                table: "supplyarr_routarr_demand_refs",
                columns: new[] { "TenantId", "Status", "ReceivedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_routarr_demand_ref_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_routarr_demand_refs");
        }
    }
}
