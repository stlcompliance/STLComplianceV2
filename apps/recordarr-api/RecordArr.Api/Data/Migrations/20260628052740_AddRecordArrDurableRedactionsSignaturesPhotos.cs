using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableRedactionsSignaturesPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_photo_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoEvidenceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PhotoPurpose = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CapturedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_photo_evidence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_redactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RedactionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RedactedRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RedactionReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RedactedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RedactedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_redactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_signature_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignatureRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SignaturePurpose = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    SignerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SignerExternalName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SignerTitle = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    SignatureFileRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CapturedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_signature_records", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_photo_evidence_TenantId",
                table: "recordarr_photo_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_photo_evidence_TenantId_PhotoEvidenceId",
                table: "recordarr_photo_evidence",
                columns: new[] { "TenantId", "PhotoEvidenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_photo_evidence_TenantId_RecordId_CapturedAt",
                table: "recordarr_photo_evidence",
                columns: new[] { "TenantId", "RecordId", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_photo_evidence_TenantId_SourceProduct_SourceObjec~",
                table: "recordarr_photo_evidence",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectRef" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redactions_TenantId",
                table: "recordarr_redactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redactions_TenantId_RedactedRecordId",
                table: "recordarr_redactions",
                columns: new[] { "TenantId", "RedactedRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redactions_TenantId_RedactionId",
                table: "recordarr_redactions",
                columns: new[] { "TenantId", "RedactionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_redactions_TenantId_SourceRecordId_Status",
                table: "recordarr_redactions",
                columns: new[] { "TenantId", "SourceRecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_records_TenantId",
                table: "recordarr_signature_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_records_TenantId_RecordId_SignedAt",
                table: "recordarr_signature_records",
                columns: new[] { "TenantId", "RecordId", "SignedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_records_TenantId_SignatureRecordId",
                table: "recordarr_signature_records",
                columns: new[] { "TenantId", "SignatureRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_signature_records_TenantId_SourceProduct_SourceOb~",
                table: "recordarr_signature_records",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectRef" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_photo_evidence");

            migrationBuilder.DropTable(
                name: "recordarr_redactions");

            migrationBuilder.DropTable(
                name: "recordarr_signature_records");
        }
    }
}
