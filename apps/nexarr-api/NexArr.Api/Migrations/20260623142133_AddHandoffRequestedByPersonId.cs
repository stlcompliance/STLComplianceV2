using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHandoffRequestedByPersonId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RequestedByPersonId",
                table: "handoff_codes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_handoff_codes_RequestedByPersonId",
                table: "handoff_codes",
                column: "RequestedByPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_handoff_codes_platform_users_RequestedByPersonId",
                table: "handoff_codes",
                column: "RequestedByPersonId",
                principalTable: "platform_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_handoff_codes_platform_users_RequestedByPersonId",
                table: "handoff_codes");

            migrationBuilder.DropIndex(
                name: "IX_handoff_codes_RequestedByPersonId",
                table: "handoff_codes");

            migrationBuilder.DropColumn(
                name: "RequestedByPersonId",
                table: "handoff_codes");
        }
    }
}
