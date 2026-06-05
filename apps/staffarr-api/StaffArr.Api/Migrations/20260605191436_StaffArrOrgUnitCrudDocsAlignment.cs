using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrOrgUnitCrudDocsAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId_SiteOrgUnit~",
                table: "staffarr_org_unit_assignments");

            migrationBuilder.AddColumn<bool>(
                name: "CanApprove",
                table: "staffarr_org_units",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSupervise",
                table: "staffarr_org_units",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ComplianceSensitive",
                table: "staffarr_org_units",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultSiteOrgUnitId",
                table: "staffarr_org_units",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "staffarr_org_units",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EffectiveEndDate",
                table: "staffarr_org_units",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EffectiveStartDate",
                table: "staffarr_org_units",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact",
                table: "staffarr_org_units",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerPersonId",
                table: "staffarr_org_units",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "staffarr_org_units",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionCode",
                table: "staffarr_org_units",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SafetySensitive",
                table: "staffarr_org_units",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SiteType",
                table: "staffarr_org_units",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamType",
                table: "staffarr_org_units",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "staffarr_org_units",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EffectiveAt",
                table: "staffarr_org_unit_assignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndsAt",
                table: "staffarr_org_unit_assignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "staffarr_org_unit_assignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "staffarr_org_unit_assignments",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_DefaultSiteOrgUnitId",
                table: "staffarr_org_units",
                column: "DefaultSiteOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_units_ManagerPersonId",
                table: "staffarr_org_units",
                column: "ManagerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId",
                table: "staffarr_org_unit_assignments",
                columns: new[] { "TenantId", "PersonId" },
                unique: true,
                filter: "\"IsPrimary\" = TRUE AND \"Status\" IN ('planned','active')");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId_SiteOrgUnit~",
                table: "staffarr_org_unit_assignments",
                columns: new[] { "TenantId", "PersonId", "SiteOrgUnitId", "DepartmentOrgUnitId", "TeamOrgUnitId", "PositionOrgUnitId" },
                unique: true,
                filter: "\"Status\" IN ('planned','active')");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_units_staffarr_org_units_DefaultSiteOrgUnitId",
                table: "staffarr_org_units",
                column: "DefaultSiteOrgUnitId",
                principalTable: "staffarr_org_units",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_org_units_staffarr_people_ManagerPersonId",
                table: "staffarr_org_units",
                column: "ManagerPersonId",
                principalTable: "staffarr_people",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_org_units_staffarr_org_units_DefaultSiteOrgUnitId",
                table: "staffarr_org_units");

            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_org_units_staffarr_people_ManagerPersonId",
                table: "staffarr_org_units");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_org_units_DefaultSiteOrgUnitId",
                table: "staffarr_org_units");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_org_units_ManagerPersonId",
                table: "staffarr_org_units");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId",
                table: "staffarr_org_unit_assignments");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId_SiteOrgUnit~",
                table: "staffarr_org_unit_assignments");

            migrationBuilder.DropColumn(
                name: "CanApprove",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "CanSupervise",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "ComplianceSensitive",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "DefaultSiteOrgUnitId",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "EffectiveEndDate",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "EffectiveStartDate",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "ManagerPersonId",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "PositionCode",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "SafetySensitive",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "SiteType",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "TeamType",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "staffarr_org_units");

            migrationBuilder.DropColumn(
                name: "EffectiveAt",
                table: "staffarr_org_unit_assignments");

            migrationBuilder.DropColumn(
                name: "EndsAt",
                table: "staffarr_org_unit_assignments");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "staffarr_org_unit_assignments");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "staffarr_org_unit_assignments");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_org_unit_assignments_TenantId_PersonId_SiteOrgUnit~",
                table: "staffarr_org_unit_assignments",
                columns: new[] { "TenantId", "PersonId", "SiteOrgUnitId", "DepartmentOrgUnitId", "TeamOrgUnitId", "PositionOrgUnitId" },
                unique: true);
        }
    }
}
