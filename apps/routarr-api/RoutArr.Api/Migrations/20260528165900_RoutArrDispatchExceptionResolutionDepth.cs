using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrDispatchExceptionResolutionDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResolutionTemplateKey",
                table: "routarr_dispatch_exceptions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SlaDueAt",
                table: "routarr_dispatch_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_SlaDueAt",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "SlaDueAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_SlaDueAt",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "ResolutionTemplateKey",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "SlaDueAt",
                table: "routarr_dispatch_exceptions");
        }
    }
}
