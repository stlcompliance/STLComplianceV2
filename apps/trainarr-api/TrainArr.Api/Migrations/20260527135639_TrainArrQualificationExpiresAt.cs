using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrQualificationExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "trainarr_qualification_issues",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE trainarr_qualification_issues AS issue
                SET "ExpiresAt" = publication."ExpiresAt"
                FROM trainarr_certification_publications AS publication
                WHERE issue."GrantPublicationId" = publication."Id"
                  AND issue."ExpiresAt" IS NULL
                  AND publication."ExpiresAt" IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId_Status_ExpiresAt",
                table: "trainarr_qualification_issues",
                columns: new[] { "TenantId", "Status", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainarr_qualification_issues_TenantId_Status_ExpiresAt",
                table: "trainarr_qualification_issues");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "trainarr_qualification_issues");
        }
    }
}
