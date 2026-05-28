using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrServiceTokenCleanupWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexarr_platform_service_token_cleanup_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RetentionDaysAfterExpiry = table.Column<int>(type: "integer", nullable: false),
                    RetentionDaysAfterRevoke = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_service_token_cleanup_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_service_token_cleanup_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurgedCount = table.Column<int>(type: "integer", nullable: false),
                    ExpiredPurgeCount = table.Column<int>(type: "integer", nullable: false),
                    RevokedPurgeCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_service_token_cleanup_runs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_tokens_RevokedAt",
                table: "service_tokens",
                column: "RevokedAt");

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_service_token_cleanup_runs_ProcessedAt",
                table: "nexarr_service_token_cleanup_runs",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexarr_platform_service_token_cleanup_settings");

            migrationBuilder.DropTable(
                name: "nexarr_service_token_cleanup_runs");

            migrationBuilder.DropIndex(
                name: "IX_service_tokens_RevokedAt",
                table: "service_tokens");
        }
    }
}
