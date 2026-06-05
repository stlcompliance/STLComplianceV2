using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPeopleDetailActivationV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternateEmail",
                table: "staffarr_people",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                table: "staffarr_people",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanLoginSnapshot",
                table: "staffarr_people",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "staffarr_people",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpectedStartDate",
                table: "staffarr_people",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasUserAccountSnapshot",
                table: "staffarr_people",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "HomeBaseLocationId",
                table: "staffarr_people",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalFirstName",
                table: "staffarr_people",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalLastName",
                table: "staffarr_people",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalMiddleName",
                table: "staffarr_people",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredName",
                table: "staffarr_people",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhone",
                table: "staffarr_people",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pronouns",
                table: "staffarr_people",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartDate",
                table: "staffarr_people",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkRelationshipType",
                table: "staffarr_people",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_people_HomeBaseLocationId",
                table: "staffarr_people",
                column: "HomeBaseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_people_staffarr_org_units_HomeBaseLocationId",
                table: "staffarr_people",
                column: "HomeBaseLocationId",
                principalTable: "staffarr_org_units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_people_staffarr_org_units_HomeBaseLocationId",
                table: "staffarr_people");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_people_HomeBaseLocationId",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "AlternateEmail",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "CanLoginSnapshot",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "ExpectedStartDate",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "HasUserAccountSnapshot",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "HomeBaseLocationId",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "LegalFirstName",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "LegalLastName",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "LegalMiddleName",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "PreferredName",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "PrimaryPhone",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "Pronouns",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "WorkRelationshipType",
                table: "staffarr_people");
        }
    }
}
