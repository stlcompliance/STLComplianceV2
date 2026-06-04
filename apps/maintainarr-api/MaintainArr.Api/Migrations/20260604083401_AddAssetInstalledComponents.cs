using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetInstalledComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_installed_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ParentAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentComponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ComponentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Make = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PartNumberSnapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstalledPartUsageRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstallDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InstalledByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstalledMeterReading = table.Column<decimal>(type: "numeric", nullable: true),
                    RemovedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RemovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RemovedMeterReading = table.Column<decimal>(type: "numeric", nullable: true),
                    RemovalReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    WarrantyStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WarrantyEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpectedLifeHours = table.Column<decimal>(type: "numeric", nullable: true),
                    ExpectedLifeMiles = table.Column<decimal>(type: "numeric", nullable: true),
                    ExpectedLifeCycles = table.Column<int>(type: "integer", nullable: true),
                    Condition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReplacementPartRefsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DocumentRefsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DefectRefsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    WorkOrderRefsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_installed_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_installed_components_maintainarr_assets_P~",
                        column: x => x.ParentAssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_ParentAssetId",
                table: "maintainarr_asset_installed_components",
                column: "ParentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_TenantId",
                table: "maintainarr_asset_installed_components",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_TenantId_ParentAsse~1",
                table: "maintainarr_asset_installed_components",
                columns: new[] { "TenantId", "ParentAssetId", "ComponentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_TenantId_ParentAsset~",
                table: "maintainarr_asset_installed_components",
                columns: new[] { "TenantId", "ParentAssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_installed_components_TenantId_ParentCompo~",
                table: "maintainarr_asset_installed_components",
                columns: new[] { "TenantId", "ParentComponentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_asset_installed_components");
        }
    }
}
