using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrMaintainArrDemandIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_maintainarr_demand_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrWorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrWorkOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MaintainarrAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_maintainarr_demand_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_maintainarr_demand_refs_supplyarr_purchase_reques~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_maintainarr_demand_ref_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    MaintainarrDemandLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_maintainarr_demand_ref_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_maintainarr_demand_ref_lines_supplyarr_maintainar~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_maintainarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_maintainarr_demand_ref_lines_supplyarr_parts_Part~",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_DemandRefId",
                table: "supplyarr_maintainarr_demand_ref_lines",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_PartId",
                table: "supplyarr_maintainarr_demand_ref_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_TenantId",
                table: "supplyarr_maintainarr_demand_ref_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_TenantId_DemandRefId~",
                table: "supplyarr_maintainarr_demand_ref_lines",
                columns: new[] { "TenantId", "DemandRefId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_TenantId_Maintainarr~",
                table: "supplyarr_maintainarr_demand_ref_lines",
                columns: new[] { "TenantId", "MaintainarrDemandLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_PurchaseRequestId",
                table: "supplyarr_maintainarr_demand_refs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId",
                table: "supplyarr_maintainarr_demand_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_MaintainarrPubli~",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "MaintainarrPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_MaintainarrWorkO~",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "MaintainarrWorkOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_Status_ReceivedAt",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "Status", "ReceivedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_maintainarr_demand_ref_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_maintainarr_demand_refs");
        }
    }
}
