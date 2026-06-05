using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFindingDocsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceRequirementRef",
                table: "assurarr_audit_findings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "EvidenceRecordRefs",
                table: "assurarr_audit_findings",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceRequirementRef",
                table: "assurarr_audit_findings");

            migrationBuilder.DropColumn(
                name: "EvidenceRecordRefs",
                table: "assurarr_audit_findings");
        }
    }
}
