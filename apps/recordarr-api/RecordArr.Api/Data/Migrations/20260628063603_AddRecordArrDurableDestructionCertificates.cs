using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableDestructionCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_destruction_certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestructionCertificateId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RetentionStatusRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisposalReviewRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DispositionAction = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExecutedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CertificateHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_destruction_certificates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_destruction_certificates_TenantId",
                table: "recordarr_destruction_certificates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_destruction_certificates_TenantId_CertificateNumb~",
                table: "recordarr_destruction_certificates",
                columns: new[] { "TenantId", "CertificateNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_destruction_certificates_TenantId_DestructionCert~",
                table: "recordarr_destruction_certificates",
                columns: new[] { "TenantId", "DestructionCertificateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_destruction_certificates_TenantId_DisposalReviewR~",
                table: "recordarr_destruction_certificates",
                columns: new[] { "TenantId", "DisposalReviewRef" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_destruction_certificates_TenantId_RecordId_Execut~",
                table: "recordarr_destruction_certificates",
                columns: new[] { "TenantId", "RecordId", "ExecutedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_destruction_certificates");
        }
    }
}
