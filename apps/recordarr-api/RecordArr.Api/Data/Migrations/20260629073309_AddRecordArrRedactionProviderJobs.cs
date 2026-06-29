using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrRedactionProviderJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_redaction_provider_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderJobId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RedactionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RedactedRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderJobRef = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RedactionPackageHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubmissionEvidenceHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastSubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProviderCallbackStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProviderCallbackRef = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ProviderCallbackReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProviderEvidenceHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_redaction_provider_jobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redaction_provider_jobs_TenantId",
                table: "recordarr_redaction_provider_jobs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redaction_provider_jobs_TenantId_ProviderJobId",
                table: "recordarr_redaction_provider_jobs",
                columns: new[] { "TenantId", "ProviderJobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redaction_provider_jobs_TenantId_ProviderName_Pro~",
                table: "recordarr_redaction_provider_jobs",
                columns: new[] { "TenantId", "ProviderName", "ProviderJobRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redaction_provider_jobs_TenantId_RedactionId_Stat~",
                table: "recordarr_redaction_provider_jobs",
                columns: new[] { "TenantId", "RedactionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redaction_provider_jobs_TenantId_Status_Requested~",
                table: "recordarr_redaction_provider_jobs",
                columns: new[] { "TenantId", "Status", "RequestedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_redaction_provider_jobs");
        }
    }
}
