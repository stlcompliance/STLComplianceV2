using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableIntakeState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_capture_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaptureRequestId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CaptureType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UploadSessionRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    EvidenceRequirementRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_capture_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recordarr_upload_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadSessionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UploadSessionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SessionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectType = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    SourceObjectId = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    UploadPurpose = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_upload_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_capture_requests_TenantId",
                table: "recordarr_capture_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_capture_requests_TenantId_CaptureRequestId",
                table: "recordarr_capture_requests",
                columns: new[] { "TenantId", "CaptureRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_capture_requests_TenantId_Status_CreatedAt",
                table: "recordarr_capture_requests",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_capture_requests_TenantId_UploadSessionRef",
                table: "recordarr_capture_requests",
                columns: new[] { "TenantId", "UploadSessionRef" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_upload_sessions_TenantId",
                table: "recordarr_upload_sessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_upload_sessions_TenantId_SourceProduct_SourceObje~",
                table: "recordarr_upload_sessions",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectType", "SourceObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_upload_sessions_TenantId_Status_CreatedAt",
                table: "recordarr_upload_sessions",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_upload_sessions_TenantId_UploadSessionId",
                table: "recordarr_upload_sessions",
                columns: new[] { "TenantId", "UploadSessionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_capture_requests");

            migrationBuilder.DropTable(
                name: "recordarr_upload_sessions");
        }
    }
}
