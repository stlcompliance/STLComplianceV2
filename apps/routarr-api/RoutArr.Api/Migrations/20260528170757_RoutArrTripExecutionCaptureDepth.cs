using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTripExecutionCaptureDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_tenant_trip_execution_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirePreTripDvirBeforeStart = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePostTripDvirBeforeComplete = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDeliveryProofBeforeComplete = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePickupProofBeforeStart = table.Column<bool>(type: "boolean", nullable: false),
                    BlockTripStartOnDvirFail = table.Column<bool>(type: "boolean", nullable: false),
                    BlockTripCompleteOnDvirFail = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_trip_execution_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_trip_execution_settings_TenantId",
                table: "routarr_tenant_trip_execution_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_tenant_trip_execution_settings");
        }
    }
}
