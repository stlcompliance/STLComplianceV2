using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrDispatchMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_dispatch_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SenderRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_dispatch_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_dispatch_messages_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_messages_TenantId",
                table: "routarr_dispatch_messages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_messages_TenantId_TripId_CreatedAt",
                table: "routarr_dispatch_messages",
                columns: new[] { "TenantId", "TripId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_messages_TripId",
                table: "routarr_dispatch_messages",
                column: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_dispatch_messages");
        }
    }
}
