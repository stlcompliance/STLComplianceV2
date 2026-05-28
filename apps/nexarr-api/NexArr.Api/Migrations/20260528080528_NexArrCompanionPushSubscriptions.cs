using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrCompanionPushSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PushDeliveredCount",
                table: "nexarr_companion_notification_dispatches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nexarr_companion_push_subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    P256dhKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AuthKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_companion_push_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_companion_push_subscriptions_TenantId",
                table: "nexarr_companion_push_subscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_companion_push_subscriptions_TenantId_UserId",
                table: "nexarr_companion_push_subscriptions",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_companion_push_subscriptions_TenantId_UserId_Endpoint",
                table: "nexarr_companion_push_subscriptions",
                columns: new[] { "TenantId", "UserId", "Endpoint" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexarr_companion_push_subscriptions");

            migrationBuilder.DropColumn(
                name: "PushDeliveredCount",
                table: "nexarr_companion_notification_dispatches");
        }
    }
}
