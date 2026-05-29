using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreWaivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_waivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WaiverKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    GateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SubjectScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_waivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_waivers_compliancecore_rule_packs_RulePackId",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_waivers_RulePackId",
                table: "compliancecore_waivers",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_waivers_TenantId",
                table: "compliancecore_waivers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_waivers_TenantId_Status_ExpiresAt",
                table: "compliancecore_waivers",
                columns: new[] { "TenantId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_waivers_TenantId_WaiverKey",
                table: "compliancecore_waivers",
                columns: new[] { "TenantId", "WaiverKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_waivers");
        }
    }
}
