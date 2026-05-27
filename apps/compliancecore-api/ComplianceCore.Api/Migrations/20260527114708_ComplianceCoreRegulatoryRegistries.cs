using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreRegulatoryRegistries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_governing_bodies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_governing_bodies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_jurisdictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoverningBodyId = table.Column<Guid>(type: "uuid", nullable: false),
                    JurisdictionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_jurisdictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_jurisdictions_compliancecore_governing_bodie~",
                        column: x => x.GoverningBodyId,
                        principalTable: "compliancecore_governing_bodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_regulatory_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_regulatory_programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_programs_compliancecore_jurisdict~",
                        column: x => x.JurisdictionId,
                        principalTable: "compliancecore_jurisdictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_rule_packs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegulatoryProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_packs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_rule_packs_compliancecore_regulatory_program~",
                        column: x => x.RegulatoryProgramId,
                        principalTable: "compliancecore_regulatory_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_governing_bodies_TenantId",
                table: "compliancecore_governing_bodies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_governing_bodies_TenantId_BodyKey",
                table: "compliancecore_governing_bodies",
                columns: new[] { "TenantId", "BodyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_jurisdictions_GoverningBodyId",
                table: "compliancecore_jurisdictions",
                column: "GoverningBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_jurisdictions_TenantId",
                table: "compliancecore_jurisdictions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_jurisdictions_TenantId_JurisdictionKey",
                table: "compliancecore_jurisdictions",
                columns: new[] { "TenantId", "JurisdictionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_programs_JurisdictionId",
                table: "compliancecore_regulatory_programs",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_programs_TenantId",
                table: "compliancecore_regulatory_programs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_programs_TenantId_ProgramKey",
                table: "compliancecore_regulatory_programs",
                columns: new[] { "TenantId", "ProgramKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_packs_RegulatoryProgramId",
                table: "compliancecore_rule_packs",
                column: "RegulatoryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_packs_TenantId",
                table: "compliancecore_rule_packs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_packs_TenantId_PackKey_VersionNumber",
                table: "compliancecore_rule_packs",
                columns: new[] { "TenantId", "PackKey", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_rule_packs");

            migrationBuilder.DropTable(
                name: "compliancecore_regulatory_programs");

            migrationBuilder.DropTable(
                name: "compliancecore_jurisdictions");

            migrationBuilder.DropTable(
                name: "compliancecore_governing_bodies");
        }
    }
}
