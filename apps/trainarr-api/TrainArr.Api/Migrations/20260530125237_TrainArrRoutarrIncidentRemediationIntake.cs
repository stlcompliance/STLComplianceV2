using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrRoutarrIncidentRemediationIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceEventKind",
                table: "trainarr_staffarr_incident_remediations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceIncidentId",
                table: "trainarr_staffarr_incident_remediations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SourceProduct",
                table: "trainarr_staffarr_incident_remediations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "staffarr");

            migrationBuilder.Sql(
                """
                UPDATE trainarr_staffarr_incident_remediations
                SET "SourceProduct" = 'staffarr',
                    "SourceIncidentId" = "StaffarrIncidentId",
                    "SourceEventKind" = COALESCE("SourceEventKind", 'staffarr.incident.created')
                WHERE "SourceIncidentId" = '00000000-0000-0000-0000-000000000000'
                """);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_incident_remediations_TenantId_SourceProd~",
                table: "trainarr_staffarr_incident_remediations",
                columns: new[] { "TenantId", "SourceProduct", "SourceIncidentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainarr_staffarr_incident_remediations_TenantId_SourceProd~",
                table: "trainarr_staffarr_incident_remediations");

            migrationBuilder.DropColumn(
                name: "SourceEventKind",
                table: "trainarr_staffarr_incident_remediations");

            migrationBuilder.DropColumn(
                name: "SourceIncidentId",
                table: "trainarr_staffarr_incident_remediations");

            migrationBuilder.DropColumn(
                name: "SourceProduct",
                table: "trainarr_staffarr_incident_remediations");
        }
    }
}
