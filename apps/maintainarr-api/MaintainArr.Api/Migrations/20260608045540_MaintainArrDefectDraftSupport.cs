using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrDefectDraftSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComponentKey",
                table: "maintainarr_defects",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectiveAction",
                table: "maintainarr_defects",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByPersonId",
                table: "maintainarr_defects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefectType",
                table: "maintainarr_defects",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeferralCode",
                table: "maintainarr_defects",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DiscoveredAt",
                table: "maintainarr_defects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscoveredByPersonId",
                table: "maintainarr_defects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureMode",
                table: "maintainarr_defects",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentReferenceId",
                table: "maintainarr_defects",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsComplianceImpacting",
                table: "maintainarr_defects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOperabilityImpacting",
                table: "maintainarr_defects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSafetyCritical",
                table: "maintainarr_defects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingCondition",
                table: "maintainarr_defects",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "maintainarr_defects",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "medium");

            migrationBuilder.AddColumn<string>(
                name: "ReadinessNotes",
                table: "maintainarr_defects",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportSource",
                table: "maintainarr_defects",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReportedAt",
                table: "maintainarr_defects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportedByPersonId",
                table: "maintainarr_defects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SidePosition",
                table: "maintainarr_defects",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceReferenceId",
                table: "maintainarr_defects",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "maintainarr_defects",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symptom",
                table: "maintainarr_defects",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemKey",
                table: "maintainarr_defects",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByPersonId",
                table: "maintainarr_defects",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_DefectType_Status",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "DefectType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_ReportedByPersonId_CreatedAt",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "ReportedByPersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_defects_TenantId_ReportSource_Status",
                table: "maintainarr_defects",
                columns: new[] { "TenantId", "ReportSource", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_maintainarr_defects_TenantId_DefectType_Status",
                table: "maintainarr_defects");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_defects_TenantId_ReportedByPersonId_CreatedAt",
                table: "maintainarr_defects");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_defects_TenantId_ReportSource_Status",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "ComponentKey",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "CorrectiveAction",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "DefectType",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "DeferralCode",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "DiscoveredAt",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "DiscoveredByPersonId",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "FailureMode",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "IncidentReferenceId",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "IsComplianceImpacting",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "IsOperabilityImpacting",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "IsSafetyCritical",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "OperatingCondition",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "ReadinessNotes",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "ReportSource",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "ReportedAt",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "ReportedByPersonId",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "SidePosition",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "SourceReferenceId",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "Symptom",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "SystemKey",
                table: "maintainarr_defects");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "maintainarr_defects");
        }
    }
}
