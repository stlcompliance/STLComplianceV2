using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrAuditPackageGenerationJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_audit_package_generation_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ToUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArtifactZip = table.Column<byte[]>(type: "bytea", nullable: true),
                    ArtifactJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_audit_package_generation_jobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_audit_package_generation_jobs_CreatedAt",
                table: "maintainarr_audit_package_generation_jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_audit_package_generation_jobs_TenantId_Status_C~",
                table: "maintainarr_audit_package_generation_jobs",
                columns: new[] { "TenantId", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_audit_package_generation_jobs");
        }
    }
}
