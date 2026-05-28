using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreRuleChangeMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_rule_change_scan_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PacksScannedCount = table.Column<int>(type: "integer", nullable: false),
                    ChangesDetectedCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_change_scan_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_rule_pack_monitor_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_pack_monitor_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_rule_change_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FromVersion = table.Column<int>(type: "integer", nullable: true),
                    ToVersion = table.Column<int>(type: "integer", nullable: true),
                    PreviousContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NewContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Source = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScanRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_change_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_rule_change_events_compliancecore_rule_chang~",
                        column: x => x.ScanRunId,
                        principalTable: "compliancecore_rule_change_scan_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_events_RulePackId",
                table: "compliancecore_rule_change_events",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_events_ScanRunId",
                table: "compliancecore_rule_change_events",
                column: "ScanRunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_events_TenantId",
                table: "compliancecore_rule_change_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_events_TenantId_PackKey_Detected~",
                table: "compliancecore_rule_change_events",
                columns: new[] { "TenantId", "PackKey", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_scan_runs_StartedAt",
                table: "compliancecore_rule_change_scan_runs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_scan_runs_TenantId",
                table: "compliancecore_rule_change_scan_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_pack_monitor_snapshots_RulePackId",
                table: "compliancecore_rule_pack_monitor_snapshots",
                column: "RulePackId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_pack_monitor_snapshots_TenantId",
                table: "compliancecore_rule_pack_monitor_snapshots",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_rule_change_events");

            migrationBuilder.DropTable(
                name: "compliancecore_rule_pack_monitor_snapshots");

            migrationBuilder.DropTable(
                name: "compliancecore_rule_change_scan_runs");
        }
    }
}
