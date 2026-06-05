using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCapaDocsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StaffArrSiteId",
                table: "assurarr_capas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffArrLocationId",
                table: "assurarr_capas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "SourceRefs",
                table: "assurarr_capas",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "ActionPlanRefs",
                table: "assurarr_capas",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "VerificationPlanRef",
                table: "assurarr_capas",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "RelatedCustomerComplaintRefs",
                table: "assurarr_capas",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "RelatedSupplierIssueRefs",
                table: "assurarr_capas",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "ComplianceRefs",
                table: "assurarr_capas",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "AuditTrail",
                table: "assurarr_capas",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OpenedAt",
                table: "assurarr_capas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StaffArrSiteId",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "StaffArrLocationId",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "SourceRefs",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "ActionPlanRefs",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "VerificationPlanRef",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "RelatedCustomerComplaintRefs",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "RelatedSupplierIssueRefs",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "ComplianceRefs",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "AuditTrail",
                table: "assurarr_capas");

            migrationBuilder.DropColumn(
                name: "OpenedAt",
                table: "assurarr_capas");
        }
    }
}
