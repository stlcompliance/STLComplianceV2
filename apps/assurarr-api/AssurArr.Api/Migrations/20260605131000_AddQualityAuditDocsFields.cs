using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityAuditDocsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "StandardRefs",
                table: "assurarr_quality_audits",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "ComplianceRefs",
                table: "assurarr_quality_audits",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "AuditeeRefs",
                table: "assurarr_quality_audits",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "AuditTrail",
                table: "assurarr_quality_audits",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StandardRefs",
                table: "assurarr_quality_audits");

            migrationBuilder.DropColumn(
                name: "ComplianceRefs",
                table: "assurarr_quality_audits");

            migrationBuilder.DropColumn(
                name: "AuditeeRefs",
                table: "assurarr_quality_audits");

            migrationBuilder.DropColumn(
                name: "AuditTrail",
                table: "assurarr_quality_audits");
        }
    }
}
