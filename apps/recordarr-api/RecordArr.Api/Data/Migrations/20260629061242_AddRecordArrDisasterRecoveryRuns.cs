using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecordArr.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordArrDisasterRecoveryRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordarr_disaster_recovery_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisasterRecoveryRunId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Scope = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RecoveryPointId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RecoveryPointCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RpoTargetMinutes = table.Column<int>(type: "integer", nullable: false),
                    RtoTargetMinutes = table.Column<int>(type: "integer", nullable: false),
                    RecoveryPointAgeMinutes = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    RpoMet = table.Column<bool>(type: "boolean", nullable: false),
                    RtoMet = table.Column<bool>(type: "boolean", nullable: false),
                    TotalRecordCount = table.Column<int>(type: "integer", nullable: false),
                    RestoredRecordCount = table.Column<int>(type: "integer", nullable: false),
                    BlockedRecordCount = table.Column<int>(type: "integer", nullable: false),
                    TotalFileCount = table.Column<int>(type: "integer", nullable: false),
                    VerifiedFileCount = table.Column<int>(type: "integer", nullable: false),
                    FailedFileCount = table.Column<int>(type: "integer", nullable: false),
                    EvidenceSummary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordarr_disaster_recovery_runs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_disaster_recovery_runs_TenantId",
                table: "recordarr_disaster_recovery_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_disaster_recovery_runs_TenantId_DisasterRecoveryR~",
                table: "recordarr_disaster_recovery_runs",
                columns: new[] { "TenantId", "DisasterRecoveryRunId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_disaster_recovery_runs_TenantId_RecoveryPointId_C~",
                table: "recordarr_disaster_recovery_runs",
                columns: new[] { "TenantId", "RecoveryPointId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_recordarr_disaster_recovery_runs_TenantId_Status_CompletedAt",
                table: "recordarr_disaster_recovery_runs",
                columns: new[] { "TenantId", "Status", "CompletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordarr_disaster_recovery_runs");
        }
    }
}
