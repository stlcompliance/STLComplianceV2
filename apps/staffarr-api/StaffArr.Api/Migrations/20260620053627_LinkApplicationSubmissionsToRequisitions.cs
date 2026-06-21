using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class LinkApplicationSubmissionsToRequisitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecruitingRequisitionId",
                table: "staffarr_employment_application_submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE ""staffarr_employment_application_submissions"" AS s
SET ""RecruitingRequisitionId"" = c.""RecruitingRequisitionId""
FROM ""staffarr_recruiting_candidates"" AS c
WHERE s.""CreatedCandidateId"" = c.""Id""
  AND s.""RecruitingRequisitionId"" IS NULL
  AND c.""RecruitingRequisitionId"" IS NOT NULL;
");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_submissions_RecruitingRequi~",
                table: "staffarr_employment_application_submissions",
                column: "RecruitingRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_submissions_TenantId_Recrui~",
                table: "staffarr_employment_application_submissions",
                columns: new[] { "TenantId", "RecruitingRequisitionId", "SubmittedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_staffarr_employment_application_submissions_staffarr_recrui~",
                table: "staffarr_employment_application_submissions",
                column: "RecruitingRequisitionId",
                principalTable: "staffarr_recruiting_requisitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staffarr_employment_application_submissions_staffarr_recrui~",
                table: "staffarr_employment_application_submissions");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_employment_application_submissions_RecruitingRequi~",
                table: "staffarr_employment_application_submissions");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_employment_application_submissions_TenantId_Recrui~",
                table: "staffarr_employment_application_submissions");

            migrationBuilder.DropColumn(
                name: "RecruitingRequisitionId",
                table: "staffarr_employment_application_submissions");
        }
    }
}
