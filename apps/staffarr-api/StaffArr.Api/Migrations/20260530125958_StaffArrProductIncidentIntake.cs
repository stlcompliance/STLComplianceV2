using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrProductIncidentIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceEventKind",
                table: "staffarr_personnel_incidents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceIncidentId",
                table: "staffarr_personnel_incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceProduct",
                table: "staffarr_personnel_incidents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceReferenceKey",
                table: "staffarr_personnel_incidents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_source_incident",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "SourceProduct", "SourceIncidentId" },
                unique: true,
                filter: "\"SourceIncidentId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staffarr_personnel_incidents_source_incident",
                table: "staffarr_personnel_incidents");

            migrationBuilder.DropColumn(
                name: "SourceEventKind",
                table: "staffarr_personnel_incidents");

            migrationBuilder.DropColumn(
                name: "SourceIncidentId",
                table: "staffarr_personnel_incidents");

            migrationBuilder.DropColumn(
                name: "SourceProduct",
                table: "staffarr_personnel_incidents");

            migrationBuilder.DropColumn(
                name: "SourceReferenceKey",
                table: "staffarr_personnel_incidents");
        }
    }
}
