using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityRiskProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_quality_risk_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RiskFactors = table.Column<string[]>(type: "text[]", nullable: false),
                    OpenIssueCount = table.Column<int>(type: "integer", nullable: false),
                    RepeatIssueCount = table.Column<int>(type: "integer", nullable: false),
                    CriticalIssueCount = table.Column<int>(type: "integer", nullable: false),
                    LastIncidentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MitigationActions = table.Column<string[]>(type: "text[]", nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_risk_profiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_risk_profiles_TenantId_RiskLevel",
                table: "assurarr_quality_risk_profiles",
                columns: new[] { "TenantId", "RiskLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_risk_profiles_TenantId_TargetType_TargetRef",
                table: "assurarr_quality_risk_profiles",
                columns: new[] { "TenantId", "TargetType", "TargetRef" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_quality_risk_profiles");
        }
    }
}
