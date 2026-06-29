using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDurableStorageReconciliations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_storage_reconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReconciliationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Scope = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TotalFiles = table.Column<int>(type: "integer", nullable: false),
                    CheckedFiles = table.Column<int>(type: "integer", nullable: false),
                    PassedFiles = table.Column<int>(type: "integer", nullable: false),
                    MissingFiles = table.Column<int>(type: "integer", nullable: false),
                    CorruptFiles = table.Column<int>(type: "integer", nullable: false),
                    QuarantinedFiles = table.Column<int>(type: "integer", nullable: false),
                    PendingScanFiles = table.Column<int>(type: "integer", nullable: false),
                    DeletedFiles = table.Column<int>(type: "integer", nullable: false),
                    IssueSummary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RemediationStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_storage_reconciliations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_storage_reconciliations_TenantId",
                table: "recordarr_storage_reconciliations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_storage_reconciliations_TenantId_ReconciliationId",
                table: "recordarr_storage_reconciliations",
                columns: new[] { "TenantId", "ReconciliationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_storage_reconciliations_TenantId_RemediationStatu~",
                table: "recordarr_storage_reconciliations",
                columns: new[] { "TenantId", "RemediationStatus", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_storage_reconciliations_TenantId_Status_Completed~",
                table: "recordarr_storage_reconciliations",
                columns: new[] { "TenantId", "Status", "CompletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_storage_reconciliations");
        }
    }
}
