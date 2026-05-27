using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrEquipmentAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_equipment_availability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AvailabilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_equipment_availability", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_equipment_availability_TenantId",
                table: "routarr_equipment_availability",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_equipment_availability_TenantId_StartsAt_EndsAt",
                table: "routarr_equipment_availability",
                columns: new[] { "TenantId", "StartsAt", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_equipment_availability_TenantId_VehicleRefKey_Start~",
                table: "routarr_equipment_availability",
                columns: new[] { "TenantId", "VehicleRefKey", "StartsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_equipment_availability");
        }
    }
}
