using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRootCauseAnalyses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RootCauseRef",
                table: "assurarr_nonconformances",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "assurarr_root_cause_analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NonconformanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Method = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PrimaryCauseCategory = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RootCauseSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ContributingFactors = table.Column<string[]>(type: "text[]", nullable: false),
                    AnalyzedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_root_cause_analyses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_root_cause_analyses_TenantId",
                table: "assurarr_root_cause_analyses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_root_cause_analyses_TenantId_NonconformanceId",
                table: "assurarr_root_cause_analyses",
                columns: new[] { "TenantId", "NonconformanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_root_cause_analyses_TenantId_Number",
                table: "assurarr_root_cause_analyses",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_root_cause_analyses_TenantId_Status",
                table: "assurarr_root_cause_analyses",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_root_cause_analyses");

            migrationBuilder.DropColumn(
                name: "RootCauseRef",
                table: "assurarr_nonconformances");
        }
    }
}
