using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTrainArrIncidentRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrainarrIncidentRemediationId",
                table: "routarr_dispatch_exceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainarrIncidentRouteStatus",
                table: "routarr_dispatch_exceptions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TrainarrIncidentRoutedAt",
                table: "routarr_dispatch_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_trainarr_remediation",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "TrainarrIncidentRemediationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routarr_dispatch_exceptions_trainarr_remediation",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "TrainarrIncidentRemediationId",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "TrainarrIncidentRouteStatus",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "TrainarrIncidentRoutedAt",
                table: "routarr_dispatch_exceptions");
        }
    }
}
