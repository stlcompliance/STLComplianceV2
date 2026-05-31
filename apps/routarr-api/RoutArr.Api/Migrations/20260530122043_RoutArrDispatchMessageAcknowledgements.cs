using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrDispatchMessageAcknowledgements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AcknowledgedAt",
                table: "routarr_dispatch_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcknowledgedByPersonId",
                table: "routarr_dispatch_messages",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AcknowledgedByUserId",
                table: "routarr_dispatch_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAcknowledgement",
                table: "routarr_dispatch_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_messages_TenantId_TripId_RequiresAcknowled~",
                table: "routarr_dispatch_messages",
                columns: new[] { "TenantId", "TripId", "RequiresAcknowledgement", "AcknowledgedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routarr_dispatch_messages_TenantId_TripId_RequiresAcknowled~",
                table: "routarr_dispatch_messages");

            migrationBuilder.DropColumn(
                name: "AcknowledgedAt",
                table: "routarr_dispatch_messages");

            migrationBuilder.DropColumn(
                name: "AcknowledgedByPersonId",
                table: "routarr_dispatch_messages");

            migrationBuilder.DropColumn(
                name: "AcknowledgedByUserId",
                table: "routarr_dispatch_messages");

            migrationBuilder.DropColumn(
                name: "RequiresAcknowledgement",
                table: "routarr_dispatch_messages");
        }
    }
}
