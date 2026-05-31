using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrComplianceCoreIncidentRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompliancecoreFactPublicationId",
                table: "routarr_dispatch_exceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompliancecoreIncidentRouteStatus",
                table: "routarr_dispatch_exceptions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompliancecoreIncidentRoutedAt",
                table: "routarr_dispatch_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_compliancecore_publication",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "CompliancecoreFactPublicationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routarr_dispatch_exceptions_compliancecore_publication",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "CompliancecoreFactPublicationId",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "CompliancecoreIncidentRouteStatus",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "CompliancecoreIncidentRoutedAt",
                table: "routarr_dispatch_exceptions");
        }
    }
}
