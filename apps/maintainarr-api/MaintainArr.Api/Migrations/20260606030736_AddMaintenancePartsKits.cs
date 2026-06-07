using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenancePartsKits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_parts_kits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    KitNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    AssetTypeApplicabilityJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    WorkOrderTypeApplicabilityJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    PmPlanRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_parts_kits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_parts_kit_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenancePartsKitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ItemDescriptionSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    SubstituteAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_parts_kit_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_maintenance_parts_kit_lines_maintainarr_mainten~",
                        column: x => x.MaintenancePartsKitId,
                        principalTable: "maintainarr_maintenance_parts_kits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kit_lines_MaintenancePartsKit~",
                table: "maintainarr_maintenance_parts_kit_lines",
                column: "MaintenancePartsKitId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kit_lines_TenantId",
                table: "maintainarr_maintenance_parts_kit_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kit_lines_TenantId_Maintenan~1",
                table: "maintainarr_maintenance_parts_kit_lines",
                columns: new[] { "TenantId", "MaintenancePartsKitId", "ItemRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kit_lines_TenantId_Maintenanc~",
                table: "maintainarr_maintenance_parts_kit_lines",
                columns: new[] { "TenantId", "MaintenancePartsKitId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kits_TenantId",
                table: "maintainarr_maintenance_parts_kits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_parts_kits_TenantId_KitNumber",
                table: "maintainarr_maintenance_parts_kits",
                columns: new[] { "TenantId", "KitNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_parts_kit_lines");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_parts_kits");
        }
    }
}
