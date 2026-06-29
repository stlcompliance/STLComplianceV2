using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrSignatureTrustServiceJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_signature_trust_service_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrustServiceJobId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SignatureRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderEnvelopeRef = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CertificateFingerprintSha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SignatureEvidenceHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubmissionEvidenceHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastSubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProviderCallbackStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProviderCallbackRef = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ProviderCallbackReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProviderCallbackEvidenceHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TrustTimestampAuthorityRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LongTermValidationStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_signature_trust_service_jobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_trust_service_jobs_TenantId",
                table: "recordarr_signature_trust_service_jobs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_trust_service_jobs_TenantId_ProviderNam~",
                table: "recordarr_signature_trust_service_jobs",
                columns: new[] { "TenantId", "ProviderName", "ProviderEnvelopeRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_trust_service_jobs_TenantId_SignatureRe~",
                table: "recordarr_signature_trust_service_jobs",
                columns: new[] { "TenantId", "SignatureRecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_trust_service_jobs_TenantId_Status_Requ~",
                table: "recordarr_signature_trust_service_jobs",
                columns: new[] { "TenantId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_trust_service_jobs_TenantId_TrustServic~",
                table: "recordarr_signature_trust_service_jobs",
                columns: new[] { "TenantId", "TrustServiceJobId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_signature_trust_service_jobs");
        }
    }
}
