using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRichPmPrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "maintainarr_pm_programs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "draft",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldDefaultValue: "submitted");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivatedAt",
                table: "maintainarr_pm_programs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivatedByPersonId",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutomationDefinitionJson",
                table: "maintainarr_pm_programs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryKey",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceDefinitionJson",
                table: "maintainarr_pm_programs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByPersonId",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DueTriggerDefinitionJson",
                table: "maintainarr_pm_programs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InspectionDefinitionJson",
                table: "maintainarr_pm_programs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerPersonId",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerRoleKey",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwningDepartmentRef",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwningSiteRef",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwningTeamRef",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PausedAt",
                table: "maintainarr_pm_programs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PausedByPersonId",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorityKey",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RetiredAt",
                table: "maintainarr_pm_programs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetiredByPersonId",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScopeDefinitionJson",
                table: "maintainarr_pm_programs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagsJson",
                table: "maintainarr_pm_programs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByPersonId",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPackageDefinitionJson",
                table: "maintainarr_pm_programs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkTypeKey",
                table: "maintainarr_pm_programs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActorPersonId",
                table: "maintainarr_audit_events",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "ActivatedByPersonId",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "AutomationDefinitionJson",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "CategoryKey",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "ComplianceDefinitionJson",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "DueTriggerDefinitionJson",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "InspectionDefinitionJson",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "OwnerPersonId",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "OwnerRoleKey",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "OwningDepartmentRef",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "OwningSiteRef",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "OwningTeamRef",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "PausedAt",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "PausedByPersonId",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "PriorityKey",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "RetiredAt",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "RetiredByPersonId",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "ScopeDefinitionJson",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "TagsJson",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "WorkPackageDefinitionJson",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "WorkTypeKey",
                table: "maintainarr_pm_programs");

            migrationBuilder.DropColumn(
                name: "ActorPersonId",
                table: "maintainarr_audit_events");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "maintainarr_pm_programs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "submitted",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldDefaultValue: "draft");
        }
    }
}
