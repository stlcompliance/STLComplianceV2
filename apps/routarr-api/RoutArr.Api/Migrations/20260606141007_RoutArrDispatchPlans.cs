using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrDispatchPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_dispatch_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DispatchDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DispatchType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PlannerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DispatcherPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StaffarrSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    RouteRefsJson = table.Column<string>(type: "text", nullable: false),
                    TripRefsJson = table.Column<string>(type: "text", nullable: false),
                    BlockerRefsJson = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CanceledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_dispatch_plans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_plans_TenantId",
                table: "routarr_dispatch_plans",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_plans_TenantId_DispatchNumber",
                table: "routarr_dispatch_plans",
                columns: new[] { "TenantId", "DispatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_plans_TenantId_Status_DispatchDate",
                table: "routarr_dispatch_plans",
                columns: new[] { "TenantId", "Status", "DispatchDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_dispatch_plans");
        }
    }
}
