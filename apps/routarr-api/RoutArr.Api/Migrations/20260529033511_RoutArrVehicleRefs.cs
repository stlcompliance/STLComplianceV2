using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrVehicleRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_vehicle_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MirroredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_vehicle_refs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_vehicle_refs_TenantId",
                table: "routarr_vehicle_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_vehicle_refs_TenantId_VehicleRefKey",
                table: "routarr_vehicle_refs",
                columns: new[] { "TenantId", "VehicleRefKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_vehicle_refs");
        }
    }
}
