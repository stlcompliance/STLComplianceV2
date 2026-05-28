using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrTrainStaffDemandIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_staffarr_demand_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrIncidentTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
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
                    table.PrimaryKey("PK_supplyarr_staffarr_demand_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_staffarr_demand_refs_supplyarr_purchase_requests_~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_trainarr_demand_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrAssignmentRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_supplyarr_trainarr_demand_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_trainarr_demand_refs_supplyarr_purchase_requests_~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_staffarr_demand_ref_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    StaffarrDemandLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_staffarr_demand_ref_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_staffarr_demand_ref_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_staffarr_demand_ref_lines_supplyarr_staffarr_dema~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_staffarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_trainarr_demand_ref_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    TrainarrDemandLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_trainarr_demand_ref_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_trainarr_demand_ref_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_trainarr_demand_ref_lines_supplyarr_trainarr_dema~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_trainarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_DemandRefId",
                table: "supplyarr_staffarr_demand_ref_lines",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_PartId",
                table: "supplyarr_staffarr_demand_ref_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_TenantId",
                table: "supplyarr_staffarr_demand_ref_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_TenantId_DemandRefId_Li~",
                table: "supplyarr_staffarr_demand_ref_lines",
                columns: new[] { "TenantId", "DemandRefId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_TenantId_StaffarrDemand~",
                table: "supplyarr_staffarr_demand_ref_lines",
                columns: new[] { "TenantId", "StaffarrDemandLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_PurchaseRequestId",
                table: "supplyarr_staffarr_demand_refs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId",
                table: "supplyarr_staffarr_demand_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_staffarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId_StaffarrIncidentId",
                table: "supplyarr_staffarr_demand_refs",
                columns: new[] { "TenantId", "StaffarrIncidentId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId_StaffarrPublication~",
                table: "supplyarr_staffarr_demand_refs",
                columns: new[] { "TenantId", "StaffarrPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId_Status_ReceivedAt",
                table: "supplyarr_staffarr_demand_refs",
                columns: new[] { "TenantId", "Status", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_DemandRefId",
                table: "supplyarr_trainarr_demand_ref_lines",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_PartId",
                table: "supplyarr_trainarr_demand_ref_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_TenantId",
                table: "supplyarr_trainarr_demand_ref_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_TenantId_DemandRefId_Li~",
                table: "supplyarr_trainarr_demand_ref_lines",
                columns: new[] { "TenantId", "DemandRefId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_TenantId_TrainarrDemand~",
                table: "supplyarr_trainarr_demand_ref_lines",
                columns: new[] { "TenantId", "TrainarrDemandLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_PurchaseRequestId",
                table: "supplyarr_trainarr_demand_refs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId",
                table: "supplyarr_trainarr_demand_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_trainarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId_Status_ReceivedAt",
                table: "supplyarr_trainarr_demand_refs",
                columns: new[] { "TenantId", "Status", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId_TrainarrAssignmentId",
                table: "supplyarr_trainarr_demand_refs",
                columns: new[] { "TenantId", "TrainarrAssignmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId_TrainarrPublication~",
                table: "supplyarr_trainarr_demand_refs",
                columns: new[] { "TenantId", "TrainarrPublicationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_staffarr_demand_ref_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_trainarr_demand_ref_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_staffarr_demand_refs");

            migrationBuilder.DropTable(
                name: "supplyarr_trainarr_demand_refs");
        }
    }
}
