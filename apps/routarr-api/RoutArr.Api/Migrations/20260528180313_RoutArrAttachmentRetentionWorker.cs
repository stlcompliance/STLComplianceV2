using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrAttachmentRetentionWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_attachment_retention_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttachmentsPurgedCount = table.Column<int>(type: "integer", nullable: false),
                    BytesReclaimed = table.Column<long>(type: "bigint", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_attachment_retention_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_attachment_retention_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RetentionDaysAfterTripClose = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_attachment_retention_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_attachment_retention_runs_TenantId",
                table: "routarr_attachment_retention_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_attachment_retention_runs_TenantId_ProcessedAt",
                table: "routarr_attachment_retention_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_attachment_retention_settings_TenantId",
                table: "routarr_tenant_attachment_retention_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_attachment_retention_runs");

            migrationBuilder.DropTable(
                name: "routarr_tenant_attachment_retention_settings");
        }
    }
}
