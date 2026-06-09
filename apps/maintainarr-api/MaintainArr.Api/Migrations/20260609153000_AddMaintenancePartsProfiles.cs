using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    public partial class AddMaintenancePartsProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_parts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NormalizedPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SupplyArrPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManufacturerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ManufacturerPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SdsDocumentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComplianceCoreMaterialKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ComplianceCoreHazardKeysJson = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_parts", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "MaintenancePartId",
                table: "maintainarr_work_order_parts_demand_lines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_parts_TenantId",
                table: "maintainarr_parts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_parts_TenantId_NormalizedPartNumber",
                table: "maintainarr_parts",
                columns: new[] { "TenantId", "NormalizedPartNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_parts_TenantId_Status",
                table: "maintainarr_parts",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_parts_TenantId_SupplyArrPartId",
                table: "maintainarr_parts",
                columns: new[] { "TenantId", "SupplyArrPartId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_MaintenancePartId",
                table: "maintainarr_work_order_parts_demand_lines",
                column: "MaintenancePartId");

            migrationBuilder.AddForeignKey(
                name: "FK_maintainarr_work_order_parts_demand_lines_maintainarr_parts_MaintenancePartId",
                table: "maintainarr_work_order_parts_demand_lines",
                column: "MaintenancePartId",
                principalTable: "maintainarr_parts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintainarr_work_order_parts_demand_lines_maintainarr_parts_MaintenancePartId",
                table: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropTable(
                name: "maintainarr_parts");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_MaintenancePartId",
                table: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropColumn(
                name: "MaintenancePartId",
                table: "maintainarr_work_order_parts_demand_lines");
        }
    }
}
