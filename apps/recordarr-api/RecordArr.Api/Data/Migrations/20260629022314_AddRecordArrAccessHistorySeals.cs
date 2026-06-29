using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrAccessHistorySeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_access_history_seals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessHistorySealId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Scope = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SealedAccessLogCount = table.Column<int>(type: "integer", nullable: false),
                    FirstAccessLogId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SealedThroughAccessLogId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SealedThroughAccessLogHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SealHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SealedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SealedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IssueSummary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_access_history_seals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_history_seals_TenantId",
                table: "recordarr_access_history_seals",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_history_seals_TenantId_AccessHistorySealId",
                table: "recordarr_access_history_seals",
                columns: new[] { "TenantId", "AccessHistorySealId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_history_seals_TenantId_RecordId_SealedAt",
                table: "recordarr_access_history_seals",
                columns: new[] { "TenantId", "RecordId", "SealedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_access_history_seals_TenantId_Status_SealedAt",
                table: "recordarr_access_history_seals",
                columns: new[] { "TenantId", "Status", "SealedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_access_history_seals");
        }
    }
}
