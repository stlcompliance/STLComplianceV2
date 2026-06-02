using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrIncidentCreateMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalInvolvedPersonIdsJson",
                table: "staffarr_personnel_incidents",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryKeysJson",
                table: "staffarr_personnel_incidents",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CreateFollowUpTask",
                table: "staffarr_personnel_incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentOrgUnitId",
                table: "staffarr_personnel_incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DiscoveredAt",
                table: "staffarr_personnel_incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmployeeSelfReport",
                table: "staffarr_personnel_incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EvidencePackageRequested",
                table: "staffarr_personnel_incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FollowUpDueAt",
                table: "staffarr_personnel_incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowUpRequired",
                table: "staffarr_personnel_incidents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImmediateActionsTaken",
                table: "staffarr_personnel_incidents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentSource",
                table: "staffarr_personnel_incidents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentType",
                table: "staffarr_personnel_incidents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationDetail",
                table: "staffarr_personnel_incidents",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerPersonId",
                table: "staffarr_personnel_incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalAttention",
                table: "staffarr_personnel_incidents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyHr",
                table: "staffarr_personnel_incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyManager",
                table: "staffarr_personnel_incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifySafetyCompliance",
                table: "staffarr_personnel_incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OutOfServiceRemoveFromDuty",
                table: "staffarr_personnel_incidents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PpeConcern",
                table: "staffarr_personnel_incidents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReadinessDecision",
                table: "staffarr_personnel_incidents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedAssetReference",
                table: "staffarr_personnel_incidents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedDocumentReference",
                table: "staffarr_personnel_incidents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedPolicyReference",
                table: "staffarr_personnel_incidents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedRouteReference",
                table: "staffarr_personnel_incidents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedSupplierReference",
                table: "staffarr_personnel_incidents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedWorkOrderReference",
                table: "staffarr_personnel_incidents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReporterPersonId",
                table: "staffarr_personnel_incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnToWorkNeeded",
                table: "staffarr_personnel_incidents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RootCause",
                table: "staffarr_personnel_incidents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SiteOrgUnitId",
                table: "staffarr_personnel_incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrainingReviewRequired",
                table: "staffarr_personnel_incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TrainingReviewReason",
                table: "staffarr_personnel_incidents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WitnessPersonIdsJson",
                table: "staffarr_personnel_incidents",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkRestriction",
                table: "staffarr_personnel_incidents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_IncidentType_ReportedAt",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "IncidentType", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_ReadinessDecision_ReportedAt",
                table: "staffarr_personnel_incidents",
                columns: new[] { "TenantId", "ReadinessDecision", "ReportedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_IncidentType_ReportedAt",
                table: "staffarr_personnel_incidents");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_personnel_incidents_TenantId_ReadinessDecision_ReportedAt",
                table: "staffarr_personnel_incidents");

            migrationBuilder.DropColumn(name: "AdditionalInvolvedPersonIdsJson", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "CategoryKeysJson", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "CreateFollowUpTask", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "DepartmentOrgUnitId", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "DiscoveredAt", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "EmployeeSelfReport", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "EvidencePackageRequested", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "FollowUpDueAt", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "FollowUpRequired", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "ImmediateActionsTaken", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "IncidentSource", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "IncidentType", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "LocationDetail", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "ManagerPersonId", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "MedicalAttention", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "NotifyHr", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "NotifyManager", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "NotifySafetyCompliance", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "OutOfServiceRemoveFromDuty", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "PpeConcern", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "ReadinessDecision", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "RelatedAssetReference", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "RelatedDocumentReference", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "RelatedPolicyReference", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "RelatedRouteReference", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "RelatedSupplierReference", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "RelatedWorkOrderReference", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "ReporterPersonId", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "ReturnToWorkNeeded", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "RootCause", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "SiteOrgUnitId", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "TrainingReviewReason", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "TrainingReviewRequired", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "WitnessPersonIdsJson", table: "staffarr_personnel_incidents");
            migrationBuilder.DropColumn(name: "WorkRestriction", table: "staffarr_personnel_incidents");
        }
    }
}
