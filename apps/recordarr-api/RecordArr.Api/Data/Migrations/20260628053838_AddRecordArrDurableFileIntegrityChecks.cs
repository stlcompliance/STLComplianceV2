using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableFileIntegrityChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_file_integrity_checks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrityCheckId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpectedChecksumSha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObservedChecksumSha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CheckMethod = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CheckedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_file_integrity_checks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_file_integrity_checks_TenantId",
                table: "recordarr_file_integrity_checks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_file_integrity_checks_TenantId_FileId_CheckedAt",
                table: "recordarr_file_integrity_checks",
                columns: new[] { "TenantId", "FileId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_file_integrity_checks_TenantId_IntegrityCheckId",
                table: "recordarr_file_integrity_checks",
                columns: new[] { "TenantId", "IntegrityCheckId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_file_integrity_checks_TenantId_RecordId_CheckedAt",
                table: "recordarr_file_integrity_checks",
                columns: new[] { "TenantId", "RecordId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_file_integrity_checks_TenantId_Status_CheckedAt",
                table: "recordarr_file_integrity_checks",
                columns: new[] { "TenantId", "Status", "CheckedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_file_integrity_checks");
        }
    }
}
