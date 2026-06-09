using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExpandMaintenancePartsKitSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivatedAt",
                table: "maintainarr_maintenance_parts_kits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivatedByPersonId",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                table: "maintainarr_maintenance_parts_kits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByPersonId",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloneSourcePartsKitId",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByPersonId",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefinitionJson",
                table: "maintainarr_maintenance_parts_kits",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EffectiveAt",
                table: "maintainarr_maintenance_parts_kits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "maintainarr_maintenance_parts_kits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KitCategoryKey",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KitTypeKey",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPersonId",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerRoleKey",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwningSiteRef",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwningTeamRef",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorityKey",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RetiredAt",
                table: "maintainarr_maintenance_parts_kits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetiredByPersonId",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TagsJson",
                table: "maintainarr_maintenance_parts_kits",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByPersonId",
                table: "maintainarr_maintenance_parts_kits",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "maintainarr_maintenance_parts_kits",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "maintainarr_maintenance_parts_kit_lines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByPersonId",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "maintainarr_inspection_templates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPersonId",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerRoleKey",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwningSiteRef",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwningTeamRef",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedAt",
                table: "maintainarr_inspection_templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedByPersonId",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RetiredAt",
                table: "maintainarr_inspection_templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetiredByPersonId",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettingsJson",
                table: "maintainarr_inspection_templates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagsJson",
                table: "maintainarr_inspection_templates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateCategoryKey",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByPersonId",
                table: "maintainarr_inspection_templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanBeSkipped",
                table: "maintainarr_inspection_template_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "maintainarr_inspection_template_categories",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "maintainarr_inspection_template_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SettingsJson",
                table: "maintainarr_inspection_template_categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SkipReasonRequired",
                table: "maintainarr_inspection_template_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TimingTracked",
                table: "maintainarr_inspection_template_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HelpText",
                table: "maintainarr_inspection_checklist_items",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SettingsJson",
                table: "maintainarr_inspection_checklist_items",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "ActivatedByPersonId",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "ApprovedByPersonId",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "CloneSourcePartsKitId",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "DefinitionJson",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "EffectiveAt",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "KitCategoryKey",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "KitTypeKey",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "OwnerPersonId",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "OwnerRoleKey",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "OwningSiteRef",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "OwningTeamRef",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "PriorityKey",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "RetiredAt",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "RetiredByPersonId",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "TagsJson",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "maintainarr_maintenance_parts_kits");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "maintainarr_maintenance_parts_kit_lines");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonId",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "OwnerPersonId",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "OwnerRoleKey",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "OwningSiteRef",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "OwningTeamRef",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "PublishedByPersonId",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "RetiredAt",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "RetiredByPersonId",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "SettingsJson",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "TagsJson",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "TemplateCategoryKey",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonId",
                table: "maintainarr_inspection_templates");

            migrationBuilder.DropColumn(
                name: "CanBeSkipped",
                table: "maintainarr_inspection_template_categories");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "maintainarr_inspection_template_categories");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "maintainarr_inspection_template_categories");

            migrationBuilder.DropColumn(
                name: "SettingsJson",
                table: "maintainarr_inspection_template_categories");

            migrationBuilder.DropColumn(
                name: "SkipReasonRequired",
                table: "maintainarr_inspection_template_categories");

            migrationBuilder.DropColumn(
                name: "TimingTracked",
                table: "maintainarr_inspection_template_categories");

            migrationBuilder.DropColumn(
                name: "HelpText",
                table: "maintainarr_inspection_checklist_items");

            migrationBuilder.DropColumn(
                name: "SettingsJson",
                table: "maintainarr_inspection_checklist_items");
        }
    }
}
