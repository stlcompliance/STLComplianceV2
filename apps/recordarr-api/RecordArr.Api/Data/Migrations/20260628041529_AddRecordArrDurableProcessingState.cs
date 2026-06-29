using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableProcessingState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_extraction_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractionResultId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExtractionType = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_extraction_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_ocr_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OcrResultId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Engine = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Language = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_ocr_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_scan_processing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScanProcessingId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScanPurpose = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    OriginalFileRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    GeneratedPdfFileRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OcrResultId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExtractionResultId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_scan_processing", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_extraction_results_TenantId",
                table: "recordarr_extraction_results",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_extraction_results_TenantId_ExtractionResultId",
                table: "recordarr_extraction_results",
                columns: new[] { "TenantId", "ExtractionResultId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_extraction_results_TenantId_RecordId_ExtractedAt",
                table: "recordarr_extraction_results",
                columns: new[] { "TenantId", "RecordId", "ExtractedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_extraction_results_TenantId_Status_ExtractedAt",
                table: "recordarr_extraction_results",
                columns: new[] { "TenantId", "Status", "ExtractedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_ocr_results_TenantId",
                table: "recordarr_ocr_results",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_ocr_results_TenantId_OcrResultId",
                table: "recordarr_ocr_results",
                columns: new[] { "TenantId", "OcrResultId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_ocr_results_TenantId_RecordId_ExtractedAt",
                table: "recordarr_ocr_results",
                columns: new[] { "TenantId", "RecordId", "ExtractedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_ocr_results_TenantId_Status_ExtractedAt",
                table: "recordarr_ocr_results",
                columns: new[] { "TenantId", "Status", "ExtractedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_scan_processing_TenantId",
                table: "recordarr_scan_processing",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_scan_processing_TenantId_RecordId_ProcessedAt",
                table: "recordarr_scan_processing",
                columns: new[] { "TenantId", "RecordId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_scan_processing_TenantId_ScanProcessingId",
                table: "recordarr_scan_processing",
                columns: new[] { "TenantId", "ScanProcessingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_scan_processing_TenantId_Status_ProcessedAt",
                table: "recordarr_scan_processing",
                columns: new[] { "TenantId", "Status", "ProcessedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_extraction_results");

            migrationBuilder.DropTable(
                name: "recordarr_ocr_results");

            migrationBuilder.DropTable(
                name: "recordarr_scan_processing");
        }
    }
}
